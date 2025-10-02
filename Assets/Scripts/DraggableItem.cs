using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image;
    [SerializeField] private CanvasGroup canvasGroup;
    [HideInInspector] public Transform parentAfterDrag;

    private void Start()
    {
        if (image == null) image = GetComponent<Image>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;

        transform.SetParent(transform.root.GetChild(0));

        image.raycastTarget = false;
        if (canvasGroup != null) canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;
    }
}