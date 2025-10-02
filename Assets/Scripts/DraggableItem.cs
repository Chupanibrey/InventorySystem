using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image;
    [SerializeField] private CanvasGroup canvasGroup;

    private GameObject dragObject;
    private Canvas canvas;
    private InventorySlotUI originalSlot;
    private bool isDragging = false;

    private void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
        originalSlot = GetComponentInParent<InventorySlotUI>();
    }

    private void Start() => CleanupDragObject();

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (originalSlot == null || !image.enabled) return;

        isDragging = true;
        CreateDragObject();

        image.raycastTarget = false;
        if (canvasGroup != null) canvasGroup.alpha = 0.3f;
    }

    private void CreateDragObject()
    {
        dragObject = new GameObject("Drag Icon");
        dragObject.transform.SetParent(canvas.transform, false);
        dragObject.transform.SetAsLastSibling();

        Image dragImage = dragObject.AddComponent<Image>();
        dragImage.sprite = image.sprite;
        dragImage.raycastTarget = false;
        dragImage.preserveAspect = true;

        CanvasGroup dragCanvasGroup = dragObject.AddComponent<CanvasGroup>();
        dragCanvasGroup.alpha = 0.8f;
        dragCanvasGroup.blocksRaycasts = false;

        RectTransform rectTransform = dragObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;
        dragObject.transform.position = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
            dragObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        RestoreOriginalAppearance();
        CleanupDragObject();
        originalSlot?.UpdateSlotDisplay();
    }

    private void RestoreOriginalAppearance()
    {
        image.raycastTarget = true;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private void CleanupDragObject()
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }
    }

    private void OnDisable()
    {
        if (isDragging)
        {
            RestoreOriginalAppearance();
            CleanupDragObject();
            isDragging = false;
        }
    }

    private void OnDestroy() => CleanupDragObject();
}