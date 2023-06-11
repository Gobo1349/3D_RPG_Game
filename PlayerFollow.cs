using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// чтобы камера следовала за игроком
public class PlayerFollow : MonoBehaviour
{
    [SerializeField]
    private Transform _playerTransform; // компонент transform главного персонажа
    private Vector3 _cameraOffset; // расстояние между камерой и игроком 
    [Range(0.01F, 1.0F)]
    public float smoothFactor = 0.9F;
    public bool lookAtPlayer = true; 

    void Start()
    {
        // замеряем расстояние между камерой и игроком
        _cameraOffset = transform.position - _playerTransform.position; 
    }

    void LateUpdate() // вызывается один раз в кадре, после завершения Update() - для камеры от 3его лица
    {
        // задаем новую позицию камеры, идущей за игроком с учетом дистанции между ними 
        Vector3 newPos = _playerTransform.position + _cameraOffset;
        transform.position = Vector3.Slerp(transform.position, newPos, smoothFactor); // смещаем камеру на новую позицию с помощью Slerp - сглаживание - плавное движение 
        if (lookAtPlayer)
        {
            transform.LookAt(_playerTransform); // LookAt - Rotates the transform so the forward vector points at /target/'s current position.
        }
    }
}
