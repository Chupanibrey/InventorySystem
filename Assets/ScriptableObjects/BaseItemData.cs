using UnityEngine;

[CreateAssetMenu(fileName = "BaseItemData", menuName = "Scriptable Objects/BaseItemData")]
public class BaseItemData : ScriptableObject
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