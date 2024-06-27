using System.Collections;
using Farm.CropPlant;
using Farm.Inventory;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Farm.Map;

namespace Cursor
{
    public class CursorManager : MonoBehaviour
    {
        public Sprite normal, tool, seed, item, canNotUse;

        //存储当前鼠标图片
        private Sprite currentSprite;
        private Image cursorImage;
        private RectTransform cursorCanvas;

        //建造图标
        private Image buildImg;
        
        //在制作完地图数据后，实现鼠标切换（耕地等显示图标）
        //屏幕坐标转世界坐标
        //获取世界坐标，需要Camera
        private Camera mainCamera;

        //转网格坐标
        private Grid currentGrid;
        private Vector3 mouseWorldPos;
        private Vector3Int mouseGridPos;
        //鼠标是否禁用
        private bool cursorEnable;

        //鼠标在当前位置是否可用
        private bool cursorPositionValid;

        private bool isClicked;

        private ItemDetails currentItem;

        private Transform playerTransform => FindObjectOfType<PlayerMove>().transform;
        
        private void OnEnable()
        {
            EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
            EventHandler.BeforeSceneUnLoadEvent += OnBeforeSceneUnLoadEvent;
            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        }

        private void OnDisable()
        {
            EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
            EventHandler.BeforeSceneUnLoadEvent -= OnBeforeSceneUnLoadEvent;
            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        }

        private void Start()
        {
            UnityEngine.Cursor.visible = false;
            
            cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
            cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
            //拿到跟随鼠标的建造图标
            buildImg = cursorCanvas.GetChild(1).GetComponent<Image>();
            buildImg.gameObject.SetActive(false);
            
            currentSprite = normal;
            SetCursorImage(normal);

            //需要为主相机添加标签
            mainCamera = Camera.main;

            isClicked = false;
        }

