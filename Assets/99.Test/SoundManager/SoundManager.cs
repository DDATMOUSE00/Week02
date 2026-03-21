using System.Collections.Generic;
using UnityEngine;

public enum MusicTrack
{
    None = 0,
    MainMenu,
    GameScene,
    GameOver,
    GameClear,
    Tutorial
}

public enum SoundEffect
{
    PlayerJump,
    PlayerSlam,
    EnemyDeath,
    Exlposion,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 5;
    private List<AudioSource> _sfxSources = new List<AudioSource>();

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> _bgmClips;
    [SerializeField] private List<AudioClip> _sfxClips;

    private Dictionary<MusicTrack, AudioClip> bgmDict = new Dictionary<MusicTrack, AudioClip>();
    private Dictionary<SoundEffect, AudioClip> sfxDict = new Dictionary<SoundEffect, AudioClip>();

    [SerializeField] private int sfxIndex = 0;


/****************************************************************************************/

    #region Unity Lifecycle

    private void Awake()
    {
        // 싱글톤 구조 적용
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
        // 각 enum 에 맞추어 bgm 사운드 클립 조정
        for (int i = 0; i < _bgmClips.Count; i++)
            bgmDict[(MusicTrack)i] = _bgmClips[i];

        // 각 enum 에 맞추어 sfx 사운드 클립 조정
        for (int i = 0; i < _sfxClips.Count; i++)
            sfxDict[(SoundEffect)i] = _sfxClips[i];

        // SFX AudioSource pool 적용
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = new GameObject("SFX_" + i);  
            obj.transform.parent = transform; // 사운드 매니저와 같은 위치에서 스폰
        
            AudioSource source = obj.AddComponent<AudioSource>(); // 생성된 오브젝트에 사운드소스 추가
            source.playOnAwake = false; // 생성시 자동으로 사운드 플레이 제거
            _sfxSources.Add(source); // 소스 리스트에 등록
        }
    }

    #endregion


/****************************************************************************************/
    #region BGM

    public void PlayBGM(MusicTrack track)
    {
        // 만약 사운드가 없을시 리턴
        if (!bgmDict.TryGetValue(track, out AudioClip clip)) return;

        _bgmSource.clip = clip; // 클립 정보 등록
        _bgmSource.loop = true; // 루프 적용
        _bgmSource.Play(); // 사운드 플레이
    }

    public void StopBGM()
    {
        if (!_bgmSource.isPlaying || _bgmSource == null) return;
        _bgmSource.Stop();
    }

    #endregion

    
/****************************************************************************************/

    #region SFX

    public void PlaySFX(SoundEffect sfx)
    {
        // 만약 사운드가 없을시 리턴
        if (!sfxDict.TryGetValue(sfx, out AudioClip clip)) return;

        // 가장 늦게 사용된 사운드 소스 등록
        AudioSource source = GetAvailableSFXSource();
        source.PlayOneShot(clip);
    }


    // 만들어진 사운드 소스들중 가장 나중에 사용된 pool 하여 사용
    private AudioSource GetAvailableSFXSource()
    {
        // 플레이된 사운드 매니저의 다음 순서 적용
        AudioSource source = _sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % _sfxSources.Count;
        return source;
    }

    #endregion

    
/****************************************************************************************/

    #region Volume

    // BGM 사운드 볼륨 적용
    public void SetBGMVolume(float volume)
    {
        _bgmSource.volume = volume;
    }

    // SFX 사운드 볼륨 적용
    public void SetSFXVolume(float volume)
    {
        foreach (var source in _sfxSources)
            source.volume = volume;
    }

    #endregion
}