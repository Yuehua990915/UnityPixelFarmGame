using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;
        private SpriteRenderer _spriteRenderer;
        public ItemDetails itemDetails;
        private BoxCollider2D _boxCollider2D;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if(itemID != 0)
            {
                Init(itemID);
            }
        }

        /// <summary>
        /// 通过ID生成对应物品图片
        /// </summary>
        /// <param name="id"></param>
        public void Init(int id)
        {
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            if (itemDetails != null)
            {
                //如果该物品没有世界图片，则用icon代替
                _spriteRenderer.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.icon;
                //修改碰撞体边界:size 和 offset
                //修改offset的原因，世界图像的锚点设置在bottom，相较于碰撞体边框会上移 y/2 的距离(刚好是图片中心点的y的值)
                _boxCollider2D.size = new Vector2(_spriteRenderer.sprite.bounds.size.x,
                                                  _spriteRenderer.sprite.bounds.size.y);
                _boxCollider2D.offset = new Vector2(0, _spriteRenderer.sprite.bounds.center.y);
            }

            if (itemDetails.itemType == ItemType.ReapableScenery)
            {
                gameObject.AddComponent<ReapItem>();
                gameObject.GetComponent<ReapItem>().InitCropData(itemDetails.itemID);
                //添加晃动脚本
                gameObject.AddComponent<ItemInteractive>();
            }
        }
    }
}
