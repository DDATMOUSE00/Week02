using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 슬라이더 참조를 위해 필요

public class TutorialManager : Singleton<TutorialManager>
{
    public enum TutorialStep { None, MoveAD, JumpCharge, InAir, SlamWait, Finished }

    [Header("현재 튜토리얼 단계")]
    [SerializeField] private TutorialStep _currentStep = TutorialStep.MoveAD;

    [Header("참조 설정")]
    [SerializeField] private PlayerControllerVersionTwo _playerController;
    [SerializeField] private Manual_Ui_sg _uiManager;
    [SerializeField] private Rigidbody2D _playerRb;

    [Header("UI 감시 설정")]
    [SerializeField] private Slider _chargeSlider; // 게이지 슬라이더 연결 필요

    [Header("튜토리얼 설정값")]
    [SerializeField] private float _slowMotionScale = 0.05f;
    [SerializeField] private float _postSlamDelay = 2.0f;

    [Header("텍스트 사진")]
    [SerializeField] private Image _press_Image;
    [SerializeField] private Image _pressHold_Image;

    private bool _isPeakDetected = false;

    public override void Init() { Debug.Log("Tutorial Manager Initialized."); }

    private void Start() { SetStep(TutorialStep.MoveAD); }

    private void Update()
    {
        if (_playerRb == null) return;

        switch (_currentStep)
        {
            case TutorialStep.JumpCharge:
                // 조건 1: 게이지가 50% 이상인가?
                // 조건 2: 플레이어가 바닥을 떠나 위로 솟구치고 있는가? (점프 실행 여부 감시)
                if (_chargeSlider != null && _chargeSlider.value >= 0.5f && _playerRb.linearVelocity.y > 0.1f)
                {
                    _isPeakDetected = false;
                    SetStep(TutorialStep.InAir);
                    Debug.Log("게이지 50% 충전 후 점프 확인: 최고점 감시 시작");
                }
                break;

            case TutorialStep.InAir:
                CheckPeakHeight();
                break;

            case TutorialStep.SlamWait:
                // 슬램 입력 감시
                //트리거 3발동
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _press_Image.gameObject.SetActive(false);
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
        if (_currentStep == TutorialStep.MoveAD)
            
            SetStep(TutorialStep.JumpCharge);
            _pressHold_Image.gameObject.SetActive(true);
    }

    private void CheckPeakHeight()
    {
        if (_playerRb == null) return;
        if (!_isPeakDetected && _playerRb.linearVelocity.y <= 0.1f)
        {   //트리거 2발동
            _pressHold_Image.gameObject.SetActive(false);
            _isPeakDetected = true;
            SetStep(TutorialStep.SlamWait);
            _press_Image.gameObject.SetActive(true);

        }
    }

    public void OnSlamInput()
    {   //트리거 3발동
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