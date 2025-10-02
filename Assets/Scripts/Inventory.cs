using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int Width = 5;
    public int Height = 4;
    public InventorySlot[,] Slots;

    // Событие уведомления об изменениях
    public System.Action OnInventoryChanged;

    private void Awake()
    {
        Slots = new InventorySlot[Width, Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Slots[x, y] = new InventorySlot();
            }
        }
    }

    [Header("Test Data")]
    public BaseItemData[] testItems;

    private void Start()
    {
        InitializeTestData();
    }

    private void InitializeTestData()
    {
        Slots[0, 0].AssignItem(testItems[0], 1);
        Slots[0, 3].AssignItem(testItems[0], 1);
        Slots[2, 3].AssignItem(testItems[0], 1);

        Slots[3, 3].AssignItem(testItems[1], 1);

        Slots[4, 2].AssignItem(testItems[2], 1);

        OnInventoryChanged?.Invoke();
    }

    private void TestAddItems()
    {
        OnInventoryChanged?.Invoke();
    }

    public bool TryAddItem(BaseItemData itemData, int quantity = 1)
    {
        return true;
    }

    public void MoveItem(int fromX, int fromY, int toX, int toY)
    {
        if (!IsValidSlot(fromX, fromY) || !IsValidSlot(toX, toY) || (fromX == toX && fromY == toY))
            return;

        InventorySlot fromSlot = Slots[fromX, fromY];
        InventorySlot toSlot = Slots[toX, toY];

        if (fromSlot.IsEmpty)
            return;

        if (!toSlot.IsEmpty && fromSlot.ItemData == toSlot.ItemData && fromSlot.ItemData.IsStackable)
        {
            int totalCount = fromSlot.ItemCount + toSlot.ItemCount;
            int maxStack = fromSlot.ItemData.MaxStackCount;

            if (totalCount <= maxStack)
            {
                toSlot.ItemCount = totalCount;
                fromSlot.ClearSlot();
            }
            else
            {
                toSlot.ItemCount = maxStack;
                fromSlot.ItemCount = totalCount - maxStack;
            }
            OnInventoryChanged?.Invoke();
            return;
        }

        BaseItemData tempData = fromSlot.ItemData;
        int tempCount = fromSlot.ItemCount;

        fromSlot.AssignItem(toSlot.ItemData, toSlot.ItemCount);
        toSlot.AssignItem(tempData, tempCount);

        OnInventoryChanged?.Invoke();
    }

    private bool IsValidSlot(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}