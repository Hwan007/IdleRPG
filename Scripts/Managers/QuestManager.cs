using System;
using System.Collections.Generic;
using System.Linq;
using Defines;
using Keiwando.BigInteger;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuestManager : MonoBehaviour {
    public static QuestManager instance;

    #region ?ÑÎìú ?ùÎûµ
    private Dictionary<EAchievementType, AchievementCounter> counterDictionary;

    [SerializeField] private BaseAchievement[] achievements;
    public StackAchievement[] quests;
    public RepeatAchievement[] repeatQuest;

    private HashSet<AchievementCounter> stoppedCounter;

    public static EAchievementType[] repeatQuestDescriptionTypeA =
    {
        EAchievementType.KillCount, EAchievementType.WeaponSummonCount, EAchievementType.ArmorSummonCount,
        EAchievementType.SkillSummonCount, EAchievementType.TotalSummonCount
    };

    public static EAchievementType[] repeatQuestDescriptionTypeB =
    {
        EAchievementType.StatUpgradeCount, EAchievementType.AttackUpgradeCount, EAchievementType.HealthUpgradeCount,
        EAchievementType.ReachPlayerLevel
    };

    // ?ÖÏ†Å ?ÑÏö© Î≥¥ÏÉÅ ?âÎèô
    private BaseRewardAction[] rewards;

    // ÏßÑÌñâÏ§ëÏù∏ ?ÖÏ†Å Î∞??òÏä§??
    public Queue<BaseAchievement> ProgressQuest { get; private set; }
    public BaseAchievement currentQuest { get; private set; }

    [SerializeField] private EAchievementType[] stackCounterList;

    [SerializeField] private EAchievementType[] preCounterList;
    #endregion

    private void Awake() {
        instance = this;
        counterDictionary = new Dictionary<EAchievementType, AchievementCounter>();
        ProgressQuest = new Queue<BaseAchievement>();
        stoppedCounter = new HashSet<AchievementCounter>();
    }

    public void InitQuestManager() {
        InitCounter();
        Debug.Assert(quests != null);
        AddToProgressQueue(quests);
        AddToProgressQueue(repeatQuest);
        rewards = InitRewardActions();
        Load();
    }

    private void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            foreach (var (type, counter) in counterDictionary)
                counter.Save();
            foreach (var quest in repeatQuest)
                quest.Save();
        }
    }

    public void Load() {
        var id = DataManager.Instance.Load<string>($"{nameof(currentQuest)}_{nameof(currentQuest.achievementID)}",
            "init");
        var count = DataManager.Instance.Load($"{nameof(currentQuest)}_{nameof(currentQuest.count)}", 0);
        currentQuest = ProgressQuest.Dequeue();

        if (id == "init") {
            SaveCurrentQuestID();
            SubscribeCounter(currentQuest);
            var ui = UIManager.instance.TryGetUI<UIQuestBar>();
            ui.ShowUI();
#if !UNITY_EDITOR
            Firebase.Analytics.FirebaseAnalytics.LogEvent($"current_quest_{(currentQuest.GetID())}");
#endif
            ui.ShowQuestRoot(currentQuest.type);
            return;
        }

        if (id == "none") {
            currentQuest = new BaseAchievement("none", "none", "none", EAchievementType.WeaponEquip);
            SaveCurrentQuestID();
            UIManager.instance.TryGetUI<UIQuestBar>().CloseUI();
            return;
        }

        while (id != currentQuest.achievementID) {
            if (ProgressQuest.Count > 0)
                currentQuest = ProgressQuest.Dequeue();
            else {
                Debug.Assert(true, "Quest not found");
                currentQuest = new BaseAchievement("none", "none", "none", EAchievementType.WeaponEquip);
                SaveCurrentQuestID();
                UIManager.instance.TryGetUI<UIQuestBar>().CloseUI();
                break;
            }
        }

        currentQuest.count = count;
        if (currentQuest.GetGoal() <= count) {
            currentQuest.CompleteAchievement();
        }
        else {
            SubscribeCounter(currentQuest);
        }

        UIManager.instance.TryGetUI<UIQuestBar>().ShowUI();
    }

    public void SaveCurrentQuestID() {
        DataManager.Instance.Save($"{nameof(currentQuest)}_{nameof(currentQuest.achievementID)}",
            currentQuest.achievementID);
    }

    private void SaveCurrentCount() {
        DataManager.Instance.Save($"{nameof(currentQuest)}_{nameof(currentQuest.count)}", currentQuest.count);
    }

    public void MoveToNextQuest() {
        UIManager.instance.TryGetUI<UIQuestBar>().CloseUI();
        if (ProgressQuest.Count == 0) {
            foreach (var quest in repeatQuest)
                ProgressQuest.Enqueue(quest);
        }

        currentQuest = ProgressQuest.Dequeue();
        SubscribeCounter(currentQuest);
        UIManager.instance.TryGetUI<UIQuestBar>().ShowUI();

        SaveCurrentQuestID();
        SaveCurrentCount();
#if !UNITY_EDITOR
        Firebase.Analytics.FirebaseAnalytics.LogEvent($"current_quest_{(currentQuest.GetID())}");
#endif
    }

    private void InitCounter() {
        foreach (var type in preCounterList) {
            var counter = new AchievementCounter {
                achievementType = type
            };
            InitializeCounter(counter);
            counterDictionary.Add(counter.achievementType, counter);
        }

        foreach (var type in stackCounterList) {
            var counter = new AchievementCounter {
                achievementType = type
            };
            InitializeCounter(counter);
            counterDictionary.Add(counter.achievementType, counter);
        }
    }

    private void AddToProgressQueue<T>(T[] quest) where T : BaseAchievement {
        for (int i = 0; i < quest.Length; ++i) {
            AddToProgressQueue(quest[i]);
        }
    }

    private void AddToProgressQueue(BaseAchievement quest) {
        quest.InitializeInfo(this);
        quest.Load();

        if (quest.isRewarded)
            return;
        ProgressQuest.Enqueue(quest);
    }

    public void SubscribeCounter(BaseAchievement achievement) {
        AchievementCounter counter;
        if (counterDictionary.TryGetValue(achievement.type, out counter)) {
            counter.onCounter += achievement.UpdateCounter;
            ++counter.Watcher;
            achievement.InitCount();
        }
        else {
            counter = new AchievementCounter { achievementType = achievement.type };
            InitializeCounter(counter);
            counterDictionary.Add(counter.achievementType, counter);

            counter.onCounter += achievement.UpdateCounter;
            ++counter.Watcher;
            achievement.InitCount();
        }
    }

    public void UnsubscribeCounter(BaseAchievement achievement) {
        if (!counterDictionary.ContainsKey(achievement.type))
            return;

        counterDictionary[achievement.type].onCounter -= achievement.UpdateCounter;
        --counterDictionary[achievement.type].Watcher;

        if (!stackCounterList.Contains(achievement.type))
            RemoveCounter(achievement.type);
    }

    private bool RemoveCounter(EAchievementType type) {
        if (!counterDictionary.ContainsKey(type))
            return false;
        if (counterDictionary[type].Watcher == 0) {
            StopCounter(type);
            counterDictionary.Remove(type, out AchievementCounter counter);
            return true;
        }
        else {
            return false;
        }
    }

    public void StopCounter(EAchievementType type) {
        if (!counterDictionary.ContainsKey(type))
            return;
        var counter = counterDictionary[type];
        if (!stoppedCounter.Add(counter))
            return;

        switch (counter.achievementType) {
            case EAchievementType.WeaponEquip:
                PlayerManager.instance.onEquipItem -= counter.CountWeaponEquip;
                break;
            case EAchievementType.ArmorEquip:
                PlayerManager.instance.onEquipItem -= counter.CountArmorEquip;
                break;
            case EAchievementType.SkillEquip:
                PlayerManager.instance.onEquipSkill -= counter.CountOnce;
                break;
            case EAchievementType.UseSpecialSkill:
                PlayerManager.instance.player.controller.onSpecial -= counter.CountOnce;
                break;
            case EAchievementType.ClearStageLevel:
                StageManager.instance.onMaxClearStageChanged -= counter.CountSetAs;
                break;
            case EAchievementType.ReachPlayerLevel:
                PlayerManager.instance.levelSystem.onLevelChange -= counter.CountSetAs;
                break;
            case EAchievementType.GoldDungeonLevel:
                StageManager.instance.goldDungeon.onDungeonLevelUP -= counter.CountSetAs;
                break;
            case EAchievementType.AwakenDungeonLevel:
                StageManager.instance.awakenDungeon.onDungeonLevelUP -= counter.CountSetAs;
                break;
            case EAchievementType.EnhanceDungeonLevel:
                StageManager.instance.enhanceDungeon.onDungeonLevelUP -= counter.CountSetAs;
                break;
            case EAchievementType.AttackUpgradeCount:
            case EAchievementType.HealthUpgradeCount:
                UpgradeManager.instance.onTrainingTypeAndCurrentLevel -= counter.CountSetAsStatus;
                break;
            case EAchievementType.WeaponSummonCount:
                SummonManager.instance.onWeaponSummonTotal -= counter.CountSetAs;
                break;
            case EAchievementType.ArmorSummonCount:
                SummonManager.instance.onArmorSummonTotal -= counter.CountSetAs;
                break;
            case EAchievementType.KillCount:
                StageManager.instance.OnMonsterKill -= counter.CountPerInvoke;
                break;
            case EAchievementType.WeaponCompositeCount:
                EquipmentManager.instance.onWeaponCompositeTotal -= counter.CountSetAs;
                break;
            case EAchievementType.ArmorCompositeCount:
                EquipmentManager.instance.onArmorCompositeTotal -= counter.CountSetAs;
                break;
            case EAchievementType.SkillSummonCount:
                SummonManager.instance.onSkillSummonTotal -= counter.CountSetAs;
                break;
            case EAchievementType.UseSkill:
                PlayerManager.instance.player.controller.onActiveSkill -= counter.CountPerInvoke;
                PlayerManager.instance.player.controller.onBuffSkill -= counter.CountPerInvoke;
                break;
            case EAchievementType.UseAutoSkill:
                SkillManager.instance.onAutoSkill -= counter.CountOnce;
                break;
            case EAchievementType.DestinyGem:
            case EAchievementType.TempestGem:
            case EAchievementType.LightningGem:
            case EAchievementType.GuardianGem:
            case EAchievementType.RageGem:
            case EAchievementType.AbyssGem:
                UpgradeManager.instance.onAwakenUpgrade -= counter.CountSetAsStatus;
                break;
            case EAchievementType.EquipEnhanceCount:
                EquipmentManager.instance.onEnhanceTotal -= counter.CountSetAs;
                break;
            case EAchievementType.ClickQuestBar:
                UIManager.instance.TryGetUI<UIQuestBar>().clearBtn.onClick.RemoveListener(counter.CountOnce);
                break;
            case EAchievementType.SkillLevelUp:
                SkillManager.instance.onSkillLevelUpTotal -= counter.CountSetAs;
                break;
        }
    }

    public void RestartCounter(EAchievementType type) {
        if (!counterDictionary.ContainsKey(type))
            return;
        var counter = counterDictionary[type];
        if (!stoppedCounter.Remove(counter))
            return;

        switch (counter.achievementType) {
            case EAchievementType.KillCount:
                StageManager.instance.OnMonsterKill += counter.CountPerInvoke;
                break;
        }
    }

    private void InitializeCounter(AchievementCounter counter) {
        int initValue = 0;
        switch (counter.achievementType) {
            case EAchievementType.WeaponEquip:
                PlayerManager.instance.onEquipItem += counter.CountWeaponEquip;
                break;
            case EAchievementType.ArmorEquip:
                PlayerManager.instance.onEquipItem += counter.CountArmorEquip;
                break;
            case EAchievementType.SkillEquip:
                PlayerManager.instance.onEquipSkill += counter.CountOnce;
                break;
            case EAchievementType.UseSpecialSkill:
                PlayerManager.instance.player.controller.onSpecial += counter.CountOnce;
                break;
            case EAchievementType.ClearStageLevel:
                StageManager.instance.onMaxClearStageChanged += counter.CountSetAs;
                initValue = 20 * StageManager.instance.MaxClearStage * StageManager.instance.MaxClearSection + StageManager.instance.MaxClearNumber;
                break;
            case EAchievementType.ReachPlayerLevel:
                PlayerManager.instance.levelSystem.onLevelChange += counter.CountSetAs;
                initValue = PlayerManager.instance.levelSystem.Level;
                break;
            case EAchievementType.GoldDungeonLevel:
                StageManager.instance.goldDungeon.onDungeonLevelUP += counter.CountSetAs;
                initValue = StageManager.instance.goldDungeon.dungeonLevel;
                break;
            case EAchievementType.AwakenDungeonLevel:
                StageManager.instance.awakenDungeon.onDungeonLevelUP += counter.CountSetAs;
                initValue = StageManager.instance.awakenDungeon.dungeonLevel;
                break;
            case EAchievementType.EnhanceDungeonLevel:
                StageManager.instance.enhanceDungeon.onDungeonLevelUP += counter.CountSetAs;
                initValue = StageManager.instance.enhanceDungeon.dungeonLevel;
                break;
            case EAchievementType.AttackUpgradeCount:
            case EAchievementType.HealthUpgradeCount:
                UpgradeManager.instance.onTrainingTypeAndCurrentLevel += counter.CountSetAsStatus;
                initValue = UpgradeManager.instance
                    .statUpgradeInfo[(int)(counter.achievementType - EAchievementType.AttackUpgradeCount)].level;
                break;
            case EAchievementType.WeaponSummonCount:
                SummonManager.instance.onWeaponSummonTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(SummonManager.instance.TotalWeaponSummonCount);
                break;
            case EAchievementType.ArmorSummonCount:
                SummonManager.instance.onArmorSummonTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(SummonManager.instance.TotalArmorSummonCount);
                break;
            case EAchievementType.TotalSummonCount:
                SummonManager.instance.onWeaponSummonTotal += counter.CountSetAs;
                SummonManager.instance.onArmorSummonTotal += counter.CountSetAs;
                SummonManager.instance.onSkillSummonTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(SummonManager.instance.TotalWeaponSummonCount) +
                            BigInteger.ToInt32(SummonManager.instance.TotalArmorSummonCount) +
                            BigInteger.ToInt32(SummonManager.instance.TotalSkillSummonCount);
                break;
            case EAchievementType.KillCount:
                StageManager.instance.OnMonsterKill += counter.CountPerInvoke;
                break;
            case EAchievementType.WeaponCompositeCount:
                EquipmentManager.instance.onWeaponCompositeTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(EquipmentManager.instance.TotalWeaponComposite);
                break;
            case EAchievementType.ArmorCompositeCount:
                EquipmentManager.instance.onArmorCompositeTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(EquipmentManager.instance.TotalArmorComposite);
                break;
            case EAchievementType.SkillSummonCount:
                SummonManager.instance.onSkillSummonTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(SummonManager.instance.TotalSkillSummonCount);
                break;
            case EAchievementType.UseSkill:
                PlayerManager.instance.player.controller.onActiveSkill += counter.CountPerInvoke;
                PlayerManager.instance.player.controller.onBuffSkill += counter.CountPerInvoke;
                break;
            case EAchievementType.UseAutoSkill:
                SkillManager.instance.onAutoSkill += counter.CountOnce;
                break;
            case EAchievementType.DestinyGem:
            case EAchievementType.TempestGem:
            case EAchievementType.LightningGem:
            case EAchievementType.GuardianGem:
            case EAchievementType.RageGem:
            case EAchievementType.AbyssGem:
                UpgradeManager.instance.onAwakenUpgrade += counter.CountSetAsStatus;
                initValue = UpgradeManager.instance.awakenUpgradeInfo[counter.achievementType - EAchievementType.LightningGem].level;
                break;
            case EAchievementType.EquipEnhanceCount:
                EquipmentManager.instance.onEnhanceTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(EquipmentManager.instance.TotalEnhanceCount);
                break;
            case EAchievementType.ClickQuestBar:
                UIManager.instance.TryGetUI<UIQuestBar>().clearBtn.onClick.AddListener(counter.CountOnce);
                break;
            case EAchievementType.SkillLevelUp:
                SkillManager.instance.onSkillLevelUpTotal += counter.CountSetAs;
                initValue = BigInteger.ToInt32(SkillManager.instance.TotalSkilllevelUp);
                break;
        }
        counter.Load(initValue);
    }

    private BaseRewardAction[] InitRewardActions() {
        BaseRewardAction[] reward = new BaseRewardAction[Enum.GetNames(typeof(EQuestRewardType)).Length];
        for (int i = 0; i < reward.Length; ++i) {
            reward[i] = new BaseRewardAction();
            reward[i].InitializeReward((EQuestRewardType)i);
        }

        return reward;
    }

    public int GetCheckerCount(EAchievementType type) {
        return counterDictionary[type].count;
    }

    public AchievementCounter GetChecker(EAchievementType type) {
        return counterDictionary[type];
    }

    public bool GiveReward(EQuestRewardType type, int amount) {
        Debug.Assert(rewards.Length > (int)type, "?ïÏùò?òÏ? ?äÏ? ERewardTypeÍ∞íÏùÑ ?¨Ïö©?òÎ†§Í≥??©Îãà??");
        return rewards[(int)type].GetReward(amount);
    }

    public bool TryClearCurrentQuest() {
        if (currentQuest.isComplete) {
            currentQuest.GetReward();

            UIManager.instance.TryGetUI<UIRewardPanel>()?.ShowUI((ECurrencyType)currentQuest.GetRewardType(),
                (BigInteger)currentQuest.GetRewardAmount());
            MoveToNextQuest();

            return true;
        }
        else {
            return false;
        }
    }
}

