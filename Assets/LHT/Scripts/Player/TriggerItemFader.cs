using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当角色进入触发器范围时 / 离开范围时
/// 调用ItemFader脚本中的方法
/// 将对应物体半透明化 / 从半透明化复原
/// </summary>
public class TriggerItemFader : MonoBehaviour
{
    /// <summary>
    /// OnTriggerEnter2D，不要忘了2D
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        //获取触发物体身上的ItemFader组件，调用FadeIn()
        //因为包含树冠和树干，所以用数组接收
        ItemFader[] itemFaders = other.GetComponentsInChildren<ItemFader>();
        //判断物体上是否有ItemFader组件
        if (itemFaders.Length > 0)
        {
            //有组件，遍历则调用虚化方法
            foreach (var item in itemFaders)
            {
               item.FadeOut(); 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ItemFader[] itemFaders = other.GetComponentsInChildren<ItemFader>();
        if (itemFaders.Length > 0)
        {
            foreach (var item in itemFaders)
            {
                item.FadeIn(); 
            }
        }
    }
}
