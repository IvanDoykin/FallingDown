using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIToWorldWrapper : NetworkBehaviour
{
    [SerializeField] private Sprite defaultItemSprite;
    [SerializeField] private GameObject cellViewPrefab;
    [SerializeField] private Transform panel;

    public PlayerInput Player { get; set; }
    private CellView[] cellViews = new CellView[LevelNetwork.InventorySize];

    [Client]
    private void Start()
    {
        GetComponentInChildren<RectTransform>(true).gameObject.SetActive(true);
        CreateInventoryCells();
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
        if (itemIconId == 0)
        {
            cellViews[cellIndex].GetComponent<Image>().sprite = defaultItemSprite;
            return;
        }

        Debug.Log("Resources/" + "/UI/" + itemIconId);
        cellViews[cellIndex].GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/" + itemIconId);
    }

    [Client]
    public void GetHandleObject(CellView cell)
    {
        int cellIndex = Array.IndexOf(cellViews, cell);
        Debug.Log("Clicked cell #" + cellIndex);
        Player.CmdGetHandleObjectFromServer(cellIndex);
        Player.CmdSetActiveCell(cellIndex);
    }
}
