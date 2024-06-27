using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject menuPanelTemp;

    public GameObject pausePanel;
    public Button pauseBtn, return2Game, settingsBtn,settingReturnBtn,return2menuBtn;
    public Slider volumeSlider;
    
    
    //当场景加载后，如果canvas下有子物体，删除panel
    //生成panel
    //控制暂停界面，时间暂停，GC

    private void Awake()
    {
        pauseBtn.onClick.AddListener(OnTimePauseBtnClick);
        return2Game.onClick.AddListener(OnTimePauseBtnClick);
        
        settingsBtn.onClick.AddListener(OnSettingsClick);
        settingReturnBtn.onClick.AddListener(Return2PauseMenuClick);
        
        return2menuBtn.onClick.AddListener(Return2MainMenu);
        
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }

    private void Start()
    {
        //生成
        Instantiate(menuPanelTemp, menuCanvas.transform);

        
        pausePanel.SetActive(false);
    }

    private void Return2PauseMenuClick()
    {
        pausePanel.transform.GetChild(1).gameObject.SetActive(false);
    }

    private void OnSettingsClick()
    {
        pausePanel.transform.GetChild(1).gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
    }

    private void OnAfterSceneLoadEvent()
    {
        if (menuCanvas.transform.childCount > 0)
        {
            Destroy(menuCanvas.transform.GetChild(0).gameObject);
        }
    }

    private void OnTimePauseBtnClick()
    {
        bool isOpen = pausePanel.activeInHierarchy;
        //打开菜单
        if (!isOpen)
        {
            System.GC.Collect();
            pausePanel.SetActive(true);
            pausePanel.transform.GetChild(1).gameObject.SetActive(false);
            //暂停时间
            Time.timeScale = 0;
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
    }

    private void Return2MainMenu()
    {
        Time.timeScale = 1;
        StartCoroutine(Back2Menu());
    }

    private IEnumerator Back2Menu()
    {
        pausePanel.SetActive(false);
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1.5f);
        Instantiate(menuPanelTemp, menuCanvas.transform);
    }
}
