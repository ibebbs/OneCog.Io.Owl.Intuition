using System.Net;
using System.Net.Sockets;

namespace OneCog.Io.Owl.Intuition.Socket
{
    public interface IFactory
    {
        IObservableUdpClient ConstructCommandSendSocket(IPEndPoint endpoint);

        IObservableUdpClient ConstructCommandReceiveSocket(IPEndPoint endpoint);
    }

    public class Factory : IFactory
    {
        public static readonly IFactory Default = new Factory();

        public IObservableUdpClient ConstructCommandReceiveSocket(IPEndPoint endpoint)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Client.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(endpoint);

            return new ObservableUdpClient(udpClient);
        }

        public IObservableUdpClient ConstructCommandSendSocket(IPEndPoint endpoint)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Client.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(endpoint);

            return new ObservableUdpClient(udpClient);
        }
    }
}
