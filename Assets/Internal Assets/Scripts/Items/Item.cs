using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Item : ScriptableObject
{
    [SerializeField] private Sprite icon;
    [SerializeField] private int id;
    [SerializeField] private GameObject itemPrefab;

    public Sprite Icon
    {
        get
        {
            return icon;
        }
    }

    public int Id
    {
        get
        {
            return id;
        }
    }

    public GameObject ItemPrefab
    {
        get
        {
            return itemPrefab;
        }
    }
}
