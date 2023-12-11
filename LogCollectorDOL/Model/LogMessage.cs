using Dapper.Contrib.Extensions;

namespace LogCollector.Model
{

    /// <summary>
    /// Record class for Log Message
    /// </summary>
    [Table("logmessage")]
    public class LogMessage
    {
        [Key]
        public int id { get; set; }

        public int application_id { get; set; }
        public DateTime date { get; set; }
        public string message { get; set; } = string.Empty;

        public LogLevel log_level { get; set; }


    }
}
