using cl2j.DataStore.Json;
using cl2j.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace cl2j.DataStore.TestApp
{
    internal class Program
    {
        private static async Task Main()
        {
            ServiceProvider serviceProvider = ConfigureServices();
            await serviceProvider.GetRequiredService<DataStoreSample>().ExecuteAsync();
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            services.AddSingleton(configuration);

            services.AddLogging(builder => { builder.AddDebug().SetMinimumLevel(LogLevel.Trace); });

            //Bootstrap the FileStorage to be available from DependencyInjection.
            //This will allow accessing IFileStorageProviderFactory instance
            services.AddFileStorage();

            //Configure the JSON DataStore.
            //The store will use the FileStorageProvider to access the file colors.json.
            services.AddDataStoreJson<string, Color>("DataStore", "colors.json", (predicate) =>
            {
                return predicate.Item1.Id == predicate.Item2;
            });

            services.AddSingleton<DataStoreSample>();

            var serviceProvider = services.BuildServiceProvider();

            //Add the FileStorgae Disk provider
            serviceProvider.UseFileStorageDisk();

            return serviceProvider;
        }
    }
}