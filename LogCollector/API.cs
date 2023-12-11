using LogCollector.Context;
using LogCollector.Model;
using System.Text.RegularExpressions;

namespace LogCollectorAPI
{

    /// <summary>
    /// Log Collector API
    /// </summary>
    public static class API
    {
        // a cache for applications. Lives forever
        static private Dictionary<string, int> appCache = new Dictionary<string, int>();

        // something to synchronize the application inserts
        static SemaphoreSlim ss = new SemaphoreSlim(1, 1);

        /// <summary>
        ///  Adds the log collector endpoint to the web application
        /// </summary>
        public static void AddLogCollectorApi(this WebApplication app)
        {

            app.MapPost("logs/LogMessage", async (LogMessageModel[] arr, IDapperContext db, ILoggerFactory loggerFactory) =>
            {

                var log = loggerFactory.CreateLogger<LogMessageModel>();
               // log.LogInformation("Received: " + arr.Length);
                try
                {
                    await Parallel.ForEachAsync(arr, async (a, ct) =>
                    {
                        if (a == null)
                        {

                            log.LogDebug("Not much to do. Null record received");
                            return;
                        }

                        if (a.application == null) a.application = "null";
                        if (a.timestamp == null || a.timestamp <= 0) a.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (String.IsNullOrEmpty(a.message)) a.message = "null";

                        int appId;

                        // try the cache
                        if (!appCache.TryGetValue(a.application, out appId))
                        {
                            // bounced
                            // synchronize DB actions
                            await ss.WaitAsync();
                            try
                            {
                                // try the cache again. perhaps something has changed since we last checked
                                if (appCache.TryGetValue(a.application, out appId)) goto jumpOut;

                                // Get the app Id 
                                var dbappId = (await db.GetApplicationByName(a.application)).FirstOrDefault();
                                // or create a new app if it has not been seen before
                                if (dbappId == null) dbappId = await db.Insert<Application>(new Application() { name = a.application });

                                if (dbappId == null)
                                {
                                    return;
                                }

                                appId = dbappId.Value;
                                appCache.Add(a.application, appId);
                            }
                            finally { ss.Release(); }
                        }

                    // Yes ... a goto label. Don't judge me
                    jumpOut:

                        // DateTime from unixtime
                        // MaxValue in case of overflow
                        var ts = a.timestamp <= 253402300799L ? DateTimeOffset.FromUnixTimeSeconds(a.timestamp.Value).DateTime : DateTime.MaxValue;

                      

                        // Default the log level to info
                        var logMessage = new LogMessage()
                        {
                            // default to info level
                            log_level = LogCollector.Model.LogLevel.info,
                            // we already figured out the app ID. by now it should not be null
                            application_id = appId,
                            // by default assuming the entire input is the message
                            message = a.message,
                            date = ts

                        }; 
                        
                        // parse the log level
                        string pattern = @"^\[(.*?)\]\s*(.*?)\s*$";
                        var match = Regex.Match(a.message, pattern);

                        if (match != null)
                        {
                            if (match.Groups.Count == 3)
                            {
                                // as expected. 3 groups
                                // try parse the log level
                                if (Enum.TryParse(match.Groups[1].Value.Trim(), true, out LogCollector.Model.LogLevel l))
                                {
                                    // all is good and parsable
                                    logMessage.log_level = l;
                                    logMessage.message = match.Groups[2].Value.Trim();
                                }
                                else
                                {
                                    //unable to parse
                                    // staying with a defautl log level and message values
                                    log.LogWarning($"Unable to parse the log level: [{match.Groups[1].Value.Trim()}]");

                                }
                            }
                        }
                        else
                        {
                            log.LogError("No Match");
                        }


                        // insert the resulting log Message
                        await db.InsertLog(logMessage);

                    });
                }
                catch (Exception ex)
                {
                    log.LogError("Something went wrong :"  +ex.Message);
                    return Results.Problem(ex.Message);
                }

                return Results.Ok();


            }).WithDescription("An Endpoint to sink your logs into...").WithOpenApi();
        }
    }
}
