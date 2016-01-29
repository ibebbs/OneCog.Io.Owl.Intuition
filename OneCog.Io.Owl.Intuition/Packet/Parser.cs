using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Packet
{
    public interface IParser
    {
        IEnumerable<IReading> GetReadings(string packets);
    }

    internal class Parser : IParser
    {
        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(Wrapper));

        private string Serialize(Wrapper value)
        {
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer.Serialize(writer, value);

                return writer.ToString();
            }
        }

        private Wrapper Deserialize(string xml)
        {
            using (StringReader reader = new StringReader(xml))
            {
                return (Wrapper)XmlSerializer.Deserialize(reader);
            }
        }

        private Wrapper GetWrapper(string packets)
        {
            try
            {
                string wrapped = Wrapper.Wrap(packets);

                Wrapper wrapper = Deserialize(wrapped);

                return wrapper;
            }
            catch (Exception e)
            {
                Instrumentation.Packet.Parser.Error(e);

                return new Wrapper();
            }
        }

        public IEnumerable<IReading> GetReadings(string packets)
        {
            Wrapper wrapper = GetWrapper(packets);

            if (wrapper.Electricity != null)
            {
                yield return wrapper.Electricity;
            }
        }
    }
}
