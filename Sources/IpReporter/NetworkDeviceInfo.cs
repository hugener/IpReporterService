using System.Net;

namespace IpReporter
{
    /// <summary>
    /// Represents a network device.
    /// </summary>
    public class NetworkDeviceInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkDeviceInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ipAddress">The ip address.</param>
        public NetworkDeviceInfo(string name, IPAddress ipAddress)
        {
            this.Name = name;
            this.IpAddress = ipAddress;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public IPAddress IpAddress { get; }
    }
}