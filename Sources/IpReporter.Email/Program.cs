// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace IpReporter.Email
{
    using System.Net;
    using Newtonsoft.Json;

    public class Program
    {
        static void Main()
        {
            var smtpSettings = new SmtpSettings(
                "your@email.com",
                "target@email.com",
                25,
                "smtp.server.com",
                new NetworkCredential("email@username.com", "password"),
                true);
            var r = JsonConvert.SerializeObject(smtpSettings);
        }
    }
}