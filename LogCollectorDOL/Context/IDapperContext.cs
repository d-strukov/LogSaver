using System.Data;
using LogCollector.Model;

namespace LogCollector.Context
{
    /// <summary>
    /// A simple repository interface 
    /// </summary>
    public interface IDapperContext
    {
        /// <summary>
        /// will create a DB connection
        /// </summary>
        /// <returns>DB Connection</returns>
        IDbConnection CreateConnection();
        /// <summary>
        /// Truncates a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>true if successfull</returns>
        Task<bool> DeleteAll<T>() where T : class;
        /// <summary>
        /// Selects * from T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>a collection of results</returns>
        Task<IEnumerable<T>> GetAll<T>() where T : class;
        /// <summary>
        /// A simple generic insert. Works for simple entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"> the T to insert</param>
        /// <returns>generated ID</returns>
        Task<int> Insert<T>(T item) where T : class;
        /// <summary>
        /// A Spceiffic Imsert for a LogMessage
        /// </summary>
        /// <param name="item"></param>
        /// <returns>generated ID</returns>
        Task<int> InsertLog(LogMessage item);
        /// <summary>
        /// Finds an application based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Ids of matching applications or null if none are matched</returns>
        Task<IEnumerable<int?>> GetApplicationByName(string name);
    }
}