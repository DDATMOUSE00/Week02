using DG.Tweening;
using UnityEngine;
using static Enemy;

[System.Serializable]
public class EnemySpriteSet
{
    public Sprite IdleSprite; //Idle 상태 스프라이트
    public Sprite PanicSprite; //Panic 상태 스프라이트
}
public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Panic,
        Dead
    }
    public enum EnemyType
    {
        A,
        B,
        C,
        D
    }

    [SerializeField] private Transform _visual; //스프라이트 움직이기 위해
    [SerializeField] private Tween _panicTween;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer _spriteRenderer; //적 스프라이트
    [SerializeField] private EnemyType _enemyType; //적 종류
    [SerializeField] private EnemySpriteSet _typeA; //A타입 스프라이트
    [SerializeField] private EnemySpriteSet _typeB; //B타입 스프라이트
    [SerializeField] private EnemySpriteSet _typeC; //C타입 스프라이트
    [SerializeField] private EnemySpriteSet _typeD; //D타입 스프라이트

    [Header("State")]
    [SerializeField] private EnemyState _state;

    [Header("Find Player")]
    [SerializeField] private Transform _player;

    [Header("Move")]
    [SerializeField] private float _idleSpeed = 0.3f; //평소 속도
    [SerializeField] private float _panicminSpeed = 0.8f; //공포상태 최소속도
    [SerializeField] private float _panicmaxSpeed = 1.3f; //공포상태 최고속도

    [Header("Panic Setting")]
    [SerializeField] private float _detectRange = 28f; //플레이어 감지범위
    [SerializeField] private float _runRange = 35; //공포상태에서 도망가는 범위
    [SerializeField] private float _panicShake = 0.04f; //떨리는 정도

    [Header("Idle Setting")]
    [SerializeField] private float _changeDirTime = 2f; //이정도 시간으로 왔다갔다
    [SerializeField] private float _idleRange = 0.4f; //이정도 범위에서 왔다갔다

    [SerializeField] private Vector2 _moveDir; //방향
    [SerializeField] private float _distance; //플레이어랑 거리
    [SerializeField] private float _dirtimer; //시간

    [SerializeField] private Vector3 _visualOriginalPos; //Visual 처음 위치 저장
    [SerializeField] private Vector3 _visualOriginalScale; //원래 크기 저장

    [SerializeField] private float _panicSpeed; //랜덤 달리기속도

    [SerializeField] private EnemyHealth _enemyHealth; //체력인데 계속 1일듯 아마
    [SerializeField] private Collider2D _col; //콜라이더
    [SerializeField] private Rigidbody2D _rb; //리지드바디

    private void Awake()
    {
        _state = EnemyState.Idle;

        _col = GetComponent<Collider2D>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _rb = GetComponent<Rigidbody2D>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_visual != null)
        {
            _visualOriginalPos = _visual.localPosition;
            _visualOriginalScale = _visual.localScale;
        }

        //랜덤 속도 만들기 인데 풀링 때문에 Init에 있으니 나중에 빼기
        _panicSpeed = Random.Range(_panicminSpeed, _panicmaxSpeed);

        if (_rb != null)
        {
            //_rb.simulated = false; //평소엔 물리 끔
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        UpdateSpriteByState();
    }

    private void UpdateSpriteByState()
    {
        if (_spriteRenderer == null)
            return;

        EnemySpriteSet spriteSet = GetCurrentSpriteSet();

        switch (_state)
        {
            case EnemyState.Idle:
                _spriteRenderer.sprite = spriteSet.IdleSprite;
                break;

            case EnemyState.Panic:
                _spriteRenderer.sprite = spriteSet.PanicSprite;
                break;

            case EnemyState.Dead:
                _spriteRenderer.sprite = spriteSet.PanicSprite; //Dead일 땐 Panic 상태로 죽을 거라서 Panic 스프라이트 유지
                break;
        }
    }

    private EnemySpriteSet GetCurrentSpriteSet()  //enemy 스프라이트 선택
    {
        switch (_enemyType)
        {
            case EnemyType.A:
                return _typeA;

            case EnemyType.B:
                return _typeB;

            case EnemyType.C:
                return _typeC;

            case EnemyType.D:
                return _typeD;
        }

        return _typeA;
    }

    private void Update()
    {
        if (_state == EnemyState.Dead)
            return;

        if (_player == null)
            return;

        _distance = Vector2.Distance(transform.position, _player.position);

        switch (_state)
        {
            case EnemyState.Idle:
                if (_distance <= _detectRange)
                {
                    ChangeState(EnemyState.Panic);
                    return;
                }
                DoIdle();
                break;

            case EnemyState.Panic:
                if (_distance >= _runRange)
                {
                    ChangeState(EnemyState.Idle);
                    return;
                }
                DoPanic();
                break;
        }
    }


    public void Init(Transform targetPlayer, EnemyType enemyType) //오브젝트풀링을 위한
    {
        _player = targetPlayer;
        _enemyType = enemyType;

        _state = EnemyState.Idle;
        _dirtimer = 0f;
        _moveDir = Vector2.zero;

        _panicSpeed = Random.Range(_panicminSpeed, _panicmaxSpeed);

        StopPanicShake();

        transform.rotation = Quaternion.identity; //회전 초기화

        if (_visual != null)
        {
            _visual.localPosition = _visualOriginalPos;
            _visual.localRotation = Quaternion.identity;
            _visual.localScale = _visualOriginalScale;
        }

        if (_col != null)
            _col.enabled = true;

        if (_rb != null)
        {
            //_rb.simulated = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.rotation = 0f;
        }

        if (_enemyHealth != null)
            _enemyHealth.Init();

        UpdateSpriteByState();
    }

    public void OnDeath()
    {
        if (_state == EnemyState.Dead)
            return;

        _state = EnemyState.Dead;

        UpdateSpriteByState();

        StopPanicShake();

        //if (col != null)
        //    col.enabled = false;

        //죽는 연출 나오게 하는
        PlayDeathPhysics();
    }


    //----------죽는 모션 시작----------
    private void PlayDeathPhysics()
    {
        if (_rb == null)
        {
            PoolManager.Instance.ReturnEnemy(this);
            return;
        }

        float dirX = 1f;

        if (_player != null)
        {
            dirX = Mathf.Sign(transform.position.x - _player.position.x);
            if (dirX == 0f)
                dirX = Random.value > 0.5f ? 1f : -1f;
        }

        _rb.simulated = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        Vector2 force = new Vector2(dirX * 80f, 3.5f);
        _rb.AddForce(force, ForceMode2D.Impulse);

        _rb.AddTorque(-dirX * 42f, ForceMode2D.Impulse);

        if (_visual != null)
        {
            _visual.DOPunchScale(new Vector3(0.15f, -0.1f, 0f), 0.2f, 8, 0.8f);
        }

        Invoke(nameof(ReturnToPool), 1.2f);
    }
    private void ReturnToPool()
    {
        if (_rb != null)
        {
            //_rb.simulated = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.rotation = 0f;
        }

        transform.rotation = Quaternion.identity;

        PoolManager.Instance.ReturnEnemy(this);
    }
    //----------죽는 모션 끝----------

    private void ChangeState(EnemyState newState)
    {
        if (_state == newState)
            return;

        _state = newState;

        UpdateSpriteByState();

        if (_state == EnemyState.Panic)
            StartPanicShake();
        else
            StopPanicShake();
    }

    private void DoIdle() //Idle상태 움직임
    {
        _dirtimer -= Time.deltaTime;

        //랜덤하게 방향 변경
        if (_dirtimer <= 0f)
        {
            _dirtimer = _changeDirTime;

            float randomX = Random.Range(-_idleRange, _idleRange);
            _moveDir = new Vector2(randomX, 0f).normalized;
        }

        //이동
        transform.position += (Vector3)(_moveDir * _idleSpeed * Time.deltaTime);
    }

    private void DoPanic() //Panic상태 움직임
    {
        if (_player == null)
            return;

        //플레이어 반대 방향으로만 이동
        float dirX = transform.position.x - _player.position.x;
        float runX = Mathf.Sign(dirX);

        //x가 같으면 랜덤
        if (runX == 0f)
        {
            if (_moveDir.x != 0f)
                runX = Mathf.Sign(_moveDir.x);
            else
                runX = Random.value > 0.5f ? 1f : -1f;
        }

        _moveDir = new Vector2(runX, 0f);
        transform.position += (Vector3)(_moveDir * _panicSpeed * Time.deltaTime);
    }


    //패닉 움직임
    private void StartPanicShake()
    {
        if (_visual == null)
            return;

        //혹시 기존 트윈 있으면 제거
        _panicTween?.Kill();

        //계속 반복되는 흔들림
        _panicTween = _visual.DOShakePosition(
            duration: 1f, // 1초짜리 (루프로 계속 반복됨)
            strength: new Vector3(_panicShake, 0f, 0f), //좌우 흔들림
            vibrato: 20, //떨림 횟수
            randomness: 90f,
            snapping: false,
            fadeOut: false
        ).SetLoops(-1, LoopType.Restart);
    }
    private void StopPanicShake()
    {
        if (_panicTween != null)
        {
            _panicTween.Kill();
            _panicTween = null;
        }

        if (_visual != null)
            _visual.localPosition = _visualOriginalPos;
    }


    //private void OnDrawGizmos()
    //{
    //    // 감지 범위
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, _detectRange);

    //    // Panic 해제 범위
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, _runRange);
    //}

        //    // Idle 배회 범위
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireSphere(transform.position, _idleRange);

        //    // 플레이어와 현재 거리 선
        //    if (_player != null)
        //    {
        //        Gizmos.color = Color.white;
        //        Gizmos.DrawLine(transform.position, _player.position);
        //    }
        //}
    }
