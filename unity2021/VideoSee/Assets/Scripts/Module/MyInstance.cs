

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using LibMVCS = XTC.FMP.LIB.MVCS;
using XTC.FMP.MOD.VideoSee.LIB.Proto;
using XTC.FMP.MOD.VideoSee.LIB.MVCS;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.IO;

namespace XTC.FMP.MOD.VideoSee.LIB.Unity
{
    /// <summary>
    /// 实例类
    /// </summary>
    public class MyInstance : MyInstanceBase
    {
        public class UiReference
        {
            public GameObject pending;
            /// <summary>
            /// 图片浏览器的工具栏
            /// </summary>
            public Transform toolbar;
            public Transform renderer;
            public Slider seeker;
            public Slider volume;
            public Text textTime;
            public Button btnPlay;
            public Button btnPause;
            public Button btnVolume;

            public Button btnLoopNone;
            public Button btnLoopSingle;

            public MediaPlayer _mediaPlayer;
            public DisplayUGUI _displayUGUI;
        }

        private ContentReader contentReader_;

        private UiReference uiReference_ = new UiReference();
        /// <summary>
        /// 拖动进度前是否在播放
        /// </summary>
        private bool wasPlayingOnScrub_;
        /// <summary>
        /// 是否在拖拽
        /// </summary>
        private bool isDraging_;
        private float videoSeekValue_;
        private float volumeAppearTimer_;
        private Coroutine coroutineUpdate_;
        private string loopMode_ = "none";


        public MyInstance(string _uid, string _style, MyConfig _config, MyCatalog _catalog, LibMVCS.Logger _logger, Dictionary<string, LibMVCS.Any> _settings, MyEntryBase _entry, MonoBehaviour _mono, GameObject _rootAttachments)
            : base(_uid, _style, _config, _catalog, _logger, _settings, _entry, _mono, _rootAttachments)
        {
        }

        /// <summary>
        /// 当被创建时
        /// </summary>
        /// <remarks>
        /// 可用于加载主题目录的数据
        /// </remarks>
        public void HandleCreated()
        {
            contentReader_ = new ContentReader(contentObjectsPool);
            contentReader_.AssetRootPath = settings_["path.assets"].AsString();
            contentReader_.ContentUri = "";

            uiReference_.pending = rootUI.transform.Find("pending").gameObject;
            uiReference_.renderer = rootUI.transform.Find("renderer");
            uiReference_._mediaPlayer = rootUI.AddComponent<MediaPlayer>();
            uiReference_._displayUGUI = uiReference_.renderer.gameObject.AddComponent<DisplayUGUI>();
            uiReference_.toolbar = rootUI.transform.Find("toolbar");
            uiReference_.textTime = uiReference_.toolbar.Find("textTime").GetComponent<Text>();
            uiReference_._mediaPlayer.m_VideoLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
            uiReference_._displayUGUI._mediaPlayer = uiReference_._mediaPlayer;
            uiReference_.seeker = uiReference_.toolbar.transform.Find("sdSeeker").GetComponent<Slider>();
            uiReference_.btnLoopNone = uiReference_.toolbar.transform.Find("btnLoopNone").GetComponent<Button>();
            uiReference_.btnLoopSingle = uiReference_.toolbar.transform.Find("btnLoopSingle").GetComponent<Button>();
            uiReference_.volume = uiReference_.toolbar.transform.Find("btnVolume/sdVolume").GetComponent<Slider>();
            uiReference_.btnPlay = uiReference_.toolbar.transform.Find("btnPlay").GetComponent<Button>();
            uiReference_.btnPause = uiReference_.toolbar.transform.Find("btnPause").GetComponent<Button>();
            uiReference_.btnVolume = uiReference_.toolbar.transform.Find("btnVolume").GetComponent<Button>();

            applyStyle();
            bindEvents();
        }

        /// <summary>
        /// 当被删除时
        /// </summary>
        public void HandleDeleted()
        {
        }

