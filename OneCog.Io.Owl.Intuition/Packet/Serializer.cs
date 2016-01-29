using System.IO;
using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Packet
{
    public interface ISerializer
    {
        Electricity DeserializeElectricity(string xml);
    }

    internal class Serializer : ISerializer
    {
        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(Electricity));
        
        public string Serialize(Electricity value)
        {
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer.Serialize(writer, value);

                return writer.ToString();
            }
        }

        public Electricity DeserializeElectricity(string xml)
        {
            using (StringReader reader = new StringReader(xml))
            {
                return (Electricity)XmlSerializer.Deserialize(reader);
            }
        }
    }
}
