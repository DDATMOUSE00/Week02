using UnityEngine;

public class PlayerEndSlowTrigger : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerControllerVersionTwo _playerController; //플레이어 받아오기

    [SerializeField] private float _slowMotionStartScale = 0.1f; //슬로우 크기
    [SerializeField] private float _normalTimeScale = 1f; //기본 시간 배속
    [SerializeField] private float _minHeightToSlow = 1.5f; //이 높이 이상일 때만 슬로우

    [SerializeField] private bool _isPlayerInside; //플레이어가 트리거 안에 있는지

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_playerController == null)
            return;

        if (other.GetComponent<PlayerControllerVersionTwo>() == _playerController)
        {
            _isPlayerInside = true;
            CheckAndApplySlow();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_playerController == null)
            return;

        if (other.GetComponent<PlayerControllerVersionTwo>() != null) 
        {
            _isPlayerInside = false;
            RestoreTimeScale();
            GameManager.Instance.UnlockPlayer();
        }
    }

    private void CheckAndApplySlow()
    {
        if (_playerController == null)
            return;

        float playerY = _playerController.transform.position.y;

        //일정 높이 이상일 때만 슬로우
        if (_isPlayerInside && playerY >= _minHeightToSlow)
        {
            ApplySlowMotion();
            GameManager.Instance.LockPlayer();
        }
        else
        {
            RestoreTimeScale();
        }
    }

    private void ApplySlowMotion()
    {
        Time.timeScale = _slowMotionStartScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = _normalTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
