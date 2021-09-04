using UnityEngine;
using Mirror;

public abstract class HandleObject : NetworkBehaviour
{
    [SerializeField] private Item handleItem;
    public Item HandleItem
    {
        get
        {
            return handleItem;
        }

        set
        {
            if (value != null)
                handleItem = value;
        }
    }
}
