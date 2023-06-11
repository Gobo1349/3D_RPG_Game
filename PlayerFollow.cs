using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ����� ������ ��������� �� �������
public class PlayerFollow : MonoBehaviour
{
    [SerializeField]
    private Transform _playerTransform; // ��������� transform �������� ���������
    private Vector3 _cameraOffset; // ���������� ����� ������� � ������� 
    [Range(0.01F, 1.0F)]
    public float smoothFactor = 0.9F;
    public bool lookAtPlayer = true; 

    void Start()
    {
        // �������� ���������� ����� ������� � �������
        _cameraOffset = transform.position - _playerTransform.position; 
    }

    void LateUpdate() // ���������� ���� ��� � �����, ����� ���������� Update() - ��� ������ �� 3��� ����
    {
        // ������ ����� ������� ������, ������ �� ������� � ������ ��������� ����� ���� 
        Vector3 newPos = _playerTransform.position + _cameraOffset;
        transform.position = Vector3.Slerp(transform.position, newPos, smoothFactor); // ������� ������ �� ����� ������� � ������� Slerp - ����������� - ������� �������� 
        if (lookAtPlayer)
        {
            transform.LookAt(_playerTransform); // LookAt - Rotates the transform so the forward vector points at /target/'s current position.
        }
    }
}
