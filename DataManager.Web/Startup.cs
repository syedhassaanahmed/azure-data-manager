using DataManager.Models;
using DataManager.Options;
using DataManager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(options => Configuration.Bind("AzureAd", options))
            .AddCookie();

            services.AddMvc();

            services.AddOptions()
                .Configure<CosmosDbOptions>(Configuration.GetSection("CosmosDb"))
                .Configure<DataFactoryOptions>(Configuration.GetSection("DataFactory"))
                .Configure<KeyVaultOptions>(Configuration.GetSection("KeyVault"))
                .Configure<StorageAccountOptions>(Configuration.GetSection("StorageAccount"))
                .Configure<DatabricksOptions>(Configuration.GetSection("Databricks"))
                .AddSingleton<CosmosDbService>()
                .AddSingleton<DataFactoryService>()
                .AddSingleton<ConnectionService>()
                .AddSingleton<TriggerService>()
                .AddSingleton<DatasetService>()
                .AddSingleton<ActivityService>()
                .AddSingleton<PipelineService>();
            
            if (Configuration.GetValue<bool>("LoadSampleData"))
            {
                var cosmosDbService = services.BuildServiceProvider().GetService<CosmosDbService>();
                LoadSampleDataAsync(cosmosDbService).Wait();
            }
        }

        private async Task LoadSampleDataAsync(CosmosDbService cosmosDbService)
        {
            await Task.WhenAll(
                    LoadSampleDataAsync<Dataset>(cosmosDbService, "dataset", "datasets.json"),
                    LoadSampleDataAsync<Job>(cosmosDbService, "job", "jobs.json")
            );
        }

        private async Task LoadSampleDataAsync<T>(CosmosDbService cosmosDbService, string collection, string fileName)
            where T : BaseEntity
        {
            var assembly = GetType().Assembly;
            var resourceName = assembly.GetManifestResourceNames().Single(r => r.EndsWith(fileName));

            var stream = assembly.GetManifestResourceStream(resourceName);
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                var documents = serializer.Deserialize<IEnumerable<T>>(jsonReader);
                var tasks = documents.Select(d => cosmosDbService.UpsertAsync(collection, d));
                await Task.WhenAll(tasks);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
