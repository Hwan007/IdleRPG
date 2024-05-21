using System;
using Defines;
using UnityEngine;
using Keiwando.BigInteger;
using UnityEngine.Serialization;

[Serializable]
public class Equipment {
    public string equipName;
    public ERarity rarity;
    public int rarityLevel;
    public event Action<int> onQuantityChange;
    public int Quantity {
        get => quantity;
        set {
            onQuantityChange?.Invoke(value);
            quantity = value;
        }
    }

    public int quantity;

    public bool IsEquipped {
        get => isEquipped;
        set {
            isEquipped = value;
            actOnEquipChange?.Invoke(isEquipped);
        }
    }

    protected bool isEquipped;

    public EEquipmentType type;

    public bool IsOwned {
        get => isOwned;
        set {
            if (value) {
                isOwned = value;
                onOwned?.Invoke();
            }
        }
    }

    public event Action onOwned;

    protected bool isOwned;
    public Color myColor;

    public Action<bool> actOnEquipChange;


    public Equipment(string equipName, int quantity, int rarityLevel, bool isEquipped, EEquipmentType type, ERarity rarity,
        int enhancementLevel, int baseEquippedEffect, int baseOwnedEffect, int basicAwakenEffect,
        int baseEnhanceStoneRequired, int baseEnhanceStoneIncrease, bool isOwned = false, bool isAwaken = false) {
        this.equipName = equipName;
        this.Quantity = quantity;
        this.rarityLevel = rarityLevel;
        this.IsEquipped = isEquipped;
        this.type = type;
        this.isOwned = isOwned;
        this.rarity = rarity;
    }

    public Equipment() {
    }

    public virtual bool CanEnhance(int maxlevel) {
        return false;
    }

    public virtual bool TryEnhance(int maxlevel) {
        return false;
    }

    public virtual BigInteger GetEnhanceStone() {
        return 0;
    }

    public bool CheckQuantity() {
        if (Quantity >= 4) {
            return true;
        }

        return false;
    }

    public virtual void SaveEquipment() {
        DataManager.Instance.Save<int>("quantity_" + equipName, Quantity);
        DataManager.Instance.Save<bool>("isEquiped_" + equipName, IsEquipped);
        DataManager.Instance.Save<bool>("isOwned_" + equipName, isOwned);
    }

    public virtual void Save(ESaveType type) {
        switch (type) {
            case ESaveType.Quantity:
                DataManager.Instance.Save<int>("quantity_" + equipName, Quantity);
                break;
            case ESaveType.IsEquipped:
                DataManager.Instance.Save<bool>("isEquiped_" + equipName, IsEquipped);
                break;
            case ESaveType.IsOwned:
                DataManager.Instance.Save<bool>("isOwned_" + equipName, isOwned);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public virtual void LoadEquipment() {
        Quantity = DataManager.Instance.Load<int>("quantity_" + equipName, 0);
        IsEquipped = DataManager.Instance.Load<bool>("isEquiped_" + equipName, false);
        isOwned = DataManager.Instance.Load<bool>("isOwned_" + equipName, false);
        if (Quantity > 0)
            isOwned = true;
    }

    public virtual BigInteger GetValue() {
        return 0;
    }

    public static bool operator <(Equipment a, Equipment b) {
        if (a.GetValue() < b.GetValue())
            return true;
        else
            return false;
    }

    public static bool operator >(Equipment a, Equipment b) {
        if (a.GetValue() > b.GetValue())
            return true;
        else
            return false;
    }
}