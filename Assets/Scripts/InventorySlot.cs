[System.Serializable]
public class InventorySlot
{
    public BaseItemData ItemData;
    public int ItemCount;

    public bool IsEmpty => ItemData == null;

    public void ClearSlot()
    {
        ItemData = null;
        ItemCount = 0;
    }

    public void AssignItem(BaseItemData data, int count)
    {
        ItemData = data;
        ItemCount = count;
    }
}