[Serializable]
public class AchievementCounter {
    public EAchievementType achievementType;
    public int count { get; private set; }
    public event Action<int> onCounter;
    public int Watcher { get; set; } = 0;

    public void CountPlus(int value) {
        if (value > 0) {
            count += value;
            onCounter?.Invoke(value);
        }
    }

    public void CountPerInvoke<T0>(T0 value0) {
        count += 1;
        onCounter?.Invoke(count);
    }

    public void CountPerInvoke<T0, T1>(T0 value0, T1 value1) {
        count += 1;
        onCounter?.Invoke(count);
    }

    public void CountSetAs(int value) {
        count = value;
        onCounter?.Invoke(count);
    }

    public void CountSetAs(BigInteger value) {
        if (value > int.MaxValue)
            count = int.MaxValue;
        else
            count = BigInteger.ToInt32(value);
        CountSetAs(count);
    }

    public void CountOnce() {
        count = 1;
        onCounter?.Invoke(count);
    }

    public void CountOnce<T0>(T0 value1) {
        count = 1;
        onCounter?.Invoke(count);
    }

    public void CountOnce<T0, T1>(T0 value1, T1 value2) {
        count = 1;
        onCounter?.Invoke(count);
    }

    public void Save() {
        DataManager.Instance.Save<int>($"{nameof(AchievementCounter)}_{achievementType.ToString()}", count);
    }

