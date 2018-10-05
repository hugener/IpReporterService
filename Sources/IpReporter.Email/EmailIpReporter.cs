// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailIpReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace IpReporter.Email
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class EmailIpReporter : IIpReporter
    {
        public async Task InitializeAsync()
        {
        }

        public async Task ReportIpAsync(string hostName, IEnumerable<NetworkDeviceInfo> networkDeviceInfos)
        {
            var smtpSettings = JsonConvert.DeserializeObject<SmtpSettings>(await File.ReadAllTextAsync(@"settings.json"));
            var mailMessage = new MailMessage(smtpSettings.Sender, smtpSettings.Recipent);
            var client = new SmtpClient
            {
                Port = smtpSettings.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = smtpSettings.Host,
                Credentials = smtpSettings.Credentials,
                EnableSsl = smtpSettings.EnableSsl,
            };
            mailMessage.Subject = $"{hostName} changed IP";

            var stringBuilder = new StringBuilder($"{hostName} has the following IPs:");
            stringBuilder.AppendLine(string.Empty);
            foreach (var networkDeviceInfo in networkDeviceInfos)
            {
                stringBuilder.AppendLine($"{networkDeviceInfo.Name} => {networkDeviceInfo.IpAddress}");
            }

            mailMessage.Body = stringBuilder.ToString();
            await client.SendMailAsync(mailMessage);
        }

        public void Dispose()
        {
        }
    }
}