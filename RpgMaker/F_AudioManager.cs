using UnityEngine;
using System.Collections.Generic;

// 音频管理器类（将JavaScript代码转换为C#）
public partial class AudioManager : MonoBehaviour
{
    // 静态变量初始化
    public static int _bgmVolume = 100;
    public static int _bgsVolume = 100;
    public static int _meVolume = 100;
    public static int _seVolume = 100;
    public static GameObject? _currentBgm;
    public static GameObject? _currentBgs;
    public static AudioSource? _bgmBuffer;
    public static AudioSource? _bgsBuffer;
    public static AudioSource? _meBuffer;
    public static List<AudioSource> _seBuffers = new List<AudioSource>();
    public static List<AudioSource> _staticBuffers = new List<AudioSource>();
    public static float _replayFadeTime = 0.5f;
    public static string _path = "audio/";

    // 属性定义
    public int BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = value;
            UpdateBgmParameters(_currentBgm);
        }
    }

    public int BgsVolume
    {
        get => _bgsVolume;
        set
        {
            _bgsVolume = value;
            UpdateBgsParameters(_currentBgs);
        }
    }

    public int MeVolume
    {
        get => _meVolume;
        set
        {
            _meVolume = value;
            UpdateMeParameters(_currentMe);
        }
    }

    public int SeVolume
    {
        get => _seVolume;
        set => _seVolume = value;
    }

    // 方法实现
    public void PlayBgm(AudioClip bgm, float? pos)
    {
        if (_currentBgm != null && _currentBgm.name == bgm.name)
        {
            UpdateBgmParameters(bgm);
        }
        else
        {
            StopBgm();
            if (bgm.name != null)
            {
                _bgmBuffer = LoadBuffer("bgm/", bgm.name);
                UpdateBgmParameters(bgm);
                if (!_meBuffer)
                {
                    _bgmBuffer.Play(true, pos ?? 0);
                }
            }
        }
        UpdateCurrentBgm(bgm, pos);
    }

    public void ReplayBgm(AudioClip bgm)
    {
        if (_currentBgm != null && _currentBgm.name == bgm.name)
        {
            UpdateBgmParameters(bgm);
        }
        else
        {
            PlayBgm(bgm, bgm.time);
            if (_bgmBuffer != null)
            {
                _bgmBuffer.FadeIn(_replayFadeTime);
            }
        }
    }

    private bool IsCurrentBgm(AudioClip bgm)
    {
        return _currentBgm != null && _bgmBuffer != null && _currentBgm.name == bgm.name;
    }

    private void UpdateBgmParameters(AudioClip bgm)
    {
        UpdateBufferParameters(_bgmBuffer, _bgmVolume, bgm);
    }

    private void UpdateCurrentBgm(AudioClip bgm, float? pos)
    {
        _currentBgm = new GameObject();
        _currentBgm.AddComponent<AudioSource>();
        _currentBgm.GetComponent<AudioSource>().clip = bgm;
        _currentBgm.GetComponent<AudioSource>().volume = bgm.volume;
        _currentBgm.GetComponent<AudioSource>().pitch = bgm.pitch * 100f;
        _currentBgm.GetComponent<AudioSource>().pan = bgm.pan * 100f;
        _currentBgm.GetComponent<AudioSource>().time = pos ?? 0f;
    }

    private void StopBgm()
    {
        if (_bgmBuffer != null)
        {
            _bgmBuffer.Stop();
            Destroy(_currentBgm);
            _currentBgm = null;
            _bgmBuffer = null;
        }
    }

    private void FadeOutBgm(float duration)
    {
        if (_bgmBuffer != null && _currentBgm != null)
        {
            _bgmBuffer.FadeOut(duration);
            _currentBgm = null;
        }
    }

    private AudioSource LoadBuffer(string folder, string name)
    {
        string ext = GetAudioFileExt();
        string url = _path + folder + UnityEngine.Util.urlEncode(name) + ext;
        return Resources.Load<AudioSource>(url);
    }

    private void UpdateBufferParameters(AudioSource buffer, int configVolume, AudioClip audio)
    {
        if (buffer != null && audio != null)
        {
            buffer.volume = configVolume * (audio.volume / 10000f);
            buffer.pitch = audio.pitch * 100f;
            buffer.pan = audio.pan * 100f;
        }
    }

    private string GetAudioFileExt()
    {
        return ".ogg";
    }

    private void CheckErrors()
    {
        List<AudioSource> allBuffers = new List<AudioSource>(_bgmBuffer, _bgsBuffer, _meBuffer);
        allBuffers.AddRange(_seBuffers);
        allBuffers.AddRange(_staticBuffers);
        foreach (AudioSource buffer in allBuffers)
        {
            if (buffer != null && buffer.isPlaying())
            {
                Debug.LogError("LoadError: " + buffer.clip.url + " (retry: " + buffer.retry + ")");
            }
        }
    }

    private void ThrowLoadError(AudioSource webAudio)
    {
        Debug.LogError(new object[] { "LoadError", webAudio.clip.url, () => webAudio.retry() });
    }

    // 其他方法（如playBgs, playMe, stopMe等）可以按照类似的方式转换，这里省略
}

// 注意：Unity中的AudioSource组件没有直接的fadeOut方法，可以使用FadeOut或FadeTo方法代替。同时，Unity中没有直接的isError属性，需要自己处理错误检查逻辑。

//这个C#代码实现了与原始JavaScript代码相似的功能，包括属性设置、音频播放、停止和音量控制。但是，需要注意的是，C#版本中使用了Unity的一些特定API和类，例如`AudioClip`、`Resources`和`Debug.LogError`。此外，一些JavaScript特性（如`Object.defineProperty`）在C#中需要不同的方式实现。

using System;
using System.Collections.Generic;
using System.Linq;
