using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Inventory inventory;

    private void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("kek");
        HandleObject handleObject = other.GetComponent<HandleObject>();
        if (handleObject != null)
        {
            inventory.AddItem(handleObject.gameObject);
        }
        else
        {
            Debug.Log("neuda4a");
        }
    }
}
