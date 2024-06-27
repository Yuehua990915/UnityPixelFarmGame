using System.Collections.Generic;
using Farm.AStar;
using Farm.NPC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class AStarTest : MonoBehaviour
{
    private AStar aStar;
    public Vector2Int startPos;
    public Vector2Int endPos;

    public Tilemap displayMap;
    public TileBase displayTile;

    public bool displayStartAndFinish;
    public bool displayPath;

    private Stack<MovementStep> npcMovementStack;

    [Header("NPC移动测试")]
    public NPCMovement npcMovement;
    public bool moveNPC;
    [SceneName]
    public string targetScene;
    public Vector2Int targetGridPos;
    public AnimationClip anim;
    
    private void Awake()
    {
        aStar = GetComponent<AStar>();
        npcMovementStack = new Stack<MovementStep>();
    }

    private void Update()
    {
        ShowPathOnGridMap();

        if (moveNPC)
        {
            moveNPC = false;
            ScheduleDetails scheduleDetails =
                new ScheduleDetails(0, 0, 0, 0, Season.春, 
                    targetScene, targetGridPos, anim, true);
            npcMovement.BuildPath(scheduleDetails);
        }
    }

    private void ShowPathOnGridMap()
    {
        if (displayMap != null && displayTile != null)
        {
            if (displayStartAndFinish)
            {
                displayMap.SetTile((Vector3Int)startPos, displayTile);
                displayMap.SetTile((Vector3Int)endPos, displayTile);
            }
            else
            {
                displayMap.SetTile((Vector3Int)startPos, null);
                displayMap.SetTile((Vector3Int)endPos, null);
            }
            if (displayPath)
            {
                var sceneName = SceneManager.GetActiveScene().name;
                aStar.BulidPath(sceneName, startPos, endPos, npcMovementStack);

                foreach (var step in npcMovementStack)
                {
                    displayMap.SetTile((Vector3Int)step.gridCoordinate, displayTile);
                }
            }
            else
            {
                if (npcMovementStack.Count > 0)
                {
                    foreach (var step in npcMovementStack)
                    {
                        displayMap.SetTile((Vector3Int)step.gridCoordinate, null);
                    }

                    npcMovementStack.Clear();
                }
            }
        }
    }
}
