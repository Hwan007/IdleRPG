using System;
using System.Collections.Generic;
using Character.Monster;
using UnityEngine;

public class AttackCollider : MonoBehaviour {
    protected AttackSystem attackSystem;
    protected LayerMask targetLayer;
    protected string targetTag;
    protected AttackData attackData;
    protected bool isContinuous;
    protected int attackCount;
    private int count;
    private float elapsedTime;
    private LinkedList<BaseData> tickList;
    private Transform attacker;

    private void Awake() {
        tickList = new LinkedList<BaseData>();
    }

    public void ClearTickList() {
        if (isContinuous)
            tickList.Clear();
    }

    private void Update() {
        elapsedTime += Time.deltaTime;

        if (isContinuous && attackCount > count && elapsedTime / attackData.tickUnitTime > count) {
            ++count;
            var tick = tickList.First;
            while (!ReferenceEquals(tick, null)) {
                if (!tick.Value.IsDead)
                    tick.Value.health.SubstractHP(tick.Value.transform.position - transform.position, attackData);
                tick = tick.Next;
            }
        }
    }

    public virtual void InitAttackCollider(BaseData character) {
        attackSystem = character.attackSystem;
        targetLayer = character.targetLayerMask;
        targetTag = character.targetTag;
        attacker = character.transform;
    }

    public virtual void InitAttackCollider(MonsterData monster) {
        attackSystem = monster.attackSystem;
        targetLayer = monster.targetLayerMask;
        targetTag = monster.targetTag;
        attacker = monster.transform;
    }

    public virtual void SetAttackData(AttackData attackData) {
        this.attackData = attackData;
        isContinuous = attackData.isContinuous;
        attackCount = attackData.attackCount;
        elapsedTime = .0f;
        count = 0;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (isContinuous) {
            if ((1 << collision.gameObject.layer) ==
                ((1 << collision.gameObject.layer) & targetLayer)) {
                var data = collision.GetComponent<BaseData>();
                if (data.IsDead) {
                    return;
                }

                tickList.AddLast(data);
            }
        }
        else {
            if ((1 << collision.gameObject.layer) ==
                ((1 << collision.gameObject.layer) & targetLayer)) {
                var data = collision.GetComponent<BaseData>();
                if (data.IsDead) {
                    return;
                }

                data.health.SubstractHP(data.transform.position - attacker.position, attackData);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (isContinuous) {
            if ((1 << collision.gameObject.layer) == ((1 << collision.gameObject.layer) & targetLayer)) {
                var data = collision.GetComponent<BaseData>();

                tickList.Remove(data);
            }
        }
    }
}