using UnityEngine;
using Mirror;

public abstract class HandleObject : NetworkBehaviour
{
    [SyncVar]
    public Transform Parent;

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (Parent == null)
            return;

        transform.SetParent(Parent);
        transform.localPosition = Vector3.zero;
    }
}
