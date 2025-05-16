using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<float> DamageGot;

    // Event function
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.TryGetComponent<Attacker>(out var attacker))
        {
            DamageGot?.Invoke(attacker.Damage);
        }
    }
}