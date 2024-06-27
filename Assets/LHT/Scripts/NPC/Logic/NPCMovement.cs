using System;
using System.Collections;
using System.Collections.Generic;
using Farm.AStar;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.NPC
{
    [RequireComponent(typeof(DataGUID))]
    public class NPCMovement : MonoBehaviour, ISaveable
    {
        public ScheduleDataList_SO schedules;
        private SortedSet<ScheduleDetails> scheduleSet;

        private Stack<MovementStep> npcSteps;
        private BoxCollider2D coll;
        private SpriteRenderer sprite;
        private Grid grid;
        private Rigidbody2D rig;

        public float normalSpeed = 2f;
        private float minSpeed = 1f;
        private float maxSpeed = 3f;
        
        public string currentScene;
        private string targetScene;
        
        private Vector3Int currentGridPos;
        private Vector3Int nextGridPos;
        private Vector3Int targetGridPos;
        private Vector3 dir;
        private Vector3 nextWorldPos;

        private bool isInstialized;
        private bool npcMove;
        private bool sceneLoad;

        private Animator anim;
        //在对话系统中调用这两个bool，做是否可以对话的判断
        public bool isMoving;
        //用schedule.interactable进行赋值
        public bool interactable;
        
        private bool canPlayStopAnim;
        public AnimatorOverrideController animOverride;
        public AnimationClip blankAnimatorClip;
        private AnimationClip stopAnimatorClip;
        private float animBreakTime;
        
        private TimeSpan GameTime => TimeManager.Instance.GameTime;
        private ScheduleDetails currentSchedule;
        private int currentDay;
        private Season currentSeason;

        //判断是否是第一次加载该人物
        //如果不是，则创建新Schedule，让npc执行保存之前的路线
        private bool isFirstLoad;
        
        private Coroutine npcMoveRoutine;
        
        private void Awake()
        {
            npcSteps = new Stack<MovementStep>();
            coll = GetComponent<BoxCollider2D>();
            sprite = GetComponent<SpriteRenderer>();
            rig = GetComponent<Rigidbody2D>();
            grid = FindObjectOfType<Grid>();

            anim = GetComponent<Animator>();
            animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
            anim.runtimeAnimatorController = animOverride;

            scheduleSet = new SortedSet<ScheduleDetails>();
            //排序
            foreach (var schedule in schedules.scheduleList)
            {
                scheduleSet.Add(schedule);
            }
        }

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
            EventHandler.BeforeSceneUnLoadEvent += OnBeforeSceneUnLoadEvent;
            EventHandler.GameMinuteEvent += OnGameMinuteEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
            EventHandler.BeforeSceneUnLoadEvent -= OnBeforeSceneUnLoadEvent;
            EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;  
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            isInstialized = false;
            isMoving = false;
        }

        private void OnEndGameEvent()
        {
            sceneLoad = false;
            npcMove = false;
            if (npcMoveRoutine != null)
            {
                StopCoroutine(npcMoveRoutine);
            }
        }

        private void Start()
        {
            //注册
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            
            //游戏开始时，设定npc初始位置
            foreach (var npcPosition in NPCManager.Instance.npcPositionList)
            {
                if (npcPosition.npc == transform)
                {
                    currentScene = npcPosition.startScene;
                    currentGridPos = grid.WorldToCell(npcPosition.position);
                    npcPosition.npc.position = currentGridPos;
                }
            }

        }

        private void Update()
        {
            if (sceneLoad)
            {
                SwitchAnimation();
            }

            animBreakTime -= Time.deltaTime;
            canPlayStopAnim = animBreakTime <= 0;
        }

        private void FixedUpdate()
        {
            if (sceneLoad)
                Movement();
        }

        private void OnAfterSceneLoadEvent()
        {
            grid = FindObjectOfType<Grid>();
            CheckVaild();
            
            sceneLoad = true;
            CheckVaild();
            if (!isInstialized)
            {
                isInstialized = true;
                InitNPC();
            }

            if (!isFirstLoad)
            {
                currentGridPos = grid.WorldToCell(transform.position);
                var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason, targetScene, (Vector2Int)targetGridPos,
                    stopAnimatorClip, interactable);
                BuildPath(schedule);
                isFirstLoad = true;
            }
        }

        private void OnBeforeSceneUnLoadEvent()
        {
            sceneLoad = false;
        }
        
        private void OnGameMinuteEvent(int minute, int hour,int day, int week, Season season)
        {
            currentDay = day;
            currentSeason = season;
            
            int time = (hour * 100) + minute;

            ScheduleDetails matchSchedule = null;
            foreach (ScheduleDetails schedule in scheduleSet)
            {
                //如果为一次性路线且已经完成，跳出本次循环
                if (schedule.isFinished)
                {
                    break;
                }
                //时间表时间 与 游戏时间相等
                if (schedule.Time == time)
                {
                    //判断日期和季节，不断跳过直到找到符合的时间表
                    if (schedule.day != day && schedule.day != 0)
                        continue;
                    if (schedule.season != season)
                        continue;
                    //得到时间表
                    matchSchedule = schedule;
                }
                //没有到指定时间
                else if(schedule.Time > time)
                {
                    break;
                }
            }
            if (matchSchedule != null)
            {       
                currentSchedule = matchSchedule;
                //设置一次性线路
                if (matchSchedule.onlyOnce)
                {
                    matchSchedule.isFinished = true;
                }
                if (matchSchedule.isFinished)
                {
                    return;
                }
                BuildPath(matchSchedule);
            }
        }
        
        private void InitNPC()
        {
            targetScene = currentScene;

            currentGridPos = grid.WorldToCell(transform.position);
            transform.position = new Vector3(currentGridPos.x + Settings.gridCellSize / 2,
                currentGridPos.y + Settings.gridCellSize / 2, 0);
            targetGridPos = currentGridPos;
        }

        #region 控制NPC可见

        private void CheckVaild()
        {
            if (currentScene == SceneManager.GetActiveScene().name)
                ShowNPC();
            else
                NotShowNPC();
        }

        private void ShowNPC()
        {
            coll.enabled = true;
            sprite.enabled = true;
            //阴影
            transform.GetChild(0).gameObject.SetActive(true);
        }

        private void NotShowNPC()
        {
            coll.enabled = false;
            sprite.enabled = false;
            //阴影
            transform.GetChild(0).gameObject.SetActive(false);
        }

        #endregion

        #region 生成NPC路径

        public void BuildPath(ScheduleDetails schedule)
        {
            //需要一个时间表，传入时间表中的场景和目标位置
            //需要一个栈，记录Astar传回的MoveStep数据
            //清空堆栈
            npcSteps.Clear();
            targetScene = schedule.targetScene;
            targetGridPos = (Vector3Int)schedule.targetGridPos;
            stopAnimatorClip = schedule.clipAtStop;
            interactable = schedule.interactable;
            //同场景移动
            if (schedule.targetScene == currentScene)
            {
                AStar.AStar.Instance.BulidPath(schedule.targetScene, (Vector2Int)currentGridPos, schedule.targetGridPos,
                    npcSteps);
            }
            //跨场景移动
            else if (schedule.targetScene != currentScene)
            {
                BuildPathCrossScene(currentScene, schedule.targetScene,
                    (Vector2Int)currentGridPos, schedule.targetGridPos);
            }

            if (npcSteps.Count > 1)
            {
                //更新步数的时间信息
                UpdateStepTimeInfo();
            }
        }

        private void BuildPathCrossScene(string currentSceneName, string targetSceneName, Vector2Int currentPos,
            Vector2Int targetPos)
        {
            SceneRoute route = NPCManager.Instance.GetRouteFromDic(currentSceneName, targetSceneName);
            if (route != null)
            {
                //遍历route中的path，得到from和goto
                for (int i = 0; i < route.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;
                    ScenePath path = route.scenePathList[i];
                    //当from坐标为99999时，说明是去往其他场景
                    if (path.fromGridCell.x >= Settings.maxGridPos)
                    {
                        fromPos = currentPos;
                    }
                    else
                    {
                        fromPos = path.fromGridCell;
                    }

                    //当goto坐标为99999时，说明时到达该场景
                    if (path.gotoGridCell.x >= Settings.maxGridPos)
                    {
                        gotoPos = targetPos;
                    }
                    else
                    {
                        gotoPos = path.gotoGridCell;
                    }

                    //压栈，先进后出，所以先把后续的路径压入栈中
                    //因此在SO文件中，后执行的路径要排序要靠上，方便压入栈中最后出栈
                    AStar.AStar.Instance.BulidPath(path.sceneName, fromPos, gotoPos, npcSteps);
                }
            }
        }

        /// <summary>
        /// 为每一步MovementStep设置基于游戏的时间
        /// </summary>
        private void UpdateStepTimeInfo()
        {
            MovementStep prevStep = null;
            //currentGameTime：走到第n步时，当前游戏内的时间点
            TimeSpan currentGameTime = GameTime;
            foreach (MovementStep step in npcSteps)
            {
                //第一步
                if (prevStep == null)
                    prevStep = step;
                step.second = currentGameTime.Seconds;
                step.minute = currentGameTime.Minutes;
                step.hour = currentGameTime.Hours;

                //计算移动一格所需的时间
                TimeSpan gridMoveStepTime;
                //判断移动方向（横向移动时间比斜向短）
                if (MoveInDiagonal(step, prevStep))
                    //时间 = 距离 / 速度
                    gridMoveStepTime = new TimeSpan(0, 0,
                        (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.gameTimeScale));
                else
                    gridMoveStepTime = new TimeSpan(0, 0,
                        (int)(Settings.gridCellSize / normalSpeed / Settings.gameTimeScale));

                currentGameTime = currentGameTime.Add(gridMoveStepTime);
                //将当前一步更新为上一步
                prevStep = step;
            }
        }

        private bool MoveInDiagonal(MovementStep cur, MovementStep prev)
        {
            return (cur.gridCoordinate.x != prev.gridCoordinate.x) && (cur.gridCoordinate.y != prev.gridCoordinate.y);
        }

        #endregion

        #region NPC移动

        /// <summary>
        /// 在FixUpdate中执行移动方法
        /// </summary>
        private void Movement()
        {
            //没有在移动
            if (!npcMove)
            {
                if (npcSteps.Count > 0)
                {
                    MovementStep nextStep = npcSteps.Pop();
                    currentScene = nextStep.sceneName;
                    CheckVaild();

                    nextGridPos = (Vector3Int)nextStep.gridCoordinate;
                    TimeSpan nextStepTime = new TimeSpan(nextStep.hour, nextStep.minute, nextStep.second);
                    //执行操作，移动一个格子
                    MoveToGridPos(nextStepTime, nextGridPos);
                }
                else if(!isMoving && canPlayStopAnim)
                {
                    StartCoroutine(SetStopAnimation());
                }
            }
        }

        /// <summary>
        /// 控制协程，保证npc之间移动互不干扰
        /// </summary>
        /// <param name="stepTime"></param>
        /// <param name="gridPos"></param>
        private void MoveToGridPos(TimeSpan stepTime, Vector3Int gridPos)
        {
            npcMoveRoutine = StartCoroutine(MoveRoutine(stepTime, gridPos));
        }

        private IEnumerator MoveRoutine(TimeSpan stepTime, Vector3Int gridPos)
        {
            npcMove = true;
            //下一步的世界坐标
            nextWorldPos = GridToWorldPos(gridPos);
            //有时间进行移动
            if (stepTime > GameTime)
            {
                //时间
                float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
                //距离
                float distance = Vector3.Distance(nextWorldPos, transform.position);
                //速度
                float speed = Mathf.Max(minSpeed, distance / timeToMove / Settings.gameTimeScale);

                if (speed <= maxSpeed)
                {
                    while (Vector3.Distance(transform.position, nextWorldPos) > Settings.pixelSize)
                    {
                        dir = (nextWorldPos - transform.position).normalized;
                        
                        Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime,
                            dir.y * speed * Time.fixedDeltaTime);
                        rig.MovePosition(rig.position + posOffset);
                        //不能一次性移动到目标位置
                        //rig.MovePosition(nextWorldPos);
                        yield return new WaitForFixedUpdate();
                    }
                }
            }
            //瞬移
            rig.position = nextWorldPos;
            //到达目标位置
            currentGridPos = gridPos;
            //停止移动
            nextGridPos = currentGridPos;
            npcMove = false;
        }

        private Vector3 GridToWorldPos(Vector3Int gridPos)
        {
            Vector3 worldPos = grid.CellToWorld(gridPos);
            return new Vector3(worldPos.x + Settings.gridCellSize / 2, worldPos.y + Settings.gridCellSize / 2, 0);
        }

        #endregion

        #region 执行动画

        private void SwitchAnimation()
        {
            isMoving = transform.position != GridToWorldPos(targetGridPos);
            
            anim.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                //触发移动时，立即停止其他动画，进入idle
                anim.SetBool("Exit", true);
                
                anim.SetFloat("DirX", dir.x);
                anim.SetFloat("DirY", dir.y);
            }
            else
            {
                anim.SetBool("Exit", false);
            }
        }

        private IEnumerator SetStopAnimation()
        {
            //面向下
            anim.SetFloat("DirX", 0);
            anim.SetFloat("DirY", -1);
            //每隔一段时间触发一次动画
            animBreakTime = Settings.animationBreakTime;
            //当到达指定时间时，切换待机动画
            if (stopAnimatorClip != null)
            {
                animOverride[blankAnimatorClip] = stopAnimatorClip;
                //功能跟trigger一样，但是不使用trigger
                //防止trigger多次触发
                anim.SetBool("EventAnimation", true);
                //下一帧恢复false
                yield return 0;
                anim.SetBool("EventAnimation", false);
            }
            else
            {
                animOverride[stopAnimatorClip] = blankAnimatorClip;
                anim.SetBool("EventAnimation", false);
            }
        }
        
        #endregion

        public string GUID => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            GameSaveData gameSaveData = new GameSaveData();
            gameSaveData.characterPosDic = new Dictionary<string, SerializableVector3>();
            gameSaveData.timeDic = new Dictionary<string, int>();
            
            //当前坐标
            gameSaveData.characterPosDic.Add("currentPos",new SerializableVector3(transform.position));
            //时间表：当前场景、目标场景、目标位置、(时间)、交互、动画
            gameSaveData.dataSceneName = currentScene;
            gameSaveData.targetScene = targetScene;
            gameSaveData.characterPosDic.Add("targetPos", new SerializableVector3(targetGridPos));
            
            gameSaveData.timeDic.Add("Season", (int)currentSeason);
            
            if (stopAnimatorClip != null)
            {
                gameSaveData.animationClipInstanceID = stopAnimatorClip.GetInstanceID();
            }
            gameSaveData.interactable = interactable;
            
            return gameSaveData;
        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            //NPC已经进行过初始化
            isInstialized = true;
            isFirstLoad = false;
            
            //当前坐标
            currentGridPos = (Vector3Int)gameSaveData.characterPosDic["currentPos"].ToVector2Int();
            //当前场景、目标场景
            currentScene = gameSaveData.dataSceneName;
            targetScene = gameSaveData.targetScene;
            //目标位置
            targetGridPos = (Vector3Int)gameSaveData.characterPosDic["targetPos"].ToVector2Int();
            //动画、交互
            if (gameSaveData.animationClipInstanceID != 0)
            {
                stopAnimatorClip = Resources.InstanceIDToObject(
                    gameSaveData.animationClipInstanceID) as AnimationClip;
            }
            interactable = gameSaveData.interactable;
            currentSeason = (Season)gameSaveData.timeDic["Season"];
        }
    }
}