using System;
using System.Collections.Generic;

namespace OneCog.Io.Owl.Intuition.Device
{
    public interface IFactory
    {
        IInstance Create(Type type, Settings.IProvider settingsProvider);
    }

    public class Factory : IFactory
    {
        private static readonly IReadOnlyDictionary<Type, Func<Settings.IValues, IInstance>> DefaultFactories = new Dictionary<Type, Func<Settings.IValues, IInstance>>
        {
            { Type.IntuitionC, settings => new Types.Cmr180(settings, Command.Endpoint.Factory.Default, Packet.Endpoint.Factory.Default) }
        };

        public static readonly IFactory Default = new Factory(DefaultFactories);

        private readonly IReadOnlyDictionary<Type, Func<Settings.IValues, IInstance>> _factories;

        private Factory(IReadOnlyDictionary<Type, Func<Settings.IValues, IInstance>> factories)
        {
            if (factories == null) throw new ArgumentNullException("factories");

            _factories = factories;
        }

        public IInstance Create(Type type, Settings.IProvider settingsProvider)
        {
            Func<Settings.IValues, IInstance> factory;

            if (_factories.TryGetValue(type, out factory))
            {
                Settings.IValues settings = settingsProvider.GetValues();

                return factory(settings);
            }
            else
            {
                throw new ArgumentException(string.Format("No factory exists for the device type '{0}'", type));
            }
        }
    }
}
