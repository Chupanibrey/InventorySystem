using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [SerializeField] private GameObject tooltipPrefab;

    private GameObject tooltipInstance;
    private TextMeshProUGUI itemNameText, itemTypeText, itemDescriptionText;
    private RectTransform rectTransform;
    private Canvas tooltipCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTooltip();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTooltip()
    {
        if (tooltipPrefab == null)
        {
            Debug.LogError("TooltipPrefab не назначен!");
            return;
        }

        CreateTooltipCanvas();
        tooltipInstance = Instantiate(tooltipPrefab, tooltipCanvas.transform);
        FindTextComponents();

        rectTransform = tooltipInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0, 1);
        }

        tooltipInstance.SetActive(false);
    }

    private void CreateTooltipCanvas()
    {
        GameObject canvasObject = new GameObject("TooltipCanvas");
        tooltipCanvas = canvasObject.AddComponent<Canvas>();
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tooltipCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObject.transform.SetParent(transform);
    }

    private void FindTextComponents()
    {
        if (tooltipInstance == null) return;

        itemNameText = FindTextComponent("ItemNameText");
        itemTypeText = FindTextComponent("ItemTypeText");
        itemDescriptionText = FindTextComponent("ItemDescriptionText");

        if (itemNameText == null || itemTypeText == null || itemDescriptionText == null)
            Debug.LogError("Не все текстовые компоненты найдены!");
    }

    private TextMeshProUGUI FindTextComponent(string name)
    {
        foreach (Transform child in tooltipInstance.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child.GetComponent<TextMeshProUGUI>();
        }
        return null;
    }

    public void ShowTooltip(BaseItemData itemData, Vector2 screenPosition)
    {
        if (itemData == null || tooltipInstance == null) return;

        itemNameText.text = itemData.ItemName;
        itemTypeText.text = GetTypeText(itemData);
        itemDescriptionText.text = itemData.Description;

        UpdatePosition(screenPosition);
        tooltipInstance.SetActive(true);
    }

    private string GetTypeText(BaseItemData itemData)
    {
        string typeText = itemData.Type switch
        {
            ItemType.Weapon => "Оружие",
            ItemType.Potion => "Зелье",
            ItemType.QuestItem => "Квестовый предмет",
            ItemType.Resource => "Ресурс",
            ItemType.Armor => "Броня",
            ItemType.Accessory => "Аксессуар",
            _ => "Предмет"
        };

        if (itemData.IsEquippable && itemData.EquipmentSlot != EquipmentSlot.None)
            typeText += $" ({GetEquipmentSlotText(itemData.EquipmentSlot)})";

        return typeText;
    }

    private string GetEquipmentSlotText(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Head => "Голова",
            EquipmentSlot.Chest => "Грудь",
            EquipmentSlot.Hands => "Руки",
            EquipmentSlot.Legs => "Ноги",
            EquipmentSlot.Feet => "Ступни",
            EquipmentSlot.Weapon => "Оружие",
            EquipmentSlot.Shield => "Щит",
            EquipmentSlot.Accessory => "Аксессуар",
            _ => "Экипировка"
        };
    }

    public void HideTooltip()
    {
        if (tooltipInstance != null)
            tooltipInstance.SetActive(false);
    }

    private void UpdatePosition(Vector2 screenPosition)
    {
        if (rectTransform == null) return;

        Vector2 tooltipPosition = screenPosition + new Vector2(20, -20);
        rectTransform.position = tooltipPosition;
        EnsureTooltipStaysOnScreen();
    }

    private void EnsureTooltipStaysOnScreen()
    {
        if (rectTransform == null) return;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float width = corners[2].x - corners[0].x;
        float height = corners[1].y - corners[0].y;
        Vector3 position = rectTransform.position;

        if (position.x + width > Screen.width) position.x = Screen.width - width;
        if (position.x < 0) position.x = 0;
        if (position.y - height < 0) position.y = height;

        rectTransform.position = position;
    }

    private void Update()
    {
        if (tooltipInstance != null && tooltipInstance.activeInHierarchy)
            UpdatePosition(Mouse.current.position.ReadValue());
    }

    private void OnDestroy()
    {
        if (tooltipInstance != null)
            Destroy(tooltipInstance);
        if (tooltipCanvas != null)
            Destroy(tooltipCanvas.gameObject);
    }
}