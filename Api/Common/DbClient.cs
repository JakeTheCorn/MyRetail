using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Api.Common
{

    public interface IDbClient
    {
        IEnumerable<T> Query<T>(string sql);
        IEnumerable<T> GetAll<T>() where T : class;
        long Insert<T>(T item) where T : class;
        bool Delete<T>(T item) where T : class;
        bool Update<T>(T obj) where T : class;
        int Execute(string command, object values = null);
    }

    public class DbClient : IDbClient
    {
        private readonly IDbManager _dbManager;

        public DbClient(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public IEnumerable<T> Query<T>(string sql)
        {
            using var connection = _dbManager.CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection.Query<T>(sql);
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            using var connection = _dbManager.CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection.GetAll<T>();
        }

        public long Insert<T>(T item) where T : class
        {
            using var connection = _dbManager.CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection.Insert(item);
        }

        public bool Delete<T>(T item) where T : class
        {
            using var connection = _dbManager.CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection.Delete(item);
        }

        public bool Update<T>(T item) where T : class
        {
            using var connection = _dbManager.CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection.Update(item);
        }

        public int Execute(string command, object values = null)
        {
            using var connection = _dbManager.CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection.Execute(command, values);
        }
    }
}