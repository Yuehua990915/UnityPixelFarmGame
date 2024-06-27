using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Inventory;
using UnityEngine;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;

    [Header("动画各部分")] public List<AnimatorType> animatorTypes;

    public SpriteRenderer holdItem;

    private Dictionary<string, Animator> animatorSwitchDic = new Dictionary<string, Animator>();

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
        //存入字典
        foreach (var anim in animators)
        {
            animatorSwitchDic.Add(anim.name,anim);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnLoadEvent += OnBeforeSceneUnLoadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
    }
    
    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnLoadEvent -= OnBeforeSceneUnLoadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
    }

    private void OnAfterSceneLoadEvent()
    {
        holdItem.sprite = null;
        holdItem.enabled = false;
    }

    private void OnHarvestAtPlayerPosition(int ID)
    {
        //实现收获物在头顶的显示，显示时间不到1s
        Sprite harvestItem = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        //协程，在显示完一个后，再执行下一个
        if (holdItem.enabled == false)
        {
            StartCoroutine(ShowItem(harvestItem));
        }
    }

    IEnumerator ShowItem(Sprite sprite)
    {
        holdItem.sprite = sprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(0.5f);
        holdItem.enabled = false;
    }
    
    private void OnBeforeSceneUnLoadEvent()
    {
        SwitchAnimator(PartType.None);
        holdItem.sprite = null;
        holdItem.enabled = false;
    }

    /// <summary>
    /// 获取当前itemDetails的itemType，转换为PartType
    /// 实现动画切换
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        PartType currentType = itemDetails.itemType switch
        {
            ItemType.Commodity => PartType.Carry,
            ItemType.Seed => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ChopTool => PartType.Chop,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            ItemType.Furniture => PartType.Carry,
            _ => PartType.None
        };

        if (isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {
            if (currentType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite == null ? itemDetails.icon : itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }
            else
            {
                holdItem.enabled = false;
            }
        }
        SwitchAnimator(currentType);
    }

    /// <summary>
    /// 通过PartType索引animator并替换
    /// </summary>
    /// <param name="partType"></param>
    private void SwitchAnimator(PartType partType)
    {
        foreach (var item in animatorTypes)
        {
            if (item.partType == partType)
            {
                animatorSwitchDic[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
        }
    }
}
