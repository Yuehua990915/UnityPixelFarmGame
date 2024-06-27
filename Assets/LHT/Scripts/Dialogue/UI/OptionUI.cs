using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public TextMeshProUGUI optionText;
    private Node currentPiece;
    private Button btn;
    private string nextPieceID;

    private bool takeQuest;
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnOptionClick);
    }

    void OnOptionClick()
    {
        var newTask = new QuestManager.QuestTask
        {
            //传入SO
            questData = Instantiate(currentPiece.quest)
        };
        
        //判断是否可以接取任务
        if (currentPiece.quest != null)
        {
            //选项为接受任务的选项
            if (takeQuest)
            {
                //添加到任务列表
                //没有接受任务时
                if (!QuestManager.Instance.HaveQuest(newTask.questData))
                {
                    //需要添加的是一个Task，而不是一个SO
                    //所以要在上面声明一个Task并传入SO
                    QuestManager.Instance.taskList.Add(newTask);
                    //将 QuestManager 中的 Quest_SO 的 IsStart 设置为true
                    QuestManager.Instance.GetQuestTask(newTask.questData).IsStarted = true;
                    //存入字典
                    if (QuestManager.Instance.questDataDic.ContainsKey(newTask.questData.questName))
                    {
                        QuestManager.Instance.questDataDic[newTask.questData.questName] = newTask.questData;
                    }
                    else
                    {
                        QuestManager.Instance.questDataDic.Add(newTask.questData.questName,newTask.questData);
                    }
                    
                    QuestManager.Instance.AcceptQuest(newTask);
                }
            }
        }
        //为空
        if (nextPieceID == "")
        {
            //关闭对话
            DialogueUI.Instance.dialoguePanel.SetActive(false);
            EventHandler.CallDialougueOptionEvent(0,null,false);
            //恢复玩家移动
            EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
            return;
        }
        //索引到目标语句
        else
        {
            EventHandler.CallDialougueOptionEvent(9999,nextPieceID,true);
        }
    }

    public void UpdateOption(Node dialoguePiece, Option option)
    {
        currentPiece = dialoguePiece;
        optionText.text = option.text;
        nextPieceID = option.targetID;
        takeQuest = option.takeQuest;
    }
}