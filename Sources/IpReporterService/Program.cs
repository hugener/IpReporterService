// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace IpReporterService
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Events;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var logger = GetLog();
            try
            {
                var isService = !(Debugger.IsAttached || args.Contains("--console"));
                var hostBuilder = new HostBuilder()
                    .ConfigureHostConfiguration(x => x.SetBasePath(Directory.GetCurrentDirectory()))
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton(logger);
                        services.AddHostedService<IpReporterService>();
                    });
                if (isService)
                {
                    logger.Information("Starting IP Reporter Service as a Service");
                    await hostBuilder.RunAsServiceAsync();
                }
                else
                {
                    logger.Information("Starting IP Reporter Service");
                    await hostBuilder.RunConsoleAsync();
                }

                logger.Information("Stopped IP Reporter Service");
            }
            catch (Exception e)
            {
                logger.Error(e, "An Error occured");
            }
        }

        private static ILogger GetLog()
        {
            var logConfiguration = new LoggerConfiguration().MinimumLevel.Is(LogEventLevel.Debug);
            logConfiguration = logConfiguration.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fffff} {Level:u3}] <{ThreadId,-5}> {SourceContext,-50:l} | {Message:lj}{NewLine}{Exception}");
            logConfiguration = logConfiguration.WriteTo.Async(x => x.File(
                    "log.txt",
                    outputTemplate: "[{Timestamp:HH:mm:ss.fffff} {Level:u3}] <{ThreadId,-5}> {SourceContext,-50:l} | {Message:lj}{NewLine}{Exception}",
                    fileSizeLimitBytes: 4_000_000,
                    retainedFileCountLimit: 8));
            return logConfiguration.Enrich.WithThreadId().CreateLogger().ForContext<Program>();
        }
    }
}