using System.Xml.Serialization;
using EasyConfigurationSection;
using EasyConfigurationSection = EasyConfigurationSection.EasyConfigurationSection;

namespace ImageStitcher
{
    public class Configuration
    {
        public static Configuration Get => new EasyConfiguration().GetSection<Configuration>("Config");

        [XmlElement]
        public ImageConfiguration Image { get; set; }
    }

    public class ImageConfiguration
    {
        [XmlAttribute]
        public int Width { get; set; } = 1000;

        [XmlAttribute]
        public int Height { get; set; } = 750;

        [XmlAttribute]
        public int Padding { get; set; } = 10;

        [XmlAttribute]
        public int Border { get; set; } = 10;
    }
}