using System;
using System.Collections.Generic;
using System.Text;
using Defines;
using Keiwando.BigInteger;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EquipmentManager : MonoBehaviour {
    #region 생략
    public static EquipmentManager instance;

    private BigInteger totalEnhanceCount;

    public BigInteger TotalEnhanceCount {
        get => totalEnhanceCount;
        set {
            onEnhanceTotal?.Invoke(value);
            totalEnhanceCount = value;
        }
    }

    private BigInteger totalWeaponComposite;
    private BigInteger totalArmorComposite;

    public BigInteger TotalWeaponComposite => totalWeaponComposite;
    public BigInteger TotalArmorComposite => totalArmorComposite;

    public event Action<BigInteger> onEnhanceTotal;
    public event Action<BigInteger> onWeaponCompositeTotal;
    public event Action<BigInteger> onArmorCompositeTotal;

    public List<WeaponInfo> weapons;
    public List<ArmorInfo> armors;

    private static Dictionary<string, Equipment> allEquipment;

    [SerializeField] private List<Sprite> weaponImages;
    [SerializeField] private List<Sprite> armorImages;

    public Dictionary<string, Sprite> images { get; private set; }

    ERarity[] rarities =
    {
        ERarity.Common,
        ERarity.Uncommon,
        ERarity.Rare,
        ERarity.Epic,
        ERarity.Legendary,
        ERarity.Mythology
    };

    [SerializeField] public Color[] rarityColors;
    [SerializeField] private Sprite[] rarityFrame;

    [SerializeField] int maxLevel = 4;
    [SerializeField] private int enhancementMaxLevel;
    public int EnhancementMaxLevel => enhancementMaxLevel;

#if UNITY_EDITOR
    [SerializeField] private int testQuantity;
#endif

    private void Awake() {
        instance = this;
        allEquipment = new Dictionary<string, Equipment>();
    }

    public void InitEquipmentManager() {
        SetAllWeapons();
        SetAllArmors();
        Load();
        images = new Dictionary<string, Sprite>();
        foreach (ERarity rarity in rarities) {
            for (int level = 1; level <= maxLevel; level++) {
                string name = $"{EEquipmentType.Weapon}_{rarity}_{level}";
                images.Add(name, weaponImages[Mathf.Clamp(4 * (int)rarity + (level - 1), 0, weaponImages.Count - 1)]);
                name = $"{EEquipmentType.Armor}_{rarity}_{level}";
                images.Add(name, armorImages[Mathf.Clamp(4 * (int)rarity + (level - 1), 0, armorImages.Count - 1)]);
            }
        }
    }

    private void Load() {
        totalEnhanceCount = new BigInteger(DataManager.Instance.Load<string>($"{nameof(EquipmentManager)}_{nameof(totalEnhanceCount)}", "0"));
        totalWeaponComposite = new BigInteger(DataManager.Instance.Load<string>($"{nameof(EquipmentManager)}_{nameof(totalWeaponComposite)}", "0"));
        totalArmorComposite = new BigInteger(DataManager.Instance.Load<string>($"{nameof(EquipmentManager)}_{nameof(totalArmorComposite)}", "0"));
    }

    public void Save(EEquipmentManagerSaveType type) {
        switch (type) {
            case EEquipmentManagerSaveType.TotalEnhance:
                DataManager.Instance.Save($"{nameof(EquipmentManager)}_{nameof(totalEnhanceCount)}", totalEnhanceCount.ToString());
                break;
            case EEquipmentManagerSaveType.TotalWeaponComposite:
                DataManager.Instance.Save($"{nameof(EquipmentManager)}_{nameof(totalWeaponComposite)}", totalWeaponComposite.ToString());
                break;
            case EEquipmentManagerSaveType.TotalArmorComposite:
                DataManager.Instance.Save($"{nameof(EquipmentManager)}_{nameof(totalArmorComposite)}", totalArmorComposite.ToString());
                break;
        }
    }

    public Sprite GetIcon(EEquipmentType type, int index) {
        switch (type) {
            case EEquipmentType.Weapon:
                return weaponImages[index];
            case EEquipmentType.Armor:
                return armorImages[index];
        }
        return null;
    }

    void SetAllWeapons() {
        LoadAllWeapon();
    }

    private void SetAllArmors() {
        LoadAllArmor();
    }

    public void LoadAllWeapon() {
        foreach (ERarity rarity in rarities) {
            var rarityIntValue = (int)rarity;
            for (int level = 1; level <= maxLevel; level++) {
                string name = $"{EEquipmentType.Weapon}_{rarity}_{level}";
                WeaponInfo weapon = weapons[4 * rarityIntValue + level - 1];
                weapon.LoadEquipment();
                AddEquipment(name, weapon);
                if (weapon.IsEquipped)
                    PlayerManager.instance.EquipItem(weapon, false);
                weapon.myColor = rarityColors[rarityIntValue];
                if (weapon.IsOwned)
                    PlayerManager.instance.ApplyOwnedEffect(weapon);
            }
        }
    }

    public void LoadAllArmor() {
        foreach (ERarity rarity in rarities) {
            var rarityIntValue = Convert.ToInt32(rarity);
            for (int level = 1; level <= maxLevel; level++) {
                string name = $"{EEquipmentType.Armor}_{rarity}_{level}";
                ArmorInfo armor = armors[4 * rarityIntValue + level - 1];
                armor.LoadEquipment();
                AddEquipment(name, armor);
                if (armor.IsEquipped)
                    PlayerManager.instance.EquipItem(armor, false);
                armor.myColor = rarityColors[rarityIntValue];
                if (armor.IsOwned)
                    PlayerManager.instance.ApplyOwnedEffect(armor);
            }
        }
    }
#if UNITY_EDITOR
    public void CreateAllWeapon() {
        foreach (ERarity rarity in rarities) {
            if (rarity == ERarity.None)
                continue;
            var rarityIntValue = Convert.ToInt32(rarity);
            for (int level = 1; level <= maxLevel; level++) {
                WeaponInfo weapon = new WeaponInfo();
                string name = $"{weapon.type}_{rarity}_{level}";
                int equippedEffect = level * ((int)Mathf.Pow(10, rarityIntValue + 1));
                int ownedEffect = (int)(equippedEffect * 0.5f);
                int awakenEffect = (4 * (int)rarity + level) * 100;
                int baseEnhanceStoneRequired = (4 * (int)rarity + level) * 100;
                int baseEnhanceStoneIncrease = (4 * (int)rarity + level) * 50;
                weapon.SetWeaponInfo(name, 0, level, false, EEquipmentType.Weapon, rarity, 1, equippedEffect,
                    ownedEffect, awakenEffect, rarityColors[rarityIntValue], baseEnhanceStoneRequired,
                    baseEnhanceStoneIncrease, false);
                weapons.Add(weapon);
            }
        }
    }

    public void CreateAllArmor() {
        foreach (ERarity rarity in rarities) {
            if (rarity == ERarity.None)
                continue;
            var rarityIntValue = Convert.ToInt32(rarity);
            for (int level = 1; level <= maxLevel; level++) {
                ArmorInfo armor = new ArmorInfo();
                string name = $"{armor.type}_{rarity}_{level}";
                int equippedEffect = level * ((int)Mathf.Pow(10, rarityIntValue + 1));
                int ownedEffect = (int)(equippedEffect * 0.5f);
                int awakenEffect = (4 * (int)rarity + level) * 100;
                int baseEnhanceStoneRequired = (4 * (int)rarity + level) * 100;
                int baseEnhanceStoneIncrease = (4 * (int)rarity + level) * 50;
                armor.SetArmorInfo(name, 0, level, false, EEquipmentType.Armor, rarity,
                    1, equippedEffect, ownedEffect, awakenEffect, rarityColors[rarityIntValue],
                    baseEnhanceStoneRequired,
                    baseEnhanceStoneIncrease, false);
                armors.Add(armor);
            }
        }
    }
#endif
    #endregion

    T GetBestItem<T>(List<T> items) where T : Equipment {
        T best = null;
        foreach (var item in items) {
            if (!item.IsOwned)
                continue;
            if (ReferenceEquals(best, null)) {
                best = item;
                continue;
            }
            if (best < item) {
                best = item;
            }
        }
        return best;
    }

    public Equipment TryGetBestItem(EEquipmentType type) {
        switch (type) {
            case EEquipmentType.Weapon:
                return GetBestItem(weapons);
            case EEquipmentType.Armor:
                return GetBestItem(armors);
            default:
                return null;
        }
    }

    public int Composite<T>(T equipment) where T : Equipment, ICompositable {
        if (equipment.CanComposite())
            return -1;
        Equipment nextEquipment = TryGetNextEquipment(equipment.equipName);
        if (nextEquipment == null)
            return -1;
        int compositeCount = equipment.Composite();
        if (!nextEquipment.IsOwned) {
            nextEquipment.IsOwned = true;
            nextEquipment.Save(ESaveType.IsOwned);
            PlayerManager.instance.ApplyOwnedEffect(nextEquipment);
        }
        nextEquipment.Quantity += compositeCount;
        return compositeCount;
    }

    public void CompositeOnce<T>(T equipment) where T : Equipment, ICompositable {
        var status = PlayerManager.instance.status;
        var score = new BigInteger(status.BattleScore.ToString());
        if (Composite(equipment) > 0) {
            equipment.Save(ESaveType.Quantity);
            TryGetNextEquipment(equipment.equipName)?.Save(ESaveType.Quantity);
        }

        if (equipment.type == EEquipmentType.Weapon) {
            ++totalWeaponComposite;
            onWeaponCompositeTotal?.Invoke(totalWeaponComposite);
            Save(EEquipmentManagerSaveType.TotalWeaponComposite);
        }
        else if (equipment.type == EEquipmentType.Armor) {
            ++totalArmorComposite;
            onArmorCompositeTotal?.Invoke(totalArmorComposite);
            Save(EEquipmentManagerSaveType.TotalArmorComposite);
        }
        PlayerManager.instance.status.InitBattleScore();
        MessageUIManager.instance.ShowPower(status.BattleScore, status.BattleScore - score);
    }

    public void CompositeAll(EEquipmentType type) {
        var status = PlayerManager.instance.status;
        var score = new BigInteger(status.BattleScore.ToString());
        switch (type) {
            case EEquipmentType.Weapon:
                CompositeAllItems(weapons);
                break;
            case EEquipmentType.Armor:
                CompositeAllItems(armors);
                break;
        }
        PlayerManager.instance.status.InitBattleScore();
        MessageUIManager.instance.ShowPower(status.BattleScore, status.BattleScore - score);
    }

    private void CompositeAllItems<T>(List<T> items) where T : Equipment, ICompositable {
        int count = 0;
        HashSet<T> updateItems = new HashSet<T>();
        foreach (var item in items) {
            var ret = Composite(item);
            if (ret < 0)
                continue;
            count += ret;
            updateItems.Add(item);
            var nextItem = TryGetNextEquipment(item.equipName);
            if (!ReferenceEquals(nextItem, null))
                updateItems.Add(nextItem as T);
        }
        foreach (var item in updateItems) {
            item.Save(ESaveType.Quantity);
        }
        var type = items[0].type;
        if (type == EEquipmentType.Weapon) {
            totalWeaponComposite += count;
            onWeaponCompositeTotal?.Invoke(totalWeaponComposite);
            Save(EEquipmentManagerSaveType.TotalWeaponComposite);
        }
        else if (type == EEquipmentType.Armor) {
            totalArmorComposite += count;
            onArmorCompositeTotal?.Invoke(totalArmorComposite);
            Save(EEquipmentManagerSaveType.TotalArmorComposite);
        }
    }

    public static void AddEquipment(string equipmentName, Equipment equipment) {
        if (!allEquipment.ContainsKey(equipmentName)) {
            allEquipment.Add(equipmentName, equipment);
        }
    }

    public static Equipment TryGetEquipment(string equipmentName) {
        if (allEquipment.TryGetValue(equipmentName, out Equipment equipment)) {
            return equipment;
        }
        else {
            return null;
        }
    }

    public Equipment TryGetEquipment(EEquipmentType type, int index) {
        switch (type) {
            case EEquipmentType.Weapon:
                return weapons[index];
            case EEquipmentType.Armor:
                return armors[index];
        }
        return null;
    }

    public Equipment TryGetNextEquipment(string currentKey) {
        int currentRarityIndex = -1;
        string currentTypeStr = "";
        int currentRarityLevel = -1;
        int maxLevel = 4;
        for (int i = 0; i < rarities.Length; i++) {
            if (currentKey.Contains("_" + rarities[i] + "_")) {
                currentRarityIndex = i;
                var splitKey = currentKey.Split("_" + rarities[i] + "_");
                currentTypeStr = splitKey[0];
                int.TryParse(splitKey[1], out currentRarityLevel);
                break;
            }
        }
        if (currentRarityIndex != -1 && currentRarityLevel != -1) {
            if (currentRarityLevel < maxLevel) {
                string nextKey = currentTypeStr + "_" + rarities[currentRarityIndex] + "_" + (currentRarityLevel + 1);
                return allEquipment.TryGetValue(nextKey, out Equipment nextEquipment) ? nextEquipment : null;
            }
            else if (currentRarityIndex < rarities.Length - 1) {
                string nextKey = currentTypeStr + "_" + rarities[currentRarityIndex + 1] + "_1";
                return allEquipment.TryGetValue(nextKey, out Equipment nextEquipment) ? nextEquipment : null;
            }
        }
        return null;
    }

    public Equipment TryGetPreviousEquipment(string currentKey) {
        int currentRarityIndex = -1;
        string currentTypeStr = "";
        int currentRarityLevel = -1;
        for (int i = 0; i < rarities.Length; i++) {
            if (currentKey.Contains("_" + rarities[i] + "_")) {
                currentRarityIndex = i;
                var splitKey = currentKey.Split("_" + rarities[i] + "_");
                currentTypeStr = splitKey[0];
                int.TryParse(splitKey[1], out currentRarityLevel);
                break;
            }
        }
        if (currentRarityIndex != -1 && currentRarityLevel != -1) {
            if (currentRarityLevel > 1) {
                string previousKey = currentTypeStr + "_" + rarities[currentRarityIndex] + "_" +
                                     (currentRarityLevel - 1);
                return allEquipment.TryGetValue(previousKey, out Equipment prevEquipment) ? prevEquipment : null;
            }
            else if (currentRarityIndex > 0) {
                string previousKey = currentTypeStr + "_" + rarities[currentRarityIndex - 1] + "_4";
                return allEquipment.TryGetValue(previousKey, out Equipment prevEquipment) ? prevEquipment : null;
            }
        }
        return null;
    }

    public void AutoEquip(EEquipmentType selectItemType) {
        int index = 0;
        var typeNames = Enum.GetNames(typeof(EEquipmentType));
        for (int i = 0; i < typeNames.Length; i++) {
            if (typeNames[i] == selectItemType.ToString()) {
                index = i;
                break;
            }
        }

        Equipment item = null;
        switch (selectItemType) {
            case EEquipmentType.Weapon:
                item = GetBestItem(weapons);
                break;
            case EEquipmentType.Armor:
                item = GetBestItem(armors);
                break;
        }

        if (!ReferenceEquals(item, null)) {
            PlayerManager.instance.EquipItem(item, true);
            item.Save(ESaveType.IsEquipped);
        }
    }

    public Sprite GetFrame(ERarity rarity) {
        return rarityFrame[(int)rarity];
    }

    public bool CanComposite(EEquipmentType type) {
        switch (type) {
            case EEquipmentType.Weapon:
                return CanComposite(weapons);
            case EEquipmentType.Armor:
                return CanComposite(armors);
        }
        return false;
    }

    private bool CanComposite<T>(List<T> items) where T : Equipment, ICompositable {
        foreach (var item in items) {
            if (item.CanComposite())
                return true;
        }
        return false;
    }

    public void Enhance(IEnhanceable item) {
        CurrencyManager.instance.SubtractCurrency(ECurrencyType.EnhanceStone, item.GetEnhanceStone());
        if (item.TryEnhance(EnhancementMaxLevel))
            ++TotalEnhanceCount;
    }

    public sbyte CanEnhance(IEnhanceable item) {
        if (!item.CanEnhance(EnhancementMaxLevel))
            return -1;
        if (CurrencyManager.instance.GetCurrency(ECurrencyType.EnhanceStone) > item.GetEnhanceStone())
            return 1;
        return 0;
    }

    public void SaveEnhanceItem(Equipment item) {
        item.Save(ESaveType.EnhancementLevel);
        item.Save(ESaveType.RequiredEnhanceStone);
        Save(EEquipmentManagerSaveType.TotalEnhance);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(EquipmentManager))]
public class CustomEditorEquipmentManager : Editor {
    private EquipmentManager manager;

    private void OnEnable() {
        manager = target as EquipmentManager;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("초기화")) {
            manager.CreateAllArmor();
            manager.CreateAllWeapon();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif