using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween을 쓰기 위해 필수!

public class FadeController_sj : MonoBehaviour // Panel 불투명도 조절해 페이드인 or 페이드아웃
{
    [Header("테스트용 오브젝트")]
    [SerializeField] private GameObject _cube;
    [SerializeField] private RectTransform _uiPanel;
    [SerializeField] private Image _fadeImage;

    void Start()
    {
        // 0. 초기화 (선택사항이지만 권장)
        DOTween.Init();
    }

    public void RunExample()
    {
        // 1. 이동 (DOMove) - 2초 동안 (5, 5, 0) 위치로 이동
        _cube.transform.DOMove(new Vector3(5, 5, 0), 2f)
            .SetEase(Ease.OutBounce); // 튕기는 효과 추가

        // 2. 크기 조절 (DOScale) - 1초 동안 2배로 커졌다가 다시 돌아오기
        _cube.transform.DOScale(2f, 1f)
            .SetLoops(2, LoopType.Yoyo); // 2번 반복 (커졌다 작아졌다)

        // 3. UI 이동 (DOAnchorPos) - UI는 DOMove 대신 앵커 좌표를 써야 정확함
        _uiPanel.DOAnchorPos(new Vector2(0, 0), 1.5f)
            .From(new Vector2(0, -1000f)); // 밑에서 위로 슥 올라오는 연출

        // 4. 투명도 (DOFade) - 1초 동안 서서히 사라지기
        _fadeImage.DOFade(0f, 1f).OnComplete(() => {
            Debug.Log("페이드가 끝났습니다! 오브젝트를 끕니다.");
            _fadeImage.gameObject.SetActive(false);
        });

        // 5. 회전 (DORotate)
        _cube.transform.DORotate(new Vector3(0, 180, 0), 1f);
    }

}