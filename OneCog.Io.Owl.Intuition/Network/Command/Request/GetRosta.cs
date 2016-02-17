
namespace OneCog.Io.Owl.Intuition.Network.Command.Request
{
    public class GetRosta : IRequest
    {
        public string AsString()
        {
            return string.Format("{0},{1},ALL", Verb.Get, Subject.Device);
        }
    }
}
