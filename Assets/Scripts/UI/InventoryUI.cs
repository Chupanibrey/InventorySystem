using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform inventoryGridParent;

    [Header("Кнопки сортировки")]
    [SerializeField] private Button sortByTypeButton;
    [SerializeField] private Button sortByNameButton;

    private InventorySlotUI[,] slotUIs;
    private Dictionary<Vector2Int, InventorySlotUI> slotPositions;

    private void Start()
    {
        InitializeUI();
        inventory.OnInventoryChanged += RefreshUI;
        inventory.OnItemMoved += OnItemMoved; // Подписываемся на событие перемещения

        if (sortByTypeButton != null)
            sortByTypeButton.onClick.AddListener(() => inventory.SortByType());

        if (sortByNameButton != null)
            sortByNameButton.onClick.AddListener(() => inventory.SortByName());
    }

    private void InitializeUI()
    {
        foreach (Transform child in inventoryGridParent)
            Destroy(child.gameObject);

        slotUIs = new InventorySlotUI[inventory.Width, inventory.Height];
        slotPositions = new Dictionary<Vector2Int, InventorySlotUI>();

        for (int y = 0; y < inventory.Height; y++)
        {
            for (int x = 0; x < inventory.Width; x++)
            {
                GameObject slotInstance = Instantiate(inventorySlotPrefab, inventoryGridParent);
                InventorySlotUI slotUI = slotInstance.GetComponent<InventorySlotUI>();
                slotUI.Init(inventory, x, y);
                slotUIs[x, y] = slotUI;
                slotPositions[new Vector2Int(x, y)] = slotUI;
            }
        }

        RefreshUI();
    }

    private void OnItemMoved(Vector2Int fromPos, Vector2Int toPos)
    {
        if (slotPositions.ContainsKey(fromPos) && slotPositions.ContainsKey(toPos))
        {
            var fromSlotUI = slotPositions[fromPos];
            var toSlotUI = slotPositions[toPos];

            Vector2 startPosition = fromSlotUI.GetWorldPosition();

            toSlotUI.AnimateMoveFrom(startPosition);
        }
    }

    private void RefreshUI()
    {
        for (int y = 0; y < inventory.Height; y++)
            for (int x = 0; x < inventory.Width; x++)
                slotUIs[x, y]?.UpdateSlotDisplay(inventory.Slots[x, y]);
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshUI;
            inventory.OnItemMoved -= OnItemMoved;
        }

        if (sortByTypeButton != null)
            sortByTypeButton.onClick.RemoveAllListeners();

        if (sortByNameButton != null)
            sortByNameButton.onClick.RemoveAllListeners();
    }
}