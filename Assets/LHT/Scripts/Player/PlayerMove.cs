using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;

[RequireComponent(typeof(DataGUID))]
public class PlayerMove : Singleton<PlayerMove>, ISaveable
{
    private Rigidbody2D rig;

    private float inputX;
    
    private float inputY;

    private Vector2 movementInput;
    
    public float speed = 2f;

    public Animator[] animators;

    private bool isMoving;
    //禁止输入
    public bool inputDisable;

    //使用工具动画方向
    private float mouseX,mouseY;

    //private bool useTool;

    //草地音效和土地音效的切换
    public bool isInGrass = false;
    protected override void Awake()
    {
        base.Awake();
        rig = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
    }
    
    private void Start()
    {
        //注册
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnLoadEvent += OnBeforeSceneUnLoadEvent;
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.MovementEvent += OnMovementEvent;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnLoadEvent -= OnBeforeSceneUnLoadEvent;
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.MovementEvent -= OnMovementEvent;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        inputDisable = true;
    }

    private void OnStartNewGameEvent(int obj)
    {
        //设置玩家初始位置
        transform.position = Settings.newGamePlayerPos;
    }

    private void OnUpdateGameStateEvent(GameState state)
    {
        switch (state)
        {
            case GameState.GamePlay:
                inputDisable = false;
                break;
            case GameState.Pause:
                inputDisable = true;
                break;
        }
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        if (itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity && 
            itemDetails.itemType != ItemType.Furniture)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            //防止在树上方砍树时，玩家背对树木
            //将纵向输入检测范围从玩家脚底，改到玩家半身
            mouseY = mouseWorldPos.y - (transform.position.y + 1.2f);
            //优先偏向距离远的方向
            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
            {
                mouseY = 0;
            }
            else
            {
                mouseX = 0;
            }
            StartCoroutine(UseToolRoutine(mouseWorldPos,itemDetails));
        }
        else
        {
            //执行完动画后，调用事件，传参
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos,itemDetails);
        }
    }

    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        //useTool = true;
        inputDisable = true;
        yield return null;
        foreach (var ani in animators)
        {
            ani.SetTrigger("UseTool");
            //改变人物面朝方向
            ani.SetFloat("InputX", mouseX);
            ani.SetFloat("InputY", mouseY);
        }
        //设置等待时间，根据工具动画效果判断（根据锄头落下的时间进行判断）
        yield return new WaitForSeconds(0.45f);
        //执行实际效果
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos,itemDetails);
        //等待动画结束后恢复状态
        yield return new WaitForSeconds(0.25f);
        //useTool = false;
        inputDisable = false;
    }
    
    /// <summary>
    /// 场景卸载后不能控制玩家移动
    /// </summary>
    private void OnBeforeSceneUnLoadEvent()
    {
        inputDisable = true;
    }
    
    /// <summary>
    /// 场景加载后可以控制玩家移动
    /// </summary>
    private void OnAfterSceneLoadEvent()
    {
        inputDisable = false;
    }

    /// <summary>
    /// 设置玩家切换场景后的初始位置
    /// </summary>
    /// <param name="targetPos"></param>
    private void OnMovementEvent(Vector3 targetPos)
    {
        transform.position = targetPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //如果对象有稻草晃动脚本，声明是在草上行走
        if (other.GetComponent<ItemInteractive>())
        {
            isInGrass = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<ItemInteractive>())
        {
            isInGrass = false;
        }
    }

    void Update()
    {
        //当禁止输入时，无法调用玩家输入方法
        if(!inputDisable)
            PlayerInput();
        else
        {
            isMoving = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
            speed *= 1.2f;
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed /= 1.2f;
        }
        
        SwitchAnimator();
    }

    private void FixedUpdate()
    {
        //通过物理组件进行移动，需要在FixedUpdate中调用
        //非阻挡状态才可移动
        if(!inputDisable)
            Movement();
    }

    /// <summary>
    /// 玩家输入
    /// </summary>
    void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(inputX, inputY).normalized;
        isMoving = movementInput != Vector2.zero;
    }

    /// <summary>
    /// 角色移动
    /// </summary>
    void Movement()
    {
        rig.MovePosition(rig.position + movementInput * speed * Time.deltaTime);
    }

    /// <summary>
    /// 切换动画
    /// </summary>
    void SwitchAnimator()
    {
        //绑定变量
        foreach (var anim in animators)
        {
            anim.SetBool("IsMoving", isMoving);
            anim.SetFloat("MouseX", mouseX);
            anim.SetFloat("MouseY", mouseY);
            if (isMoving)
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
                anim.SetFloat("Speed", speed);
            }
        }
    }

    public string GUID => GetComponent<DataGUID>().guid;
    
    public GameSaveData GenerateSaveData()
    {
        GameSaveData gameSaveData = new GameSaveData();
        
        gameSaveData.characterPosDic = new Dictionary<string, SerializableVector3>();
        gameSaveData.characterPosDic.Add(this.name, new SerializableVector3(transform.position));

        return gameSaveData;
    }

    public void RestoreData(GameSaveData gameSaveData)
    {
        transform.position = gameSaveData.characterPosDic[this.name].ToVector3();
    }
}
