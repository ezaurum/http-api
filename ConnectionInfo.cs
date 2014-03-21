using System.Net;

namespace Ezaurum.HttpApi
{
    public class ConnectionInfo
    {
        public IPEndPoint EndPoint { get; set; }
        public double RetryInterval { get; set; }
        public int MaximumRetry { get; set; }
    }
}