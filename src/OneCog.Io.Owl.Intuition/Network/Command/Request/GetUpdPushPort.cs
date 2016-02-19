
namespace OneCog.Io.Owl.Intuition.Network.Command.Request
{
    public class GetUpdPushPort : IRequest
    {
        public string AsString()
        {
            return string.Format("{0},{1}", Verb.Get, Subject.Udp);
        }
    }
}
