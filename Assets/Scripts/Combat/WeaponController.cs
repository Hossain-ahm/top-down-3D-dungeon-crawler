using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType { Ranged, Melee }

[RequireComponent(typeof(RangedAttack))]
[RequireComponent(typeof(MeleeAttack))]
public class WeaponController : MonoBehaviour
{
    [Header("State")]
    public WeaponType currentWeapon = WeaponType.Ranged;

    private RangedAttack _ranged;
    private MeleeAttack _melee;

    private void Awake()
    {
        _ranged = GetComponent<RangedAttack>();
        _melee = GetComponent<MeleeAttack>();
    }

    // Bound to "Attack" action (left click)
    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentWeapon == WeaponType.Ranged)
            _ranged.Fire();
        else
            _melee.Swing();
    }

    // Bound to "Next" action (key '2' by default)
    public void OnNext(InputValue value)
    {
        if (!value.isPressed) return;

        currentWeapon = currentWeapon == WeaponType.Ranged ? WeaponType.Melee : WeaponType.Ranged;
        Debug.Log($"Switched to {currentWeapon}");
    }
}