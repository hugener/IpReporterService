// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmtpSettings.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace IpReporter.Email
{
    using System.Net;

    public class SmtpSettings
    {
        public SmtpSettings(string sender, string recipent, int port, string host, NetworkCredential credentials, bool enableSsl)
        {
            this.Sender = sender;
            this.Recipent = recipent;
            this.Port = port;
            this.Host = host;
            this.Credentials = credentials;
            this.EnableSsl = enableSsl;
        }

        public string Sender { get; }

        public string Recipent { get; }

        public int Port { get; }

        public string Host { get; }

        public NetworkCredential Credentials { get; }

        public bool EnableSsl { get; }
    }
}