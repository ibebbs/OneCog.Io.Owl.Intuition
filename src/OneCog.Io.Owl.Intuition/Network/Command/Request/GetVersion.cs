
namespace OneCog.Io.Owl.Intuition.Network.Command.Request
{
    public class GetVersion : IRequest
    {
        public string AsString()
        {
            return string.Format("{0},{1}", Verb.Get, Subject.Version);
        }
    }
}
