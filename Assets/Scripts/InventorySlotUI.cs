using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI stackCountText;
    [SerializeField] private Button slotButton; // Кнопка если понадобится

    private Inventory inventory;
    private int slotIndexX;
    private int slotIndexY;

    public void Init(Inventory inventory, int x, int y)
    {
        this.inventory = inventory;
        slotIndexX = x;
        slotIndexY = y;
    }

    public void UpdateSlotUI(InventorySlot slot)
    {
        if (slot != null && !slot.IsEmpty)
        {
            itemIconImage.sprite = slot.ItemData.Icon;
            itemIconImage.enabled = true;
            stackCountText.text = slot.ItemCount > 1 ? slot.ItemCount.ToString() : "";
        }
        else
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
            stackCountText.text = "";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnUseButton()
    {

    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggableItem != null)
        {
            InventorySlotUI fromSlot = draggableItem.parentAfterDrag.GetComponent<InventorySlotUI>();
            if (fromSlot != null)
            {
                inventory.MoveItem(fromSlot.slotIndexX, fromSlot.slotIndexY, slotIndexX, slotIndexY);
            }
        }
    }
}