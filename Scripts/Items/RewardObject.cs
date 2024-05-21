using System;
using System.Collections;
using System.Collections.Generic;
using Keiwando.BigInteger;
using UnityEngine;
using Random = UnityEngine.Random;

public class RewardObject : MonoBehaviour {
    #region »ý·«
    public event Action onEnd;
    private Func<float, float, Vector3> moving;
    private float duration;
    private float elapsedTime;
    private Transform target;
    private Vector3 damping;

    public SpriteRenderer rewardImage;
    private float delay;
    private Vector3 startPos;

    public void InitRewardObject(MonsterDropData monsterDropData) {
        if (monsterDropData.rewardType < EQuestRewardType.BaseAtk)
            rewardImage.sprite = CurrencyManager.instance.GetIcon((ECurrencyType)monsterDropData.rewardType);
        else {
            // TODO
            // ready for other reward icon
        }
        onEnd += () => GameManager.instance.GetReward(monsterDropData.rewardType, monsterDropData.currentRewardAmount);
        onEnd += () => MessageUIManager.instance.ShowObtainMessage((ECurrencyType)monsterDropData.rewardType, monsterDropData.currentRewardAmount.ChangeToShort());
    }
    public RewardObject BackToPool(Queue<RewardObject> rewardPool) {
        onEnd += () => rewardPool.Enqueue(this);
        return this;
    }
    #endregion
    public void EndEffect() {
        onEnd?.Invoke();
        onEnd = null;
        gameObject.SetActive(false);
    }

    public RewardObject FlyTo(Transform target, float duration, float delay = 1.0f) {
        gameObject.SetActive(true);
        this.target = target;
        startPos = transform.position;
        this.duration = duration;
        this.delay = delay;
        elapsedTime = .0f;
        moving = Line;
        return this;
    }

    private Vector3 Line(float current, float full) {
        return Vector3.Lerp(transform.position, target.position, current / full);
    }

    private void Update() {
        if (ReferenceEquals(target, null) || target == null)
            EndEffect();
        elapsedTime += Time.deltaTime;
        if (elapsedTime > delay) {
            if (elapsedTime < (delay + duration) && Vector3.Distance(transform.position, target.position) > 0.1f) {
                transform.position = moving(elapsedTime - delay, duration);
            }
            else {
                EndEffect();
            }
        }
    }
}

[Serializable]
public class MonsterDropData {
    [field: SerializeField] public EQuestRewardType rewardType { get; protected set; }
    [field: SerializeField] public int baseRewardAmount { get; protected set; }
    [field: SerializeField] public int increasePerLevel { get; protected set; }
    [field: SerializeField] public int level { get; protected set; }
    public BigInteger currentRewardAmount {
        get => reward * Random.Range(90, 110) / 100;
        protected set => reward = value;
    }

    public BigInteger straightRewardAmount => reward;
    private BigInteger reward;

    public void SetLevel(int value) {
        level = value;
        currentRewardAmount = baseRewardAmount + (BigInteger)increasePerLevel * level;
    }

    public void InitCurrentReward() {
        currentRewardAmount = baseRewardAmount + (BigInteger)increasePerLevel * level;
    }
}