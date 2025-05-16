using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.GPUSort;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float _maxHp;

    [SerializeField]
    private UnityEvent Die;

    [SerializeField]
    private UnityEvent<float> HpChanged;

    [SerializeField]
    private UnityEvent<float> HpChangedPercent;

    private float _hp;

    public float HP
    {
        get => _hp;
        set
        {
            _hp = value;
            HpChanged?.Invoke(_hp);

            HpChangedPercent?.Invoke(_hp / _maxHp);
            if (_hp <= 0)
                Die?.Invoke();
        }
    }


    private void Start()
    {
        Init();
    }

    public void Init()
    {
        HP = _maxHp;
    }

    public void GetDamage(float damage)
    {
        HP -= damage;
    }

    public void AddHealth(float hp)
    {
        HP += hp;
    }
}