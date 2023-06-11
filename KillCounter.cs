using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class KillCounter : MonoBehaviour
{
    private int _killCount; // ������� �������
    private Text _countText; // ������ �� ������ ����������

    private void Awake()
    {
        _countText = GetComponent<Text>();

        _countText.text = _killCount.ToString();
    }

    // ������� - ���� ���� ���� 
    private void OnEnemyDeath()
    {
        _killCount++;

        _countText.text = _killCount.ToString();
    }

    // ��� ��������� � ���������� ������� 
    // https://docs.microsoft.com/ru-ru/dotnet/csharp/programming-guide/events/how-to-subscribe-to-and-unsubscribe-from-events
    private void OnEnable() // ����� � ������ ���� ������ ���������� - ������������� �� ������� ������ �������
    {
        Enemy.OnDeath += OnEnemyDeath; // OnEnemyDeath - ���������� �������. ������������� ������� �� Enemy.OnDeath
    }

    private void OnDisable()  // ����������
    {
        Enemy.OnDeath -= OnEnemyDeath;
    }
}
