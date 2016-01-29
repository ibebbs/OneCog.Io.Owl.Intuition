
namespace OneCog.Io.Owl.Intuition.Command.Request
{
    public class GetRosta : IRequest
    {
        public string AsString()
        {
            return string.Format("{0},{1},ALL", Verb.Get, Subject.Device);
        }
    }
}
