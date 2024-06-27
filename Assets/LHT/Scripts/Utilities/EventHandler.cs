using System;
using System.Collections;
using System.Collections.Generic;
using Farm.CropPlant;
using Farm.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EventHandler
{
    public static event Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;
    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
    {
        //?.Invoke 简写，正常写法需要判断UpdateInventoryUI是否为空
        UpdateInventoryUI?.Invoke(location,list);
    }
    
    /// <summary>
    /// 拖拽物品到世界地图时生成该物品，该功能为测试已经注释，无调用
    /// </summary>
    public static event Action<int, Vector3> InstantiateItemInScene;
    public static void CallInstantiateItemInScene(int ID, Vector3 pos)
    {
        InstantiateItemInScene?.Invoke(ID, pos);
    }
    
    /// <summary>
    /// 从背包中丢出物品事件
    /// </summary>
    public static event Action<int, Vector3, ItemType> DropItemInScene;
    public static void CallDropItemInScene(int ID, Vector3 pos,ItemType itemType)
    {
        DropItemInScene?.Invoke(ID, pos, itemType);
    }

    public static event Action<ItemDetails, bool> ItemSelectedEvent;
    public static void CallSelectedEvent(ItemDetails itemDetails, bool isSelect)
    {
        ItemSelectedEvent?.Invoke(itemDetails,isSelect);
    }

    /// <summary>
    /// 以分钟为基准
    /// </summary>
    public static event Action<int, int,int,int,Season> GameMinuteEvent;

    public static void CallMinuteEvent(int minute,int hour,int day,int week, Season season)
    {
        GameMinuteEvent?.Invoke(minute,hour,day,week,season);
    }

    /// <summary>
    /// 以小时为基准
    /// </summary>
    public static event Action<int,int,int,int,int,Season> GameDateEvent;

    public static void CallDateEvent(int hour,int day,int week, int month,int year,Season season)
    {
        GameDateEvent?.Invoke(hour,day,week,month,year,season);
    }

    public static event Action<int, Season> GameDayEvent;

    public static void CallGameDayEvent(int day, Season season)
    {
        GameDayEvent?.Invoke(day,season);
    }

    /// <summary>
    /// 场景切换（确认传送位置）事件
    /// </summary>
    public static event Action<string, Vector3> TransitionEvent;
    public static void CallTransitionEvent(string sceneToGo, Vector3 posToGo)
    {
        TransitionEvent?.Invoke(sceneToGo,posToGo);
    }
    
    /// <summary>
    /// 场景卸载前调用该事件
    /// </summary>
    public static event Action BeforeSceneUnLoadEvent;
    public static void CallBeforeSceneUnLoadEvent()
    {
        BeforeSceneUnLoadEvent?.Invoke();
    }
    
    /// <summary>
    /// 场景加载后调用该事件
    /// </summary>
    public static event Action AfterSceneLoadEvent;
    public static void CallAfterSceneLoadEvent()
    {
        AfterSceneLoadEvent?.Invoke();
    }

    public static event Action<Vector3> MovementEvent;

    public static void CallMovementEvent(Vector3 targetPos)
    {
        MovementEvent?.Invoke(targetPos);
    }

    /// <summary>
    /// 点击事件
    /// V3:鼠标点击位置
    /// ItemDetails：当前物品信息
    /// 玩家触发动画、种植种子、树木砍伐...
    /// </summary>
    public static event Action<Vector3,ItemDetails> MouseClickedEvent;
    public static void CallMouseClickedEvent(Vector3 pos, ItemDetails itemDetails)
    {
        MouseClickedEvent?.Invoke(pos,itemDetails);
    }
    
    public static event Action<Vector3,ItemDetails> ExecuteActionAfterAnimation;
    public static void CallExecuteActionAfterAnimation(Vector3 pos, ItemDetails itemDetails)
    {
        ExecuteActionAfterAnimation?.Invoke(pos,itemDetails);
    }

    /// <summary>
    /// 在指定瓦片种植的事件
    /// </summary>
    public static event Action<int, TileDetails> PlantSeedEvent;
    public static void CallPlantSeedEvent(int ID, TileDetails tileDetails)
    {
        PlantSeedEvent?.Invoke(ID,tileDetails);
    }

    /// <summary>
    /// 收获物ID
    /// </summary>
    public static event Action<int> HarvestAtPlayerPosition;
    public static void CallHarvestAtPlayerPosition(int cropID)
    {
        HarvestAtPlayerPosition?.Invoke(cropID);
    }

    public static event Action RefreshCurrentMap;
    public static void CallRefreshCurrentMap()
    {
        RefreshCurrentMap?.Invoke();
    }

    /// <summary>
    /// 粒子特效事件
    /// 参数：得到粒子特效的类型和位置
    /// 特效位置：在SO文件中添加
    /// </summary>
    public static event Action<ParticleEffectType, Vector3> ParticleEffectEvent;
    public static void CallParticleEffectEvent(ParticleEffectType type, Vector3 effectPos)
    {
        ParticleEffectEvent?.Invoke(type,effectPos);
    }
    
    /// <summary>
    /// 第一次加载场景时在地图生成树、石头、杂草
    /// </summary>
    public static event Action GenerateCropEvent;
    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }

    public static event Action<Node> ShowDialogueEvent;
    public static void CallShowDialogueEvent(Node node)
    {
        ShowDialogueEvent?.Invoke(node);
    }

    //向dialogueManager中传递index
    public static event Action<int,string,bool> DialougueOptionEvent;
    public static void CallDialougueOptionEvent(int index, string targetPiece, bool playDialogue)
    {
        DialougueOptionEvent?.Invoke(index,targetPiece,playDialogue);
    }

    public static event Action<SlotType, InventoryBag_SO> BaseBagOpenEvent;
    public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagSo)
    {
        BaseBagOpenEvent?.Invoke(slotType, bagSo);
    }
    
    public static event Action<SlotType, InventoryBag_SO> BaseBagCloseEvent;
    public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagSo)
    {
        BaseBagCloseEvent?.Invoke(slotType, bagSo);
    }
    
    public static event Action<GameState> UpdateGameStateEvent;
    public static void CallUpdateGameStateEvent(GameState state)
    {
        UpdateGameStateEvent?.Invoke(state);
    }

    public static event Action<ItemDetails, bool> ShowTradeUI;
    public static void CallShowTradeUI(ItemDetails itemDetails, bool isSell)
    {
        ShowTradeUI?.Invoke(itemDetails,isSell);
    }

    public static event Action<int> PlayerMoneyChangedEvent;
    public static void CallPlayerMoneyChangedEvent(int money)
    {
        PlayerMoneyChangedEvent?.Invoke(money);
    }

    public static event Action<int, Vector3> BuildFurnitureEvent;
    public static void CallBuildFurnitureEvent(int ID, Vector3 mousePos)
    {
        BuildFurnitureEvent?.Invoke(ID, mousePos);
    }

    public static event Action<Season, LightShift, float> LightShiftChangeEvent;
    public static void CallLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        LightShiftChangeEvent?.Invoke(season, lightShift, timeDifference);
    }

    public static event Action<SoundDetail> InitSoundEffect;
    public static void CallInitSoundEffect(SoundDetail soundDetail)
    {
        InitSoundEffect?.Invoke(soundDetail);
    }
    
    public static event Action<SoundName> PlaySoundEvent;
    public static void CallPlaySoundEvent(SoundName soundName)
    {
        PlaySoundEvent?.Invoke(soundName);
    }
    
    public static event Action<int> GetSaveSlotIndex;
    public static void CallGetSaveSlotIndex(int saveSlotIndex)
    {
        GetSaveSlotIndex?.Invoke(saveSlotIndex);
    }
    
    //选择新存档，开启新游戏时呼叫事件，重置数据
    public static event Action<int> StartNewGameEvent;
    public static void CallStartNewGameEvent(int saveSlotIndex)
    {
        StartNewGameEvent?.Invoke(saveSlotIndex);
    }

    public static event Action EndGameEvent;
    public static void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }
}
