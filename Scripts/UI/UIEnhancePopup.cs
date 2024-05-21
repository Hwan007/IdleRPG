using System.Text;
using Defines;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class UIEnhancePopup : UIPanel {
    [SerializeField] private HoldCheckerButton enhanceBtn;
    private Equipment equipment;

    // rare
    [SerializeField] private Image backEffect;

    // image
    [SerializeField] private Image itemImage;

    // 강화 ?�벨
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private TMP_Text rarityText;

    // cost
    [SerializeField] private Image costImage;
    [SerializeField] private TMP_Text costText;

    // currency
    [SerializeField] private Image currencyImage;
    [SerializeField] private TMP_Text currencyText;

    // effect
    [SerializeField] private TMP_Text ownedEffectText;
    [SerializeField] private TMP_Text equipedEffectText;

    [SerializeField] private Transform questGuide;

    public void ShowUI<T>(T item) where T : Equipment {
        base.ShowUI();

        costImage.sprite = CurrencyManager.instance.GetIcon(ECurrencyType.EnhanceStone);
        currencyImage.sprite = CurrencyManager.instance.GetIcon(ECurrencyType.EnhanceStone);

        equipment = item;
        // backEffect.color = item.myColor;
        backEffect.sprite = EquipmentManager.instance.GetFrame(item.rarity);

        if (EquipmentManager.instance.images.TryGetValue(item.equipName, out Sprite sprite))
            itemImage.sprite = sprite;

        rarityText.text = $"{Strings.rareKor[(int)item.rarity]} {item.rarityLevel}";

        switch (item) {
            case WeaponInfo weaponInfo:
                titleText.text = $" ( {weaponInfo.enhancementLevel} / {EquipmentManager.instance.EnhancementMaxLevel} )";
                break;
            case ArmorInfo armorInfo:
                titleText.text = $" ( {armorInfo.enhancementLevel} / {EquipmentManager.instance.EnhancementMaxLevel} )";
                break;
            default:
                titleText.text = "";
                break;
        }

        UpdateCostAndCurrency();
    }

    protected override void InitializeBtns() {
        base.InitializeBtns();

        enhanceBtn.onClick.AddListener(TryEnhanceItem);
        enhanceBtn.onExit.AddListener(SaveEnhanceItem);
    }

    private void SaveEnhanceItem() {
        EquipmentManager.instance.SaveEnhanceItem(equipment);
        CurrencyManager.instance.SaveCurrencies();
    }

    private void TryEnhanceItem() {
        var ret = EquipmentManager.instance.CanEnhance(equipment);
        if (ret == 1) {
            EquipmentManager.instance.Enhance(equipment);
            UpdateCostAndCurrency();
        }
        else if (ret == 0) {
            MessageUIManager.instance.ShowCenterMessage("");
        }
        else if (ret == -1) {
            MessageUIManager.instance.ShowCenterMessage("");
        }
        Debug.Assert(ret is >= -1 and <= 1, "Not Defined");
        // failed enhance item
    }

    private void UpdateCostAndCurrency() {
        var cost = equipment.GetEnhanceStone();
        costText.text = cost.ChangeToShort();

        var currency = CurrencyManager.instance.GetCurrency(ECurrencyType.EnhanceStone).ChangeToShort();
        currencyText.text = currency;

        StringBuilder sb = new StringBuilder();

        switch (equipment) {
            case WeaponInfo weaponInfo:
                sb.Clear().Append(" : ").Append("").Append(weaponInfo.ownedEffect).Append(CustomText.SetColor(" (\u25b2", EColorType.Green))
                    .Append(CustomText.SetColor((weaponInfo.ownedEffect + weaponInfo.baseOwnedEffect).ChangeToShort(),
                        EColorType.Green)).Append(")");
                ownedEffectText.text = sb.ToString();

                sb.Clear().Append(" : ").Append("").Append(weaponInfo.equippedEffect).Append(CustomText.SetColor(" (\u25b2", EColorType.Green))
                    .Append(CustomText.SetColor((weaponInfo.equippedEffect + weaponInfo.baseEquippedEffect).ChangeToShort(),
                        EColorType.Green)).Append(")");
                equipedEffectText.text = sb.ToString();
                titleText.text = $" ( {weaponInfo.enhancementLevel} / {EquipmentManager.instance.EnhancementMaxLevel} )";
                break;
            case ArmorInfo armorInfo:
                sb.Clear().Append(" : ").Append("").Append(armorInfo.ownedEffect).Append(CustomText.SetColor(" (\u25b2", EColorType.Green))
                    .Append(CustomText.SetColor((armorInfo.ownedEffect + armorInfo.baseOwnedEffect).ChangeToShort(),
                        EColorType.Green)).Append(")");
                ownedEffectText.text = sb.ToString();

                sb.Clear().Append(" : ").Append(" +").Append(armorInfo.equippedEffect).Append(CustomText.SetColor(" (\u25b2", EColorType.Green))
                    .Append(CustomText.SetColor((armorInfo.equippedEffect + armorInfo.baseEquippedEffect).ChangeToShort(),
                        EColorType.Green)).Append(")");
                equipedEffectText.text = sb.ToString();
                titleText.text = $" ( {armorInfo.enhancementLevel} / {EquipmentManager.instance.EnhancementMaxLevel} )";
                break;
            default:
                ownedEffectText.text = "";
                equipedEffectText.text = "";
                titleText.text = "";
                break;
        }
    }

    public override void ShowQuestRoot(EAchievementType type) {
        switch (type) {
            case EAchievementType.EquipEnhanceCount:
                questGuide.position = enhanceBtn.transform.position;
                questGuide.gameObject.SetActive(true);
                QuestManager.instance.currentQuest.onComplete += (x) => questGuide.gameObject.SetActive(false);
                break;
        }
    }
}