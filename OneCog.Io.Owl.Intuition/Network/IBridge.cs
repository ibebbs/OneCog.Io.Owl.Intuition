using System;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Network
{

    public interface IBridge : IDisposable
    {
        Task<Command.Response.Version> Send(Command.Request.GetVersion message);
        Task<Command.Response.Rosta> Send(Command.Request.GetRosta message);
        Task<Command.Response.Device> Send(Command.Request.GetDevice message);
        Task<Command.Response.Udp> Send(Command.Request.GetUpdPushPort message);
        Task<Command.Response.Udp> Send(Command.Request.SetUdpPushPort message);
        Task<Command.Response.Save> Send(Command.Request.Save message);

        IObservable<Packet.IReading> Readings { get; }
    }
}
