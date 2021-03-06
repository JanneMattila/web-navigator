using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using System.Threading.Tasks;

namespace WebNavigator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Web Navigator started");
            using var openTelemetry = Sdk.CreateTracerProviderBuilder()
                .AddSource("WebNavigator")
                .AddConsoleExporter()
                .Build();

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
#if DEBUG
                .AddUserSecrets<Program>()
#endif
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var navigateUri = configuration.GetValue<string>("navigateUri");

            var navigator = new Navigator();
            await navigator.NavigateAsync(navigateUri);
        }
    }
}