        private void Update()
        {
            if (cursorCanvas == null) return;

            //鼠标跟随
            cursorImage.transform.position = Input.mousePosition;

            //在没有与UI互动时，实时切换鼠标状态
            if (!InteractWithUI() && cursorEnable)
            {
                SetCursorImage(currentSprite);
                CheckCursorValid();
                StartCoroutine(CheckPlayInput());
            }
            //有UI互动时，固定为normal
            else
            {
                SetCursorImage(normal);
                buildImg.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 鼠标点击方法
        /// </summary>
        IEnumerator CheckPlayInput()
        {
            //点击鼠标左键,触发事件
            if (Input.GetMouseButtonDown(0) && cursorPositionValid && !isClicked)
            {
                isClicked = true;
                EventHandler.CallMouseClickedEvent(mouseWorldPos,currentItem);
                yield return new WaitForSeconds(1.5f);
                isClicked = false;
            }
        }
        
        private void OnBeforeSceneUnLoadEvent()
        {
            cursorEnable = false;
        }

        /// <summary>
        /// 完成地图数据后，获得网格
        /// </summary>
        private void OnAfterSceneLoadEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
        }

        /// <summary>
        /// 物品选择事件，选择物品时切换对应鼠标图片
        /// </summary>
        /// <param name="itemDetails"></param>
        /// <param name="isSelected"></param>
        private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
        {
            //在物体没有被选中时，不可以给currentItem赋值
            //同时鼠标也不应该对Grid进行检测
            if (!isSelected)
            {
                currentItem = null;
                cursorEnable = false;
                currentSprite = normal;
                
                buildImg.gameObject.SetActive(false);
            }
            else
            {
                currentItem = itemDetails;
                //TODO:添加所有类型对应图片
                //根据类型返回对应图片
                currentSprite = itemDetails.itemType switch
                {
                    ItemType.Seed => seed,
                    ItemType.Commodity => item,
                    ItemType.ChopTool => tool,
                    ItemType.HoeTool => tool,
                    ItemType.WaterTool => tool,
                    ItemType.BreakTool => tool,
                    ItemType.ReapTool => tool,
                    ItemType.Furniture => tool,
                    ItemType.CollectTool => tool,
                    _ => normal
                };
                cursorEnable = true;

                //显示建造图标
                if (itemDetails.itemType == ItemType.Furniture)
                {
                    buildImg.gameObject.SetActive(true);
                    buildImg.sprite = itemDetails.itemOnWorldSprite;
                    buildImg.SetNativeSize();
                }
            }
        }

        /// <summary>
        /// 是否与UI互动
        /// </summary>
        ///<returns></returns>
        private bool InteractWithUI()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获得鼠标网格坐标
        /// </summary>
        private void CheckCursorValid()
        {
            mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
            mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

            //判断使用范围，需要拿到Player的Grid坐标
            Vector3Int playGridPos = currentGrid.WorldToCell(playerTransform.position);

            buildImg.rectTransform.position = Input.mousePosition;
            
            //如果超出使用范围
            if (Mathf.Abs(mouseGridPos.x - playGridPos.x) > currentItem.itemUseRadius || Mathf.Abs(mouseGridPos.y - playGridPos.y) > currentItem.itemUseRadius)
            {
                SetCursorInValid();
                return;
            }
            
            TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
            Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
            if (currentTile != null && !isClicked)
            {
                CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
                
                //根据物品栏中选中的物品类型，判断对应瓦片地图的信息
                //例如物品为商品，判断对应瓦片的canDrop
                switch (currentItem.itemType)
                {
                    //TODO：补全
                    case ItemType.Seed:
                        if(currentTile.daysSinceDig > -1 && currentTile.seedItemID == -1) 
                            SetCursorValid();
                        else SetCursorInValid();
                        break;
                    case ItemType.Commodity:
                        //同时判断瓦片和物品是否可以Drop
                        if(currentTile.canDrop && currentItem.canDropped) SetCursorValid();
                        else SetCursorInValid();
                        break;
                    case ItemType.HoeTool:
                        if(currentTile.canDig && currentTile.seedItemID == -1) SetCursorValid();
                        else SetCursorInValid();
                        break;
                    case ItemType.WaterTool:
                        if(currentTile.daysSinceDig > -1 && currentTile.daysSinceWatered == -1)SetCursorValid();  
                        else SetCursorInValid();
                        break;
                    case ItemType.CollectTool:
                        if (currentCrop != null)
                        {
                            if (currentCrop.CheckToolAvailable(currentItem.itemID))
                                if (currentTile.growthDays >= currentCrop.TotalGrowthDay) SetCursorValid(); else SetCursorInValid();
                        }
                        else
                            SetCursorInValid();
                        break;
                    case ItemType.BreakTool:
                    case ItemType.ChopTool:
                        //不希望只有在点击瓦片时才能检测到树木
                        //所以调用GridMapManager中的方法通过碰撞体检测树木
                        //判断树木不为空，且在成熟时鼠标才可用，获得瓦片信息和Crop信息，做比较
                        //判断当前工具是否可以用于收获
                        if (crop != null)
                        {
                            if (crop.canHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID))
                                SetCursorValid();
                            else
                                SetCursorInValid();
                        }
                        else
                            SetCursorInValid();
                        break;
                    case ItemType.ReapTool:
                        Debug.Log(GridMapManager.Instance.HaveReapableItemInRadius(currentItem, mouseWorldPos));
                        if (GridMapManager.Instance.HaveReapableItemInRadius(currentItem, mouseWorldPos))
                            SetCursorValid();
                        else
                            SetCursorInValid();
                        break;
                    case ItemType.Furniture:
                        buildImg.gameObject.SetActive(true);
                        var bluePrintDetails =
                            InventoryManager.Instance.bluePrintDataListSo.GetBluePrintDetails(currentItem.itemID);
                        
                        if (currentTile.canPlaceFurniture &&
                            InventoryManager.Instance.CheckStock(currentItem.itemID) &&
                            !OtherFurnitureInRadius(bluePrintDetails))
                        {
                            SetCursorValid();
                        }
                        else
                        {
                            SetCursorInValid();
                        }
                        break;
                }
            }
            else
            {
                SetCursorInValid();
            }
        }

        private bool OtherFurnitureInRadius(BluePrintDetails bluePrintDetails)
        {
            var buildItem = bluePrintDetails.buildPrefab;
            Vector2 point = mouseWorldPos;
            var size = buildItem.GetComponent<BoxCollider2D>().size;

            var otherColl = Physics2D.OverlapBox(point, size, 0);
            
            if (otherColl != null)
                return otherColl.GetComponent<Furniture>();
            return false;
        }
        
        #region 设置鼠标样式
        
        /// <summary>
        /// 设置鼠标图片
        /// </summary>
        /// <param name="sprite"></param>
        private void SetCursorImage(Sprite sprite)
        {
            cursorImage.sprite = sprite;
            cursorImage.color = new Color(1, 1, 1, 1);
        }
        
        /// <summary>
        /// 鼠标可用时调用，设置为完全颜色
        /// </summary>
        private void SetCursorValid()
        {
            SetCursorImage(currentSprite);
            cursorPositionValid = true;
            cursorImage.color = new Color(1, 1, 1, 1);
            buildImg.color = new Color(1, 1, 1, 0.5f);
        }
        
        /// <summary>
        /// 鼠标不可用时调用，设置为红色半透明
        /// </summary>
        private void SetCursorInValid()
        {
            SetCursorImage(canNotUse);
            cursorPositionValid = false;
            //cursorImage.color = new Color(1, 0, 0, 0.4f);
            buildImg.color = new Color(1, 0, 0, 0.5f);
        }
        
        #endregion
    }
}