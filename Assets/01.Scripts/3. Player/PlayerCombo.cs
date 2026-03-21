using UnityEngine;

public class PlayerCombo : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField] private Vector2 _maxMultipy = Vector2.one;
    [SerializeField] private int _maxCombo = 999;
    [SerializeField] private float _comboDuration = 1.0f;

    [Header("Combo Level")]
    [SerializeField] private int _combo_level_01 = 100;
    [SerializeField] private int _combo_level_02 = 500;
    [SerializeField] private int _combo_level_03 = 999;

    [Header("UI | 채워야하는 필드. (없으면 UI 연동 안됨")]
    [SerializeField] private ComboUITextBehaviour _comboText;

    private int _currentKill = 0;
    private int _currentCombo = 0;
    private float _currentComboDuration = 0f;
    private int _currentComboLevel = 0;

    public int CurrentKill => _currentKill;
    public int CurrentCombo => _currentCombo;
    public float CurrentComboDuration => _currentComboDuration;
    public bool HasCombo => _currentCombo > 0 && _currentComboDuration > 0f;
    public int CurrentComboLevel => _currentComboLevel;

    public float CurrentComboRatio
    {
        get
        {
            if (_maxCombo <= 0)
                return 0f;

            return Mathf.Clamp01((float)_currentCombo / _maxCombo);
        }
    }

    public float CurrentComboRemainRatio
    {
        get
        {
            if (_currentCombo <= 0 || _comboDuration <= 0f)
                return 0f;

            return Mathf.Clamp01(_currentComboDuration / _comboDuration);
        }
    }

    public Vector2 CurrentComboMultiplier
    {
        get
        {
            return Vector2.Lerp(Vector2.one, _maxMultipy, CurrentComboRatio);
        }
    }

    private void OnValidate()
    {
        _maxMultipy.x = Mathf.Max(0f, _maxMultipy.x);
        _maxMultipy.y = Mathf.Max(0f, _maxMultipy.y);

        _maxCombo = Mathf.Max(1, _maxCombo);
        _comboDuration = Mathf.Max(0f, _comboDuration);

        _combo_level_01 = Mathf.Clamp(_combo_level_01, 1, _maxCombo);
        _combo_level_02 = Mathf.Clamp(_combo_level_02, _combo_level_01, _maxCombo);
        _combo_level_03 = Mathf.Clamp(_combo_level_03, _combo_level_02, _maxCombo);
    }

    public void AddKill()
    {
        _currentKill++;

        bool canKeepCombo = _currentCombo > 0 && _currentComboDuration > 0f;

        if (canKeepCombo)
            AddCombo();
        else
            _currentCombo = 1;

        _currentComboDuration = _comboDuration;
        RefreshComboLevel();

        if (_comboText != null)
            _comboText.AddKill(CurrentCombo, CurrentComboRatio, CurrentComboRemainRatio);
    }

    private void AddCombo()
    {
        _currentCombo = Mathf.Min(_currentCombo + 1, _maxCombo);
    }

    private void RefreshComboLevel()
    {
        _currentComboLevel = EvaluateComboLevel(_currentCombo);
    }

    private int EvaluateComboLevel(int comboCount)
    {
        if (comboCount >= _combo_level_03)
            return 3;

        if (comboCount >= _combo_level_02)
            return 2;

        if (comboCount >= _combo_level_01)
            return 1;

        return 0;
    }

    private void ResetCombo()
    {
        _currentCombo = 0;
        _currentComboDuration = 0f;
        _currentComboLevel = 0;

        if (_comboText != null)
            _comboText.ResetCombo();
    }

    public void Update()
    {
        if (_currentCombo <= 0)
            return;

        if (_currentComboDuration > 0f)
            _currentComboDuration -= Time.deltaTime;

        if (_comboText != null)
            _comboText.SetRemainRatio(CurrentComboRemainRatio);

        if (_currentComboDuration <= 0f)
            ResetCombo();
    }
}