        /// <summary>
        /// 当被打开时
        /// </summary>
        /// <remarks>
        /// 可用于加载内容目录的数据
        /// </remarks>
        public void HandleOpened(string _source, string _uri)
        {
            rootUI.gameObject.SetActive(true);
            rootWorld.gameObject.SetActive(true);

            uiReference_.pending.SetActive(true);
            uiReference_.toolbar.gameObject.SetActive(style_.toolbar.visibleMode == "show");

            string url = "";
            if (_source == "assloud://")
            {
                url = Path.Combine(contentReader_.AssetRootPath, contentReader_.ContentUri);
                url = Path.Combine(url, _uri);
            }
            else if (_source == "file://")
            {
                url = _uri;
            }

            loopMode_ = style_.toolbar.btnLoop.mode;
            uiReference_.pending.SetActive(false);
            uiReference_._mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, url, false);
            uiReference_.btnPlay.gameObject.SetActive(true);
            uiReference_.btnPause.gameObject.SetActive(false);
            uiReference_.volume.gameObject.SetActive(false);
            uiReference_.volume.value = style_.muted ? 0f : 1.0f;
            uiReference_.btnLoopNone.gameObject.SetActive(style_.toolbar.btnLoop.visible && loopMode_ == "none");
            uiReference_.btnLoopSingle.gameObject.SetActive(style_.toolbar.btnLoop.visible && loopMode_ == "single");
            uiReference_._mediaPlayer.Control.SetLooping(loopMode_ == "single");
            uiReference_._mediaPlayer.Control.SetVolume(uiReference_.volume.value);

            play();
        }

        /// <summary>
        /// 当被关闭时
        /// </summary>
        public void HandleClosed()
        {
            stop();
            rootUI.gameObject.SetActive(false);
            rootWorld.gameObject.SetActive(false);
        }

