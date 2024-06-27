using System;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 自定义Unity编辑器类，创建一个下拉菜单功能
/// 挂载[SceneName]标签的属性会出现下拉菜单
/// </summary>
[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    //默认场景为空
    private int sceneIndex = -1;
    //
    private GUIContent[] sceneNames;
    //设置裁剪格式
    private readonly string[] scenePathSplit = { "/", ".unity" };
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //没有场景build时不执行
        if(EditorBuildSettings.scenes.Length == 0) return;
        
        if(sceneIndex == -1)
            GetSceneNameArray(property);

        int oldIndex = sceneIndex;
        //设置弹出式选择框
        sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);
        //更换选项时执行
        if(oldIndex != sceneIndex)
            property.stringValue = sceneNames[sceneIndex].text;
    }

    /// <summary>
    /// 获得所有场景的名称
    /// </summary>
    /// <param name="property"></param>
    void GetSceneNameArray(SerializedProperty property)
    {
        //接收所有building的场景（得到场景数量）
        var scenes = EditorBuildSettings.scenes;
        //初始化数组
        sceneNames = new GUIContent[scenes.Length];

        for (int i = 0; i < sceneNames.Length; i++)
        {
            //获得每个场景的路径
            string path = scenes[i].path;
            //得到的是该格式路径：
            //Assets/LHT/Scenes/PersistentScene.unity
            //Debug.Log(path);
            
            //裁剪字符串
            //使用Split后会得到string数组，拿到最后一个元素就是场景名
            string[] splitPath = path.Split(scenePathSplit, StringSplitOptions.RemoveEmptyEntries);
            string sceneName = "";

            if (splitPath.Length > 0)
            {
                sceneName = splitPath[splitPath.Length - 1];
            }
            else
            {
                //防止报空
                sceneName = "(Deleted Scene)";
            }
            sceneNames[i] = new GUIContent(sceneName);
        }
        //判断数组是否为空
        if (sceneNames.Length == 0)
        {
            sceneNames = new[] { new GUIContent("Check Your Bulid Settings") };
        }
        
        //判断场景是否为空
        //TransitionManager中 string sceneName默认也为空
        //当创建新的传送点时,string sceneToGo是空的
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool nameFound = false;

            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneNames[i].text == property.stringValue)
                {
                    sceneIndex = i;
                    nameFound = true;
                    break;
                }
            }

            if (nameFound == false)
            {
                sceneIndex = 0;
            }
        }
        else
        {
            sceneIndex = 0;
        }
        //选中场景
        property.stringValue = sceneNames[sceneIndex].text;
    }
}