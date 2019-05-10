using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ImageStitcher
{
    public class Configuration
    {
        [XmlElement]
        public ImageConfiguration Image { get; set; } = new ImageConfiguration();

        [XmlElement]
        public OutputConfiguration Output { get; set; } = new OutputConfiguration();
    }

    public class ImageConfiguration
    {
        [XmlAttribute]
        public int Width { get; set; } = 1000;

        [XmlAttribute]
        public int Height { get; set; } = 750;

        [XmlAttribute]
        public int Padding { get; set; } = 15;

        [XmlAttribute]
        public int Border { get; set; } = 10;

        [XmlElement]
        public string Font { get; set; } = "Arial, 50pt";
    }

    public class OutputConfiguration
    {
        [XmlElement]
        public string OutputDirectory { get; set; } = Path.Combine(Application.StartupPath, "Output");

        [XmlAttribute]
        public int Compression { get; set; } = 80;

        [XmlElement]
        public TextboxDefaults TextboxDefaults { get; set; } = new TextboxDefaults();
    }

    public class TextboxDefaults
    {
        [XmlElement]
        public string Line1 { get; set; } = "Dental Today";

        [XmlElement]
        public string Line2 { get; set; }

        [XmlElement]
        public string Line3 { get; set; }

        [XmlElement]
        public string Line4 { get; set; } = "Dr Johay Amith";
    }
}