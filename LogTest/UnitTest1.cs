using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using LogCollector.Context;
using LogCollector.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LogTest
{
    public class Tests
    {

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.test.json")
                .Build();
            return config;
        }

        static readonly IDapperContext _db = new DapperContext(InitConfiguration());
        //static readonly IDapperContext _db = Substitute.For<IDapperContext>();
        [SetUp]
        public async Task Setup()
        {
            await _db.DeleteAll<LogMessage>();
            await _db.DeleteAll<Application>();

            var app = new Application()
            {
                name = "test"
            };

            var appId = await _db.Insert<Application>(app);

            Console.WriteLine(appId);

            for (var i = 0; i < 10; i++)
            {
                await _db.InsertLog(new LogMessage()
                {
                    application_id = appId,
                    log_level = LogCollector.Model.LogLevel.error,
                    message = "test1Message",
                    date = DateTime.UtcNow
                });
            }

        }

        [Test]
        public void TestApp()
        {
            //    _db. GetApplicationByName(Arg.Is("application")).Returns(Task.FromResult(new int?[] { 1 }.AsEnumerable()));
            Console.WriteLine(_db.GetApplicationByName("application").Result.SingleOrDefault());
        }


        [Test]
        public void TestRegex()
        {
            string input = @"[ debug] this sa asdf klksdfmg;l dafsdf sdf;asdf asdfwqe []awqe[ ]asd [ sadf]as]df] # $ 

asdf";
            string pattern = @"^\[(.*?)\]\s*(.*?)\s*$";
            var m = Regex.Match(input, pattern, RegexOptions.Singleline);

            Console.WriteLine(m.Groups.Count);
            foreach (var m2 in m.Groups)
            {
                Console.WriteLine(m2.ToString());
            }

        }

        [Test]
        public void TestApplications()
        {

            Console.WriteLine($"Application {_db.GetApplicationByName("test").Result.FirstOrDefault()}");

            var r = _db.GetAll<Application>().Result;
            foreach (var r2 in r)
            {
                Console.WriteLine($"id:{r2.id} name: {r2.name}");

            }
            var l = _db.GetAll<LogMessage>().Result;
            foreach (var l2 in l)
            {
                Console.WriteLine($"id:{l2.id} name: {l2.message} {l2.log_level}");
            }
            Assert.Pass();
        }

        [Test]
        public async Task TestTheWholeThing()
        {
            var waf = new WAF(s =>
            {
                s.AddSingleton<IDapperContext, DapperContext>();
                s.AddSingleton(InitConfiguration());
                s.AddLogging(l => l.AddConsole());
            });

            var client = waf.CreateClient();

            var input = new LogMessageModel[1500];
            var levels = Enum.GetValues<LogCollector.Model.LogLevel>();

            Random random = new Random();
            int rand = 0;

            for (int i = 0; i < input.Length - 5; i++)
            {

                rand = random.Next(0, levels.Length);
                input[i] =
                new LogMessageModel()
                {
                    application = $"appName {(i + 1) / 100}",
                    message = $"[{levels[rand]}] This is the message from above {i}",
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                };
            }

            input[1495] =
          new LogMessageModel()
          {
              application = null,
              message = "Null applicaiton; Negative Time; This is the message from above",
              timestamp = -1,
          };

            input[1496] =
            new LogMessageModel()
            {
                application = null,
                message = "Null applicaiton; This is the message from above",
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
            };



            input[1497] =
            new LogMessageModel()
            {
                application = "appName",
                message = @" I am Multiline!!!! asdf asdfas df
            
     
as df This i


s the message from above",
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
            };

            input[1498] =
            new LogMessageModel()
            {
                application = "appName",
                message = "No Log Level; This is the message from above",
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
            };

            input[1499] =
            new LogMessageModel()
            {
                application = "appName",
                message = "[ asd] Unknown Log level This is the message from above",
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
            };


            JsonContent content = JsonContent.Create(input);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var res = await client.PostAsync("logs/LogMessage", content);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

        }
    }
}