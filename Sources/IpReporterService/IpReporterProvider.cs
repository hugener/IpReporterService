// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IpReporterProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace IpReporterService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using IpReporter;
    using Sundew.Base.Collections;
    using Sundew.Base.Threading;

    public class IpReporterProvider : IDisposable
    {
        private static readonly string[] FileExtensions = { ".dll", ".exe" };
        private readonly AsyncLock asyncLock = new AsyncLock();
        private readonly Dictionary<string, (DateTime lastWriteTime, IReadOnlyList<IIpReporter> reporters)> ipReporters =
                new Dictionary<string, (DateTime, IReadOnlyList<IIpReporter>)>();

        private readonly FileSystemWatcher fileSystemWatcher;

        public IpReporterProvider()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.OnCurrentDomainAssemblyResolve;
            this.fileSystemWatcher = new FileSystemWatcher(Path.Combine(Directory.GetCurrentDirectory(), "IpReporters"));
            this.fileSystemWatcher.Changed += this.OnFileSystemWatcherChanged;
            this.LoadIpReporters(this.fileSystemWatcher.Path);
        }

        public IReadOnlyList<IIpReporter> GetIpReporters()
        {
            using (var lockResult = this.asyncLock.WaitAsync().Result)
            {
                if (lockResult.Check())
                {
                    return this.ipReporters.SelectMany(x => x.Value.reporters).ToList();
                }

                return new List<IIpReporter>();
            }
        }

        private void OnFileSystemWatcherChanged(object sender, FileSystemEventArgs e)
        {
            this.LoadIpReporters(e.FullPath);
        }

        private async void LoadIpReporters(string path)
        {
            using (var lockResult = await this.asyncLock.WaitAsync())
            {
                if (lockResult.Check())
                {
                    var filePaths = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(x => FileExtensions.Contains(Path.GetExtension(x))).ToReadOnly();
                    foreach (var removedAssemblies in this.ipReporters.Keys.ToList().Except(filePaths))
                    {
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(removedAssemblies));
                        var (_, reporters) = this.ipReporters[removedAssemblies];
                        RemoveIpReporters(reporters);
                        this.ipReporters.Remove(removedAssemblies);
                    }

                    foreach (var filePath in filePaths)
                    {
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(filePath));
                        var fileInfo = new FileInfo(filePath);
                        if (this.ipReporters.TryGetValue(filePath, out var value))
                        {
                            if (value.lastWriteTime < fileInfo.LastWriteTime)
                            {
                                await this.UpdateIpReporters(fileInfo, value.reporters);
                            }
                        }
                        else
                        {
                            await this.AddIpReporters(fileInfo);
                        }
                    }
                }
            }
        }

        private static void RemoveIpReporters(IReadOnlyList<IIpReporter> ipReporters)
        {
            ipReporters.ForEach(x => x.Dispose());
        }

        private async Task AddIpReporters(FileInfo fileInfo)
        {
            var assembly = Assembly.LoadFile(fileInfo.FullName);
            var ipReporters = CreateIpReporters(assembly).ToList();
            await ipReporters.ForEachAsync(x => x.InitializeAsync());
            this.ipReporters[fileInfo.FullName] = (fileInfo.LastWriteTime, ipReporters);
        }

        private Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), args.Name.Split(',')[0] + ".dll"));
        }

        private async Task UpdateIpReporters(FileInfo fileInfo, IReadOnlyList<IIpReporter> ipReporters)
        {
            RemoveIpReporters(ipReporters);
            await this.AddIpReporters(fileInfo);
        }

        private static IEnumerable<IIpReporter> CreateIpReporters(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract && typeof(IIpReporter).IsAssignableFrom(type) && !type.IsInterface)
                {
                    yield return (IIpReporter)Activator.CreateInstance(type);
                }
            }
        }

        public void Dispose()
        {
            this.fileSystemWatcher?.Dispose();
        }
    }
}