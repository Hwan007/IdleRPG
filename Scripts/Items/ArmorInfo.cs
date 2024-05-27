using System;
using Defines;
using Keiwando.BigInteger;
using UnityEngine;
using static UnityEditor.Progress;

[Serializable]
public class ArmorInfo : Equipment, IEnhanceable, ICompositable, IAwakenable {
    public int enhancementLevel;
    public int baseEquippedEffect;
    public BigInteger equippedEffect;
    public int baseOwnedEffect;
    public BigInteger ownedEffect;
    public static int enhancementMaxLevel = 100;
    public int basicAwakenEffect;
    public bool isAwaken;
    public int baseEnhanceStoneRequired;
    public int baseEnhanceStoneIncrease;
    public BigInteger requiredEnhanceStone;

    public ArmorInfo(string equipName, int quantity, int level, bool isEquipped, EEquipmentType type, ERarity rarity,
        int enhancementLevel, int baseEquippedEffect, int baseOwnedEffect, int basicAwakenEffect,
        int baseEnhanceStoneRequired, int baseEnhanceStoneIncrease, bool isOwned = false, bool isAwaken = false) : base(
        equipName, quantity, level, isEquipped, type, rarity, isOwned) {
        this.enhancementLevel = enhancementLevel;
        this.baseEquippedEffect = baseEquippedEffect;
        this.baseOwnedEffect = baseOwnedEffect;

        this.isAwaken = isAwaken;

        equippedEffect = this.baseEquippedEffect;
        ownedEffect = this.baseOwnedEffect;

        this.baseEnhanceStoneRequired = baseEnhanceStoneRequired;
        this.baseEnhanceStoneIncrease = baseEnhanceStoneIncrease;
        requiredEnhanceStone = new BigInteger(baseEnhanceStoneRequired);
        requiredEnhanceStone += (BigInteger)(baseEnhanceStoneIncrease) * enhancementLevel;
    }

    public ArmorInfo() : base() {
        type = EEquipmentType.Armor;
        equippedEffect = new BigInteger();
        ownedEffect = new BigInteger();
    }

    public void SetArmorInfo(string name, int quantity, int level, bool OnEquipped, EEquipmentType type, ERarity eRarity,
        int enhancementLevel, int basicEquippedEffect, int basicOwnedEffect, int basicAwakenEffect, Color myColor,
        int baseEnhanceStoneRequired, int baseEnhanceStoneIncrease, bool isOwned = false, bool isAwaken = false) {
        this.equipName = name;
        this.Quantity = quantity;
        this.rarityLevel = level;
        this.IsEquipped = OnEquipped;
        this.type = type;
        this.rarity = eRarity;
        this.enhancementLevel = enhancementLevel;
        this.baseEquippedEffect = basicEquippedEffect;
        this.baseOwnedEffect = basicOwnedEffect;
        this.basicAwakenEffect = basicAwakenEffect;
        this.myColor = myColor;

        this.isOwned = isOwned;
        this.isAwaken = isAwaken;

        equippedEffect = this.baseEquippedEffect;
        ownedEffect = this.baseOwnedEffect;

        this.baseEnhanceStoneRequired = baseEnhanceStoneRequired;
        this.baseEnhanceStoneIncrease = baseEnhanceStoneIncrease;
        requiredEnhanceStone = new BigInteger(baseEnhanceStoneRequired);
        requiredEnhanceStone += (BigInteger)(baseEnhanceStoneIncrease) * enhancementLevel;
    }

    public bool TryEnhance(int maxlevel) {
        equippedEffect += baseEquippedEffect;
        ownedEffect += baseOwnedEffect;

        enhancementLevel++;

        requiredEnhanceStone += (baseEnhanceStoneIncrease);
        return true;
    }

    public BigInteger GetEnhanceStone() {
        return requiredEnhanceStone;
    }

    public override void SaveEquipment() {
        base.SaveEquipment();
        DataManager.Instance.Save<int>("enhancementLevel_" + equipName, enhancementLevel);
        DataManager.Instance.Save<string>("equippedEffect_" + equipName, equippedEffect.ToString());
        DataManager.Instance.Save<string>("ownedEffect_" + equipName, ownedEffect.ToString());
        DataManager.Instance.Save<string>($"{requiredEnhanceStone}_{equipName}", requiredEnhanceStone.ToString());
    }

    public override void Save(ESaveType type) {
        switch (type) {
            case ESaveType.EnhancementLevel:
                DataManager.Instance.Save<int>("enhancementLevel_" + equipName, enhancementLevel);
                DataManager.Instance.Save<string>("equippedEffect_" + equipName, equippedEffect.ToString());
                DataManager.Instance.Save<string>("ownedEffect_" + equipName, ownedEffect.ToString());
                break;
            case ESaveType.EquippedEffect:
                DataManager.Instance.Save<string>("equippedEffect_" + equipName, equippedEffect.ToString());
                break;
            case ESaveType.OwnedEffect:
                DataManager.Instance.Save<string>("ownedEffect_" + equipName, ownedEffect.ToString());
                break;
            case ESaveType.RequiredEnhanceStone:
                DataManager.Instance.Save<string>($"{requiredEnhanceStone}_{equipName}", requiredEnhanceStone.ToString());
                break;
            default:
                base.Save(type);
                break;
        }
    }

    public override void LoadEquipment() {
        base.LoadEquipment();
        enhancementLevel = DataManager.Instance.Load<int>("enhancementLevel_" + equipName, 0);
        equippedEffect = new BigInteger(DataManager.Instance.Load<string>($"{nameof(equippedEffect)}_{equipName}", baseEquippedEffect.ToString()));
        ownedEffect = new BigInteger(DataManager.Instance.Load<string>($"{nameof(ownedEffect)}_{equipName}", baseOwnedEffect.ToString()));
        requiredEnhanceStone = new BigInteger(DataManager.Instance.Load<string>($"{nameof(requiredEnhanceStone)}_{equipName}", baseEnhanceStoneRequired.ToString()));
    }

    public bool CanEnhance(int maxlevel) {
        return enhancementLevel >= maxlevel;
    }

    public override BigInteger GetValue() {
        return equippedEffect;
    }

    public bool CanComposite() {
        if (Quantity >= 4)
            return true;
        return false;
    }

    public int Composite() {
        int compositeCount = Quantity / 4;
        Quantity %= 4;
        return compositeCount;
    }

    public bool CanAwaken(int enhancementMaxLevel) {
        if (isAwaken) return false;
        if (enhancementLevel == enhancementMaxLevel)
            return true;
        return false;
    }

    public int Awaken() {
        isAwaken = true;
        return basicAwakenEffect;
    }
}