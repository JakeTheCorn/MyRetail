using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Api.Common;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Api.IntegrationTests
{
    public class TestDbClient : IDbClient
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public TestDbClient SetConnection(IDbConnection connection)
        {
            _connection = connection;
            return this;
        }
        
        public TestDbClient SetTransaction(IDbTransaction transaction)
        {
            _transaction = transaction;
            return this;
        }

        public IEnumerable<T> Query<T>(string sql)
        {
            return _connection.Query<T>(sql, transaction: _transaction);
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            return _connection.GetAll<T>(_transaction);
        }

        public long Insert<T>(T item) where T : class
        {
            return _connection.Insert(item, _transaction);
        }

        public bool Delete<T>(T item) where T : class
        {
            return _connection.Delete(item, _transaction);
        }

        public bool Update<T>(T item) where T : class
        {
            return _connection.Update(item, _transaction);
        }

        public int Execute(string command, object values = null)
        {
            return _connection.Execute(command, values, _transaction);
        }
    }

    public class TestHttpClient
    {
        private readonly HttpClient _client;

        public TestHttpClient(HttpClient client)
        {
            _client = client;
        }

        public HttpResponseMessage Get(string path)
        {
            return _client.GetAsync(path).Result;
        }

        public HttpResponseMessage Post<TBodyType>(string path, TBodyType body)
        {
            var jsonContent = BuildJsonContent(body);
            
            return _client.PostAsync(path, jsonContent).Result;
        }

        private StringContent BuildJsonContent<T>(T serializable)
        {
            var json = JsonConvert.SerializeObject(serializable);

            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public HttpResponseMessage Delete(string path)
        {
            return _client.DeleteAsync(path).Result;
        }

        public HttpResponseMessage Put<TBodyType>(string path, TBodyType body)
        {
            var json = BuildJsonContent(body);

            return _client.PutAsync(path, json).Result;
        }
    }

    public class ApiTestCase
    {
        private IConfigurationRoot _configurationRoot;
        private static TestServer _server;
        protected static TestDbClient Db;

        private static SqlConnection _connection;
        private SqlTransaction _transaction;
        private IServiceProvider _services;
        private IServiceCollection _serviceCollection;

        [SetUp]
        public void SetUpApiTestCase()
        {
            if (_configurationRoot is null)
            {
                _configurationRoot = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("settings.json")
                    .Build();
            }

            if (_connection is null)
            {
                _connection = new SqlConnection(GetDbConnectionString());
        
                _connection.Open();
            }

            if (Db is null)
                Db = new TestDbClient();

            _transaction = _connection.BeginTransaction();

            Db.SetConnection(_connection).SetTransaction(_transaction);

            if (_server is null)
            {
                Startup.SetDbClient(Db);
                var webHostBuilder = new WebHostBuilder()
                    .UseConfiguration(_configurationRoot)
                    .UseStartup<Startup>();
                
                _server = new TestServer(webHostBuilder);
            }

            _services = _server.Host.Services;

            Client = new TestHttpClient(_server.CreateClient());
        }
        
        protected T GetService<T>() => (T)_services.GetService(typeof(T));

        
        [TearDown]
        public void TearDownApiTestCase()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }

        private string GetDbConnectionString()
        {
            return _configurationRoot.GetConnectionString("SqlServer");
        }

        protected ResponseExpectations Expect(HttpResponseMessage response)
        {
            return new ResponseExpectations(response);
        }

        protected DbExpectations Expect(IDbClient db)
        {
            return new DbExpectations(db);
        }

        protected TestHttpClient Client { get; set; }
    }

    public class DbExpectations
    {
        private readonly IDbClient _db;

        public DbExpectations(IDbClient db)
        {
            _db = db;
        }

        public DbExpectations ToHaveEmptyTables(params string[] tableNames)
        {
            foreach (var tableName in tableNames)
                Assert.AreEqual(0, _db.Query<int>($"select count(*) from {tableName}").Single());

            return this;
        }
    }

    public class ResponseExpectations
    {
        private readonly HttpResponseMessage _response;

        public ResponseExpectations(HttpResponseMessage response)
        {
            _response = response;
        }

        public ResponseExpectations ToBeOk()
        {
            Assert.AreEqual(HttpStatusCode.OK, _response.StatusCode, "Response should have expected status code");
            return this;
        }

        public ResponseExpectations ToHaveBody<TBodyType>(TBodyType expectedBody)
        {
            var body = GetResponseBody<TBodyType>();
            
            Assert.AreEqual(expectedBody, body, "Response body did not match expectation");

            return this;
        }
        
        public ResponseExpectations And => this;


        private TBodyType GetResponseBody<TBodyType>()
        {
            return _response.Parse<TBodyType>();
        }
    }

    public static class HttpExtensions
    {
        public static TBodyType Parse<TBodyType>(this HttpResponseMessage response)
        {
            var responseBodyString = response.Content.ReadAsStringAsync().Result;

            if (typeof(TBodyType) == typeof(String))
                return (TBodyType)(object)responseBodyString;

            return JsonConvert.DeserializeObject<TBodyType>(responseBodyString);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            if (services.IsReadOnly)
                throw new ReadOnlyException($"{nameof(services)} is read only");

            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
            if (serviceDescriptor != null)
            {
                services.Remove(serviceDescriptor);
            }

            return services;
        }
        
        public static void SwapTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        {
            if (services.Any(x => x.ServiceType == typeof(TService) && x.Lifetime == ServiceLifetime.Transient))
            {
                var serviceDescriptors = services.Where(x => x.ServiceType == typeof(TService) && x.Lifetime == ServiceLifetime.Transient).ToList();
                foreach (var serviceDescriptor in serviceDescriptors)
                {
                    services.Remove(serviceDescriptor);
                }
            }

            services.AddTransient(typeof(TService), (sp) => implementationFactory(sp));
        }
    }
}
