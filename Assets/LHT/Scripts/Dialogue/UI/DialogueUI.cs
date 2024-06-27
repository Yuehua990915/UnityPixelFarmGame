using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : Singleton<DialogueUI>
{
    public GameObject dialoguePanel;
    public Image leftFace, rightFace;
    public TextMeshProUGUI leftName, rightName;
    public TextMeshProUGUI dialogueText;
    public GameObject continueBox;
    public Transform optionBox;
    public OptionUI optionPrefab;

    protected override void Awake()
    {
        base.Awake();
        dialoguePanel.SetActive(false);
    }

    private void OnEnable()
    {
        EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
    }

    private void OnDisable()
    {
        EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
    }

    private void OnShowDialogueEvent(Node dialoguePiece)
    {
        StartCoroutine(ShowDialugueUI(dialoguePiece));
    }

    IEnumerator ShowDialugueUI(Node dialoguePiece)
    {
        if (dialoguePiece != null)
        {
            dialoguePiece.isDone = false;
            dialoguePanel.SetActive(true);
            continueBox.SetActive(false);
            optionBox.gameObject.SetActive(false);
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            dialogueText.text = string.Empty;
            
            //赋值
            //npc对话
            if (dialoguePiece.name != string.Empty)
            {
                if (dialoguePiece.isLeft)
                {
                    leftFace.gameObject.SetActive(true);
                    rightFace.gameObject.SetActive(false);
                    leftFace.sprite = dialoguePiece.faceImg;
                    leftName.text = dialoguePiece.name;
                }
                else
                {
                    rightFace.gameObject.SetActive(true);
                    leftFace.gameObject.SetActive(false);
                    rightFace.sprite = dialoguePiece.faceImg;
                    rightName.text = dialoguePiece.name;
                }
            }
            //旁白对话
            else
            {
                rightFace.gameObject.SetActive(false);
                leftFace.gameObject.SetActive(false);
            }
            //对话框文本赋值、显示效果
            yield return dialogueText.DOText(dialoguePiece.text, 1f).WaitForCompletion();
            dialoguePiece.isDone = true;
            //创建Options
            if (dialoguePiece.optionList.Count > 0 && dialoguePiece.isDone)
            {
                optionBox.gameObject.SetActive(true);
                CreateOption(dialoguePiece);
            }
            else if(dialoguePiece.hasPause && dialoguePiece.isDone)
            {
                continueBox.SetActive(true);
            }
        }
        else
        {
            //关闭UI
            dialoguePanel.SetActive(false);
            //玩家可以移动
            EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
            yield break;
        }
    }

    private void CreateOption(Node dialoguePiece)
    {
        //循环销毁之前生成的option
        if (optionBox.childCount > 0)
        {
            for (int i = 0; i < optionBox.childCount; i++)
            {
                Destroy(optionBox.GetChild(i).gameObject);
            }
        }
        //循环生成optionPrefab
        for (int i = 0; i < dialoguePiece.optionList.Count; i++)
        {
            var option = Instantiate(optionPrefab, optionBox);
            option.UpdateOption(dialoguePiece, dialoguePiece.optionList[i]);
        }
    }
}