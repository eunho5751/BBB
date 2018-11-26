using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1)]
public class WeaponInventory : SerializedMonoBehaviour
{
    [ShowInInspector, HideInEditorMode, DisableInPlayMode, DisableContextMenu]
    private List<Weapon> _weapons = new List<Weapon>();
    
    private event Action<Weapon, Weapon> _onWeaponSwapped;
    private Dictionary<string, Transform> _socketMap = new Dictionary<string, Transform>();

    protected virtual void Awake()
    {
        AddSockets();
    }

    protected virtual void Start()
    {
        // Add all weapons.
        foreach (var weapon in GetComponentsInChildren<Weapon>())
        {
            AddWeapon(weapon);
        }

        if (_weapons.Count > 0)
        {
            foreach (var weapon in _weapons)
                weapon.Owner = gameObject;
            Swap(0);
        }
    }

    private void AddSockets()
    {
        foreach (var socket in GetComponentsInChildren<WeaponSocket>())
        {
            if (!string.IsNullOrEmpty(socket.SocketName))
                _socketMap.Add(socket.SocketName, socket.transform);
        }
    }

    public void Swap(int weaponIndex)
    {
        if (weaponIndex < _weapons.Count)
        {
            Weapon from = CurrentWeapon;
            Weapon to = _weapons[weaponIndex];
            from?.gameObject.SetActive(false);
            to.gameObject.SetActive(true);

            CurrentWeapon = to;
            _onWeaponSwapped?.Invoke(from, to);
        }
    }

    public void AddWeapon(Weapon weapon)
    {
        Transform socket = null;
        if (_socketMap.TryGetValue(weapon.SocketName, out socket))
        {
            weapon.transform.SetParent(socket.transform, false);
            _weapons.Add(weapon);
        }
    }

    public void RemoveWeapon(Weapon weapon)
    {
        if (weapon.Droppable)
        {
            weapon.Owner = null;
            weapon.transform.parent = null;
            _weapons.Remove(weapon);
        }
    }

    public event Action<Weapon, Weapon> OnWeaponSwapped
    {
        add { _onWeaponSwapped += value; }
        remove { _onWeaponSwapped -= value; }
    }

    public Weapon CurrentWeapon { get; private set; }
}

