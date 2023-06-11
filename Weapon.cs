using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // количество урона, который наносит объект 
    [SerializeField]
    private float _damage;

    // получаем доступ извне, чтобы узнать значение 
    public float GetDamage // свойство 
    {
        get { return _damage; }
    }
}
