using System.Collections;
using UnityEngine;

public class Hotdog : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Collider2D _col;


    [SerializeField] Vector2 _spawnForce = new Vector2(0, 4f);
    [SerializeField] float _rotationalForce = 360f;

    [SerializeField] float _delayTime = .5f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        if (_rb == null)
        {
            Debug.Log("[Hotdog] : Rigidbody reference is null");
        }

        if (_col == null)
        {
            return;
        }
        _col.enabled = false;
    }

    void Start()
    {
        
        _rb.linearVelocity = (_spawnForce);
        _rb.angularVelocity = Random.Range(-_rotationalForce, _rotationalForce);
        StartCoroutine( EnableColliderAfter(_delayTime) );

        StartCoroutine (DestroyHotdogAfter( 15f ));
    }


    IEnumerator EnableColliderAfter(float duration)
    {
        
        yield return new WaitForSeconds(duration);

        _col.enabled = true;
    }

    IEnumerator DestroyHotdogAfter(float duration)
    {
        
        yield return new WaitForSeconds(duration);

        _col.enabled = true;
    }


    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);   
        }
    }

}
