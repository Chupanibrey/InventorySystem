using UnityEngine;

[CreateAssetMenu(fileName = "BaseItemData", menuName = "Inventory/Item Data")]
public class BaseItemData : ScriptableObject
{
    [Header("Основная информация")]
    public string ItemName;
    public Sprite Icon;
    [TextArea] public string Description;
    public ItemType Type;

    [Header("Свойства предмета")]
    public bool IsStackable;
    public int MaxStackCount = 1;
    public string ItemID;

    [Header("Экипировка")]
    public bool IsEquippable = false;
    public EquipmentSlot EquipmentSlot = EquipmentSlot.None;
}

public enum ItemType
{
    Weapon,
    Potion,
    QuestItem,
    Resource,
    Armor,
    Accessory
}

public enum EquipmentSlot
{
    None,
    Head,
    Chest,
    Hands,
    Legs,
    Feet,
    Weapon,
    Shield,
    Accessory
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}