
//*************************************************************************************
//   !!! Generated by the fmp-cli 1.80.0.  DO NOT EDIT!
//*************************************************************************************

using System;
using System.IO;
using UnityEngine;

namespace XTC.FMP.MOD.VideoSee.LIB.Unity
{
    /// <summary>
    /// 资源读取器
    /// </summary>
    public class ResourceReader
    {
        protected ObjectsPool contentObjectsPool_ { get; private set; }

        public ResourceReader(ObjectsPool _contentObjectsPool)
        {
            contentObjectsPool_ = _contentObjectsPool;
        }

        /// <summary>
        /// 资产库的根目录的绝对路径
        /// </summary>
        public string AssetRootPath { get; set; }

        /// <summary>
        /// 资源的短路径，格式为 包名/_resources/资源名
        /// </summary>
        public string ResourceUri { get; set; }

        /// <summary>
        /// 加载纹理
        /// </summary>
        /// <param name="_file">文件相对路径，相对于包含format.json的资源文件夹</param>
        public void LoadTexture(string _file, Action<Texture2D> _onFinish, Action _onError)
        {
            string dir = Path.Combine(AssetRootPath, ResourceUri);
            string filefullpath = Path.Combine(dir, _file);
            contentObjectsPool_.LoadTexture(filefullpath, null, _onFinish, _onError);
        }

        /// <summary>
        /// 加载文本
        /// </summary>
        /// <param name="_file">文件相对路径，相对于包含format.json的资源文件夹</param>
        public void LoadText(string _file, Action<byte[]> _onFinish, Action _onError)
        {
            string dir = Path.Combine(AssetRootPath, ResourceUri);
            string filefullpath = Path.Combine(dir, _file);
            contentObjectsPool_.LoadText(filefullpath, null, _onFinish, _onError);
        }

        /// <summary>
        /// 加载音频
        /// </summary>
        /// <param name="_file">文件相对路径，相对于包含format.json的资源文件夹</param>
        public void LoadAudioClip(string _file, Action<AudioClip> _onFinish, Action _onError)
        {
            string dir = Path.Combine(AssetRootPath, ResourceUri);
            string filefullpath = Path.Combine(dir, _file);
            contentObjectsPool_.LoadAudioClip(filefullpath, null, _onFinish, _onError);
        }

    }
}

