using System;

namespace OneCog.Io.Owl.Intuition.Device
{
    public interface IInstance
    {
        IObservable<Command.IResponse> Send(IObservable<Command.IRequest> requests);

        IObservable<Packet.IReading> Readings { get; }

        Type Type { get; }
    }
}
