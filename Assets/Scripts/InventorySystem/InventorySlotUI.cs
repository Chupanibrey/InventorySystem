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
    [SerializeField] private float appearDuration = 0.3f;
    [SerializeField] private float moveDuration = 0.4f;

    private Inventory inventory;
    private int slotIndexX, slotIndexY;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private Coroutine appearCoroutine;
    private Coroutine moveCoroutine;
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

    public Vector2 GetWorldPosition()
    {
        return rectTransform.position;
    }

    public void AnimateMoveFrom(Vector2 startPosition)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateMoveCoroutine(startPosition));
    }

    private IEnumerator AnimateMoveCoroutine(Vector2 startPosition)
    {
        Vector2 endPosition = rectTransform.position;

        itemIconImage.transform.position = startPosition;

        float time = 0;
        while (time < moveDuration)
        {
            itemIconImage.transform.position = Vector2.Lerp(startPosition, endPosition, time / moveDuration);
            time += Time.deltaTime;
            yield return null;
        }

        itemIconImage.transform.localPosition = Vector3.zero;
    }

    public void UpdateSlotDisplay(InventorySlot slot)
    {
        bool wasEmpty = currentSlot?.IsEmpty ?? true;
        bool isEmpty = slot.IsEmpty;

        currentSlot = slot;

        if (slot.IsEmpty)
        {
            if (!wasEmpty && isEmpty)
                StartDisappearAnimation();
            else
                ClearSlotVisuals();

            isEquipped = false;
        }
        else
        {
            if (wasEmpty && !isEmpty)
                StartAppearAnimation(slot);
            else if (!wasEmpty && !isEmpty)
                StartUpdateAnimation(slot);
            else
                UpdateVisuals(slot);
        }

        UpdateBackgroundColor();
    }

    private void StartAppearAnimation(InventorySlot slot)
    {
        if (appearCoroutine != null) StopCoroutine(appearCoroutine);
        appearCoroutine = StartCoroutine(AnimateAppear(slot));
    }

    private void StartUpdateAnimation(InventorySlot slot)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimatePulse(slot));
    }

    private void StartDisappearAnimation()
    {
        if (appearCoroutine != null) StopCoroutine(appearCoroutine);
        appearCoroutine = StartCoroutine(AnimateDisappear());
    }

    private IEnumerator AnimateAppear(InventorySlot slot)
    {
        UpdateVisuals(slot);

        itemIconImage.transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        float time = 0;
        while (time < appearDuration)
        {
            float progress = time / appearDuration;
            itemIconImage.transform.localScale = Vector3.one * progress;
            canvasGroup.alpha = progress;
            time += Time.deltaTime;
            yield return null;
        }

        itemIconImage.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    private IEnumerator AnimatePulse(InventorySlot slot)
    {
        UpdateVisuals(slot);

        Vector3 originalIconScale = itemIconImage.transform.localScale;
        Vector3 targetScale = originalIconScale * 1.2f;

        yield return AnimateIconScale(originalIconScale, targetScale, animationDuration / 2);
        yield return AnimateIconScale(targetScale, originalIconScale, animationDuration / 2);
    }

    private IEnumerator AnimateDisappear()
    {
        float time = 0;
        Vector3 startScale = itemIconImage.transform.localScale;
        float startAlpha = canvasGroup.alpha;

        while (time < animationDuration)
        {
            float progress = time / animationDuration;
            itemIconImage.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            time += Time.deltaTime;
            yield return null;
        }

        ClearSlotVisuals();
    }

    private IEnumerator AnimateIconScale(Vector3 from, Vector3 to, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            itemIconImage.transform.localScale = Vector3.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        itemIconImage.transform.localScale = to;
    }

    private void UpdateVisuals(InventorySlot slot)
    {
        itemIconImage.sprite = slot.ItemData.Icon;
        itemIconImage.enabled = true;
        stackCountText.text = slot.ItemCount > 1 ? slot.ItemCount.ToString() : "";
        isEquipped = slot.IsEquipped;

        itemIconImage.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        itemIconImage.transform.localPosition = Vector3.zero;
    }

    private void ClearSlotVisuals()
    {
        itemIconImage.sprite = null;
        itemIconImage.enabled = false;
        stackCountText.text = "";
        itemIconImage.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        itemIconImage.transform.localPosition = Vector3.zero;
    }

    private void UpdateBackgroundColor()
    {
        if (backgroundImage == null) return;

        if (currentSlot == null || currentSlot.IsEmpty)
        {
            backgroundImage.color = isPointerOver ? hoverColor : emptyColor;
            return;
        }

        backgroundImage.color = isPointerOver ?
            (isEquipped ? equippedHoverColor : hoverColor) :
            (isEquipped ? equippedColor : normalColor);
    }

    private void OnItemEquipped(BaseItemData itemData)
    {
        var slot = inventory.Slots[slotIndexX, slotIndexY];
        if (!slot.IsEmpty && slot.ItemData == itemData)
        {
            isEquipped = true;
            UpdateBackgroundColor();

            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateEquip());
        }
    }

    private IEnumerator AnimateEquip()
    {
        Vector3 originalScale = itemIconImage.transform.localScale;
        Vector3 equippedScale = originalScale * 1.1f;
        yield return AnimateIconScale(originalScale, equippedScale, animationDuration);
    }

    private void OnItemUnequipped(BaseItemData itemData)
    {
        var slot = inventory.Slots[slotIndexX, slotIndexY];
        if (!slot.IsEmpty && slot.ItemData == itemData)
        {
            isEquipped = false;
            UpdateBackgroundColor();

            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateUnequip());
        }
    }

    private IEnumerator AnimateUnequip()
    {
        Vector3 currentScale = itemIconImage.transform.localScale;
        yield return AnimateIconScale(currentScale, Vector3.one, animationDuration);
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
        return new Vector2(rectTransform.position.x + slotRect.width, rectTransform.position.y);
    }

    private void ScaleElement(float targetScale)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
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
            StartCoroutine(AnimateUse());
            inventory.UseItem(slotIndexX, slotIndexY);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventory.RemoveItem(slotIndexX, slotIndexY, 1);
        }

        TooltipManager.Instance.HideTooltip();
    }

    private IEnumerator AnimateUse()
    {
        Vector3 originalScale = itemIconImage.transform.localScale;
        Vector3 useScale = originalScale * 0.8f;

        yield return AnimateIconScale(originalScale, useScale, animationDuration / 2);
        yield return AnimateIconScale(useScale, originalScale, animationDuration / 2);
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

        if (appearCoroutine != null)
            StopCoroutine(appearCoroutine);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

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