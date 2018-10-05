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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Sundew.Base.Collections;

    public class IpReporterService : IHostedService, IDisposable
    {
        private IpReporterProvider ipReportProvider;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.ipReportProvider = new IpReporterProvider();
            NetworkChange.NetworkAddressChanged += this.ReportIpAddress;
            this.ReportIpAddress(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            NetworkChange.NetworkAddressChanged -= this.ReportIpAddress;
            this.ipReportProvider.Dispose();
            return Task.CompletedTask;
        }

        void IDisposable.Dispose()
        {
        }

        private async void ReportIpAddress(object sender, EventArgs e)
        {
            var hostName = Dns.GetHostName();
            var networkDeviceInfoProvider = new NetworkDeviceInfoProvider();
            var networkDeviceInfos = networkDeviceInfoProvider.GetNetworkDeviceInfos().ToReadOnly();
            if (networkDeviceInfos != null && networkDeviceInfos.Any())
            {
                foreach (var ipReporter in this.ipReportProvider.GetIpReporters())
                {
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(ipReporter.GetType().Assembly.Location));
                    await ipReporter.ReportIpAsync(hostName, networkDeviceInfos);
                }
            }
        }
    }
}