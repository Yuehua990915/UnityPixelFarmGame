using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    //对象池对象列表
    public List<GameObject> poolPrefabs;
    //对象池列表（每种对象prefab对应一个对象池）
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();

    private Queue<GameObject> soundQueue = new Queue<GameObject>();
    
    private void OnEnable()
    {
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffect += InitSoundEffect;
    }

    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= InitSoundEffect;
    }

    private void OnParticleEffectEvent(ParticleEffectType effectType, Vector3 effectPos)
    {
        //WORKFLOW: 根据特效类型进行补全 (2024-04-29)
        //根据类型返回对应的pool
        var objPool = effectType switch
        {
            ParticleEffectType.LeavesFalling01 => poolEffectList[0],
            ParticleEffectType.LeavesFalling02 => poolEffectList[1],
            ParticleEffectType.Rock => poolEffectList[2],
            ParticleEffectType.ReapableScenery => poolEffectList[3],
            _ => null,
        };

        //从Pool中拿到obj
        var obj = objPool.Get();
        obj.transform.position = effectPos;
        //设置协程，等待一段时间后再释放
        StartCoroutine(ReleaseObj(objPool, obj));
    }

    IEnumerator ReleaseObj(ObjectPool<GameObject> objectPool, GameObject obj)
    {
        yield return new WaitForSeconds(1.5f);
        objectPool.Release(obj);
    }

    private void Start()
    {
        CreatePool();
    }

    /// <summary>
    /// 创建粒子特效对象池
    /// </summary>
    private void CreatePool()
    {
        foreach (var item in poolPrefabs)
        {
            //创建父物体，命名为Prefab的名字
            Transform parent = new GameObject(item.name).transform;
            //将parent的父物体设置为Pool Manager
            parent.SetParent(transform);

            var newPool = new ObjectPool<GameObject>(
                //生成Prefab
                () => Instantiate(item,parent),
                //获取Prefab
                e => { e.gameObject.SetActive(true);},
                //释放
                e => { e.gameObject.SetActive(false);},
                //销毁
                e => { Destroy(e);}
                //默认为true
                //true,
                //默认容量、最大容量
                //10,30
            );
            //将生成的pool加入List
            poolEffectList.Add(newPool);
        }
    }

    #region 音频对象池

    /// <summary>
    /// 创建音频对象池
    /// </summary>
    private void CreateSoundPool()
    {
        //在对象池Manager下创建一个父物体
        var parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(transform);
        
        //预先生成
        for (int i = 0; i < 20; i++)
        {
            GameObject newObj = Instantiate(poolPrefabs[4], parent);
            newObj.SetActive(false);
            soundQueue.Enqueue(newObj);
        }
    }

    /// <summary>
    /// 获取对象池中的音频对象
    /// </summary>
    /// <returns></returns>
    private GameObject GetSoundPoolObj()
    {
        if (soundQueue.Count < 2)
        {
            CreateSoundPool();
        }
        return soundQueue.Dequeue();
    }

    /// <summary>
    /// 启动音频对象，播放结束后重新入池
    /// </summary>
    /// <param name="soundDetail"></param>
    private void InitSoundEffect(SoundDetail soundDetail)
    {
        var obj= GetSoundPoolObj();
        obj.GetComponent<Sound>().SetSound(soundDetail);
        obj.SetActive(true);
        StartCoroutine(DisableSoundObj(obj, soundDetail.soundClip.length));
    }

    private IEnumerator DisableSoundObj(GameObject soundObj, float duration)
    {
        yield return new WaitForSeconds(duration);
        soundObj.SetActive(false);
        //重新入队
        soundQueue.Enqueue(soundObj);
    }
    #endregion
}
