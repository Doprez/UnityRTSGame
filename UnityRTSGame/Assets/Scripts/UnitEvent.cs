using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEvent : MonoBehaviour
{
    public static UnitEvent current;

    private void Awake()
    {
        current = this;
    }

    public event Action OnSelected;

    public void UnitSelected()
    {
        if (OnSelected == null)
        {
            OnSelected();
        }
    }
    
}
