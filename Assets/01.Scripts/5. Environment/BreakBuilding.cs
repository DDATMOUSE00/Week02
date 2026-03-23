using UnityEngine;

public class BreakBuilding : MonoBehaviour
{
    [SerializeField] private GameObject BuildingSprite;
    [SerializeField] private Rigidbody2D[] _leftPieces;
    [SerializeField] private Rigidbody2D[] _rightPieces;

    [Header("Direction")]
    [SerializeField] private Vector2 _baseDirection = new Vector2(1f, -1f);
    [SerializeField] private bool _normalizeDirection = true;

    [Header("Force")]
    [SerializeField] private float _explodeForce = 40f;
    [SerializeField] private float _randomForce = 0f;
    [SerializeField] private float _rotateForce = 50f;

    private bool _isBroken = false;

    /*private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isBroken) return;

        if (BuildingSprite != null)
            BuildingSprite.SetActive(false);

        PieceActive(_leftPieces, true);
        PieceActive(_rightPieces, true);

        BreakNow();
    }
*/

    public void SlamBuilding()
    {
        //if (_isBroken) return;
        //_isBroken = true;

        if (BuildingSprite != null)
            BuildingSprite.SetActive(false);

        PieceActive(_leftPieces, true);
        PieceActive(_rightPieces, true);
        BreakNow();
    }
    private void BreakNow()
    {
        if (_isBroken) return;
        GameManager.Instance.ScoreIncreaseBuilding();

        _isBroken = true;

        Vector2 forceDirection = _baseDirection;
        if (_normalizeDirection && forceDirection.sqrMagnitude > 0f)
            forceDirection = forceDirection.normalized;

        ApplyForceToPiece(_leftPieces, -forceDirection);
        ApplyForceToPiece(_rightPieces, forceDirection);
    }

    private void PieceActive(Rigidbody2D[] pieces, bool active)
    {
        if (pieces == null) return;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            pieces[i].gameObject.SetActive(active);
        }
    }

    private void ApplyForceToPiece(Rigidbody2D[] pieces, Vector2 direction)
    {
        if (pieces == null) return;

        for (int i = 0; i < pieces.Length; i++)
        {
            Rigidbody2D piece = pieces[i];
            if (piece == null) continue;

            piece.simulated = true;
            piece.bodyType = RigidbodyType2D.Dynamic;
            piece.transform.SetParent(null, true);

            float forceMul = 1f + Random.Range(-_randomForce, _randomForce);
            piece.AddForce(direction * (_explodeForce * forceMul), ForceMode2D.Impulse);

            float torque = Random.Range(-_rotateForce, _rotateForce);
            piece.AddTorque(torque, ForceMode2D.Impulse);
        }
    }
}
