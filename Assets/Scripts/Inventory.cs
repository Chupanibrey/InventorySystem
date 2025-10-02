using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public int Width = 5;
    public int Height = 4;
    public InventorySlot[,] Slots { get; private set; }

    public event Action OnInventoryChanged;
    public event Action<BaseItemData> OnItemUsed;

    [Header("Тестовые данные")]
    public BaseItemData[] testItems;

    private void Awake() => InitializeSlots();

    private void InitializeSlots()
    {
        Slots = new InventorySlot[Width, Height];
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                Slots[x, y] = new InventorySlot();
    }

    private void Start() => InitializeTestData();

    private void InitializeTestData()
    {
        // Очистка слотов
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                Slots[x, y].ClearSlot();

        // Заполнение тестовыми данными
        if (testItems != null && testItems.Length > 0)
        {
            Slots[0, 0].AssignItem(testItems[0], 2);
            Slots[0, 1].AssignItem(testItems[0], 1);
            Slots[1, 1].AssignItem(testItems[1], 1);
            Slots[1, 2].AssignItem(testItems[2], 1);
        }

        OnInventoryChanged?.Invoke();
    }

    public bool TryAddItem(BaseItemData itemData, int quantity = 1)
    {
        Debug.Log($"Попытка добавить предмет '{itemData.ItemName}' в количестве {quantity} шт.");

        // Сначала пробуем добавить в существующие стеки
        if (itemData.IsStackable)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (!Slots[x, y].IsEmpty && Slots[x, y].ItemData == itemData &&
                        Slots[x, y].ItemCount < itemData.MaxStackCount)
                    {
                        int spaceAvailable = itemData.MaxStackCount - Slots[x, y].ItemCount;
                        int amountToAdd = Mathf.Min(quantity, spaceAvailable);

                        Debug.Log($"Добавление в существующий стек [{x}, {y}]: +{amountToAdd} шт. (было {Slots[x, y].ItemCount})");

                        Slots[x, y].ItemCount += amountToAdd;
                        quantity -= amountToAdd;

                        if (quantity <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }
        }

        // Ищем пустые слоты
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (Slots[x, y].IsEmpty)
                {
                    int amountToAdd = Mathf.Min(quantity, itemData.MaxStackCount);
                    Debug.Log($"Добавление в пустой слот [{x}, {y}]: {amountToAdd} шт.");

                    Slots[x, y].AssignItem(itemData, amountToAdd);
                    quantity -= amountToAdd;

                    if (quantity <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        if (quantity > 0)
        {
            Debug.Log($"Не удалось добавить {quantity} шт. - нет места в инвентаре");
        }

        return quantity <= 0;
    }

    public void MoveItem(int fromX, int fromY, int toX, int toY)
    {
        if (!IsValidSlot(fromX, fromY) || !IsValidSlot(toX, toY) || (fromX == toX && fromY == toY))
            return;

        InventorySlot fromSlot = Slots[fromX, fromY];
        InventorySlot toSlot = Slots[toX, toY];

        if (fromSlot.IsEmpty) return;

        Debug.Log($"Перемещение предмета '{fromSlot.ItemData.ItemName}' из [{fromX}, {fromY}] в [{toX}, {toY}]");

        // Попытка объединения стеков
        if (!toSlot.IsEmpty && fromSlot.ItemData == toSlot.ItemData && fromSlot.ItemData.IsStackable)
        {
            int totalCount = fromSlot.ItemCount + toSlot.ItemCount;
            int maxStack = fromSlot.ItemData.MaxStackCount;

            Debug.Log($"Объединение стеков: {fromSlot.ItemCount} + {toSlot.ItemCount} = {totalCount} (макс. {maxStack})");

            if (totalCount <= maxStack)
            {
                toSlot.ItemCount = totalCount;
                fromSlot.ClearSlot();
                Debug.Log($"Стеки объединены в слот [{toX}, {toY}]");
            }
            else
            {
                toSlot.ItemCount = maxStack;
                fromSlot.ItemCount = totalCount - maxStack;
                Debug.Log($"Частичное объединение: в [{toX}, {toY}] - {maxStack}, в [{fromX}, {fromY}] - {fromSlot.ItemCount}");
            }
            OnInventoryChanged?.Invoke();
            return;
        }

        // Обычное перемещение
        Debug.Log($"Обмен предметами между слотами");
        BaseItemData tempData = fromSlot.ItemData;
        int tempCount = fromSlot.ItemCount;

        fromSlot.AssignItem(toSlot.ItemData, toSlot.ItemCount);
        toSlot.AssignItem(tempData, tempCount);

        OnInventoryChanged?.Invoke();
    }

    public void UseItem(int x, int y)
    {
        if (!IsValidSlot(x, y) || Slots[x, y].IsEmpty) return;

        var slot = Slots[x, y];
        BaseItemData itemData = slot.ItemData;

        if (itemData == null) return;

        OnItemUsed?.Invoke(itemData);

        switch (itemData.Type)
        {
            case ItemType.Potion:
                Debug.Log($"Использовано зелье: {itemData.ItemName}. Было: {slot.ItemCount} шт.");
                slot.ItemCount--;
                if (slot.ItemCount <= 0)
                {
                    slot.ClearSlot();
                    Debug.Log($"Слот [{x}, {y}] очищен - предмет закончился");
                }
                else
                {
                    Debug.Log($"Осталось зелий: {slot.ItemCount} шт.");
                }
                OnInventoryChanged?.Invoke();
                break;

            case ItemType.Weapon:
                Debug.Log($"Экипировано оружие: {itemData.ItemName}");
                // Логика экипировки оружия
                break;

            case ItemType.QuestItem:
                Debug.Log($"Использован квестовый предмет: {itemData.ItemName}");
                // Логика для квестовых предметов
                break;

            case ItemType.Resource:
                Debug.Log($"Использован ресурс: {itemData.ItemName}. Осталось: {slot.ItemCount} шт.");
                break;
        }
    }

    public void RemoveItem(int x, int y, int amount = 1)
    {
        if (!IsValidSlot(x, y) || Slots[x, y].IsEmpty) return;

        var slot = Slots[x, y];
        BaseItemData itemData = slot.ItemData;

        Debug.Log($"Удаление предмета '{itemData.ItemName}' из слота [{x}, {y}]. Удалено: {amount} шт., было: {slot.ItemCount} шт.");

        slot.ItemCount -= amount;

        if (slot.ItemCount <= 0)
        {
            slot.ClearSlot();
            Debug.Log($"Слот [{x}, {y}] полностью очищен");
        }
        else
        {
            Debug.Log($"В слоте осталось: {slot.ItemCount} шт.");
        }

        OnInventoryChanged?.Invoke();
    }

    private bool IsValidSlot(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    [ContextMenu("Добавить случайный предмет")]
    private void AddRandomItemTest()
    {
        if (testItems != null && testItems.Length > 0)
        {
            var randomItem = testItems[UnityEngine.Random.Range(0, testItems.Length)];
            TryAddItem(randomItem, 1);
        }
    }

    [ContextMenu("Обновить UI")]
    public void ForceRefreshUI() => OnInventoryChanged?.Invoke();
}