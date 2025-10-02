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

    private void Start()
    {
        TestAddItems();
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
        if (!IsValidSlot(fromX, fromY) || !IsValidSlot(toX, toY)) return;

        InventorySlot fromSlot = Slots[fromX, fromY];
        InventorySlot toSlot = Slots[toX, toY];

        if (fromSlot.IsEmpty) return;

        if (!fromSlot.IsEmpty && !toSlot.IsEmpty &&
            fromSlot.ItemData == toSlot.ItemData &&
            fromSlot.ItemData.IsStackable)
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
        }
        else
        {
            // Смена слотов
            InventorySlot tempSlot = new InventorySlot();
            tempSlot.AssignItem(toSlot.ItemData, toSlot.ItemCount);

            toSlot.AssignItem(fromSlot.ItemData, fromSlot.ItemCount);
            fromSlot.AssignItem(tempSlot.ItemData, tempSlot.ItemCount);
        }

        OnInventoryChanged?.Invoke();
    }

    // Проверка выхода за границы массива
    private bool IsValidSlot(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}