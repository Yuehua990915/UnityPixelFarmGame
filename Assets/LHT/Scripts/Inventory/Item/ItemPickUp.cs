using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        /// <summary>
        /// 进入，判断是否有Item组件，有则拾取
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            Item item = other.GetComponent<Item>();
            if (item != null)
            {
                //在itemDetails中查找是否可以捡起
                if (item.itemDetails.canPicked)
                {
                    InventoryManager.Instance.AddItem(item, 1,true); 
                    //播放拾取音效
                    EventHandler.CallPlaySoundEvent(SoundName.PickUp);
                }
            }
        }
    }
}
