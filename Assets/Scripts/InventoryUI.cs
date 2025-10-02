using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform inventoryGridParent;

    private InventorySlotUI[,] slotUIs;

    private void Start()
    {
        InitializeUI();
        inventory.OnInventoryChanged += RefreshUI; // �������� �� �������
    }

    private void InitializeUI()
    {
        slotUIs = new InventorySlotUI[inventory.Width, inventory.Height];

        for (int y = 0; y < inventory.Height; y++)
        {
            for (int x = 0; x < inventory.Width; x++)
            {
                GameObject slotInstance = Instantiate(inventorySlotPrefab, inventoryGridParent);
                InventorySlotUI slotUI = slotInstance.GetComponent<InventorySlotUI>();
                slotUI.Init(inventory, x, y);
                slotUIs[x, y] = slotUI;
            }
        }
        RefreshUI();
    }

    private void RefreshUI()
    {
        for (int y = 0; y < inventory.Height; y++)
        {
            for (int x = 0; x < inventory.Width; x++)
            {
                slotUIs[x, y].UpdateSlotUI(inventory.Slots[x, y]);
            }
        }
    }
}