using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIEquipment : UIBase {
    private Equipment equipment;

    [SerializeField] public Toggle toggle;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text count;
    [SerializeField] private Slider countSlider;
    [SerializeField] private TMP_Text rarity;
    [SerializeField] private GameObject equipMark;

    private UIEquipmentPanel uiEquipmentPanel;

    public void AddListener(UnityAction<bool> action) {
        toggle.onValueChanged.AddListener(action);
    }

    public void SetUI<T>(T item, UIEquipmentPanel uiPanel) where T : Equipment {
        uiEquipmentPanel = uiPanel;

        equipment = item;
        //TODO show information of item
        if (EquipmentManager.instance.images.TryGetValue(item.equipName, out Sprite sprite))
            itemIcon.sprite = sprite;

        background.sprite = EquipmentManager.instance.GetFrame(item.rarity);
        count.text = $"{item.Quantity}/4";
        countSlider.value = item.Quantity;

        switch (equipment) {
            case WeaponInfo weapon:
                level.text = "Lv." + weapon.enhancementLevel.ToString();
                break;
            case ArmorInfo armor:
                level.text = "Lv." + armor.enhancementLevel.ToString();
                break;
            default:
                level.text = "";
                break;
        }
        rarity.text = $"{Strings.rareKor[(int)item.rarity]} {item.rarityLevel}";

        equipment.actOnEquipChange += UpdateEquippedMark;
        equipment.onQuantityChange += UpdateQuantityUI;
    }

    public void ShowUI(Equipment item, UIEquipmentPanel uiPanel) {
        // base.ShowUI();
        gameObject.SetActive(true);

        SetUI(item, uiPanel);
    }

    public override void CloseUI() {
        base.CloseUI();

        if (equipment != null) {
            equipment.actOnEquipChange -= UpdateEquippedMark;
            equipment.onQuantityChange -= UpdateQuantityUI;
            equipment = null;
        }
        gameObject.SetActive(false);
    }

    public Equipment GetInfo() {
        return equipment;
    }

    public void UpdateQuantityUI(int amount) {
        if (ReferenceEquals(equipment, null))
            return;
        count.text = amount.ToString() + "/4";
        countSlider.value = amount;
    }

    public void UpdateEnhanceLevelUI() {
        if (ReferenceEquals(equipment, null))
            return;

        switch (equipment) {
            case WeaponInfo weaponInfo:
                level.text = weaponInfo.enhancementLevel.ToString();
                break;
            case ArmorInfo armorInfo:
                level.text = armorInfo.enhancementLevel.ToString();
                break;
            default:
                level.text = "";
                break;
        }
    }

    public void UpdateEquippedMark(bool isEquip) {
        equipMark.SetActive(isEquip);
        if (isEquip)
            toggle.isOn = true;
    }

    public void RevealUI() {
        gameObject.SetActive(true);
        equipment.onOwned -= RevealUI;
    }
}