using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CellView : MonoBehaviour, IPointerDownHandler
{
    private UIToWorldWrapper ui;

    private void Start()
    {
        ui = GetComponentInParent<UIToWorldWrapper>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Click");
        ui.GetHandleObject(this);
    }
}
