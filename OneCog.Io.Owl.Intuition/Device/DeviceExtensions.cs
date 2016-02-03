using System;

namespace OneCog.Io.Owl.Intuition.Device
{
    public static class DeviceExtensions
    {
        public static IObservable<Command.IResponse> SendTo(this IObservable<Command.IRequest> source, IInstance instance)
        {
            return instance.Send(source);
        }
    }
}
