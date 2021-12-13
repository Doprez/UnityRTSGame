using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInventory : MonoBehaviour
{
    public List<ItemObject> Inventory;

    public void AddItemToInventory()
    {
        Debug.Log("Item Added");
    }
}
