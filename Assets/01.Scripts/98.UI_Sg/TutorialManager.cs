using System.Collections;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    public enum TutorialStep { None, MoveAD, JumpCharge, InAir, SlamWait, Finished }

    [Header("현재 튜토리얼 단계")]
    [SerializeField] private TutorialStep _currentStep = TutorialStep.MoveAD;

    [Header("참조 설정")]
    [SerializeField] private PlayerControllerVersionTwo _playerController;
    [SerializeField] private Manual_Ui_sg _uiManager;
    [SerializeField] private Rigidbody2D _playerRb;

    [Header("튜토리얼 설정값")]
    [SerializeField] private float _slowMotionScale = 0.05f;
    [SerializeField] private float _postSlamDelay = 2.0f;

    private bool _isPeakDetected = false;

    public override void Init() { Debug.Log("Tutorial Manager Initialized."); }

    private void Start() { SetStep(TutorialStep.MoveAD); }

    private void Update()
    {
        if (_playerRb == null) return;

        switch (_currentStep)
        {
            case TutorialStep.JumpCharge:
                // [수정] 플레이어 코드를 고치지 않고, 물리 상태를 감시하여 점프 판단
                // Y축 속도가 일정 이상 올라가면 점프한 것으로 간주
                if (_playerRb.linearVelocity.y > 0.5f)
                {
                    _isPeakDetected = false;
                    SetStep(TutorialStep.InAir);
                    Debug.Log("감시 로직: 플레이어 상승 감지 -> InAir 단계로 전환");
                }
                break;

            case TutorialStep.InAir:
                CheckPeakHeight();
                break;

            case TutorialStep.SlamWait:
                // [수정] 슬램 입력 감시 (Input System이나 기존 Input 클래스 사용)
                // 플레이어 컨트롤러를 수정할 수 없으므로 여기서 직접 키 입력을 체크함
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    OnSlamInput();
                }
                break;
        }
    }

    public void SetStep(TutorialStep nextStep)
    {
        _currentStep = nextStep;
        if (_uiManager == null) return;

        switch (_currentStep)
        {
            case TutorialStep.MoveAD: _uiManager.ShowOnlyAD(); break;
            case TutorialStep.JumpCharge: _uiManager.ShowOnlySpace(); break;
            case TutorialStep.SlamWait:
                Time.timeScale = _slowMotionScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                _uiManager.ShowOnlySpace();
                break;
            case TutorialStep.Finished:
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = 0.02f;
                _uiManager.HideAll();
                StartCoroutine(FinishTutorialRoutine());
                break;
        }
    }

    public void OnTrigger1Entered()
    {
        if (_currentStep == TutorialStep.MoveAD) SetStep(TutorialStep.JumpCharge);
    }

    private void CheckPeakHeight()
    {
        // 상승하다가 속도가 줄어들면 최고점으로 판단
        if (!_isPeakDetected && _playerRb.linearVelocity.y <= 0.1f)
        {
            _isPeakDetected = true;
            SetStep(TutorialStep.SlamWait);
        }
    }

    public void OnSlamInput()
    {
        if (_currentStep == TutorialStep.SlamWait)
            SetStep(TutorialStep.Finished);
    }

    private IEnumerator FinishTutorialRoutine()
    {
        yield return new WaitForSecondsRealtime(_postSlamDelay);
        if (EventManager.Instance != null)
            EventManager.Instance.PostNotification(MEventType.StageStarted, this, null);
    }
}