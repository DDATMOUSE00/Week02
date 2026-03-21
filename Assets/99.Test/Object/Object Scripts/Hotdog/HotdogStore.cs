using UnityEngine;

public class HotdogStore : MonoBehaviour
{
    [Header("State")]

    [Header("Start Setting")]
    [SerializeField] private bool _isAlive;
    [SerializeField] private bool _hasCalled;
    [SerializeField] private int _hotdogCount = 1;

    [Header("Plater data")]
    [SerializeField] GameObject _player;


    [Header("Get Components")]
    [SerializeField] private Collider2D _col; //콜라이더
    [SerializeField] private SpriteRenderer _spriteRenderer;



    [Header("Hotdog Perfab")]
    [SerializeField] private GameObject _hotDog;

    [Header("Sprites")]
    [SerializeField] private Sprite _startSprite;
    [SerializeField] private Sprite _destroySprite;


    private void Reset()
    {
        _col = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        if (_col == null)
        {
            Debug.Log("[HotdogStore] : Collider Reference is null");
        }

        if (_player == null)
        {
            Debug.Log("[HotdogStore] : Player Reference is null");
        }

        transform.rotation = Quaternion.identity; //회전 초기화

        _isAlive = true;
        _hasCalled = false;

        _spriteRenderer.sprite = _startSprite;

        // 콜라이더 킴
        if (_col != null)
            _col.enabled = true;
    }

    public void DestoyStore()
    {
        _isAlive = false;
        ExecuteDeathSequence();
    }

    private void ExecuteDeathSequence()
    {
        _spriteRenderer.sprite = _destroySprite;

        // 콜라이더 끔
        if (_col != null)
           _col.enabled = false;

        //죽는 연출 나오게 하는
        SpawnHotdogs();
    }


    //----------죽는 모션 시작----------
    private void SpawnHotdogs()
    {
        for(int i = 0; i < _hotdogCount; i++)
        {
            Instantiate(_hotDog, transform.position, Quaternion.identity);
        }
    }

    //----------죽는 모션 끝----------

    private void DoIdle() //Idle상태 움직임
    {
        
    }

    
}