// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IpReporterService.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace IpReporterService
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using IpReporter;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Sundew.Base.Collections;

    public class IpReporterService : IHostedService, IDisposable
    {
        private const string IpSeparator = ", ";
        private readonly ILogger logger;
        private IpReporterProvider ipReportProvider;

        public IpReporterService(ILogger logger)
        {
            this.logger = logger.ForContext<IpReporterService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.Information("Starting IpReporterService");
            this.ipReportProvider = new IpReporterProvider();
            NetworkChange.NetworkAddressChanged += this.ReportIpAddress;
            this.ReportIpAddress(this, EventArgs.Empty);
            this.logger.Information("Started IpReporterService");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.Information("Stopping IpReporterService");
            NetworkChange.NetworkAddressChanged -= this.ReportIpAddress;
            this.ipReportProvider.Dispose();
            this.logger.Information("Stopped IpReporterService");
            return Task.CompletedTask;
        }

        void IDisposable.Dispose()
        {
        }

        private async void ReportIpAddress(object sender, EventArgs e)
        {
            this.logger.Information("Reporting network addresses");
            var hostName = Dns.GetHostName();
            var networkDeviceInfoProvider = new NetworkDeviceInfoProvider();
            var networkDeviceInfos = networkDeviceInfoProvider.GetNetworkDeviceInfos().ToReadOnly();
            if (networkDeviceInfos != null && networkDeviceInfos.Any())
            {
                this.logger.Information("New IPs: {IPs}", this.GetIps(networkDeviceInfos));
                foreach (var ipReporter in this.ipReportProvider.GetIpReporters())
                {
                    try
                    {
                        this.logger.Information("Reporting IPs to {IpReporterType}", ipReporter.GetType());
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(ipReporter.GetType().Assembly.Location));
                        await ipReporter.ReportIpAsync(hostName, networkDeviceInfos);
                        this.logger.Information("Reported IPs to {IpReporterType}", ipReporter.GetType());
                    }
                    catch (Exception exception)
                    {
                        this.logger.Error(exception, "An error occured while reporting IP: {IpReporterType}", ipReporter.GetType());
                    }
                }
            }
        }

        private string GetIps(IReadOnlyItems<NetworkDeviceInfo> networkDeviceInfos)
        {
            if (networkDeviceInfos.Count == 0)
            {
                return "No IPs";
            }

            var stringBuilder = new StringBuilder();
            foreach (var networkDeviceInfo in networkDeviceInfos)
            {
                stringBuilder.Append($"{networkDeviceInfo.Name}: {networkDeviceInfo.IpAddress}");
                stringBuilder.Append(IpSeparator);
            }

            return stringBuilder.ToString(0, stringBuilder.Length - IpSeparator.Length);
        }
    }
}