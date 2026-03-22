using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 슬라이더 참조를 위해 필요

public class TutorialManager : Singleton<TutorialManager>
{
    public enum TutorialStep { None, MoveAD, JumpCharge, InAir, SlamWait, Finished }

    [Header("현재 튜토리얼 단계")]
    [SerializeField] private TutorialStep _currentStep = TutorialStep.None;

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
    [SerializeField] private GameObject _press_Image;
    [SerializeField] private GameObject _pressHold_Image;

    [Header("입력 레퍼런스")]
    [SerializeField] private InputActionReference _slamActionReference; // 인스펙터에서 Jump 액션 연결

    public float GameStartPosition;



    private bool _isPeakDetected = false;
    private bool _canJumpNow = false;
    public override void Init() {}

    private void Start() { /*SetStep(TutorialStep.MoveAD); */}
    public void OnTrigger1Entered()
        {
            // 중괄호를 추가하여 안전하게 처리
            if (_currentStep == TutorialStep.MoveAD)
            {
                SetStep(TutorialStep.JumpCharge);
                if (_pressHold_Image != null) _pressHold_Image.gameObject.SetActive(true);
            }
        }
    private void Update()
    {
        if (_playerRb == null) return;

        switch (_currentStep)
        {
            case TutorialStep.JumpCharge:
                // 1. 게이지가 0.5를 넘었는지 먼저 확인
                if (_chargeSlider != null && _chargeSlider.value >= 0.5f)
                {
                    if (!_canJumpNow)
                    {
                        _canJumpNow = true;
                        //Debug.Log("준비 완료! 이제 점프하세요.");
                    }
                }

                // 2. 준비된 상태에서 플레이어의 Y축 속도가 상승하면 단계 전환
                // 게이지가 이미 0으로 떨어졌어도 _canJumpNow가 true이므로 인식됨
                if (_canJumpNow && _playerRb.linearVelocity.y > 0.1f)
                {
                    _canJumpNow = false; // 플래그 리셋
                    SetStep(TutorialStep.InAir);
                    if (_pressHold_Image != null) _pressHold_Image.gameObject.SetActive(false);
                  //  Debug.Log("점프 성공! 공중 상태 진입.");
                }
                break;

            case TutorialStep.InAir:
                CheckPeakHeight();
                break;
            case TutorialStep.SlamWait:
                // [수정] New Input System 방식으로 변경
                if (_slamActionReference != null && _slamActionReference.action.WasPressedThisFrame())
                {
                    if (_press_Image != null) _press_Image.gameObject.SetActive(false);
                    OnSlamInput();

                    // 디버그 로직 (원하실 경우 추가)
                    //Debug.Log("<color=orange>[Tutorial]</color> 슬램 입력 감지 (New Input System)");
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
            case TutorialStep.MoveAD:
                _uiManager.ShowOnlyAD();
                break;
            case TutorialStep.JumpCharge: 
                _uiManager.ShowOnlySpace(); 
                break;
            case TutorialStep.SlamWait:
                Time.timeScale = _slowMotionScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                _uiManager.ShowOnlySpace();
                break;
            case TutorialStep.Finished:
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = 0.02f;
                _uiManager.HideAll();
                GameManager.Instance.GameStart(); //트리거로 작동하고 싶음 이줄 삭제하고 [Point]StartTrigger 프리팹 배치
                break;
        }
    }
    //트리거 1 발동
    

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
    public void StartTutorial()
    {
       // Debug.Log("<color=green>[Tutorial]</color> 튜토리얼 본격 시작!");

        // [핵심 추가] 시작 씬에서 꺼졌던 UI 오브젝트 자체를 강제로 다시 켭니다.
        if (_uiManager != null)
        {
            _uiManager.gameObject.SetActive(true);
        }

        SetStep(TutorialStep.MoveAD);
    }
    
}