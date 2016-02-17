using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Network.Socket
{
    public interface IObservableUdpClient : IDisposable
    {
        IObservable<UdpReceiveResult> Receive();

        IObservable<int> Send(byte[] datagram, int bytes, IPEndPoint endPoint);
    }

    internal class ObservableUdpClient : IObservableUdpClient
    {
        private readonly UdpClient _client;

        public ObservableUdpClient(UdpClient client)
        {
            _client = client;
        }

        public void Dispose()
        {
            _client.Close();
        }

        public IObservable<UdpReceiveResult> Receive()
        {
            return Observable.FromAsync(_client.ReceiveAsync).Repeat();
        }

        public IObservable<int> Send(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            return _client.SendAsync(datagram, bytes, endPoint).ToObservable();
        }
    }
}
