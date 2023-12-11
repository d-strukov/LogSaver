using Dapper.Contrib.Extensions;

namespace LogCollector.Model
{
    /// <summary>
    /// Record class for application
    /// </summary>
    [Table("application")]
    public class Application
    {
        [Key]
        public int id { get; set; }
        
        public string? name { get; set; }
    }
}
