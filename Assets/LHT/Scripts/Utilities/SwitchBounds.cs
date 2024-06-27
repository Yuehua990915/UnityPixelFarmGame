using UnityEngine;
using Cinemachine;

public class SwitchBounds : MonoBehaviour
{
    //切换场景时调用 SwitchConfineShape()
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchConfineShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchConfineShape;
    }

    /// <summary>
    /// 获取场景中的Bounds，在切换场景时调用
    /// </summary>
    private void SwitchConfineShape()
    {
        PolygonCollider2D confineShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();

        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();
        //将Collider 2D 给到 (成员属性)BoundingShape2D
        confiner.m_BoundingShape2D = confineShape;
        
        //清除缓存，防止切换场景后 碰撞边界没有变化
        confiner.InvalidatePathCache();
    }
}
