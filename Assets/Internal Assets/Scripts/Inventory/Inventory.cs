using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Inventory
{
    private int size = 4;
    private List<Cell> cells = new List<Cell>();
    public List<Cell> Cells
    {
        get
        {
            return cells;
        }
    }

    public Inventory()
    {
        for (int i = 0; i < size; i++)
        {
            cells.Add(new Cell());
        }
    }

    //private void Start()
    //{
    //    Cell[] tempCells = GetComponentsInChildren<Cell>();
    //    foreach (var cell in tempCells)
    //    {
    //        cells.Add(cell);
    //    }
    //}

    public void AddItem(GameObject newItem)
    {
        Item item = newItem.GetComponent<HandleObject>().HandleItem;
        Cell cell = FindEmptyCell();

        if (cell != null && item != null)
        {
            cell.SetItem(item);
            //Destroy(newItem);
        }
    }

    public Cell FindEmptyCell()
    {
        foreach (var cell in Cells)
        {
            if (cell.IsEmpty)
                return cell;
        }

        return null;
    }
}
