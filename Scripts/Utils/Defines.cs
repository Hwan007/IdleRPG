using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Defines {
    public enum ELockType {
        LockIcon,
        Appear,
    }
    public enum ESkillDataType {
        Quantity,
        Level,
        EquipIndex,
        IsOwned,
    }
    public enum ESaveType {
        Quantity,
        IsEquipped,
        EnhancementLevel,
        IsOwned,
        EquippedEffect,
        OwnedEffect,
        RequiredEnhanceStone
    }
    public enum EIconType {
        Heart,
        Clock,
        Skull
    }
    public enum EDungeonType {
        Gold,
        Awaken,
        Enhance,
    }
    public enum ESkillType {
        Active,
        Buff,
        Passive,
    }
    public enum ESkillAttackType {
        Single,
        Multiple,
    }
    public enum EMonsterType {
        Basic, Elite, Boss, Obstacle
    }
    public enum EStageState {
        Normal, Inter, Boss,
        Dungeon
    }

    public enum EColorType {
        Gold,
        Green
    }

    public enum EShopType {
        Gold,
        Dia,
        Package,
        DarkMarket,
    }

    public enum EFsmState {
        Stop = -1,
        Idle = 0,
        Run,
        Death,
        Spawn,
        Dash,
        Hit,
        NormalAttack,
        SkillAttack1,
        SkillAttack2,
        SkillAttack3,
        SkillAttack4,
        SkillAttack5,
        SkillAttack6,
        SkillAttack7,
        SkillAttack8,
        SkillAttack9,
        SkillAttack10,
        SkillAttack11,
        SkillAttack12,
        SkillAttack13,
        SkillAttack14,
        SkillAttack15,
        SkillAttack16,
        SkillAttack17,
        SkillAttack18,
        SkillAttack19,
        SkillAttack20,
        SkillAttack21,
        SkillAttack22,
        SkillAttack23,
        SkillAttack24,
    }

    public enum EStatusType {
        ATK, // 공격
        HP, // 체력
        DMG_REDU, // ?��?지 감소
        MP, // 마나
        MP_RECO, // 마나 ?�복
        CRIT_CH, // 치명?� ?�률
        CRIT_DMG, // 치명?� 증폭
        ATK_SPD, // 공격 ?�도
        MOV_SPD, // ?�동 ?�도
        SKILL_DMG, // ?�킬 증폭
    }

    public enum ETrainingType {
        Normal,
        Awaken,
        Speciality,
        Relic,
    }

    public enum ECalculatePositionType {
        Circle,
        Line,
        Outback,
        Stop,
    }

    public enum EDataType {
        Attack, Health, AttackSpeed, Accuracy, CritRange, CritDamage,
        CurrentHealth,
        CurrentExp, MaxExp, CurrentLevel,
    }

    public enum EUpgradeType {
        Training,
        Awaken,
        SummonWeapon,
        SummonArmor,
        SummonSkill,
        WeaponEquip,
        ArmorEquip,
        SkillEquip,
        GoldDungeon,
        AwakenDungeon,
        EnhanceDungeon,
    }

    public enum EEquipmentManagerSaveType {
        TotalEnhance,
        TotalWeaponComposite,
        TotalArmorComposite,
    }
}

public enum EAchievementType {
    WeaponEquip = 0,
    ArmorEquip,
    SkillEquip,
    UseSpecialSkill,
    UseSkill,

    GoldDungeonLevel = 5,
    AwakenDungeonLevel,
    EnhanceDungeonLevel,

    StatUpgradeCount = 10,
    AttackUpgradeCount,
    HealthUpgradeCount,

    WeaponSummonCount = 15,
    ArmorSummonCount,
    SkillSummonCount,
    TotalSummonCount,

    ClearStageLevel = 20,
    ReachPlayerLevel,

    WeaponCompositeCount = 25,
    ArmorCompositeCount,

    KillCount = 30,

    UseAutoSkill = 35,
    ClickQuestBar,

    LightningGem = 40,
    GuardianGem,
    DestinyGem,
    TempestGem,
    RageGem,
    AbyssGem,

    EquipEnhanceCount,
    SkillLevelUp,
}

public enum ECurrencyType {
    Gold = 0, // 골드
    Dia, // ?�이??
    EnhanceStone, // 강화??
    AwakenStone, // 각성??
    WeaponSummonTicket, // 무기 ?�환 ?�켓
    ArmorSummonTicket, // 방어�??�환 ?�켓
    GoldInvitation, // 골드 ?�전 ?�장�?
    AwakenInvitation, // 각성 ?�전 ?�장�?
    EnhanceInvitation,
    Exp,
}

public enum EQuestRewardType {
    Gold = 0,
    Dia,
    EnhanceStone,
    AwakenStone,
    WeaponSummonTicket,
    ArmorSummonTicket,
    GoldInvitation,
    AwakenInvitation,
    EnhanceInvitation,
    Exp,
    BaseAtk,
    BaseHp,
    BaseDef,
    BaseCritCh,
    BaseCritDmg,
    BaseAtkSpd,
}

public enum ENormalRewardType {
    Gold = 0,
    Dia,
    EnhanceStone,
    AwakenStone,
    WeaponSummonTicket,
    ArmorSummonTicket,
    GoldInvitation,
    AwakenInvitation,
    EnhanceInvitation,
    Exp,
    Weapon,
    Armor,
    None
}


// ?�비 ?�??
public enum EEquipmentType {
    Weapon,
    Armor,
    Skill,
    Accessory
    // 기�? ?�비 ?�??..
}

// ?��???
public enum ERarity {
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythology,
    // Ancient

    None
    // 기�? ?��???..
}