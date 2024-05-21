using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Character.Monster;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoBehaviour {
    #region 필드 생략
    private StageManager stageManager => StageManager.instance;
    private PlayerData player => PlayerManager.instance.player;

    [SerializeField] private Vector3 minPos;
    [SerializeField] private Vector3 maxPos;

    private Defines.EStageState state;

    private Coroutine generatingCoroutine;

    private int spawnMaxCount => stageData.MaxSpawn;
    private WaitForSeconds regenTime = new WaitForSeconds(0.3f);

    public LinkedList<MonsterData> monstersOnField = new LinkedList<MonsterData>();
    private Queue<MonsterData> monsterPool = new Queue<MonsterData>();

    private bool isOnClear;

    private StageDataSO stageData;
    private DungeonData dungeonData;
    #endregion
    private void Start() {
        stageManager.OnStateChange += SetStageStageStage;
    }

    public void SetStageData(StageDataSO data) {
        stageData = data;
        isOnClear = false;
    }

    public void SetDungeonData(DungeonData data) {
        dungeonData = data;
        isOnClear = false;
    }

    public void StartGoldDungeonGenerate() {
        StopGenerating();

        while (monsterPool.Count > 0)
            Destroy(monsterPool.Dequeue().gameObject);

        generatingCoroutine = StartCoroutine(GoldDungeonGenerate());
    }

    IEnumerator GoldDungeonGenerate() {
        int count = 0;
        while (!isOnClear) {
            while (count < dungeonData.stageSO.MaxSpawn) {
                InstantiateGoldMonster();
                ++count;
                yield return regenTime;
            }

            yield return null;
        }
    }

    public MonsterData InstantiateAwakenBoss(DungeonData data) {
        StopGenerating();

        Vector2 playerPos = player.transform.position;
        var bossSpawnPoint = data.stageSO.BossSpawnPosition;

        var monster = Instantiate(data.stageSO.BossMonsterPrefab);

        monster.controller.onDeathEnd += () => EnqueueMonster(monster);
        monster.transform.position = bossSpawnPoint;
        monster.InitMonster();
        monster.InitializeData(data.stageSO.BossMonsterBaseStatus, data.stageSO.BossMonsterPerLevel, data.dungeonLevel,
            data.stageSO.BossMonsterReward);
        monster.gameObject.SetActive(true);
        monstersOnField.AddLast(monster);
        return monster;
    }

    public void StartInfiniteGenerating() {
        StopGenerating();

        while (monsterPool.Count > 0)
            Destroy(monsterPool.Dequeue().gameObject);

        generatingCoroutine = StartCoroutine(GenerateInfiniteMonster());
    }

    private IEnumerator GenerateInfiniteMonster() {
        int count = 0;
        while (!isOnClear) {
            if (monstersOnField.Count < spawnMaxCount) {
                InstantiateMonster();
                ++count;
                yield return regenTime;
            }
            else
                yield return null;
        }
    }

    public void StopGenerating() {
        if (generatingCoroutine == null)
            return;
        StopCoroutine(generatingCoroutine);
    }

    public MonsterData InstantiateBoss() {
        Vector2 playerPos = player.transform.position;
        var bossSpawnPoint = stageData.BossSpawnPosition;

        var monster = Instantiate(stageData.BossMonsterPrefab);

        var targetLevel = 140 * (stageData.StageSection - 1) + 20 * (stageManager.currentStage) + stageData.StageNumber;
        var targetReward = stageData.BossMonsterReward;
        foreach (var reward in targetReward) {
            reward.SetLevel(targetLevel);
        }

        monster.controller.onDeathEnd += () => EnqueueMonster(monster);
        monster.transform.position = bossSpawnPoint;
        monster.InitMonster();
        BaseStatus bossStatus;
        if (stageData.StageNumber % 20 == 0)
            bossStatus = stageData.Number20BossBonusStatus + stageData.BossMonsterBaseStatus;
        else
            bossStatus = stageData.BossMonsterBaseStatus;
        monster.InitializeData(bossStatus, stageData.BossMonsterPerLevel, targetLevel, stageData.BossMonsterReward);
        monster.gameObject.SetActive(true);
        monstersOnField.AddLast(monster);
        return monster;
    }

    private void InstantiateAwakenMonster() {
    }

    private void InstantiateGoldMonster() {
        int index = Random.Range(0, dungeonData.stageSO.BasicMonstersBaseStatus.Length);
        BaseStatus targetBase = dungeonData.stageSO.BasicMonstersBaseStatus[index];
        BaseStatus targetPerLevel = dungeonData.stageSO.BasicMonstersPerLevel[index];
        int targetLevel = dungeonData.dungeonLevel;
        MonsterDropData[] targetReward = dungeonData.stageSO.BasicMonstersReward;

        MonsterData monster;

        if (monsterPool.Count > 0) {
            monster = monsterPool.Dequeue();
            monster.gameObject.SetActive(true);
        }
        else {
            monster = Instantiate(dungeonData.stageSO.BasicMonstersPrefab[index]);
            monster.InitMonster();
            monster.controller.onDeathEnd += () => EnqueueMonster(monster);
            monster.controller.onDeathStart += () => AddKillCount(monster);
        }

        monster.transform.position =
            dungeonData.stageSO.BasicMonsterSpawnPosition[
                Random.Range(0, dungeonData.stageSO.BasicMonsterSpawnPosition.Length - 1)];
        monster.InitializeData(targetBase, targetPerLevel, targetLevel, targetReward);
        monstersOnField.AddLast(monster);
    }

    private void InstantiateMonster() {
        int index = Random.Range(0, stageData.BasicMonstersBaseStatus.Length);
        BaseStatus targetBase = stageData.BasicMonstersBaseStatus[index];
        BaseStatus targetPerLevel = stageData.BasicMonstersPerLevel[index];
        int targetLevel = 140 * (stageData.StageSection - 1) + 20 * (stageManager.currentStage) +
                          stageData.StageNumber;
        MonsterDropData[] targetReward = stageData.BasicMonstersReward;

        foreach (var reward in targetReward) {
            reward.SetLevel(targetLevel);
        }

        MonsterData monster;

        if (monsterPool.Count > 0) {
            monster = monsterPool.Dequeue();
            monster.gameObject.SetActive(true);
        }
        else {
            monster = Instantiate(stageData.BasicMonstersPrefab[index]);
            monster.InitMonster();
            monster.controller.onDeathEnd += () => EnqueueMonster(monster);
            monster.controller.onDeathStart += () => AddKillCount(monster);
            monster.controller.onDeathStart += () => monster.DropReward();
        }

        Vector2 randPos = new Vector2(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y));
        monster.transform.position = randPos;
        monster.InitializeData(targetBase, targetPerLevel, targetLevel, targetReward);
        monstersOnField.AddLast(monster);
    }

    private void EnqueueMonster(MonsterData monster) {
        if (isOnClear)
            return;

        if (monstersOnField.Contains(monster))
            monstersOnField.Remove(monster);

        if (monster.type != Defines.EMonsterType.Boss) {
            monster.gameObject.SetActive(false);
            monsterPool.Enqueue(monster);
        }
        else {
            Destroy(monster.gameObject);
        }
    }

    private void AddKillCount(MonsterData monster) {
        if (monster.type != Defines.EMonsterType.Boss)
            stageManager.AddKillCount();
    }

    public void ClearMonsters() {
        isOnClear = true;
        foreach (var monster in monstersOnField) {
            monster.gameObject.SetActive(false);
            monsterPool.Enqueue(monster);
        }

        monstersOnField.Clear();
    }

    private void SetStageStageStage(Defines.EStageState state) {
        this.state = state;
    }
}