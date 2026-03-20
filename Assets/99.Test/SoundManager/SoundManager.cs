using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 10;
    private List<AudioSource> sfxSources = new List<AudioSource>();

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> bgmClips;
    [SerializeField] private List<AudioClip> sfxClips;

    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    private int sfxIndex = 0;


/****************************************************************************************/

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        // Dictionary 초기화
        foreach (var clip in bgmClips)
            bgmDict[clip.name] = clip;

        foreach (var clip in sfxClips)
            sfxDict[clip.name] = clip;

        // SFX AudioSource 풀 생성
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = new GameObject("SFX_" + i);
            obj.transform.parent = transform;

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sfxSources.Add(source);
        }
    }

    #endregion


/****************************************************************************************/
    #region BGM

    public void PlayBGM(string key)
    {
        if (!bgmDict.ContainsKey(key))
        {
            Debug.LogWarning($"BGM '{key}' not found");
            return;
        }

        bgmSource.clip = bgmDict[key];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    #endregion

    
/****************************************************************************************/

    #region SFX

    public void PlaySFX(string key)
    {
        if (!sfxDict.ContainsKey(key))
        {
            Debug.LogWarning($"SFX '{key}' not found");
            return;
        }

        AudioSource source = GetAvailableSFXSource();
        source.PlayOneShot(sfxDict[key]);
    }

    private AudioSource GetAvailableSFXSource()
    {
        // 순환 방식 (라운드 로빈)
        AudioSource source = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSources.Count;
        return source;
    }

    #endregion

    
/****************************************************************************************/

    #region Volume

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        foreach (var source in sfxSources)
            source.volume = volume;
    }

    #endregion
}