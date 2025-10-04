using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image;
    [SerializeField] private CanvasGroup canvasGroup;

    private GameObject dragObject;
    private Canvas canvas;
    private InventorySlotUI originalSlot;
    private bool isDragging;
    private Coroutine returnCoroutine;

    private const float PICKUP_DURATION = 0.15f;
    private const float DRAG_APPEAR_DURATION = 0.2f;
    private const float RETURN_DURATION = 0.2f;
    private const float DRAG_DISAPPEAR_DURATION = 0.15f;

    private void Awake()
    {
        image = image ?? GetComponent<Image>();
        canvasGroup = canvasGroup ?? GetComponent<CanvasGroup>();
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
        canvasGroup.alpha = 0.3f;

        StartCoroutine(AnimatePickup());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            Vector3 targetPosition = eventData.position;
            dragObject.transform.position = Vector3.Lerp(dragObject.transform.position, targetPosition, Time.deltaTime * 15f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        StartCoroutine(AnimateDropReturn());
        RestoreOriginalAppearance();
        CleanupDragObject();
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

        StartCoroutine(AnimateDragAppear(rectTransform));
    }

    private IEnumerator AnimatePickup()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 dragScale = originalScale * 1.1f;

        yield return AnimateScale(transform, originalScale, dragScale, PICKUP_DURATION);
    }

    private IEnumerator AnimateDragAppear(RectTransform rectTransform)
    {
        rectTransform.position = transform.position;
        rectTransform.localScale = Vector3.zero;

        yield return AnimateScale(rectTransform, Vector3.zero, Vector3.one, DRAG_APPEAR_DURATION);
    }

    private IEnumerator AnimateDropReturn()
    {
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(AnimateReturnToOriginal());
        yield return returnCoroutine;
    }

    private IEnumerator AnimateReturnToOriginal()
    {
        yield return AnimateScale(transform, transform.localScale, Vector3.one, RETURN_DURATION);
    }

    private IEnumerator AnimateScale(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            target.localScale = Vector3.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        target.localScale = to;
    }

    private void RestoreOriginalAppearance()
    {
        image.raycastTarget = true;
        canvasGroup.alpha = 1f;
    }

    private void CleanupDragObject()
    {
        if (dragObject != null)
        {
            StartCoroutine(AnimateDragDisappear());
        }
    }

    private IEnumerator AnimateDragDisappear()
    {
        if (dragObject == null) yield break;

        RectTransform rectTransform = dragObject.GetComponent<RectTransform>();
        Vector3 startScale = rectTransform.localScale;

        float time = 0;
        while (time < DRAG_DISAPPEAR_DURATION)
        {
            if (dragObject != null)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, time / DRAG_DISAPPEAR_DURATION);
            }
            time += Time.deltaTime;
            yield return null;
        }

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

    private void OnDestroy()
    {
        CleanupDragObject();
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
    }
}