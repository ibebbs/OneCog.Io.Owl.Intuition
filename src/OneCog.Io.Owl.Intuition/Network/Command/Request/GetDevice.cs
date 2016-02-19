
namespace OneCog.Io.Owl.Intuition.Network.Command.Request
{
    public class GetDevice : IRequest
    {
        public GetDevice(int index)
        {
            Index = index;
        }

        public string AsString()
        {
            return string.Format("{0},{1},{2}", Verb.Get, Subject.Device, Index);
        }

        public int Index { get; private set; }
    }
}
