using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "SO/SkillSummonGacha", fileName = "SkillSummonGacha")]
public class SkillSummonGacha : ScriptableObject {
    public int[] weightPerRarities;
    private int totalWeight;

    public virtual BaseSkillData Summon() {
        InitWeight();
        var ran = Random.Range(1, totalWeight + 1);

        int current = 0;
        for (int i = 0; i < weightPerRarities.Length; ++i) {
            current += weightPerRarities[i];
            if (current >= ran) {
                var skills = SkillManager.instance.GetSkillsOnRarity((ERarity)i);
                Debug.Assert(skills.Count > 0, $"{(ERarity)i}");
                var index = Random.Range(0, skills.Count);
                return skills[index];
            }
        }

        Debug.Assert(false, "");
        return null;
    }

    public virtual void InitWeight() {
        int ret = 0;
        foreach (int weightPerRarity in weightPerRarities) {
            ret += weightPerRarity;
        }

        totalWeight = ret;
    }

    public virtual float GetPercentage(ERarity rarity) {
        float weight = weightPerRarities[(int)rarity];
        float percentage = weight / totalWeight;
        return percentage;
    }
}
