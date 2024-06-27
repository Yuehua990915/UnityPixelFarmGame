using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Inventory;
using Farm.Save;
using UnityEngine;

[RequireComponent(typeof(DataGUID))]
public class QuestManager : Singleton<QuestManager>, ISaveable
{
    public AllQuestData_SO allQuestData;
    /// <summary>
    /// 因为最后要进行游戏数据保存，SO文件数据不会变化
    /// 所以定义一个类，来当作 保存和加载任务列表
    /// </summary>
    [System.Serializable]
    public class QuestTask
    {
        public QuestData_SO questData;
        
        #region 任务状态
        public bool IsStarted
        {
            get => questData.isStarted;
            set => questData.isStarted = value;
        }
        public bool IsComplete
        {
            get => questData.isCompleted;
            set => questData.isCompleted = value;
        }
        public bool IsFinished
        {
            get => questData.isFinished;
            set => questData.isFinished = value;
        }
        #endregion
    }
    
    [Header("Reward Item Prefab")] 
    public Item rewardPrefab;
    
    public List<QuestTask> taskList = new List<QuestTask>();
    [HideInInspector]public Dictionary<string, QuestData_SO> questDataDic = new Dictionary<string, QuestData_SO>();

    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        taskList.Clear();
        questDataDic.Clear();
    }

    private void Start()
    {
        //注册
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }
    
    /// <summary>
    /// 传入任务模版，判断该模版是否在当前任务列表中
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool HaveQuest(QuestData_SO data)
    {
        if (data != null)
            //Linq Enumerable.Any
            //确定序列中是否包含元素或存在元素满足指定条件。
            return taskList.Any(q => q.questData.questName == data.questName);
        
        else return false;
    }
    
    /// <summary>
    /// 传入任务模版，拿到任务所在的列表
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public QuestTask GetQuestTask(QuestData_SO data)
    {
        return taskList.Find(q => q.questData.questName == data.questName);
    }

    public QuestData_SO GetQuestDataSo(string questName)
    {
        if (questDataDic.ContainsKey(questName))
        {
            return questDataDic[questName];
        }
        return null;
    }
    
    /// <summary>
    /// 接取任务时调用，检索背包，有对应物品时更新任务中的数量
    /// </summary>
    /// <param name="questTask"></param>
    public void AcceptQuest(QuestTask questTask)
    {
        questTask.IsStarted = true;
        questTask.IsComplete = false;
        questTask.IsFinished = false;

        UpdateQuestRequire(questTask);
    }

    
    /// <summary>
    /// 打开任务UI时调用，更新计数
    /// </summary>
    public void UpdateQuestRequire(QuestTask questTask)
    {
        //排除未接受的任务 和 已完成的任务
        if (questTask != null && questTask.IsStarted && !questTask.IsFinished)
        {
            //只要有一个requireAmount不满足，将状态变更为未完成
            questTask.IsComplete = true;
            //拿到requireID 和 amount，与背包数据做比较
            foreach (var require in questTask.questData.questRequires)
            {
                require.currentAmount = InventoryManager.Instance.GetItemAmount(require.itemID);
                if (require.currentAmount < require.requireAmount)
                {
                    questTask.IsComplete = false;
                }
                //TODO: 对话计数任务 (2024-05-21)
            }
            //判断更新后任务完成状态
            //满足任务提交条件
            if (questTask.IsComplete == true)
            {
                questTask.IsStarted = false;
                // questTask.IsFinished = false;
            }
            //不满足条件时
            else
            {
                questTask.IsStarted = true;
            }
        }
        //TODO: 对话计数 (2024-05-21)
    }
    
    /// <summary>
    /// 提交任务时调用，执行背包添加 & 删除、任务状态更改
    /// </summary>
    /// <param name="questTask"></param>
    public void GetQuestReward(QuestTask questTask)
    {
        //判断任务状态
        if (questTask != null && questTask.IsComplete)
        {
            //从背包中删除任务物品
            foreach (var require in questTask.questData.questRequires)
            {
                InventoryManager.Instance.RemoveItem(require.itemID, require.requireAmount);
            }
            
            //获得奖励
            foreach (var reward in questTask.questData.QuestRewards)
            {
                //添加金币数量
                if (reward.rewardID == 9999)
                {
                    InventoryManager.Instance.playerBag.money += reward.rewardAmount;
                    EventHandler.CallPlayerMoneyChangedEvent(InventoryManager.Instance.playerBag.money);
                }
                //添加道具
                else
                {
                    //需要通过拿到Item
                    var playerPos = GameObject.FindWithTag("Player").transform;
                    var itemParent = GameObject.FindWithTag("ItemParent").transform;
                    Item newItem = Instantiate(rewardPrefab, playerPos.position, Quaternion.identity, itemParent);
                    newItem.itemID = reward.rewardID;
                    newItem.Init(reward.rewardID);
                    InventoryManager.Instance.AddItem(newItem, reward.rewardAmount, true);
                }
            }
            questTask.IsComplete = false;
            questTask.IsFinished = true;
        }
    }

    public string GUID => GetComponent<DataGUID>().guid;
    public GameSaveData GenerateSaveData()
    {
        GameSaveData gameSaveData = new GameSaveData();
        gameSaveData.taskName = new List<string>();
        gameSaveData.taskStateDic = new Dictionary<string, bool>();
        
        foreach (var item in taskList)
        {
            //任务名
            gameSaveData.taskName.Add(item.questData.questName);
            //任务状态
            gameSaveData.taskStateDic.Add(item.questData.questName+"isStarted", item.IsStarted);
            gameSaveData.taskStateDic.Add(item.questData.questName+"isCompleted", item.IsComplete);
            gameSaveData.taskStateDic.Add(item.questData.questName+"isFinished", item.IsFinished);
        }
        
        return gameSaveData;
    }
    
    public void RestoreData(GameSaveData gameSaveData)
    {
        taskList.Clear();
        foreach (var taskName in gameSaveData.taskName)
        {
            QuestData_SO questData = allQuestData.GetQuestData(taskName);
            QuestTask questTask = new QuestTask()
            {
                questData = questData,
                IsStarted = gameSaveData.taskStateDic[taskName + "isStarted"],
                IsComplete = gameSaveData.taskStateDic[taskName + "isCompleted"],
                IsFinished = gameSaveData.taskStateDic[taskName + "isFinished"]
            };
            taskList.Add(questTask);
            
            if (questDataDic.ContainsKey(questTask.questData.questName))
                questDataDic[questTask.questData.questName] = questTask.questData;
            else
                questDataDic.Add(questTask.questData.questName, questTask.questData);
        }

        
    }
}
