using System;
using System.IO;
using Farm.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(DataGUID))]
public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI farmName, dataTime, saveType;
    public GameObject firstStartPanel;
    private Button currentBtn, deleteDataSlotBtn;
    private DataSlot currentData;

    private int Index => transform.GetSiblingIndex();
    private void Awake()
    {
        currentBtn = GetComponent<Button>();
        currentBtn.onClick.AddListener(LoadGameData);

        deleteDataSlotBtn = transform.GetChild(3).GetComponent<Button>();
        deleteDataSlotBtn.onClick.AddListener(DeleteSlotData);
    }

    private void DeleteSlotData()
    {
        //拿到文件路径，判断是否有文件，有则删除
        var path = SaveLoadManager.Instance.jsonFolder + "data" + Index + ".json";
        if (File.Exists(path))
        {
            File.Delete(path);
            SaveLoadManager.Instance.dataSlots[Index] = null;
            //更新UI
            SetUpSlotUI();
        }
    }

    private void OnEnable()
    {
        SetUpSlotUI();
    }

    private void LoadGameData()
    {
        if (currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            Debug.Log("空");
            //关闭当前界面，打开创建角色、农场界面
            firstStartPanel.SetActive(true);
            transform.parent.parent.gameObject.SetActive(false);
            //呼叫事件
            EventHandler.CallGetSaveSlotIndex(Index);
        }
    }

    /// <summary>
    /// 设置存档条的文本显示
    /// </summary>
    private void SetUpSlotUI()
    {
        currentData = SaveLoadManager.Instance.dataSlots[Index];
        if (currentData != null)
        {
            //文本赋值
            dataTime.text = currentData.SaveDataTime;
            //农场名
            //farmName.text = 
            //存档类型
            //saveType.text =
            // if (saveType.text == "自动存档")
            // {
            //     saveType.color = Color.red;
            // }
            // else
            // {
            //     saveType.color = Color.blue;
            // }
        }
        else
        {
            dataTime.text = "";
        }
    }
}
