using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Transition
{
    [RequireComponent(typeof(DataGUID))]
    public class TransitionManager : MonoBehaviour, ISaveable
    {
        [SceneName]
        public string startSceneName = string.Empty;

        private CanvasGroup fadeCanvasGroup;

        private bool isFade;

        private void Awake()
        {
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnLoadScene());
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private void OnTransitionEvent(string sceneToGo, Vector3 posToGo)
        {
            if (!isFade)
                StartCoroutine(TransitionScene(sceneToGo, posToGo));
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            //通过组件查找对象
            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        }

        /// <summary>
        /// 协程，异步加载(叠加)场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            //获得最后一个场景（刚刚加载的场景）
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            //设置为活动场景（添加obj时会添加到该场景）
            SceneManager.SetActiveScene(newScene);

            //测试：场景1：Persistent，场景2：UI，场景3：02.Home(UnLoad)
            //结果：Persistent，UI， Home
            //未加载场景总是排在所有激活场景之后，加载场景根据在Hierarchy中的上下位置进行排序
            // for (int i = 0; i < SceneManager.sceneCount; i++)
            // {
            //     Debug.Log(SceneManager.GetSceneAt(i).name);
            // }
        }

        /// <summary>
        /// 协程，异步切换场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator TransitionScene(string sceneName, Vector3 targetPos)
        {
            EventHandler.CallBeforeSceneUnLoadEvent();
            
            //执行淡入
            yield return Fade(1);
            
            //卸载当前激活场景
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            //加载新的场景
            yield return LoadSceneActive(sceneName);
            
            EventHandler.CallMovementEvent(targetPos);
            
            EventHandler.CallAfterSceneLoadEvent();
            //执行淡出
            yield return Fade(0);
        }

        /// <summary>
        /// 淡入淡出场景
        /// </summary>
        /// <param name="targetAlpha">1：黑  0：透明</param>
        /// <returns></returns>
        IEnumerator Fade(float targetAlpha)
        {
            isFade = true;
            //开启射线遮挡，防止鼠标透过加载界面点击游戏场景
            fadeCanvasGroup.blocksRaycasts = true;

            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.fadeDuration;

            //目标透明的和当前透明的不近似
            while (!Mathf.Approximately(targetAlpha,fadeCanvasGroup.alpha))
            {
                //渐变
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }
            //关闭射线遮挡
            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;
        }

        private IEnumerator UnLoadScene()
        {
            EventHandler.CallBeforeSceneUnLoadEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }
        
        
        public string GUID => GetComponent<DataGUID>().guid;

        /// <summary>
        /// 加载指定场景
        /// 不使用TransitionScene是因为会卸载当前场景
        /// </summary>
        /// <param name="sceneName"></param>
        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return Fade(1f);

            //在游戏过程中加载另外的游戏存档（当前场景可能是01.Field、02.Home...）
            if (SceneManager.GetActiveScene().name != "PersistentScene")
            {
                //保存场景数据
                EventHandler.CallBeforeSceneUnLoadEvent();
                //卸载场景
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }
            
            //调用方法异步叠加场景
            yield return LoadSceneActive(sceneName);
            EventHandler.CallAfterSceneLoadEvent();
            yield return Fade(0);
        }
        
        public GameSaveData GenerateSaveData()
        {
            GameSaveData gameSaveData = new GameSaveData();
            gameSaveData.dataSceneName = SceneManager.GetActiveScene().name;

            return gameSaveData;
        }
        
        public void RestoreData(GameSaveData gameSaveData)
        {
            StartCoroutine(LoadSaveDataScene(gameSaveData.dataSceneName));
        }
    }
}