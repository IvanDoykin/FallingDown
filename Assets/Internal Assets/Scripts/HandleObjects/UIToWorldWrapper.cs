using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIToWorldWrapper : NetworkBehaviour
{
    private const int defaultSpriteId = 0;

    [SerializeField] private Sprite defaultItemSprite;
    [SerializeField] private GameObject cellViewPrefab;
    [SerializeField] private Transform panel;
    [SerializeField] private TextMeshProUGUI health;

    public Player Player { get; set; }
    private CellView[] cellViews = new CellView[LevelNetwork.InventorySize];

    [Client]
    private void Start()
    {
        GetComponentInChildren<RectTransform>(true).gameObject.SetActive(true);
        CreateInventoryCells();
    }

    [Client]
    public void ChangeHealth(int value)
    {
        if (value < 0)
            value = 0;

        health.text = value.ToString();
    }

    [Client]
    private void CreateInventoryCells()
    {
        Debug.Log("startCreate");

        for (int i = 0; i < LevelNetwork.InventorySize; i++)
        {
            GameObject cellView = Instantiate(cellViewPrefab, panel);
            CellView tempCellView = cellView.GetComponent<CellView>();

            cellViews[i] = tempCellView;
            cellView.GetComponent<Image>().sprite = defaultItemSprite;
        }
    }

    [Client]
    public void SetSpriteToCell(int cellIndex, int itemIconId)
    {
        if (itemIconId == defaultSpriteId)
        {
            cellViews[cellIndex].GetComponent<Image>().sprite = defaultItemSprite;
            return;
        }

        Debug.Log("Resources/" + "/UI/" + itemIconId);
        cellViews[cellIndex].GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/" + itemIconId);
    }

    [Client]
    public void AddItem(int cellIndex)
    {
        cellViews[cellIndex].Add();
    }

    [Client]
    public void RemoveItem(int cellIndex)
    {
        cellViews[cellIndex].Remove();
        if (GetCellAmounts(cellIndex) == 0)
            SetSpriteToCell(cellIndex, defaultSpriteId);
    }

    [Client]
    public int GetCellAmounts(int cellIndex)
    {
        return cellViews[cellIndex].Amounts;
    }

    [Client]
    public void GetHandleObject(CellView cell)
    {
        int cellIndex = Array.IndexOf(cellViews, cell);
        Debug.Log("Clicked cell #" + cellIndex);
        Player.CmdGetHandleObjectFromServer(cellIndex);
    }
}
