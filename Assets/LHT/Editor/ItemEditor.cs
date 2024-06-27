using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    //在Assets中获取数据库（itemDataList_SO）
    //获取_SO中的List
    private ItemDataList_SO database;
    private List<ItemDetails> itemList = new List<ItemDetails>();
    //模版文件（左侧列表）
    private VisualTreeAsset listTemp;
    private ListView itemListView;
    
    //绑定数据
    //点击左侧列表，右侧显示信息，
    //1.需要得到界面（右侧的 Scroll View 
    //2.需要得到数据（itemDetails
    private ScrollView itemDetailSection;
    //当选中左侧列表某一项时，拿到这一项的itemDetails
    private ItemDetails activeItem;
    //声明右侧预览图片
    private VisualElement previewIcon;
    //声明默认图片
    private Sprite defaultIcon;

    [MenuItem("LHT/UI Toolkit/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }
    
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/LHT/Editor/ItemEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        #region 路径索引
        //获取UI Document中制作的 左侧列表模版
        listTemp = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/LHT/Editor/ItemListTemp.uxml");
        //获取UI Builder中左侧列表
        itemListView = root.Q<VisualElement>("Left").Q<ListView>("ListView");
        //获取UI Builder中右侧界面
        itemDetailSection = root.Q<ScrollView>("Right");
        //获取右侧界面中的图片
        previewIcon = itemDetailSection.Q<VisualElement>("Icon");
        //路径索引设置默认图片
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LHT/Sprite/moon.png");
        //获得按键，添加点击事件
        root.Q<Button>("AddButton").clicked += OnAddItemClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteClicked;

        LoadDataBase();
        GenerateView();
        #endregion
    }

    #region 按键事件
    /// <summary>
    /// 删除按键点击事件
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDeleteClicked()
    {
        //删除当前对象
        itemList.Remove(activeItem);
        //刷新
        itemListView.Rebuild();
        //右侧不可见
        itemDetailSection.visible = false;
    }

    /// <summary>
    /// 添加按键点击事件
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void OnAddItemClicked()
    {
        //创建对象
        ItemDetails newItem = new ItemDetails();
        //初始名字
        newItem.itemName = "NEW ITEM";
        //ID
        newItem.itemID = 1001 + itemList.Count;
        //加入list
        itemList.Add(newItem);
        //刷新数据
        itemListView.Rebuild();
    }
    #endregion

    /// <summary>
    /// 获取_SO文件
    /// </summary>
    private void LoadDataBase()
    {
        //在Assets中筛选文件
        var dataArray = AssetDatabase.FindAssets("ItemDataList_SO");
        //得到筛选后的路径数组，只取第一个（精准筛选）
        if (dataArray.Length > 1)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            //通过路径加载文件，得到数据库数据
            database = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataList_SO)) as ItemDataList_SO;
        }
        //拿到_SO中的列表，也就是在inspector中填写的数据
        itemList = database.itemDetailList;
        //如果不标记则无法保存数据
        EditorUtility.SetDirty(database);
        //测试是否拿到数据，输出1001
        //Debug.Log(itemList[0].itemID);
    }

    /// <summary>
    /// 生成左侧列表模版
    /// </summary>
    private void GenerateView()
    {
        //委托，接收模版复制方法
        Func<VisualElement> makeItem = () => listTemp.CloneTree();
        //委托，将列表元素和数据绑定
        //e：表示一个listTemp
        //i：listTemp的序号（下标）
        //列表包含图片和名称
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            //序号要小于 itemList 的数量，否则为空
            if (i < itemList.Count)
            {
                //找到 Temp 中的 Icon，将itemList中的图片赋给 Icon
                //保证是有icon可以赋值的
                if(itemList[i].icon != null)
                    e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].icon.texture;
                //找到 Temp 中的 Label，将itemList中的 itemName赋值给 Label
                //三元运算符判断是否有itemName
                e.Q<Label>("Name").text = itemList[i].itemName == null ? "NO ITEM" : itemList[i].itemName; 
            }
        };
        
        //找到UI Builder中左侧的列表，生成Temp
        //UI Toolkit Samples中的固定格式 
        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;
        
        //给左侧列表添加点击事件
        itemListView.onSelectionChange += OnListSelectChanged;
        //当打开编辑器时，右侧默认不显示，只有在点击列表后才显示
        itemDetailSection.visible = false;
    }

    /// <summary>
    /// 列表点击事件：拿到数据、激活数据、可视化
    /// IEnumerable相当于List，在触发点击时，没有设置多选，所以列表中只有一项
    /// 因此拿到列表第一个数据即可
    /// </summary>
    /// <param name="selectItem"></param>
    private void OnListSelectChanged(IEnumerable<object> selectItem)
    {
        //当在列表中选中后，拿到数据itemDetailSection
        //拿到IEnumerable中第一个数据
        //Linq.First()
        activeItem = (ItemDetails)selectItem.First();
        //激活数据
        GetItemDetails();
        //当触发点击事件时，让右侧界面可见
        itemDetailSection.visible = true;
    }

    /// <summary>
    /// 获取数据、绑定数据
    /// </summary>
    void GetItemDetails()
    {
        //执行VisualElement下一帧的重绘
        //当有数据更改/撤销动作时调用，重新绘制界面
        //重要！！
        itemDetailSection.MarkDirtyRepaint();

        #region 绑定
        //绑定后都需要有回调，方便修改值后更新
        //绑定ID
        itemDetailSection.Q<IntegerField>("ItemID").value = activeItem.itemID;
        itemDetailSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemID = evt.newValue;
        });
        //绑定Name
        itemDetailSection.Q<TextField>("ItemName").value = activeItem.itemName;
        itemDetailSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemName = evt.newValue;
            //在右侧修改数据时，希望左侧列表同步更新
            itemListView.Rebuild();
        });
        //绑定Type
        //Simple中给出的固定格式，需要生成一下enum格式
        itemDetailSection.Q<EnumField>("ItemType").Init(activeItem.itemType);
        itemDetailSection.Q<EnumField>("ItemType").value = activeItem.itemType;
        itemDetailSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            //强转
            activeItem.itemType = (ItemType)evt.newValue;
        });
        //绑定Icon
        //1.找到右侧Icon，和ObjectField进行绑定
        //是否为null，为null则设置默认图片
        previewIcon.style.backgroundImage = activeItem.icon == null ? defaultIcon.texture : activeItem.icon.texture;
        itemDetailSection.Q<ObjectField>("ItemIcon").value = activeItem.icon;
        itemDetailSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            Sprite newSprite = (Sprite)evt.newValue;
            //回调实时更新ObjectField框中的内容
            activeItem.icon = newSprite;
            //同时更新预览图片 (为null则显示默认图片)
            previewIcon.style.backgroundImage = newSprite == null ? defaultIcon.texture : newSprite.texture;
            //同时更新左侧列表图片
            itemListView.Rebuild();
        });
        //绑定World Sprite
        //注意编写方法
        itemDetailSection.Q<ObjectField>("WorldSprite").value = activeItem.itemOnWorldSprite;
        itemDetailSection.Q<ObjectField>("WorldSprite").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemOnWorldSprite = (Sprite)evt.newValue;
        });
        //绑定Description
        itemDetailSection.Q<TextField>("Description").value = activeItem.itemDetail;
        itemDetailSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemDetail = evt.newValue;
        });
        //绑定Use Radius
        itemDetailSection.Q<IntegerField>("UseRadius").value = activeItem.itemUseRadius;
        itemDetailSection.Q<IntegerField>("UseRadius").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemUseRadius = evt.newValue;
        });
        //绑定Can PickUp
        itemDetailSection.Q<Toggle>("CanPickUp").value = activeItem.canPicked;
        itemDetailSection.Q<Toggle>("CanPickUp").RegisterValueChangedCallback(evt =>
        {
            activeItem.canPicked = evt.newValue;
        });
        //绑定Can Dropped
        itemDetailSection.Q<Toggle>("CanDropped").value = activeItem.canDropped;
        itemDetailSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
        {
            activeItem.canDropped = evt.newValue;
        });
        //绑定Can Carried
        itemDetailSection.Q<Toggle>("CanCarried").value = activeItem.canCarried;
        itemDetailSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt =>
        {
            activeItem.canCarried = evt.newValue;
        });
        //绑定Price
        itemDetailSection.Q<IntegerField>("ItemPrice").value = activeItem.itemPrice;
        itemDetailSection.Q<IntegerField>("ItemPrice").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemPrice = evt.newValue;
        });
        //绑定Sell Percentage
        itemDetailSection.Q<Slider>("SellPercent").value = activeItem.salePercentage;
        itemDetailSection.Q<Slider>("SellPercent").RegisterValueChangedCallback(evt =>
        {
            activeItem.salePercentage = evt.newValue;
        });

        #endregion
    }
}