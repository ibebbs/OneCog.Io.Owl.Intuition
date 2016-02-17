
namespace OneCog.Io.Owl.Intuition.Network.Command.Request
{
    public class Save : IRequest
    {
        public string AsString()
        {
            return Verb.Save;
        }
    }
}
