using Api.Common;
using Api.Features.Products;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Api
{
    public class Startup
    {
        private static IDbClient _dbClient;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddTransient<IDbManager, DbManager>()
                .AddTransient<IProductsService, ProductsService>()
                .AddTransient<IProductsDataService, ProductsDataService>();

            ConfigureDbClient(services);
            
            services.AddControllers();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
            });
        }

        private static void ConfigureDbClient(IServiceCollection services)
        {
            if (_dbClient is not null)
                services.AddSingleton(_dbClient);
            else
                services.AddTransient<IDbClient, DbClient>();
        }

        public static void SetDbClient(IDbClient dbClient)
        {
            _dbClient = dbClient;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // new DatabaseCreator().CreateDatabase();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}