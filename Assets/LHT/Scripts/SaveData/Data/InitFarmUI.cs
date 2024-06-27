using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitFarmUI : MonoBehaviour
{
    private Button boyBtn, girlBtn;
    public TMP_InputField playerName, farmName;
    private Image showImg;
    public Sprite boyImg, girlImg;
    private bool isBoy;

    private Button confirmBtn, cancelBtn;

    private int currentIndex;

    private GameObject waringPanel;
    
    private void Awake()
    {
        transform.parent.gameObject.SetActive(false);
        waringPanel = transform.parent.GetChild(1).gameObject;
            
        boyBtn = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Button>();
        girlBtn = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Button>();
        boyBtn.transform.GetChild(0).gameObject.SetActive(false);
        girlBtn.transform.GetChild(0).gameObject.SetActive(false);
        
        playerName.text = string.Empty;
        farmName.text = string.Empty;
        
        showImg = transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();

        confirmBtn = transform.GetChild(1).GetChild(1).GetComponent<Button>();
        cancelBtn = transform.GetChild(1).GetChild(2).GetComponent<Button>();
        
        //添加监听
        boyBtn.onClick.AddListener(SwitchSex2Boy);
        girlBtn.onClick.AddListener(SwitchSex2Girl);
        
        confirmBtn.onClick.AddListener(StartNewGame);
        cancelBtn.onClick.AddListener(Return2MainMenu);
    }

    private void OnEnable()
    {
        EventHandler.GetSaveSlotIndex += OnGetSaveSlotIndex;
    }

    private void OnDisable()
    {
        EventHandler.GetSaveSlotIndex -= OnGetSaveSlotIndex;
    }

    private void OnGetSaveSlotIndex(int index)
    {
        currentIndex = index;
    }

    private void Return2MainMenu()
    {
        transform.parent.parent.GetChild(0).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
        
        boyBtn.transform.GetChild(0).gameObject.SetActive(false);
        girlBtn.transform.GetChild(0).gameObject.SetActive(false);
        showImg.sprite = boyImg;
    }

    private void StartNewGame()
    {
        if (playerName.text != string.Empty || farmName.text != string.Empty)
        {
            //开始新游戏
            Debug.Log("开始新游戏");
            //TODO: 传入isBoy，playerName，FarmName (2024-06-24)
            //呼叫事件，重置数据（item、地图、npc）
            EventHandler.CallStartNewGameEvent(currentIndex);
            
            
            //打开timeLine、暂停时间
            
        }
        else
        {
            Debug.Log("请输入玩家名和农场名");
            StartCoroutine(ShowNoNameWarning());
        }
    }

    private IEnumerator ShowNoNameWarning()
    {
        waringPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        waringPanel.SetActive(false);
    }
    
    private void SwitchSex2Girl()
    {
        isBoy = false;
        SwitchSex();
    }

    private void SwitchSex2Boy()
    {
        isBoy = true;
        SwitchSex();
    }
    
    private void SwitchSex()
    {
        //高亮
        boyBtn.transform.GetChild(0).gameObject.SetActive(isBoy);
        girlBtn.transform.GetChild(0).gameObject.SetActive(!isBoy);
        if (isBoy)
        {
            //右侧显示
            showImg.sprite = boyImg;
        }
        else
        {
            showImg.sprite = girlImg;
        }
    }
}

