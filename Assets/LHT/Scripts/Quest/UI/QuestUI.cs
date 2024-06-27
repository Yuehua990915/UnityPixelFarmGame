using Farm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuestUI : Singleton<QuestUI>
{
    [Header("任务UI")]
    public Transform taskBox;
    public GameObject taskDetailsBox;
    private Button closeBtn;
    private Button submitBtn;
    [Header("右侧任务信息")]
    public TextMeshProUGUI taskTitle;
    public TextMeshProUGUI taskDescribe;
    [Header("预制体父物体")]
    public GameObject ListBox;
    public GameObject requireBox;
    [FormerlySerializedAs("RewardBox")] public GameObject rewardBox;
    [Header("预制体")]
    public GameObject taskPrefab;
    public GameObject requirePrefab;
    public GameObject rewardPrefab;
    [Header("任务完成图片")]
    public GameObject completeImg;

    private bool isOpen;
    private QuestData_SO currentQuestData;
    protected override void Awake()
    {
        base.Awake();
        closeBtn = transform.GetChild(0).GetChild(0).GetComponent<Button>();
        closeBtn.onClick.AddListener(OnButtonClick);
        submitBtn = transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<Button>();
        submitBtn.onClick.AddListener(SubmitQuest);

        submitBtn.gameObject.SetActive(false);
        taskBox.gameObject.SetActive(false);
        taskDetailsBox.SetActive(false);

    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SwitchUIShow();
        }
    }

    private void SwitchUIShow()
    {
        isOpen = !isOpen;
        taskBox.gameObject.SetActive(isOpen);
        taskDetailsBox.SetActive(false);
        completeImg.SetActive(false);
        PlayerMove.Instance.inputDisable = taskBox.gameObject.activeSelf;
        GetTaskData();
    }

    private void GetTaskData()
    {
        if (isOpen)
        {
            //拿到左侧任务列表，循环生成任务预制体
            //先销毁再生成
            DestoryPrefab(ListBox, 0);
            foreach (var quest in QuestManager.Instance.taskList)
            {
                var task = Instantiate(taskPrefab, ListBox.transform);
                task.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = quest.questData.questName;
                //添加点击事件
                var listBtn = task.GetComponent<Button>();
                listBtn.onClick.AddListener(OnListButtonClick);
            }
            //TODO: 为左侧列表添加点击事件，点击列表时，高亮、显示右侧界面 (2024-05-22)
        }
    }
    
    /// <summary>
    /// 同步数据到UI
    /// </summary>
    /// <param name="taskData"></param>
    public void ShowQuestDetails(QuestData_SO taskData)
    {
        if (taskData != null)
        {
            taskTitle.text = taskData.questName;
            taskDescribe.text = taskData.description;
            submitBtn.gameObject.SetActive(false);
            //遍历清空任务目标
            //防止销毁文本
            DestoryPrefab(requireBox, 1);
            //遍历生成任务目标并赋值
            foreach (var item in taskData.questRequires)
            {
                string requireItemName = InventoryManager.Instance.GetItemDetails(item.itemID).itemName;
                
                var require = Instantiate(requirePrefab, requireBox.transform);
                require.GetComponent<Image>().sprite = item.itemImg;
                var requireText = require.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                requireText.text = requireItemName + "(" + item.currentAmount + "/" + item.requireAmount + ")";

                if (!taskData.isFinished)
                {
                    if (item.currentAmount >= item.requireAmount)
                    {
                        requireText.color = new Color(53/255f,147/255f,0,1);
                    }
                    //物品消耗后，require不足，颜色变回黑色
                    else
                    {
                        requireText.color = Color.black;
                    }
                    
                    if (taskData.isCompleted)
                    {
                        submitBtn.gameObject.SetActive(true);
                    }
                }
                //已提交任务
                else
                {
                    requireText.text = requireItemName + "(" + item.requireAmount + "/" + item.requireAmount + ")";
                    requireText.color = Color.gray;
                    //显示已完成图标
                    completeImg.SetActive(true);
                }
            }

            //遍历生成任务奖励并赋值
            //防止销毁文本
            DestoryPrefab(rewardBox, 1);
            //循环生成
            foreach (var item in taskData.QuestRewards)
            {
                var reward = Instantiate(rewardPrefab, rewardBox.transform);

                reward.GetComponent<Image>().sprite = item.rewardImg;

                var rewardText = reward.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                rewardText.text = "x" + item.rewardAmount;

                //为金币奖励时，设置颜色为金色
                //TODO: 更新金币判断方式 (2024-05-22)
                if (item.rewardAmount > 25)
                {
                    rewardText.color = new Color(152/255f, 100/255f, 0, 255/255f);
                }
            }
        }
    }
    
    
    private void OnButtonClick()
    {
        taskBox.gameObject.SetActive(false);
        isOpen = false;
        PlayerMove.Instance.inputDisable = false;
    }
    
    /// <summary>
    /// 列表按键点击事件
    /// </summary>
    private void OnListButtonClick()
    {
        //更新高亮显示
        
        //打开右侧面板
        taskDetailsBox.gameObject.SetActive(true);
        //通过任务名索引 SO 文件
        var buttonSelf = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        //拿到任务名
        string currentTaskName = buttonSelf.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
        //拿到数据
        currentQuestData = QuestManager.Instance.GetQuestDataSo(currentTaskName);
        //更新数据
        QuestManager.Instance.UpdateQuestRequire(QuestManager.Instance.GetQuestTask(currentQuestData));
        //更新UI显示
        ShowQuestDetails(currentQuestData);
    }

    private void SubmitQuest()
    {
        QuestManager.QuestTask task = QuestManager.Instance.GetQuestTask(currentQuestData);
        QuestManager.Instance.GetQuestReward(QuestManager.Instance.GetQuestTask(currentQuestData));
        ShowQuestDetails(task.questData);
    }
    
    private void DestoryPrefab(GameObject obj, int startIndex)
    {
        if (obj.transform.childCount > 0)
        {
            for (int i = startIndex; i < obj.transform.childCount; i++)
            {
                Destroy(obj.transform.GetChild(i).gameObject);
            }
        }
    }
}
