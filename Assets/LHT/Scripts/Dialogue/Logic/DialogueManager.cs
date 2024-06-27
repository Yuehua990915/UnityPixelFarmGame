using System.Collections;
using System.Collections.Generic;
using Farm.NPC;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : Singleton<DialogueManager>
{
    //通过Season拿到对话列表
    public Dialogue_SO dialogueData;
    [HideInInspector]public Tree currentTree;
    private List<Node> dialogueList = new List<Node>();
    private int currentIndex;
    private Season currentSeason;
    
    public bool isTalking;
    
    public bool canTalk;
    
    private void OnEnable()
    {
        EventHandler.DialougueOptionEvent += OnDialougueOptionEvent;

    }
    private void OnDisable()
    {
        EventHandler.DialougueOptionEvent -= OnDialougueOptionEvent;
    }

    private void OnDialougueOptionEvent(int index, string nextPieceID, bool playDialogue)
    {
        //对话结束
        if (index != 9999)
        {
            currentIndex = index;
            isTalking = false;
        }
        //执行对话
        if (playDialogue)
        {
            var piece = currentTree.nodeDic[nextPieceID];
            EventHandler.CallShowDialogueEvent(piece);
            isTalking = false;
            if (piece.isEnd)
                currentIndex = 9999;
            else
                currentIndex++;
        }
    }

    public void GetDialogueListAndTree(Season season)
    {
        canTalk = true;
        currentIndex = 0;
        currentSeason = season;
        foreach (var tree in dialogueData.treeList)
        {
            if (currentSeason == tree.season)
            {
                currentTree = tree;
                dialogueList = tree.nodeList;
            }
        }
        currentTree = dialogueData.treeList[0];
    }


    public void StartRoutine(UnityEvent OnFinishEvent)
    {
        StartCoroutine(DialogueRoutine(OnFinishEvent));
    }
    
    //执行对话
    IEnumerator DialogueRoutine(UnityEvent OnFinishEvent)
    {
        isTalking = true;
        //有对话、索引没超出
        if (dialogueList.Count != 0 && currentIndex < dialogueList.Count)
        {
            //从list拿到piece
            var piece = dialogueList[currentIndex];
            //判断是否为任务对话
            if (piece.quest != null)
            {
                //判断是否接受任务
                //如果接受了任务
                if (QuestManager.Instance.HaveQuest(piece.quest))
                {
                    var questTask = QuestManager.Instance.GetQuestTask(piece.quest);
                    //判断任务完成状态
                    if (questTask.IsComplete)
                    {
                        if (currentIndex < dialogueList.Count)
                        {
                            currentIndex++;
                            piece = dialogueList[currentIndex];
                            //跳出对话
                            currentIndex = 9999;
                        }
                        else
                        {
                            piece = null;
                        }
                    }
                    else if (questTask.IsFinished)
                    {
                        if (currentIndex < dialogueList.Count)
                        {
                            currentIndex += 2;
                            piece = dialogueList[currentIndex];
                        }
                        else
                        {
                            piece = null;
                        }
                    }
                }
            }

            if (piece.isEnd)
            {
                currentIndex = 9999;
            }

            //传入UI
            EventHandler.CallShowDialogueEvent(piece);
            yield return new WaitUntil(() => piece.isDone);
            currentIndex++;
            if (piece.optionList.Count == 0)
            {
                isTalking = false;
            }
        }
        else
        {
            EventHandler.CallShowDialogueEvent(null);
            foreach (var piece in dialogueList)
            {
                piece.isDone = false;
            }
            currentIndex = 0;
            isTalking = false;
            //事件
            if (OnFinishEvent != null)
            {
                OnFinishEvent.Invoke();
                canTalk = false;
            }
        }
    }
}