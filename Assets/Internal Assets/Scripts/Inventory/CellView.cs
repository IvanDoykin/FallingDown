using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CellView : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private TextMeshProUGUI amountsText;
    private UIToWorldWrapper ui;

    private int amounts = 0;
    public int Amounts
    {
        get
        {
            return amounts;
        }
    }

    public void Add(int value = 1)
    {
        amounts += value;
        amountsText.enabled = amounts <= 0;

        if (amounts <= 0)
            amounts = 0;

        amountsText.text = amounts.ToString();
    }

    public void Remove()
    {
        Add(-1);
    }

    private void Start()
    {
        ui = GetComponentInParent<UIToWorldWrapper>();
        amountsText.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Click");
        ui.GetHandleObject(this);
    }
}
