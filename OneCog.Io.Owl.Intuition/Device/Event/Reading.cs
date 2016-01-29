
namespace OneCog.Io.Owl.Intuition.Device.Event
{
    public class Reading
    {
        public Reading(Packet.IReading value)
        {
            Value = value;
        }

        public Packet.IReading Value { get; private set; }
    }
}
