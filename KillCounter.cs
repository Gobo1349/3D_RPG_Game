using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class KillCounter : MonoBehaviour
{
    private int _killCount; // счетчик убийств
    private Text _countText; // ссылка на объект интерфейса

    private void Awake()
    {
        _countText = GetComponent<Text>();

        _countText.text = _killCount.ToString();
    }

    // событие - если враг умер 
    private void OnEnemyDeath()
    {
        _killCount++;

        _countText.text = _killCount.ToString();
    }

    // при включении и выключении объекта 
    // https://docs.microsoft.com/ru-ru/dotnet/csharp/programming-guide/events/how-to-subscribe-to-and-unsubscribe-from-events
    private void OnEnable() // сразу в начале игры объект включается - подписывается на событие смерти монстра
    {
        Enemy.OnDeath += OnEnemyDeath; // OnEnemyDeath - обработчик события. Подписываемся методом на Enemy.OnDeath
    }

    private void OnDisable()  // аналогично
    {
        Enemy.OnDeath -= OnEnemyDeath;
    }
}
