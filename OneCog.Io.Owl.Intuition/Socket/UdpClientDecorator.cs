using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Socket
{
    internal class UdpClientDecorator : IUdpClient
    {
        private readonly UdpClient _client;

        public UdpClientDecorator(UdpClient client)
        {
            _client = client;
        }

        public void Dispose()
        {
            _client.Close();
        }

        public Task<UdpReceiveResult> ReceiveAsync()
        {
            return _client.ReceiveAsync();
        }

        public Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            return _client.SendAsync(datagram, bytes, endPoint);
        }
    }
}
