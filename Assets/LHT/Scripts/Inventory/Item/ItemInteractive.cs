using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当玩家进入时，实现晃动效果
/// </summary>
public class ItemInteractive : MonoBehaviour
{
    private bool isAnimating;
    private WaitForSeconds pause = new WaitForSeconds(0.04f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAnimating)
        {
            //玩家在左侧
            if (other.transform.position.x < transform.position.x)
            {
                //向右晃动
                StartCoroutine(RotateRight());
            }
            else
            {
                //向左晃动
                StartCoroutine(RotateLeft());
            }

            if (other.CompareTag("Player"))
            {
                EventHandler.CallPlaySoundEvent(SoundName.WalkOnGrass);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isAnimating)
        {
            //退出时反向操作
            if (other.transform.position.x > transform.position.x)
            {
                //向右晃动
                StartCoroutine(RotateRight());
            }
            else
            {
                //向左晃动
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.WalkOnGrass);
        }
    }

    IEnumerator RotateLeft()
    {
        isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0,0,2);
            yield return pause;
        }

        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0,0,-2);
            yield return pause;
        }
        //回正方向
        transform.GetChild(0).Rotate(0,0,2);
        yield return pause;
        
        isAnimating = false;
    }
    
    IEnumerator RotateRight()
    {
        isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0,0,-2);
            yield return pause;
        }

        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0,0,2);
            yield return pause;
        }
        //回正方向
        transform.GetChild(0).Rotate(0,0,-2);
        yield return pause;
        
        isAnimating = false;
    }
}
