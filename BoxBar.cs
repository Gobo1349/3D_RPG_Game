using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ’ѕ врагов

public class BoxBar : BarBase
{
    // задаем макс и текущее колво жизней 
    public override void SetValue (float value, float maxValue)
    {
        Debug.Log("set health value, value: " + value + "max:  " + maxValue);
        base.SetValue(value, maxValue); // обращаемс€ к родителю 
        // будем мен€ть локальные размеры по оси X, остальные не трогаем 
        transform.localScale = new Vector3(transform.localScale.x * value / maxValue, transform.localScale.y, transform.localScale.z);
    }
}