        private void applyStyle()
        {
            Func<string, Color> convertColor = (_color) =>
            {
                Color color;
                if (!ColorUtility.TryParseHtmlString(_color, out color))
                    color = Color.white;
                return color;
            };
            rootUI.transform.Find("bg").gameObject.SetActive(style_.background.visible);
            rootUI.transform.Find("bg").GetComponent<RawImage>().color = convertColor(style_.background.color);

            // 等待
            alignByAncor(uiReference_.pending.transform, style_.pending.anchor);
            loadTextureFromTheme(style_.pending.image, (_texture) =>
            {
                uiReference_.pending.GetComponent<RawImage>().texture = _texture;
            }, () => { });

            uiReference_.btnLoopSingle.gameObject.SetActive(style_.toolbar.btnLoop.visible);
            uiReference_.btnLoopNone.gameObject.SetActive(style_.toolbar.btnLoop.visible);

            alignByAncor(uiReference_.toolbar, style_.toolbar.anchor);


            uiReference_.btnPlay.GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.anchor.height, style_.toolbar.anchor.height);
            uiReference_.btnPlay.GetComponent<LayoutElement>().minWidth = style_.toolbar.anchor.height;
            loadTextureFromTheme(style_.toolbar.btnPlay.icon, (_texture) =>
            {
                uiReference_.btnPlay.GetComponent<RawImage>().texture = _texture;
            }, () => { });
            uiReference_.btnPause.GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.anchor.height, style_.toolbar.anchor.height);
            uiReference_.btnPause.GetComponent<LayoutElement>().minWidth = style_.toolbar.anchor.height;
            loadTextureFromTheme(style_.toolbar.btnPause.icon, (_texture) =>
            {
                uiReference_.btnPause.GetComponent<RawImage>().texture = _texture;
            }, () => { });
            uiReference_.btnLoopSingle.GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.anchor.height, style_.toolbar.anchor.height);
            uiReference_.btnLoopSingle.GetComponent<LayoutElement>().minWidth = style_.toolbar.anchor.height;
            uiReference_.btnLoopNone.GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.anchor.height, style_.toolbar.anchor.height);
            uiReference_.btnLoopNone.GetComponent<LayoutElement>().minWidth = style_.toolbar.anchor.height;
            loadTextureFromTheme(style_.toolbar.btnLoop.icon, (_texture) =>
            {
                uiReference_.btnLoopSingle.GetComponent<RawImage>().texture = _texture;
            }, () => { });
            uiReference_.btnVolume.GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.anchor.height, style_.toolbar.anchor.height);
            uiReference_.btnVolume.GetComponent<LayoutElement>().minWidth = style_.toolbar.anchor.height;
            loadTextureFromTheme(style_.toolbar.btnVolume.icon, (_texture) =>
            {
                uiReference_.btnVolume.GetComponent<RawImage>().texture = _texture;
            }, () => { });

            uiReference_.textTime.GetComponent<Text>().fontSize = style_.toolbar.txtTime.fontSize;
            uiReference_.textTime.GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.anchor.height, style_.toolbar.anchor.height);
            uiReference_.textTime.GetComponent<LayoutElement>().minWidth = style_.toolbar.txtTime.width;

            // 时间进度条
            uiReference_.seeker.GetComponent<RectTransform>().sizeDelta = new Vector2(0, style_.toolbar.sliderTime.height);
            loadTextureFromTheme(style_.toolbar.sliderTime.background.image, (_texture) =>
            {
                var border = new Vector4(style_.toolbar.sliderTime.background.border.left, style_.toolbar.sliderTime.background.border.bottom, style_.toolbar.sliderTime.background.border.right, style_.toolbar.sliderTime.background.border.top);
                uiReference_.seeker.transform.Find("Background").GetComponent<Image>().sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, border);
            }, () => { });
            loadTextureFromTheme(style_.toolbar.sliderTime.fill.image, (_texture) =>
            {
                var border = new Vector4(style_.toolbar.sliderTime.background.border.left, style_.toolbar.sliderTime.background.border.bottom, style_.toolbar.sliderTime.background.border.right, style_.toolbar.sliderTime.background.border.top);
                uiReference_.seeker.transform.Find("Fill Area/Fill").GetComponent<Image>().sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, border);
            }, () => { });

            uiReference_.seeker.transform.Find("Handle Slide Area/Handle").GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.sliderTime.height, 0);
            loadTextureFromTheme(style_.toolbar.sliderTime.handle.image, (_texture) =>
            {
                uiReference_.seeker.transform.Find("Handle Slide Area/Handle").GetComponent<RawImage>().texture = _texture;
            }, () => { });

            // 音量进度条
            uiReference_.volume.GetComponent<RectTransform>().sizeDelta = new Vector2(style_.toolbar.sliderVolume.width, style_.toolbar.sliderVolume.height);
            loadTextureFromTheme(style_.toolbar.sliderVolume.background.image, (_texture) =>
            {
                uiReference_.volume.transform.Find("Background").GetComponent<RawImage>().texture = _texture;
            }, () => { });
            alignByAncor(uiReference_.volume.transform.Find("Fill Area"), style_.toolbar.sliderVolume.fill.anchor);
            alignByAncor(uiReference_.volume.transform.Find("Handle Slide Area"), style_.toolbar.sliderVolume.fill.anchor);
            loadTextureFromTheme(style_.toolbar.sliderVolume.fill.image, (_texture) =>
            {
                uiReference_.volume.transform.Find("Fill Area/Fill").GetComponent<RawImage>().texture = _texture;
            }, () => { });
            uiReference_.volume.transform.Find("Handle Slide Area/Handle").GetComponent<RectTransform>().sizeDelta = new Vector2(0, style_.toolbar.sliderVolume.handle.anchor.height);
            loadTextureFromTheme(style_.toolbar.sliderVolume.handle.image, (_texture) =>
            {
                uiReference_.volume.transform.Find("Handle Slide Area/Handle").GetComponent<RawImage>().texture = _texture;
            }, () => { });
        }

        private void bindEvents()
        {
            uiReference_.renderer.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (style_.toolbar.visibleMode == "hide")
                    return;
                uiReference_.toolbar.gameObject.SetActive(!uiReference_.toolbar.gameObject.activeSelf);
            });
            uiReference_.btnLoopNone.onClick.AddListener(() =>
            {
                loopMode_ = "single";
                switchLoopMode();
                uiReference_._mediaPlayer.Control.SetLooping(true);
            });
            uiReference_.btnLoopSingle.onClick.AddListener(() =>
            {
                loopMode_ = "none";
                switchLoopMode();
                uiReference_._mediaPlayer.Control.SetLooping(false);
            });
            uiReference_.volume.onValueChanged.AddListener((_value) =>
            {
                volumeAppearTimer_ = 0;
                uiReference_._mediaPlayer.Control.SetVolume(_value);
            });
            uiReference_.btnPlay.onClick.AddListener(() =>
            {
                play();
            });
            uiReference_.btnPause.onClick.AddListener(() =>
            {
                uiReference_.btnPause.gameObject.SetActive(false);
                uiReference_.btnPlay.gameObject.SetActive(true);
                uiReference_._mediaPlayer.Control.Pause();
            });
            Button btnVolume = uiReference_.toolbar.transform.Find("btnVolume").GetComponent<UnityEngine.UI.Button>();
            btnVolume.onClick.AddListener(() =>
            {
                if (uiReference_.volume.gameObject.activeSelf)
                {
                    volumeAppearTimer_ = 99f;
                }
                else
                {
                    mono_.StartCoroutine(popupVolume());
                }
            });


            UnityEngine.EventSystems.EventTrigger eventTrigger = uiReference_.seeker.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            // 创建开始拖拽事件
            UnityEngine.EventSystems.EventTrigger.Entry entryBeginDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryBeginDrag.eventID = UnityEngine.EventSystems.EventTriggerType.BeginDrag;
            entryBeginDrag.callback.AddListener((_e) =>
            {
                isDraging_ = true;
                onSeekerBeginDrag();
            });
            eventTrigger.triggers.Add(entryBeginDrag);

            // 创建结束拖拽事件
            UnityEngine.EventSystems.EventTrigger.Entry entryEndDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryEndDrag.eventID = UnityEngine.EventSystems.EventTriggerType.EndDrag;
            entryEndDrag.callback.AddListener((_e) =>
            {
                onSeekerEndDrag();
                isDraging_ = false;
            });
            eventTrigger.triggers.Add(entryEndDrag);

            // 创建拖拽事件
            UnityEngine.EventSystems.EventTrigger.Entry entryDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryDrag.eventID = UnityEngine.EventSystems.EventTriggerType.Drag;
            entryDrag.callback.AddListener((_e) =>
            {
                onSeekerDrag();
            });
            eventTrigger.triggers.Add(entryDrag);

            // 创建点击事件(Click事件是按键抬起后，所以此处使用PointerDown更合适)
            UnityEngine.EventSystems.EventTrigger.Entry clickDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
            clickDrag.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            clickDrag.callback.AddListener((_e) =>
            {
                // PointerDown 必然在 BeginDrag前触发，所以wasPlayingOnScrub_需在此赋值
                wasPlayingOnScrub_ = uiReference_._mediaPlayer.Control.IsPlaying();
                onSeekerDrag();
                mono_.StartCoroutine(delayPlayOnClick());
            });
            eventTrigger.triggers.Add(clickDrag);
        }

        private void play()
        {
            uiReference_.btnPlay.gameObject.SetActive(false);
            uiReference_.btnPause.gameObject.SetActive(true);
            uiReference_._mediaPlayer.Control.Play();
            if (null != coroutineUpdate_)
            {
                mono_.StopCoroutine(coroutineUpdate_);
            }
            coroutineUpdate_ = mono_.StartCoroutine(update());
        }

        private void stop()
        {
            if (null != coroutineUpdate_)
            {
                mono_.StopCoroutine(coroutineUpdate_);
            }
            if (null != uiReference_._mediaPlayer.Control)
            {
                uiReference_._mediaPlayer.Control.Stop();
                uiReference_._mediaPlayer.Control.Rewind();
            }
            uiReference_.btnPlay.gameObject.SetActive(true);
            uiReference_.btnPause.gameObject.SetActive(false);
        }

        private void onSeekerBeginDrag()
        {
        }

        private void onSeekerEndDrag()
        {
            if (wasPlayingOnScrub_)
                uiReference_._mediaPlayer.Control.Play();
        }

        private void onSeekerDrag()
        {
            if (uiReference_.seeker.value != videoSeekValue_)
            {
                uiReference_._mediaPlayer.Control.Seek(uiReference_.seeker.value * uiReference_._mediaPlayer.Info.GetDurationMs());
            }
        }

        private IEnumerator update()
        {
            while (true)
            {
                yield return new UnityEngine.WaitForEndOfFrame();
                if (uiReference_._mediaPlayer.Info != null && uiReference_._mediaPlayer.Info.GetDurationMs() > 0f)
                {
                    float time = uiReference_._mediaPlayer.Control.GetCurrentTimeMs();
                    float duration = uiReference_._mediaPlayer.Info.GetDurationMs();
                    float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);
                    videoSeekValue_ = d;
                    if (!isDraging_)
                        uiReference_.seeker.value = d;

                    int leftMS = (int)(uiReference_._mediaPlayer.Info.GetDurationMs() - uiReference_._mediaPlayer.Control.GetCurrentTimeMs());
                    int left = leftMS <= 0 ? 0 : leftMS / 1000 + 1;
                    uiReference_.textTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", left / (60 * 60), left / 60, left % 60);
                    if (leftMS <= 0)
                    {
                        uiReference_.textTime.text = "00:00:00";
                        if (!uiReference_._mediaPlayer.Control.IsLooping())
                        {
                            break;
                        }
                    }
                }
            }
            stop();
        }

        private IEnumerator popupVolume()
        {
            uiReference_.volume.gameObject.SetActive(true);
            volumeAppearTimer_ = 0;
            while (volumeAppearTimer_ < 2)
            {
                yield return new UnityEngine.WaitForEndOfFrame();
                volumeAppearTimer_ += UnityEngine.Time.deltaTime;
            }
            uiReference_.volume.gameObject.SetActive(false);
        }

        private void switchLoopMode()
        {
            uiReference_.btnLoopNone.gameObject.SetActive(loopMode_ == "none");
            uiReference_.btnLoopSingle.gameObject.SetActive(loopMode_ == "single");
        }

        private IEnumerator delayPlayOnClick()
        {
            uiReference_._mediaPlayer.Control.Pause();
            yield return new WaitForSeconds(0.3f);
            if (isDraging_)
                yield break;
            if (wasPlayingOnScrub_)
                uiReference_._mediaPlayer.Control.Play();
        }
    }
}
