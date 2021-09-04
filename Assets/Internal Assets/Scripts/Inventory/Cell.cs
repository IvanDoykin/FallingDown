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

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (Item == null || inventory.FindEmptyCell() == null || inventory.HandleObjectsOperator.PlayerHand.GetComponentInChildren<HandleObject>() != null)
    //        return;

    //    inventory.HandleObjectsOperator.CreateItemInHands(Item);
    //    RemoveItem();
    //}
}
