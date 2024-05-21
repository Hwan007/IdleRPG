using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour {
    public static EffectManager instance;
    [SerializeField] RewardObject rewardPrefab;
    private Queue<RewardObject> rewardPool;
    public float flyTime;
    public float delayTime;

    private void Awake() {
        instance = this;
        rewardPool = new Queue<RewardObject>();
    }

    public void DropReward(Vector3 worldPosition, MonsterDropData[] rewardDatas) {
        RewardObject obj;
        foreach (var reward in rewardDatas) {
            if (reward.rewardType == EQuestRewardType.Exp) {
                PlayerManager.instance.levelSystem.EarnExp(reward.currentRewardAmount);
                continue;
            }

            if (rewardPool.Count > 0) {
                obj = rewardPool.Dequeue();
            }
            else {
                obj = Instantiate(rewardPrefab);
            }

            obj.transform.position = worldPosition;
            obj.InitRewardObject(reward);
            obj.BackToPool(rewardPool);
            obj.FlyTo(PlayerManager.instance.player.transform, flyTime, delayTime);
        }
    }
}
