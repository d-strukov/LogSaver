namespace LogCollector.Model
{
    /// <summary>
    /// Model for the communication with the Log Collector API
    /// </summary>
    public class LogMessageModel
    {
        /// <summary>
        /// Unix Timestamp (seconds)
        /// </summary>
        public long? timestamp { get; set; }
        /// <summary>
        /// Application Name
        /// </summary>
        public string? application { get; set; }
        /// <summary>
        /// Log message. e.g. [debug] This is a test message
        /// </summary>
        public string? message { get; set; }

    }
}