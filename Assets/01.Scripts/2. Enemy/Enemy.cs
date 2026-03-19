using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Panic,
        Dead
    }

    [SerializeField] private Transform visual; //캐릭터 스프라이트
    private Tween panicTween;

    [Header("State")]
    [SerializeField] private EnemyState state;

    [Header("Find Player")]
    [SerializeField] private Transform player;

    [Header("Move")]
    [SerializeField] private float idleSpeed = 0.3f; //평소 속도
    [SerializeField] private float panicminSpeed = 0.8f; //공포상태 최소속도
    [SerializeField] private float panicmaxSpeed = 1.3f; //공포상태 최고속도

    [Header("Panic Setting")]
    [SerializeField] private float detectRange = 5f; //플레이어 감지범위
    [SerializeField] private float runRange = 15; //공포상태에서 도망가는 범위
    [SerializeField] private float panicShake = 0.04f; //떨리는 정도

    [Header("Idle Setting")]
    [SerializeField] private float changeDirTime = 2f; //이정도 시간으로 왔다갔다
    [SerializeField] private float idleRange = 0.4f; //이정도 범위에서 왔다갔다

    private Vector2 moveDir; //방향
    private float _distance; //플레이어랑 거리
    private float _dirtimer; //시간

    private Vector3 VisualOriginalPos; //Visual 처음 위치 저장

    [SerializeField] private float panicSpeed; //랜덤 달리기속도

    private EnemyHealth enemyHealth; //체력인데 계속 1일듯 아마
    private Collider2D col; //콜라이더

    private void Awake()
    {
        state = EnemyState.Idle;

        col = GetComponent<Collider2D>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (visual != null)
            VisualOriginalPos = visual.localPosition;

        //랜덤 속도 만들기 인데 풀링 때문에 Init에 있으니 나중에 빼기
        panicSpeed = Random.Range(panicminSpeed, panicmaxSpeed);
    }

    private void Update()
    {
        if (state == EnemyState.Dead)
            return;

        if (player == null)
            return;

        _distance = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case EnemyState.Idle:
                if (_distance <= detectRange)
                {
                    ChangeState(EnemyState.Panic);
                    return;
                }
                DoIdle();
                break;

            case EnemyState.Panic:
                if (_distance >= runRange)
                {
                    ChangeState(EnemyState.Idle);
                    return;
                }
                DoPanic();
                break;
        }
    }


    public void Init(Transform targetPlayer) //오브젝트풀링을 위한
    {
        player = targetPlayer;

        state = EnemyState.Idle;
        _dirtimer = 0f;
        moveDir = Vector2.zero;

        panicSpeed = Random.Range(panicminSpeed, panicmaxSpeed);

        StopPanicShake();

        if (visual != null)
            visual.localPosition = VisualOriginalPos;

        if (col != null)
            col.enabled = true;

        if (enemyHealth != null)
            enemyHealth.Init();
    }

    public void OnDeath()
    {
        if (state == EnemyState.Dead)
            return;

        state = EnemyState.Dead;

        StopPanicShake();

        if (col != null)
            col.enabled = false;

        //죽는 연출 넣기
        PoolManager.Instance.ReturnEnemy(this);
    }


    private void ChangeState(EnemyState newState)
    {
        if (state == newState)
            return;

        state = newState;

        if (state == EnemyState.Panic)
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
            _dirtimer = changeDirTime;

            float randomX = Random.Range(-idleRange, idleRange);
            moveDir = new Vector2(randomX, 0f).normalized;
        }

        //이동
        transform.position += (Vector3)(moveDir * idleSpeed * Time.deltaTime);
    }

    private void DoPanic() //Panic상태 움직임
    {
        if (player == null)
            return;

        //플레이어 반대 방향으로만 이동
        float dirX = transform.position.x - player.position.x;
        float runX = Mathf.Sign(dirX);

        //x가 같으면 랜덤
        if (runX == 0f)
        {
            if (moveDir.x != 0f)
                runX = Mathf.Sign(moveDir.x);
            else
                runX = Random.value > 0.5f ? 1f : -1f;
        }

        moveDir = new Vector2(runX, 0f);
        transform.position += (Vector3)(moveDir * panicSpeed * Time.deltaTime);
    }


    //패닉 움직임
    private void StartPanicShake()
    {
        if (visual == null)
            return;

        //혹시 기존 트윈 있으면 제거
        panicTween?.Kill();

        //계속 반복되는 흔들림
        panicTween = visual.DOShakePosition(
            duration: 1f, // 1초짜리 (루프로 계속 반복됨)
            strength: new Vector3(panicShake, 0f, 0f), //좌우 흔들림
            vibrato: 20, //떨림 횟수
            randomness: 90f,
            snapping: false,
            fadeOut: false
        ).SetLoops(-1, LoopType.Restart);
    }
    private void StopPanicShake()
    {
        if (panicTween != null)
        {
            panicTween.Kill();
            panicTween = null;
        }

        if (visual != null)
            visual.localPosition = VisualOriginalPos;
    }






    //범위 체크 Gizmos
    //private void OnDrawGizmos()
    //{
    //    // 감지 범위
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, detectRange);

    //    // Panic 해제 범위
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, runRange);

    //    // Idle 배회 범위
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, idleRange);

    //    // 플레이어와 현재 거리 선
    //    if (player != null)
    //    {
    //        Gizmos.color = Color.white;
    //        Gizmos.DrawLine(transform.position, player.position);
    //    }
    //}
}