    public void Load(int initValue = 0) {
        if (achievementType
            is EAchievementType.WeaponEquip
            or EAchievementType.ArmorEquip
            or EAchievementType.SkillEquip
            or EAchievementType.UseSpecialSkill
            or EAchievementType.UseSkill
            or EAchievementType.UseAutoSkill)
            count = DataManager.Instance.Load<int>($"{nameof(AchievementCounter)}_{achievementType.ToString()}", 0);
        else
            count = initValue;
    }

    public void CountWeaponEquip(Equipment from, Equipment to) {
        if (to.type == EEquipmentType.Weapon)
            CountOnce();
    }

    public void CountArmorEquip(Equipment from, Equipment to) {
        if (to.type == EEquipmentType.Armor)
            CountOnce();
    }

    public void CountSetAsStatus(EStatusType type, int level) {
        switch (type, achievementType) {
            case (EStatusType.ATK, EAchievementType.AttackUpgradeCount):
            case (EStatusType.HP, EAchievementType.HealthUpgradeCount):
            case (EStatusType.ATK, EAchievementType.LightningGem):
            case (EStatusType.DMG_REDU, EAchievementType.GuardianGem):
            case (EStatusType.CRIT_CH, EAchievementType.DestinyGem):
            case (EStatusType.CRIT_DMG, EAchievementType.TempestGem):
            case (EStatusType.ATK_SPD, EAchievementType.RageGem):
            case (EStatusType.SKILL_DMG, EAchievementType.AbyssGem):
                CountSetAs(level);
                break;
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(QuestManager))]
public class CustomEditorQuestManaver : Editor {
    private TextAsset csvFile1;
    private TextAsset csvFile2;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();


