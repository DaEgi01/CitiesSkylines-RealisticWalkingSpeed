using System.Xml.Serialization;

namespace RealisticWalkingSpeed.Configuration
{
    [XmlRoot("Configuration")]
    public class ConfigurationDto
    {
        public float AnimationSpeedFactor { get; set; } = 1.9f;
    }
}
