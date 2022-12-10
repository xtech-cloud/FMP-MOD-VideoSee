
using System.Xml.Serialization;
using UnityEngineInternal;

namespace XTC.FMP.MOD.VideoSee.LIB.Unity
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class MyConfig : MyConfigBase
    {
        public class Background
        {
            [XmlAttribute("visible")]
            public bool visible { get; set; } = true;
            [XmlAttribute("color")]
            public string color { get; set; } = "#000000FF";
        }

        public class Slider
        {
            [XmlAttribute("width")]
            public int width { get; set; } = 200;
        }

        public class ButtonLoop
        {
            [XmlAttribute("visible")]
            public bool visible { get; set; } = true;
            [XmlAttribute("mode")]
            public string mode { get; set; } = "none";
        }

        public class Toolbar
        {
            [XmlAttribute("visibleMode")]
            public string visibleMode { get; set; } = "auto";
            [XmlElement("Anchor")]
            public Anchor anchor { get; set; } = new Anchor();
            [XmlElement("SliderProgress")]
            public Slider sliderProgress { get; set; } = new Slider();
            [XmlElement("ButtonLoop")]
            public ButtonLoop btnLoop { get; set; } = new ButtonLoop();
        }

        public class Style
        {
            [XmlAttribute("name")]
            public string name { get; set; } = "";
            [XmlAttribute("primaryColor")]
            public string primaryColor { get; set; } = "";
            [XmlElement("Background")]
            public Background background { get; set; } = new Background();
            [XmlElement("ToolBar")]
            public Toolbar toolbar { get; set; } = new Toolbar();
        }


        [XmlArray("Styles"), XmlArrayItem("Style")]
        public Style[] styles { get; set; } = new Style[0];
    }
}

