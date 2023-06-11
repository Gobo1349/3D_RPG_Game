using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// �� ������

public class BoxBar : BarBase
{
    // ������ ���� � ������� ����� ������ 
    public override void SetValue (float value, float maxValue)
    {
        Debug.Log("set health value, value: " + value + "max:  " + maxValue);
        base.SetValue(value, maxValue); // ���������� � �������� 
        // ����� ������ ��������� ������� �� ��� X, ��������� �� ������� 
        transform.localScale = new Vector3(transform.localScale.x * value / maxValue, transform.localScale.y, transform.localScale.z);
    }
}
