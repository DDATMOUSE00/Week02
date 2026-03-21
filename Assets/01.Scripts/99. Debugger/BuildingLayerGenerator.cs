using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BuildingLayerGenerator : MonoBehaviour
{
    [Header("바닥")]
    [SerializeField] private Transform _floorReference;

    [Header("레이어 루트")]
    [SerializeField] private Transform _veryFarLayerRoot;
    [SerializeField] private Transform _farLayerRoot;
    [SerializeField] private Transform _frontLayerRoot;

    [Header("Very Far Layer Settings")]
    [SerializeField] private GameObject _veryFarMountainPrefab;
   
    [Header("Far Layer Settings")]
    [SerializeField] private GameObject _farBuildingPrefab;

    [Header("Front Building 종류")]
    [SerializeField] private List<GameObject> _frontBuildingPrefabs = new();
    
    [Header("Front Building 간격 (X = 간격 최소 Y = 간격 최대)")]

    [SerializeField] private Vector2 _frontGapRange = new(0.5f, 0f);
    [SerializeField] private bool _avoidRepeat = true;

    [Header("건물 스폰 범위 간격")]
    [SerializeField] private float _leftPadding;
    [SerializeField] private float _rightPadding;

    [Header("Random")]
    [SerializeField] private bool _useFixedSeed;
    [SerializeField] private int _randomSeed;

    public void GenerateAll()
    {
        ClearAll();
        GenerateVeryFarLayer();
        GenerateFarLayer();
        GenerateFrontLayer();
    }
    public void GenerateVeryFarLayer()
    {
        if (_veryFarLayerRoot == null || _veryFarMountainPrefab == null)
        {
            Debug.LogWarning("세팅실패");
            return;
        }

        if (!TryGetFloorRange(out float minX, out float maxX))
        {
            Debug.LogWarning("바닥문제");
            return;
        }

        ClearLayer(_veryFarLayerRoot);

        float startX = minX - _leftPadding;
        float endX = maxX + _rightPadding;

        float worldWidth = GetPrefabWidth(_veryFarMountainPrefab);
        if (worldWidth <= 0f)
        {
            Debug.LogWarning("산넓이맵넘음");
            return;
        }

        float currentLeftX = startX;
        while (currentLeftX < endX)
        {
            GameObject instance = CreateInstance(_veryFarMountainPrefab, _veryFarLayerRoot);
            instance.name = $"{_veryFarMountainPrefab.name}_VeryFar";

            Vector3 pos = instance.transform.position;
            pos.x = currentLeftX + (worldWidth * 0.5f);
            //pos.y = _veryFarY;
            instance.transform.position = pos;

            currentLeftX += worldWidth; 
        }
    }
    public void GenerateFarLayer()
    {
        if (_farLayerRoot == null || _farBuildingPrefab == null)
        {
            Debug.LogWarning("Far Layer 설정 비었음");
            return;
        }

        if (!TryGetFloorRange(out float minX, out float maxX))
        {
            Debug.LogWarning("Floor 범위 계산 불가");
            return;
        }

        ClearLayer(_farLayerRoot);

        float startX = minX - _leftPadding;
        float endX = maxX + _rightPadding;

        SpriteRenderer prefabSpriteRenderer = _farBuildingPrefab.GetComponentInChildren<SpriteRenderer>();
        if (prefabSpriteRenderer == null || prefabSpriteRenderer.sprite == null)
        {
            Debug.LogWarning("Far building 프리팹 이미지 빠짐");
            return;
        }

        float spriteWidth = prefabSpriteRenderer.sprite.bounds.size.x;
        float scaleX = prefabSpriteRenderer.transform.localScale.x;
        float worldWidth = Mathf.Abs(spriteWidth * scaleX);

        if (worldWidth <= 0f)
        {
            Debug.LogWarning("Far building 폭 계산 실패");
            return;
        }

        float currentLeftX = startX;

        while (currentLeftX < endX)
        {
            GameObject instance = CreateInstance(_farBuildingPrefab, _farLayerRoot);
            instance.name = $"{_farBuildingPrefab.name}_Far";

            Vector3 position = instance.transform.position;
            position.x = currentLeftX + (worldWidth * 0.5f);
            position.z = 0f;
            instance.transform.position = position;

            currentLeftX += worldWidth;
        }
    }

    public void GenerateFrontLayer()
    {
        if (_frontLayerRoot == null || _frontBuildingPrefabs == null || _frontBuildingPrefabs.Count == 0)
        {
            Debug.LogWarning("Front Layer 설정 비어있음");
            return;
        }

        if (!TryGetFloorRange(out float minX, out float maxX))
        {
            Debug.LogWarning("Floor 범위를 계산불가");
            return;
        }

        ClearLayer(_frontLayerRoot);

        if (_useFixedSeed)
        {
            Random.InitState(_randomSeed);
        }

        float startX = minX - _leftPadding;
        float endX = maxX + _rightPadding;

        float currentX = startX;
        int previousIndex = -1;

        while (currentX < endX)
        {
            int prefabIndex = GetRandomFrontPrefabIndex(previousIndex);
            if (prefabIndex < 0)
            {
                break;
            }

            GameObject prefab = _frontBuildingPrefabs[prefabIndex];
            if (prefab == null)
            {
                currentX += 1f;
                continue;
            }

            float prefabWidth = GetPrefabWidth(prefab);
            if (prefabWidth <= 0f)
            {
                currentX += 1f;
                continue;
            }

            GameObject instance = CreateInstance(prefab, _frontLayerRoot);
            instance.name = $"{prefab.name}_Front";

            Vector3 position = instance.transform.position;
            position.x = currentX + (prefabWidth * 0.5f);
            position.z = 0f;
            instance.transform.position = position;


            float gap = Random.Range(_frontGapRange.x, _frontGapRange.y);
            currentX += prefabWidth + gap;
            previousIndex = prefabIndex;
        }
    }

    public void ClearAll()
    {
        ClearLayer(_veryFarLayerRoot);
        ClearLayer(_farLayerRoot);
        ClearLayer(_frontLayerRoot);
    }

    public void ClearFarLayer()
    {
        ClearLayer(_farLayerRoot);
    }

    public void ClearFrontLayer()
    {
        ClearLayer(_frontLayerRoot);
    }

    private int GetRandomFrontPrefabIndex(int previousIndex)
    {
        if (_frontBuildingPrefabs == null || _frontBuildingPrefabs.Count == 0)
        {
            return -1;
        }

        if (_frontBuildingPrefabs.Count == 1)
        {
            return 0;
        }

        int index = Random.Range(0, _frontBuildingPrefabs.Count);

        if (!_avoidRepeat)
        {
            return index;
        }

        int safetyCount = 20;

        while (index == previousIndex && safetyCount > 0)
        {
            index = Random.Range(0, _frontBuildingPrefabs.Count);
            safetyCount--;
        }

        return index;
    }

   

    private bool TryGetFloorRange(out float minX, out float maxX)
    {
        minX = 0f;
        maxX = 0f;

        if (_floorReference == null)
        {
            return false;
        }

        Collider2D collider2D = _floorReference.GetComponent<Collider2D>();
        if (collider2D != null)
        {
            Bounds bounds = collider2D.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
            return true;
        }

        Renderer targetRenderer = _floorReference.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            Bounds bounds = targetRenderer.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
            return true;
        }

        Vector3 position = _floorReference.position;
        float width = _floorReference.lossyScale.x;

        minX = position.x - (width * 0.5f);
        maxX = position.x + (width * 0.5f);

        return true;
    }

    private float GetPrefabWidth(GameObject prefab)
    {
        if (prefab == null)
        {
            return 0f;
        }

        SpriteRenderer spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float scaleX = spriteRenderer.transform.localScale.x;
            return Mathf.Abs(spriteWidth * scaleX);
        }

        Renderer targetRenderer = prefab.GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
        {
            return targetRenderer.bounds.size.x;
        }

        return 0f;
    }

    private GameObject CreateInstance(GameObject prefab, Transform parent)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Generate Buildings");
            return instance;
        }
#endif
        return Instantiate(prefab, parent);
    }

    private void ClearLayer(Transform root)
    {
        if (root == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
            }

            return;
        }
#endif

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(root.GetChild(i).gameObject);
        }
    }
}