using System.Collections.Generic;
using System.Xml.Serialization;

namespace OneCog.Io.Owl.Intuition.Configuration
{
    public interface ISettings
    {
        IEnumerable<IDeviceSettings> Devices { get; }
    }

    public class Settings : ISettings
    {
        IEnumerable<IDeviceSettings> ISettings.Devices
        {
            get { return Devices; }
        }

        public IEnumerable<DeviceSettings> Devices { get; set; }
    }
}
