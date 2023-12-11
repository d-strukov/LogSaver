using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Data;
using Dapper.Contrib.Extensions;
using LogCollector.Model;

namespace LogCollector.Context
{
    /// <summary>
    /// IMplementation of IDapperCOntext
    /// </summary>
    public class DapperContext : IDapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            // Get the connection string from the config file
            _configuration = configuration;
            var cs = _configuration.GetConnectionString("SqlConnection");
            if (cs == null) throw new Exception("SqlConnection not found in the config file");
            _connectionString = cs;
        }

        public IDbConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);

        public async Task<IEnumerable<T>> GetAll<T>() where T : class
        {
            return await CreateConnection().GetAllAsync<T>();

        }

        public async Task<bool> DeleteAll<T>() where T : class
        {
            return await CreateConnection().DeleteAllAsync<T>();

        }

        public async Task<int> Insert<T>(T item) where T : class
        {
            return await CreateConnection().InsertAsync<T>(item);

        }

        public async Task<IEnumerable<int?>> GetApplicationByName(string name)
        {
            return await CreateConnection().QueryAsync<int?>("select id from application where name like @name", new {name = name});

        }

        public async Task<int> InsertLog(LogMessage item)
        {
            return await CreateConnection().ExecuteAsync($"INSERT INTO public.logmessage( application_id, date, message, log_level) VALUES ( @appid, @date, @message, cast(@loglevel  as loglevel)); ", new { appid = item.application_id, date = item.date, message=item.message, loglevel=item.log_level.ToString() });

        }


    }
}
