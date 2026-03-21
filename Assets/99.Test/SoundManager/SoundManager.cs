using System.Collections.Generic;
using UnityEngine;

public enum BGM
{
    None = 0,
    GameScene,
    GameOver,
    GameClear,
    Tutorial
}

public enum PlayerSFX
{
    Jump,
    SlamHeavy,
    SlamLight,
    Charge,
    Levelup,
    Ping
}


public enum EnemySFX
{
    None,
    Panic,
    Dead,
    Explosion,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;

    [Header("SFX Pool")]
    [SerializeField] private int playerSFXPoolSize = 5;
    [SerializeField] private int enemySFXPoolSize = 5;
    private AudioSource _playerSources;
    private List<AudioSource> _enemySources = new List<AudioSource>();

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> _bGMClips;
    [SerializeField] private List<AudioClip> _playerSFXClips;
    [SerializeField] private List<AudioClip> _enemySFXClips;


    [Header("Active BGM")]
    [SerializeField] private AudioClip _currentBGM;


    private Dictionary<BGM, AudioClip> _bgmDict = new Dictionary<BGM, AudioClip>();
    private Dictionary<PlayerSFX, AudioClip> _playerSFXDict = new Dictionary<PlayerSFX, AudioClip>();
    private Dictionary<EnemySFX, AudioClip> _enemySFXDict = new Dictionary<EnemySFX, AudioClip>();

    [SerializeField] private int _playerSFXIndex = 0;
    [SerializeField] private int _enemySFXIndex = 0;


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
        _bgmSource = GetComponent<AudioSource>();


        // 각 enum 에 맞추어 bgm 사운드 클립 조정
        for (int i = 0; i < _bGMClips.Count; i++)
            _bgmDict[(BGM)i] = _bGMClips[i];


        // 각 enum 에 맞추어 sfx 사운드 클립 조정
        for (int i = 0; i < _playerSFXClips.Count; i++)
            _playerSFXDict[(PlayerSFX)i] = _playerSFXClips[i];
        for (int i = 0; i < _enemySFXClips.Count; i++)
            _enemySFXDict[(EnemySFX)i] = _enemySFXClips[i];

        
        GameObject playerSFXSource = new GameObject("Player_SFX");  
        playerSFXSource.transform.parent = transform; // 사운드 매니저와 같은 위치에서 스폰
    
        AudioSource playerSource = playerSFXSource.AddComponent<AudioSource>(); // 생성된 오브젝트에 사운드소스 추가
        playerSource.playOnAwake = false; // 생성시 자동으로 사운드 플레이 제거
        _playerSources = playerSource; // 소스 리스트에 등록


        // 에너미의 SFX 전용 pool 적용
        for (int i = 0; i < playerSFXPoolSize; i++)
        {
            GameObject obj = new GameObject("Enemy_SFX_" + i);  
            obj.transform.parent = transform; // 사운드 매니저와 같은 위치에서 스폰
        
            AudioSource source = obj.AddComponent<AudioSource>(); // 생성된 오브젝트에 사운드소스 추가
            source.playOnAwake = false; // 생성시 자동으로 사운드 플레이 제거
            _enemySources.Add(source); // 소스 리스트에 등록
        }
    }

    #endregion


/****************************************************************************************/
    #region BGM

    public void PlayBGM(BGM track)
    {
        // 만약 사운드가 없을시 리턴
        if (!_bgmDict.TryGetValue(track, out AudioClip clip)) return;
        if (_currentBGM == clip) return;

        _currentBGM = clip;
        _bgmSource.clip = clip; // 클립 정보 등록
        _bgmSource.loop = true; // 루프 적용
        _bgmSource.Play(); // 사운드 플레이
    }


    public void StopBGM()
    {
        if (!_bgmSource.isPlaying || _bgmSource == null) return;
        _currentBGM = _bgmDict[BGM.None];
        _bgmSource.Stop();
    }

    #endregion

    
/****************************************************************************************/

    #region SFX

    public void PlaySFX(PlayerSFX sfx)
    {
        // 만약 사운드가 없을시 리턴
        if (!_playerSFXDict.TryGetValue(sfx, out AudioClip clip)) 
        {
            Debug.Log("[SoundManager]: Give Player SFX is Null");
            return;
        }

        // 가장 늦게 사용된 사운드 소스 등록 // true 일 시 플레이어 효과음
        _playerSources.PlayOneShot(clip);
    }

    public void PlaySFX(EnemySFX sfx)
    {
        // 만약 사운드가 없을시 리턴
        if (!_enemySFXDict.TryGetValue(sfx, out AudioClip clip))
        {
            Debug.Log("[SoundManager]: Give Enemy SFX is Null");
            return;
        }
        AudioSource source = GetAvailableSFXSource();
        // 가장 늦게 사용된 사운드 소스 등록 // false 일 시 적 효과음
        source.PlayOneShot(clip);
    }


    // 만들어진 사운드 소스들중 가장 나중에 사용된 pool 하여 사용
    private AudioSource GetAvailableSFXSource()
    {
        // 플레이된 사운드 매니저의 다음 순서 적용
        AudioSource source;

        source = _enemySources[_enemySFXIndex];
        _enemySFXIndex = (_enemySFXIndex + 1) % _enemySources.Count;

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
        // set player audio sorce
        _playerSources.volume = volume;    

        // Set enemy effect audio source 
        foreach (var enemySource in _enemySources)
        {
            enemySource.volume = volume;
        }
    }

    #endregion
}