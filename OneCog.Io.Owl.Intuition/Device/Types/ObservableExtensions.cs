using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OneCog.Io.Owl.Intuition.Device.Types
{
    public static class ObservableExtensions
    {
        public static IObservable<Command.IResponse> ExecuteCommandOfType<TRequest, TResponse>(this IObservable<Tuple<Command.IRequest, IObserver<Command.IResponse>>> source, Func<TRequest, Task<TResponse>> execute)
            where TRequest : Command.IRequest
            where TResponse : Command.IResponse

        {
            Func<TRequest, IObserver<Command.IResponse>, Task<Command.IResponse>> selector =
                async (command, observer) =>
                {
                    TResponse response = default(TResponse);

                    try
                    {
                        response = await execute(command);

                        observer.OnNext(response);
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                    }

                    return response;
                };

            return source.Where(tuple => tuple.Item1 is TRequest).SelectMany(tuple => selector((TRequest)tuple.Item1, tuple.Item2));
        }
    }
}
