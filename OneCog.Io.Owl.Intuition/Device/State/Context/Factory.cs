﻿using System;

namespace OneCog.Io.Owl.Intuition.Device.State.Context
{
    public interface IFactory
    {
        IDisconnected ForDisconnected();

        IConnection ForConnection();

        IConfiguration ForConfiguration(Command.Endpoint.IInstance commandEndpoint, Values.Version version);

        IListen ForListen(Command.Endpoint.IInstance commandEndpoint, Values.Version version);

        IFault ForFault(Command.Endpoint.IInstance commandEndpoint, Exception exception);
    }

    internal class Factory : IFactory
    {
        private readonly Settings.IProvider _settingsProvider;
        private readonly Command.Endpoint.IFactory _commandEndpointFactory;

        public Factory(Settings.IProvider settingsProvider, Command.Endpoint.IFactory commandEndpointFactory)
        {
            _settingsProvider = settingsProvider;
            _commandEndpointFactory = commandEndpointFactory;
        }

        public IDisconnected ForDisconnected()
        {
            return new Disconnected();
        }

        public IConnection ForConnection()
        {
            Settings.IValues settings = _settingsProvider.GetValues();

            return new Connection(settings);
        }

        public IConfiguration ForConfiguration(Command.Endpoint.IInstance commandEndpoint, Values.Version version)
        {
            Settings.IValues settings = _settingsProvider.GetValues();

            return new Configuration(commandEndpoint, version, settings.AutoConfigurePacketPort, settings.LocalPacketEndpoint);
        }

        public IListen ForListen(Command.Endpoint.IInstance commandEndpoint, Values.Version version)
        {
            Settings.IValues settings = _settingsProvider.GetValues();

            return new Listen(commandEndpoint, version, settings);
        }

        public IFault ForFault(Command.Endpoint.IInstance commandEndpoint, Exception exception)
        {
            return new Fault(commandEndpoint, exception);
        }
    }
}
