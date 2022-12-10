

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

            public Button btnLoopNone;
            public Button btnLoopSingle;

            public MediaPlayer _mediaPlayer;
            public DisplayUGUI _displayUGUI;
        }

        private ContentReader contentReader_;

        private UiReference uiReference_ = new UiReference();
        private bool wasPlayingOnScrub_;
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
            uiReference_.volume = uiReference_.toolbar.transform.Find("sdVolume").GetComponent<Slider>();
            uiReference_.btnPlay = uiReference_.toolbar.transform.Find("btnPlay").GetComponent<Button>();
            uiReference_.btnPause = uiReference_.toolbar.transform.Find("btnPause").GetComponent<Button>();

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
            uiReference_.volume.value = 1.0f;
            uiReference_.btnLoopNone.gameObject.SetActive(style_.toolbar.btnLoop.visible && loopMode_ == "none");
            uiReference_.btnLoopSingle.gameObject.SetActive(style_.toolbar.btnLoop.visible && loopMode_ == "single");
            uiReference_._mediaPlayer.Control.SetLooping(loopMode_ == "single");

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
            Color primaryColor = convertColor(style_.primaryColor);
            uiReference_.pending.GetComponent<RawImage>().color = primaryColor;
            rootUI.transform.Find("bg").gameObject.SetActive(style_.background.visible);
            rootUI.transform.Find("bg").GetComponent<RawImage>().color = convertColor(style_.background.color);

            uiReference_.btnLoopSingle.gameObject.SetActive(style_.toolbar.btnLoop.visible);
            uiReference_.btnLoopNone.gameObject.SetActive(style_.toolbar.btnLoop.visible);
            uiReference_.seeker.transform.Find("Fill Area/Fill").GetComponent<Image>().color = primaryColor;
            uiReference_.seeker.transform.Find("Handle Slide Area/Handle").GetComponent<Image>().color = primaryColor;
            uiReference_.volume.transform.Find("Fill Area/Fill").GetComponent<Image>().color = primaryColor;

            var rtSeeker = uiReference_.seeker.GetComponent<RectTransform>();
            rtSeeker.sizeDelta = new Vector2(style_.toolbar.sliderProgress.width, rtSeeker.sizeDelta.y);

            alignByAncor(uiReference_.toolbar, style_.toolbar.anchor);
        }

        private void bindEvents()
        {
            uiReference_.renderer.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (style_.toolbar.visibleMode == "hide")
                    return;
                uiReference_.toolbar.gameObject.SetActive(true);
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
                onSeekerBeginDrag();
            });
            eventTrigger.triggers.Add(entryBeginDrag);

            // 创建结束拖拽事件
            UnityEngine.EventSystems.EventTrigger.Entry entryEndDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryEndDrag.eventID = UnityEngine.EventSystems.EventTriggerType.EndDrag;
            entryEndDrag.callback.AddListener((_e) =>
            {
                onSeekerEndDrag();
            });
            eventTrigger.triggers.Add(entryEndDrag);
            uiReference_.seeker.onValueChanged.AddListener((_value) =>
            {
                onSeekerDrag();
            });
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
            wasPlayingOnScrub_ = uiReference_._mediaPlayer.Control.IsPlaying();
            if (wasPlayingOnScrub_)
            {
                uiReference_._mediaPlayer.Control.Pause();
            }
            onSeekerDrag();
        }

        private void onSeekerEndDrag()
        {
            if (wasPlayingOnScrub_)
            {
                uiReference_._mediaPlayer.Control.Play();
                wasPlayingOnScrub_ = false;
            }
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
    }
}
