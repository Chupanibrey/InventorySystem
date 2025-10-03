using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IPointerClickHandler
{
    [Header("Ссылки")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI stackCountText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Цвета")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color equippedColor = new Color(0.2f, 0.8f, 0.2f, 0.6f);
    [SerializeField] private Color equippedHoverColor = new Color(0.3f, 0.9f, 0.3f, 0.8f);

    [Header("Анимация")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationDuration = 0.2f;

    private Inventory inventory;
    private int slotIndexX, slotIndexY;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private bool isEquipped;
    private bool isPointerOver;
    private InventorySlot currentSlot;

    public void Init(Inventory inventory, int x, int y)
    {
        this.inventory = inventory;
        slotIndexX = x;
        slotIndexY = y;
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        inventory.OnItemEquipped += OnItemEquipped;
        inventory.OnItemUnequipped += OnItemUnequipped;

        if (itemIconImage.GetComponent<DraggableItem>() == null)
            itemIconImage.gameObject.AddComponent<DraggableItem>();
    }

    public void UpdateSlotDisplay(InventorySlot slot)
    {
        currentSlot = slot;

        if (slot.IsEmpty)
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
            stackCountText.text = "";
            isEquipped = false;
        }
        else
        {
            itemIconImage.sprite = slot.ItemData.Icon;
            itemIconImage.enabled = true;
            stackCountText.text = slot.ItemCount > 1 ? slot.ItemCount.ToString() : "";
            isEquipped = slot.IsEquipped;
        }

        UpdateBackgroundColor();
    }

    private void UpdateBackgroundColor()
    {
        if (backgroundImage == null) return;

        if (currentSlot == null || currentSlot.IsEmpty)
        {
            backgroundImage.color = isPointerOver ? hoverColor : emptyColor;
            return;
        }

        if (isPointerOver)
        {
            backgroundImage.color = isEquipped ? equippedHoverColor : hoverColor;
        }
        else
        {
            backgroundImage.color = isEquipped ? equippedColor : normalColor;
        }
    }

    private void OnItemEquipped(BaseItemData itemData)
    {
        var slot = inventory.Slots[slotIndexX, slotIndexY];
        if (!slot.IsEmpty && slot.ItemData == itemData)
        {
            isEquipped = true;
            UpdateBackgroundColor();
        }
    }

    private void OnItemUnequipped(BaseItemData itemData)
    {
        var slot = inventory.Slots[slotIndexX, slotIndexY];
        if (!slot.IsEmpty && slot.ItemData == itemData)
        {
            isEquipped = false;
            UpdateBackgroundColor();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        UpdateBackgroundColor();
        ScaleElement(hoverScale);

        var slot = inventory.Slots[slotIndexX, slotIndexY];
        if (!slot.IsEmpty)
            TooltipManager.Instance.ShowTooltip(slot.ItemData, GetTooltipPosition());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateBackgroundColor();
        ScaleElement(1f);
        TooltipManager.Instance.HideTooltip();
    }

    private Vector2 GetTooltipPosition()
    {
        var slotRect = rectTransform.rect;
        var position = rectTransform.position;
        return new Vector2(position.x + slotRect.width, position.y);
    }

    private void ScaleElement(float targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
    }

    private IEnumerator AnimateScale(float targetScale)
    {
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float time = 0;

        while (time < animationDuration)
        {
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = endScale;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedItem = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (droppedItem == null) return;

        var fromSlot = droppedItem.GetComponentInParent<InventorySlotUI>();
        if (fromSlot != null)
            inventory.MoveItem(fromSlot.slotIndexX, fromSlot.slotIndexY, slotIndexX, slotIndexY);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var slot = inventory.Slots[slotIndexX, slotIndexY];
        if (slot.IsEmpty) return;

        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            inventory.UseItem(slotIndexX, slotIndexY);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventory.RemoveItem(slotIndexX, slotIndexY, 1);
        }

        TooltipManager.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        isPointerOver = false;
        TooltipManager.Instance?.HideTooltip();

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            rectTransform.localScale = originalScale;
        }

        UpdateBackgroundColor();
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnItemEquipped -= OnItemEquipped;
            inventory.OnItemUnequipped -= OnItemUnequipped;
        }
    }
}