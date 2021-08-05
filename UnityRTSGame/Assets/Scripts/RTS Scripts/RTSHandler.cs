using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSHandler : MonoBehaviour
{
    public List<Unit> units = new List<Unit>();

    public List<Unit> GetMyUnits()
    {
        return units;
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
    }

}
