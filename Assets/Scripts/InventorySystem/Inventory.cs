using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int Width = 5;
    public int Height = 4;
    public InventorySlot[,] Slots { get; private set; }
    public Dictionary<EquipmentSlot, InventorySlot> EquipmentSlots { get; private set; }

    public event Action OnInventoryChanged;
    public event Action<BaseItemData> OnItemUsed;
    public event Action<BaseItemData> OnItemEquipped;
    public event Action<BaseItemData> OnItemUnequipped;
    public event Action<Vector2Int, Vector2Int> OnItemMoved;

    [Header("Тестовые данные")]
    public BaseItemData[] testItems;

    private void Awake()
    {
        InitializeSlots();
        InitializeEquipmentSlots();
    }

    private void Start() => InitializeTestData();

    private void InitializeSlots()
    {
        Slots = new InventorySlot[Width, Height];
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                Slots[x, y] = new InventorySlot();
    }

    private void InitializeEquipmentSlots()
    {
        EquipmentSlots = new Dictionary<EquipmentSlot, InventorySlot>();
        foreach (EquipmentSlot slotType in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (slotType != EquipmentSlot.None)
                EquipmentSlots[slotType] = new InventorySlot();
        }
    }

    private void InitializeTestData()
    {
        ClearAllSlots();

        if (testItems != null && testItems.Length > 0 && Width > 0 && Height > 0)
        {
            if (testItems.Length > 0) Slots[0, 0].AssignItem(testItems[0], 2);
            if (testItems.Length > 0) Slots[0, 1].AssignItem(testItems[0], 2);
            if (testItems.Length > 1) Slots[1, 1].AssignItem(testItems[1], 1);
            if (testItems.Length > 2) Slots[1, 2].AssignItem(testItems[2], 1);
        }

        OnInventoryChanged?.Invoke();
    }

    private void ClearAllSlots()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                Slots[x, y].ClearSlot();

        foreach (var slot in EquipmentSlots.Values)
            slot.ClearSlot();
    }

    public bool TryAddItem(BaseItemData itemData, int quantity = 1)
    {
        if (itemData == null) return false;

        int remainingQuantity = quantity;

        if (itemData.IsStackable)
            remainingQuantity = AddToExistingStacks(itemData, remainingQuantity);

        if (remainingQuantity <= 0)
        {
            OnInventoryChanged?.Invoke();
            return true;
        }

        remainingQuantity = AddToEmptySlots(itemData, remainingQuantity);

        bool success = remainingQuantity <= 0;
        if (success) OnInventoryChanged?.Invoke();

        return success;
    }

    private int AddToExistingStacks(BaseItemData itemData, int quantity)
    {
        int remaining = quantity;

        for (int y = 0; y < Height && remaining > 0; y++)
        {
            for (int x = 0; x < Width && remaining > 0; x++)
            {
                var slot = Slots[x, y];
                if (!slot.IsEmpty && slot.ItemData == itemData && slot.ItemCount < itemData.MaxStackCount)
                {
                    int spaceAvailable = itemData.MaxStackCount - slot.ItemCount;
                    int amountToAdd = Mathf.Min(remaining, spaceAvailable);

                    slot.ItemCount += amountToAdd;
                    remaining -= amountToAdd;
                }
            }
        }

        return remaining;
    }

    private int AddToEmptySlots(BaseItemData itemData, int quantity)
    {
        int remaining = quantity;

        for (int y = 0; y < Height && remaining > 0; y++)
        {
            for (int x = 0; x < Width && remaining > 0; x++)
            {
                var slot = Slots[x, y];
                if (slot.IsEmpty)
                {
                    int amountToAdd = Mathf.Min(remaining, itemData.MaxStackCount);
                    slot.AssignItem(itemData, amountToAdd);
                    remaining -= amountToAdd;
                }
            }
        }

        return remaining;
    }

    public void MoveItem(int fromX, int fromY, int toX, int toY)
    {
        if (!IsValidSlot(fromX, fromY) || !IsValidSlot(toX, toY)) return;
        if (fromX == toX && fromY == toY) return;

        InventorySlot fromSlot = Slots[fromX, fromY];
        InventorySlot toSlot = Slots[toX, toY];

        if (fromSlot.IsEmpty) return;

        // Сохраняем информацию для анимации
        var fromPosition = new Vector2Int(fromX, fromY);
        var toPosition = new Vector2Int(toX, toY);

        if (TryMergeStacks(fromSlot, toSlot))
        {
            OnInventoryChanged?.Invoke();
            OnItemMoved?.Invoke(fromPosition, toPosition);
            return;
        }

        SwapSlots(fromSlot, toSlot);
        OnInventoryChanged?.Invoke();
        OnItemMoved?.Invoke(fromPosition, toPosition);
    }

    private bool TryMergeStacks(InventorySlot fromSlot, InventorySlot toSlot)
    {
        if (toSlot.IsEmpty || fromSlot.ItemData != toSlot.ItemData || !fromSlot.ItemData.IsStackable)
            return false;

        int totalCount = fromSlot.ItemCount + toSlot.ItemCount;
        int maxStack = fromSlot.ItemData.MaxStackCount;

        if (totalCount <= maxStack)
        {
            toSlot.ItemCount = totalCount;
            fromSlot.ClearSlot();
            return true;
        }
        else
        {
            toSlot.ItemCount = maxStack;
            fromSlot.ItemCount = totalCount - maxStack;
            return true;
        }
    }

    private void SwapSlots(InventorySlot slotA, InventorySlot slotB)
    {
        var tempData = slotA.ItemData;
        var tempCount = slotA.ItemCount;
        var tempEquipped = slotA.IsEquipped;

        slotA.AssignItem(slotB.ItemData, slotB.ItemCount);
        if (slotB.IsEquipped) slotA.SetEquipped(true);

        slotB.AssignItem(tempData, tempCount);
        if (tempEquipped) slotB.SetEquipped(true);
    }

    public void UseItem(int x, int y)
    {
        if (!IsValidSlot(x, y) || Slots[x, y].IsEmpty) return;

        var slot = Slots[x, y];
        var itemData = slot.ItemData;

        if (itemData == null) return;

        OnItemUsed?.Invoke(itemData);

        switch (itemData.Type)
        {
            case ItemType.Potion:
                UsePotion(slot, x, y);
                break;
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Accessory:
                if (itemData.IsEquippable) ToggleEquipment(slot, x, y);
                break;
        }
    }

    private void UsePotion(InventorySlot slot, int x, int y)
    {
        slot.ItemCount--;

        if (slot.ItemCount <= 0)
            slot.ClearSlot();

        OnInventoryChanged?.Invoke();
    }

    private void ToggleEquipment(InventorySlot slot, int x, int y)
    {
        if (slot.IsEquipped)
            UnequipItem(slot, x, y);
        else
            EquipItem(slot, x, y);
    }

    private void EquipItem(InventorySlot slot, int x, int y)
    {
        if (!slot.CanBeEquipped) return;

        var itemData = slot.ItemData;
        var equipmentSlot = itemData.EquipmentSlot;

        if (EquipmentSlots.ContainsKey(equipmentSlot) && !EquipmentSlots[equipmentSlot].IsEmpty)
        {
            SwapWithEquipped(slot, equipmentSlot);
        }
        else
        {
            slot.SetEquipped(true);
            if (EquipmentSlots.ContainsKey(equipmentSlot))
            {
                EquipmentSlots[equipmentSlot].AssignItem(itemData, slot.ItemCount);
                EquipmentSlots[equipmentSlot].SetEquipped(true);
            }
        }

        OnItemEquipped?.Invoke(itemData);
        OnInventoryChanged?.Invoke();
    }

    private void SwapWithEquipped(InventorySlot slot, EquipmentSlot equipmentSlot)
    {
        var equippedSlot = EquipmentSlots[equipmentSlot];
        var tempData = equippedSlot.ItemData;
        var tempCount = equippedSlot.ItemCount;

        equippedSlot.AssignItem(slot.ItemData, slot.ItemCount);
        slot.AssignItem(tempData, tempCount);
    }

    private void UnequipItem(InventorySlot slot, int x, int y)
    {
        var itemData = slot.ItemData;
        var equipmentSlot = itemData.EquipmentSlot;

        for (int inventoryY = 0; inventoryY < Height; inventoryY++)
        {
            for (int inventoryX = 0; inventoryX < Width; inventoryX++)
            {
                if (Slots[inventoryX, inventoryY].IsEmpty)
                {
                    Slots[inventoryX, inventoryY].AssignItem(itemData, slot.ItemCount);
                    slot.ClearSlot();

                    if (EquipmentSlots.ContainsKey(equipmentSlot))
                        EquipmentSlots[equipmentSlot].ClearSlot();

                    OnItemUnequipped?.Invoke(itemData);
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
        }
    }

    public void RemoveItem(int x, int y, int amount = 1)
    {
        if (!IsValidSlot(x, y) || Slots[x, y].IsEmpty) return;

        var slot = Slots[x, y];

        if (slot.IsEquipped)
        {
            UnequipItem(slot, x, y);
            return;
        }

        slot.ItemCount -= amount;

        if (slot.ItemCount <= 0)
            slot.ClearSlot();

        OnInventoryChanged?.Invoke();
    }

    public void SortByType()
    {
        var allItems = CollectAllItems();

        allItems.Sort((a, b) =>
        {
            int typeComparison = a.ItemData.Type.CompareTo(b.ItemData.Type);
            return typeComparison != 0 ? typeComparison : string.Compare(a.ItemData.ItemName, b.ItemData.ItemName, StringComparison.Ordinal);
        });

        RedistributeItems(allItems);
    }

    public void SortByName()
    {
        var allItems = CollectAllItems();

        allItems.Sort((a, b) =>
            string.Compare(a.ItemData.ItemName, b.ItemData.ItemName, StringComparison.Ordinal));

        RedistributeItems(allItems);
    }

    private List<InventorySlot> CollectAllItems()
    {
        var items = new List<InventorySlot>();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var slot = Slots[x, y];
                if (!slot.IsEmpty)
                {
                    items.Add(new InventorySlot
                    {
                        ItemData = slot.ItemData,
                        ItemCount = slot.ItemCount,
                        IsEquipped = slot.IsEquipped
                    });
                }
            }
        }

        return items;
    }

    private void RedistributeItems(List<InventorySlot> sortedItems)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                Slots[x, y].ClearSlot();

        int itemIndex = 0;
        for (int y = 0; y < Height && itemIndex < sortedItems.Count; y++)
        {
            for (int x = 0; x < Width && itemIndex < sortedItems.Count; x++)
            {
                var item = sortedItems[itemIndex];
                Slots[x, y].AssignItem(item.ItemData, item.ItemCount);

                if (item.IsEquipped)
                {
                    Slots[x, y].SetEquipped(true);
                    var equipmentSlot = item.ItemData.EquipmentSlot;
                    if (EquipmentSlots.ContainsKey(equipmentSlot))
                    {
                        EquipmentSlots[equipmentSlot].AssignItem(item.ItemData, item.ItemCount);
                        EquipmentSlots[equipmentSlot].SetEquipped(true);
                    }
                }

                itemIndex++;
            }
        }

        UpdateEquipmentVisuals();
        OnInventoryChanged?.Invoke();
    }

    private void UpdateEquipmentVisuals()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var slot = Slots[x, y];
                if (slot.IsEquipped)
                {
                    OnItemEquipped?.Invoke(slot.ItemData);
                }
            }
        }
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

    [ContextMenu("Сортировать по типу")]
    private void SortByTypeTest() => SortByType();

    [ContextMenu("Сортировать по имени")]
    private void SortByNameTest() => SortByName();

    [ContextMenu("Обновить UI")]
    public void ForceRefreshUI() => OnInventoryChanged?.Invoke();
}