using System.Net;
using System.Net.Sockets;

namespace Contrequarte.SmartPlugFinder
{
    class UdpState
    {
        public UdpState(IPEndPoint endPoint, UdpClient udpClient)
        {
            EndPoint = endPoint;
            this.UdpClient = udpClient;
        }
        public IPEndPoint EndPoint {get; set;}
        public UdpClient UdpClient {get; set;}
    }
}
