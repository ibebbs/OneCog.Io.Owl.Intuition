using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Socket
{
    public interface IUdpClient : IDisposable
    {
        Task<UdpReceiveResult> ReceiveAsync();

        Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint);
    }
}
