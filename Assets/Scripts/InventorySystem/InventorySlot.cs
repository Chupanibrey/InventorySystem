using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public BaseItemData ItemData;
    public int ItemCount;
    public bool IsEquipped;

    public bool IsEmpty => ItemData == null;
    public bool CanBeEquipped => !IsEmpty && ItemData.IsEquippable;

    public void ClearSlot()
    {
        ItemData = null;
        ItemCount = 0;
        IsEquipped = false;
    }

    public void AssignItem(BaseItemData data, int count)
    {
        ItemData = data;
        ItemCount = count;
        IsEquipped = false;
    }

    public void SetEquipped(bool equipped)
    {
        if (CanBeEquipped)
            IsEquipped = equipped;
    }
}