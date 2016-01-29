using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using OneCog.Core;
using OneCog.Core.Reactive.Linq;

namespace OneCog.Io.Owl.Intuition.Device.Types
{
    internal class Cmr180 : IInstance
    {
        private readonly Settings.IValues _settings;
        private readonly Command.Endpoint.IFactory _commandEndpointFactory;
        private readonly Packet.Endpoint.IFactory _packetEndpointFactory;
        private readonly IScheduler _commandScheduler;

        private readonly RefCounted<Command.Endpoint.IInstance> _commandEndpoint;
        private readonly IObservable<Packet.IReading> _readings;

        public Cmr180(Settings.IValues settings, Command.Endpoint.IFactory commandEndpointFactory, Packet.Endpoint.IFactory packetEndpointFactory, IScheduler commandScheduler = null)
        {
            _settings = settings;
            _commandEndpointFactory = commandEndpointFactory;
            _packetEndpointFactory = packetEndpointFactory;
            _commandScheduler = commandScheduler ?? new EventLoopScheduler();

            _commandEndpoint = new RefCounted<Command.Endpoint.IInstance>(ConstructCommandEndpoint, endpoint => endpoint.Dispose());
            
            _readings = Observable
                .Using(CreatePacketEndpoint, endpoint => endpoint.Readings)
                .Publish()
                .RefCount();
        }

        private Packet.Endpoint.IInstance CreatePacketEndpoint()
        {
            Packet.Endpoint.IInstance endpoint = _packetEndpointFactory.CreateEndpoint(_settings);

            endpoint.Open();

            return endpoint;
        }

        private Command.Endpoint.IInstance ConstructCommandEndpoint()
        {
            Command.Endpoint.IInstance commandEndpoint = _commandEndpointFactory.CreateEndpoint(_settings);

            commandEndpoint.Open();

            return commandEndpoint;
        }

        public IObservable<Packet.IReading> Readings
        {
            get { return _readings; }
        }

        public IObservable<Command.IResponse> Send(IObservable<Command.IRequest> requests)
        {
            return Observable.Create<Command.IResponse>(
                observer =>
                {
                    IConnectableObservable<Command.IRequest> requestObservable = requests.ObserveOn(_commandScheduler).Publish();

                    IObservable<Command.IResponse> commandObservable = Observables.UsingRefCounted(_commandEndpoint, 
                        commandEndpoint => Observable
                            .Merge<Command.IResponse>(
                                requestObservable.OfType<Command.Request.GetDevice>().SelectMany(command => commandEndpoint.Send(command)),
                                requestObservable.OfType<Command.Request.GetRosta>().SelectMany(command => commandEndpoint.Send(command)),
                                requestObservable.OfType<Command.Request.GetUpdPushPort>().SelectMany(command => commandEndpoint.Send(command)),
                                requestObservable.OfType<Command.Request.GetVersion>().SelectMany(command => commandEndpoint.Send(command)),
                                requestObservable.OfType<Command.Request.Save>().SelectMany(command => commandEndpoint.Send(command)),
                                requestObservable.OfType<Command.Request.SetUdpPushPort>().SelectMany(command => commandEndpoint.Send(command))
                            )
                    );

                    return new CompositeDisposable(
                          commandObservable.Subscribe(observer),
                          requestObservable.Connect()
                    );
                }
            );
        }

        public Type Type
        {
            get { return Type.IntuitionC; }
        }
    }
}
