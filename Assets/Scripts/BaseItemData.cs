using UnityEngine;

public abstract class BaseItemData : ScriptableObject
{
    public string ItemName;
    public Sprite Icon;
    [TextArea] public string Description;
    public ItemType Type;
    public bool IsStackable;
    public int MaxStackCount = 1;
}

public enum ItemType
{
    Weapon,
    Potion,
    QuestItem
}