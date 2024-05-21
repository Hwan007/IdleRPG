using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Gacha", menuName = "ScriptableObject/Summon", order = 0)]
[Serializable]
public class EquipSummonGacha : ScriptableObject {
    public GachaPerLevel[] eachWeight;
    private int totalWeight;

    public EquipSummonGacha() {
        eachWeight = new GachaPerLevel[Enum.GetNames(typeof(ERarity)).Length - 1];
        totalWeight = 0;
    }

    public virtual string Summon() {
        StringBuilder sb = new StringBuilder();

        InitWeight();

        var ran = Random.Range(1, totalWeight + 1);

        GetRarityAndLevel(ref sb, ran);

        return sb.ToString();
    }

    protected virtual void GetRarityAndLevel(ref StringBuilder sb, int ran) {
        int current = 0;
        int rarity = 0;
        int level = 1;
        foreach (var perRarityLevel in eachWeight) {
            level = 1;
            for (int i = 0; i < 4; ++i) {
                var per = perRarityLevel.GetWeightPerLevel(i);
                Debug.Assert(per != 0, "");

                current += per;
                if (current >= ran) {
                    sb.Append((ERarity)rarity + "_" + level);
                    return;
                }
                level = Mathf.Clamp(level + 1, 1, 4);
            }
            rarity = Mathf.Clamp(rarity + 1, 0, 5);
        }
        sb.Append((ERarity)rarity + "_" + level);
        Debug.Assert(false, "");
    }

    public virtual void InitWeight() {
        int ret = 0;
        foreach (var gachaPerRarityLevel in eachWeight) {
            ret += gachaPerRarityLevel.GetWeight();
        }

        totalWeight = ret;
    }

    public virtual float GetPercentage(ERarity rarity) {
        return eachWeight[(int)rarity].GetPercentage(totalWeight);
    }
#if UNITY_EDITOR
    public virtual void InitSubWeight() {
        foreach (var gachaPerRarityLevel in eachWeight) {
            gachaPerRarityLevel.InitGacha();
        }
    }
#endif
}

[Serializable]
public class GachaPerLevel {
    [SerializeField] public int rarityWeight;
    [SerializeField] public int[] subWeight;

    public GachaPerLevel() {
        subWeight = new int[4] { 30, 25, 25, 20 };
    }

    public int GetWeight() {
        int ret = 0;
        foreach (var weight in subWeight) {
            ret += rarityWeight * weight;
        }

        return ret;
    }

    public int GetWeightPerLevel(int rareLevel) {
        var ret = subWeight[rareLevel] * rarityWeight;
        return ret;
    }

    public void InitGacha() {
        subWeight = new int[] { 30, 25, 25, 20 };
    }

    public float GetPercentage(float totalWeight) {
        float current = GetWeight();
        return current / totalWeight;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(EquipSummonGacha))]
public class SummonGachaEditor : Editor {
    private EquipSummonGacha gacha;

    private void OnEnable() {
        gacha = target as EquipSummonGacha;
        int lengthCondition = Enum.GetNames(typeof(ERarity)).Length - 1;

        if (gacha.eachWeight.Length != lengthCondition) {
            List<GachaPerLevel> temp = new List<GachaPerLevel>();
            temp.AddRange(gacha.eachWeight);

            if (temp.Count < lengthCondition) {
                while (temp.Count < lengthCondition)
                    temp.Add(new GachaPerLevel());
            }
            else if (temp.Count > lengthCondition) {
                while (temp.Count > lengthCondition)
                    temp.RemoveAt(temp.Count - 1);
            }

            gacha.eachWeight = temp.ToArray();
            EditorUtility.SetDirty(target);
        }
    }

    public override void OnInspectorGUI() {
        gacha.InitWeight();
        for (int i = 0; i < Enum.GetNames(typeof(ERarity)).Length - 1; ++i) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{(ERarity)i} °¡ÁßÄ¡");
            GUILayout.Label((100 * gacha.GetPercentage((ERarity)i)).ToString("F5") + "%");
            int total = EditorGUILayout.IntField("Total Weight", gacha.eachWeight[i].rarityWeight);
            if (total != gacha.eachWeight[i].rarityWeight) {
                gacha.eachWeight[i].rarityWeight = total;
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Weight per Level");
            for (int j = 0; j < 4; ++j) {
                var input = EditorGUILayout.IntField(gacha.eachWeight[i].subWeight[j]);
                if (input != gacha.eachWeight[i].subWeight[j]) {
                    gacha.eachWeight[i].subWeight[j] = input;
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}
#endif