        csvFile1 = EditorGUILayout.ObjectField("?®Ïùº ?òÏä§??CSV File", csvFile1, typeof(TextAsset), true) as TextAsset;
        if (GUILayout.Button("Load QuestData from CSV")) {
            LoadGuideQuestFromCSV(csvFile1);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        csvFile2 = EditorGUILayout.ObjectField("Î∞òÎ≥µ ?òÏä§??CSV File", csvFile2, typeof(TextAsset), true) as TextAsset;
        if (GUILayout.Button("Load QuestData from CSV")) {
            LoadLevelQuestFromCSV(csvFile2);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void LoadGuideQuestFromCSV(TextAsset csv) {
        List<StackAchievement> onlyoneQuests = new List<StackAchievement>();

        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Ï≤?Î≤àÏß∏ Ï§??§Îçî) Í±¥ÎÑà?∞Í∏∞
        {
            string line = lines[i];
            if (!string.IsNullOrWhiteSpace(line)) {
                string[] fields = line.Split(',');

                bool isSuccess = false;
                string str;

                string id = fields[0].Trim();
                string title = fields[1].Trim();
                string description = fields[2].Trim();

                str = fields[3].Trim();
                isSuccess = int.TryParse(str, out int type);
                if (!isSuccess) {
                    Debug.LogWarning($"Failed 3 => {str}");
                    continue;
                }

                str = fields[4].Trim();
                isSuccess = int.TryParse(str, out int goal);
                if (!isSuccess) {
                    Debug.LogWarning($"Failed 4 => {str}");
                    continue;
                }

                str = fields[5].Trim();
                isSuccess = int.TryParse(str, out int descGoal);
                if (!isSuccess) {
                    Debug.LogWarning($"Failed 5 => {str}");
                    continue;
                }

                str = fields[6].Trim();
                isSuccess = int.TryParse(str, out int rewardType);
                if (!isSuccess) {
                    Debug.LogWarning($"Failed 5 => {str}");
                    continue;
                }

                str = fields[7].Trim();
                isSuccess = int.TryParse(str, out int reward);
                if (!isSuccess) {
                    Debug.LogWarning($"Failed 6 => {str}");
                    continue;
                }

                StackAchievement quest =
                    new StackAchievement(id, title, description,
                        (EAchievementType)type, goal, descGoal, (EQuestRewardType)rewardType, reward);

                onlyoneQuests.Add(quest);
            }
        }

        (target as QuestManager).quests = onlyoneQuests.ToArray();
        EditorUtility.SetDirty(target);
    }

    private void LoadLevelQuestFromCSV(TextAsset csv) {
        List<RepeatAchievement> repeatQuests = new List<RepeatAchievement>();

        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) {
            string line = lines[i];
            if (!string.IsNullOrWhiteSpace(line)) {
                string[] fields = line.Split(',');

                bool isSuccess = false;
                string str;

                string id = fields[0].Trim();
                string title = fields[1].Trim();
                string description = fields[2].Trim();

                str = fields[3].Trim();
                isSuccess = int.TryParse(str, out int type);
                if (!isSuccess) {
                    Debug.LogWarning($"Failed 3 => {str}");
                    continue;
                }

                str = fields[4].Trim();
                string[] levelStr = str.Split(' ');
                List<int> level = new List<int>();
                foreach (var item in levelStr) {
                    if (int.TryParse(item, out int ret))
                        level.Add(ret);
                }

                str = fields[5].Trim();
                string[] goalStr = str.Split(' ');
                List<int> goal = new List<int>();
                foreach (var item in goalStr) {
                    if (int.TryParse(item, out int ret))
                        goal.Add(ret);
                }

                str = fields[6].Trim();
                int.TryParse(str, out int rewardType);

                str = fields[7].Trim();
                string[] rewardStr = str.Split(' ');
                List<int> rewardAmount = new List<int>();
                foreach (var item in rewardStr) {
                    if (int.TryParse(item, out int ret))
                        rewardAmount.Add(ret);
                }

                if (level.Count != goal.Count || level.Count != rewardAmount.Count) {
                    Debug.LogWarning($"count doesn't match. => line{i}");
                    continue;
                }

                var quest = new RepeatAchievement(level.ToArray(), goal.ToArray(), (EQuestRewardType)rewardType,
                    rewardAmount.ToArray(), id, title, description, (EAchievementType)type);
                repeatQuests.Add(quest);
            }
        }

        (target as QuestManager).repeatQuest = repeatQuests.ToArray();
        EditorUtility.SetDirty(target);
    }
}
#endif