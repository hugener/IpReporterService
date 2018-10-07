// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIpReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace IpReporter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Sundew.Base.Initialization;

    public interface IIpReporter : IInitializable, IDisposable
    {
        Task ReportIpAsync(string hostName, IEnumerable<NetworkDeviceInfo> networkDeviceInfos);
    }
}