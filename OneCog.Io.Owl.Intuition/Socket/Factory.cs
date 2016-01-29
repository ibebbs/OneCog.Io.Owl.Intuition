using System.Net;
using System.Net.Sockets;

namespace OneCog.Io.Owl.Intuition.Socket
{
    public interface IFactory
    {
        IUdpClient ConstructCommandSendSocket(IPEndPoint endpoint);

        IUdpClient ConstructCommandReceiveSocket(IPEndPoint endpoint);
    }

    public class Factory : IFactory
    {
        public static readonly IFactory Default = new Factory();

        public IUdpClient ConstructCommandReceiveSocket(IPEndPoint endpoint)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Client.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(endpoint);

            return new UdpClientDecorator(udpClient);
        }

        public IUdpClient ConstructCommandSendSocket(IPEndPoint endpoint)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Client.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(endpoint);

            return new UdpClientDecorator(udpClient);
        }
    }
}
