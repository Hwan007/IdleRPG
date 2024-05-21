using System;
using System.Collections;
using System.Collections.Generic;
using Keiwando.BigInteger;
using Unity.Collections;
using UnityEngine;


[Serializable]
public class BaseLevelSystem {
    public event Action<int> onLevelChange;
    public event Action<BigInteger, BigInteger> onEarnEXP;
    public BigInteger ExpCap => expCap;
    public BigInteger Exp => currentExp;
    public int Level => currentLevel;
    protected BigInteger expCap = 10;
    protected int level = 1;
    protected BigInteger currentExp;
    protected int currentLevel;
    [SerializeField] protected int baseExp;
    [SerializeField] protected int maxLevel;

    public void InitSystem(int _level, BigInteger _exp) {
        expCap = GetRequiredExp(_level);
        level = _level;
        currentLevel = level;
        currentExp = _exp;
    }

    public virtual void EarnExp(int earn) {
        currentExp += earn;
        while (currentExp >= expCap) {
            LevelUp();
        }
        onEarnEXP?.Invoke(currentExp, expCap);
    }

    public virtual void EarnExp(BigInteger earn) {
        currentExp += earn;
        while (currentExp >= expCap) {
            LevelUp();
        }
        onEarnEXP?.Invoke(currentExp, expCap);
    }

    protected virtual BigInteger GetNextRequiredExp(BigInteger _exp) {
        _exp += _exp / 5;
        return _exp;
    }

    public virtual BigInteger GetRequiredExp(int _level) {
        return baseExp * Convert.ToInt32(Mathf.Pow(1.2f, _level - 1));
    }

    public virtual void LevelUp() {
        if (maxLevel <= level)
            return;
        currentExp -= expCap;
        expCap = GetNextRequiredExp(expCap);
        level++;
        currentLevel = level;
        onLevelChange?.Invoke(level);
    }

    public virtual void SaveLevelExp(string id) {
        DataManager.Instance.Save<int>($"{id}_{nameof(currentLevel)}", currentLevel);
        DataManager.Instance.Save<string>($"{id}_Í{nameof(currentExp)}", currentExp.ToString());
    }

    public virtual void LoadLevelExp(string id) {
        var lv = DataManager.Instance.Load<int>($"{id}_{nameof(currentLevel)}", 1);
        var ex = new BigInteger(DataManager.Instance.Load<string>($"{id}_{nameof(currentExp)}", "0"));
        InitSystem(lv, ex);
    }
}
