using UnityEngine;
using DG.Tweening;

/// <summary>
/// 该脚本包含两个方法
/// 用于控制精灵图片半透明化和复原
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 从半透明中恢复
    /// </summary>
    public void FadeIn()
    {
        //精灵默认颜色是(1，1，1，1)
        Color targetColor = new Color(1,1,1,1);
        //将fadeDuration设为常量，存入Settings方便修改
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }
    /// <summary>
    /// 半透明化
    /// </summary>
    public void FadeOut()
    {
        //设置精灵图片透明度
        Color targetColor = new Color(1,1,1,Settings.targetFade);
        
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }
}
