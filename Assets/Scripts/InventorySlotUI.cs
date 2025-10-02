using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI stackCountText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    private Inventory inventory;
    private int slotIndexX;
    private int slotIndexY;
    private bool isPointerOver = false;
    private DraggableItem draggableItem;

    public void Init(Inventory inventory, int x, int y)
    {
        this.inventory = inventory;
        slotIndexX = x;
        slotIndexY = y;

        draggableItem = itemIconImage.GetComponent<DraggableItem>();
        if (draggableItem == null)
            draggableItem = itemIconImage.gameObject.AddComponent<DraggableItem>();
    }

    public void UpdateSlotUI(InventorySlot slot) => UpdateSlotDisplay();

    public void UpdateSlotDisplay()
    {
        var slot = inventory.Slots[slotIndexX, slotIndexY];

        if (slot != null && !slot.IsEmpty)
        {
            itemIconImage.sprite = slot.ItemData.Icon;
            itemIconImage.enabled = true;
            stackCountText.text = slot.ItemCount > 1 ? slot.ItemCount.ToString() : "";

            if (draggableItem != null)
            {
                draggableItem.enabled = true;
                RestoreIconAppearance();
            }
        }
        else
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
            stackCountText.text = "";

            if (draggableItem != null)
                draggableItem.enabled = false;
        }
    }

    private void RestoreIconAppearance()
    {
        if (itemIconImage != null)
        {
            itemIconImage.raycastTarget = true;

            CanvasGroup canvasGroup = itemIconImage.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            Color color = itemIconImage.color;
            color.a = 1f;
            itemIconImage.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        if (backgroundImage != null) backgroundImage.color = hoverColor;

        if (!inventory.Slots[slotIndexX, slotIndexY].IsEmpty)
        {
            var slot = inventory.Slots[slotIndexX, slotIndexY];
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        if (backgroundImage != null) backgroundImage.color = normalColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem droppedItem = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (droppedItem != null)
        {
            InventorySlotUI fromSlot = droppedItem.GetComponentInParent<InventorySlotUI>();
            if (fromSlot != null && fromSlot != this)
            {
                inventory.MoveItem(fromSlot.slotIndexX, fromSlot.slotIndexY, slotIndexX, slotIndexY);
            }
            else if (fromSlot == this)
            {
                UpdateSlotDisplay();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var slot = inventory.Slots[slotIndexX, slotIndexY];
        if (slot.IsEmpty)
            return;

        // Двойной клик - использование
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            Debug.Log($"Двойной клик: использование предмета '{slot.ItemData.ItemName}' из слота [{slotIndexX}, {slotIndexY}]");
            inventory.UseItem(slotIndexX, slotIndexY);
        }
        // Правый клик - удаление
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"Правый клик: удаление предмета '{slot.ItemData.ItemName}' из слота [{slotIndexX}, {slotIndexY}]");
            inventory.RemoveItem(slotIndexX, slotIndexY, 1);
        }
    }

    private void OnDisable()
    {
        if (isPointerOver)
        {
            isPointerOver = false;
        }
    }
}