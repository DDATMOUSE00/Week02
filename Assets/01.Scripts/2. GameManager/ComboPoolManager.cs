using System.Collections.Generic;
using UnityEngine;

public class ComboPoolManager : Singleton<ComboPoolManager>
{
    [Header("Pool Setting")]
    [SerializeField] private ComboTextBehaviour _prefab;
    [SerializeField] private int _initialSize = 200;
    [SerializeField] private Transform _poolParent;

    [Header("Offset")]
    [SerializeField] private float yOffset;


    [SerializeField] private Queue<ComboTextBehaviour> _enemyPool = new Queue<ComboTextBehaviour>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
            return;

        for (int i = 0; i < _initialSize; i++)
        {
            CreateComboText();
        }
    }

    private ComboTextBehaviour CreateComboText()
    {
        ComboTextBehaviour newTmp = Instantiate(_prefab, _poolParent);
        newTmp.gameObject.SetActive(false);
        _enemyPool.Enqueue(newTmp);
        return newTmp;
    }

    public ComboTextBehaviour GetComboText()
    {
        if (_enemyPool.Count == 0)
        {
            CreateComboText();
        }

        ComboTextBehaviour enemy = _enemyPool.Dequeue();
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    // 외부에서 바로 콤보 텍스트를 재생할 수 있는 헬퍼 함수다.
    public ComboTextBehaviour Play(Vector3 worldPosition, int comboCount)
    {
        ComboTextBehaviour comboText = GetComboText();
        comboText.Play(new Vector3(worldPosition.x, worldPosition.y + yOffset, worldPosition.z), comboCount);
        return comboText;
    }

    public void ReturnQueue(ComboTextBehaviour comboText)
    {
        comboText.gameObject.SetActive(false);
        comboText.transform.SetParent(_poolParent);
        _enemyPool.Enqueue(comboText);
    }

    public override void Init()
    {
    }
}