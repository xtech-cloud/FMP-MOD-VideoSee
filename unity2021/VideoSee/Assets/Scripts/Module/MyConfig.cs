
using System.Security.Cryptography;
using System.Xml.Serialization;
using UnityEngineInternal;

namespace XTC.FMP.MOD.VideoSee.LIB.Unity
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class MyConfig : MyConfigBase
    {
        public class Border
        {
            [XmlAttribute("left")]
            public int left { get; set; } = 0;
            [XmlAttribute("right")]
            public int right { get; set; } = 0;
            [XmlAttribute("top")]
            public int top { get; set; } = 0;
            [XmlAttribute("bottom")]
            public int bottom { get; set; } = 0;
        }


        public class UiSprite
        {
            [XmlAttribute("image")]
            public string image { get; set; } = "";
            [XmlElement("Border")]
            public Border border { get; set; } = new Border();
            [XmlElement("Anchor")]
            public Anchor anchor { get; set; } = new Anchor();
        }

        public class Padding
        {
            [XmlAttribute("left")]
            public int left { get; set; } = 0;
            [XmlAttribute("right")]
            public int right { get; set; } = 0;
            [XmlAttribute("top")]
            public int top { get; set; } = 0;
            [XmlAttribute("bottom")]
            public int bottom { get; set; } = 0;
        }

        public class Background
        {
            [XmlAttribute("visible")]
            public bool visible { get; set; } = true;
            [XmlAttribute("color")]
            public string color { get; set; } = "#000000FF";
        }

        public class UiSlider
        {
            [XmlAttribute("width")]
            public int width { get; set; } = 0;
            [XmlAttribute("height")]
            public int height { get; set; } = 4;

            [XmlElement("Background")]
            public UiSprite background { get; set; } = new UiSprite();
            [XmlElement("Fill")]
            public UiSprite fill { get; set; } = new UiSprite();
            [XmlElement("Handle")]
            public UiSprite handle { get; set; } = new UiSprite();
        }


        public class UiButton
        {
            [XmlAttribute("icon")]
            public string icon { get; set; } = "";
        }

        public class ButtonLoop : UiButton
        {
            [XmlAttribute("visible")]
            public bool visible { get; set; } = true;
            [XmlAttribute("mode")]
            public string mode { get; set; } = "none";
        }
        
        public class ButtonClose : UiButton
        {
            [XmlAttribute("visible")]
            public bool visible { get; set; } = true;

            [XmlArray("OnClickSubjects"), XmlArrayItem("Subject")]
            public Subject[] OnClickSubjects { get; set; }  = new Subject[0];
        }

        public class UiText
        {
            [XmlAttribute("fontSize")]
            public int fontSize { get; set; } = 14;
            [XmlAttribute("width")]
            public int width { get; set; } = 160;
        }

        public class Toolbar
        {
            [XmlAttribute("visibleMode")]
            public string visibleMode { get; set; } = "auto";
            [XmlElement("Anchor")]
            public Anchor anchor { get; set; } = new Anchor();
            [XmlElement("ButtonLoop")]
            public ButtonLoop btnLoop { get; set; } = new ButtonLoop();
            [XmlElement("ButtonPlay")]
            public UiButton btnPlay { get; set; } = new UiButton();
            [XmlElement("ButtonPause")]
            public UiButton btnPause { get; set; } = new UiButton();
            [XmlElement("ButtonVolume")]
            public UiButton btnVolume { get; set; } = new UiButton();
            [XmlElement("TextTime")]
            public UiText txtTime { get; set; } = new UiText();
            [XmlElement("SliderTime")]
            public UiSlider sliderTime { get; set; } = new UiSlider();
            [XmlElement("SliderVolume")]
            public UiSlider sliderVolume { get; set; } = new UiSlider();
            [XmlElement("ButtonClose")]
            public ButtonClose btnClose{ get; set; } = new ButtonClose();
        }

        public class Style
        {
            [XmlAttribute("name")]
            public string name { get; set; } = "";
            [XmlAttribute("muted")]
            public bool muted { get; set; } = false; 
            [XmlElement("Pending")]
            public UiElement pending { get; set; } = new UiElement();
            [XmlElement("Background")]
            public Background background { get; set; } = new Background();
            [XmlElement("ToolBar")]
            public Toolbar toolbar { get; set; } = new Toolbar();
        }


        [XmlArray("Styles"), XmlArrayItem("Style")]
        public Style[] styles { get; set; } = new Style[0];
    }
}

