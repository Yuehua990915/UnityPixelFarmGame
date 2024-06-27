using UnityEngine;

namespace Farm.Inventory
{
    
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTrans;
        private BoxCollider2D coll;

        public float gravity = -3.5f;
        //是否着陆
        private bool isGround;

        private float distance;
        private Vector2 direction;
        private Vector3 targetPos;


        private void Awake()
        {
            spriteTrans = transform.GetChild(0);
            coll = GetComponent<BoxCollider2D>();
            //飞行过程中关闭碰撞体
            coll.enabled = false;
        }

        private void Update()
        {
            Bounce();
        }

        /// <summary>
        /// 头顶处生成物体
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dir"></param>
        public void InitBounceItem(Vector3 target, Vector2 dir)
        {
            coll.enabled = false;
            direction = dir;
            targetPos = target;
            distance = Vector3.Distance(target, transform.position);

            //Vector3.up * 1.5f 代表从头顶位置生成物体
            spriteTrans.position += Vector3.up * 1.5f;
        }

        /// <summary>
        /// 物体飞行
        /// </summary>
        private void Bounce()
        {
            //用 <= 规避 float 的误差
            isGround = spriteTrans.position.y <= transform.position.y;
            //每帧检测，在没有达到目标位置时持续运动
            //横向移动
            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;
            }

            //纵向移动
            if (!isGround)
            {
                spriteTrans.position += Vector3.up * gravity * Time.deltaTime;
            }
            //落地后
            else
            {
                spriteTrans.position = transform.position;
                coll.enabled = true;
            }
        }
    }
}