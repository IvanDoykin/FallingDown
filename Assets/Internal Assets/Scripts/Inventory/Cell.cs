using UnityEngine;

public class Cell
{
    public Item Item { get; private set; }

    public bool IsEmpty
    {
        get
        {
            return (Item == null);
        }
    }

    private void Start()
    {
        Item = null;
    }

    public void SetItem(Item addedItem)
    {
        Item = addedItem;
    }

    public void RemoveItem()
    {
        Item = null;
    }
}
