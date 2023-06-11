using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// ХП игрока 

public class ImageBar : BarBase
{
    [SerializeField]
    private Image _image;
    // задаем макс и текущее колво жизней 
    public override void SetValue(float value, float maxValue)
    {
        //base.SetValue(value, maxValue); // обращаемся к родителю 

        _image.fillAmount = value / maxValue; 
    }
}

