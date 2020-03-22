using System.IO;
using System.Xml.Serialization;

namespace RealisticWalkingSpeed.Configuration
{
    public class ConfigurationService
    {
        private readonly string _configurationFileFullName;

        public ConfigurationService(string configurationFileFullName)
        {
            _configurationFileFullName = configurationFileFullName;
        }

        public ConfigurationDto Load()
        {
            var serializer = new XmlSerializer(typeof(ConfigurationDto));
            using (var streamReader = new StreamReader(_configurationFileFullName))
            {
                return (ConfigurationDto)serializer.Deserialize(streamReader);
            }
        }

        public void Save(ConfigurationDto configuration)
        {
            var serializer = new XmlSerializer(typeof(ConfigurationDto));
            using (var streamWriter = new StreamWriter(_configurationFileFullName))
            {
                serializer.Serialize(streamWriter, configuration);
            }
        }
    }
}
