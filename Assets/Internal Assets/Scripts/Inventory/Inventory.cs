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

    public void AddItem(GameObject newItem)
    {
        Item item = newItem.GetComponent<HandleObject>().HandleItem;
        Cell cell = FindEmptyCell();

        if (cell != null && item != null)
        {
            cell.SetItem(item);
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
