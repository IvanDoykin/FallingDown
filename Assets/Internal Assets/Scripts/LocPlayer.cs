using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocPlayer : MonoBehaviour
{
    private Inventory inventory;

    private void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleObject handleObject = other.GetComponent<HandleObject>();
        if (handleObject != null)
        {
            inventory.AddItem(handleObject.gameObject);
        }
        else
        {
            Debug.Log("not null handleObject");
        }
    }
}
