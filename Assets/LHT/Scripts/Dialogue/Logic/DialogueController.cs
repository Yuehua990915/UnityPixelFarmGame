using Farm.NPC;
using UnityEngine;
using UnityEngine.Events;

//挂在NPC身上
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(NPCMovement))]
public class DialogueController : MonoBehaviour
{
    public Dialogue_SO dialogueData;

    private Season currentSeason;
    private NPCMovement npc => GetComponent<NPCMovement>();
    private GameObject uiSign;
    
    private bool canTalk;
    private bool isTrade;
    public UnityEvent OnFinishEvent;
    
    private void Awake()
    {
        uiSign = transform.GetChild(1).gameObject;
    }

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
        EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
        EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
    }

    private void OnBaseBagOpenEvent(SlotType arg1, InventoryBag_SO arg2)
    {
        isTrade = true;
    }
    private void OnBaseBagCloseEvent(SlotType arg1, InventoryBag_SO arg2)
    {
        isTrade = false;
    }
    
    private void OnGameMinuteEvent(int arg1, int arg2, int arg3, int arg4, Season season)
    {
        currentSeason = season;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.dialogueData = dialogueData;
            DialogueManager.Instance.GetDialogueListAndTree(currentSeason);
            
            canTalk = npc.interactable && !npc.isMoving;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canTalk = npc.interactable && !npc.isMoving && !isTrade;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        canTalk = false;
    }
    
    private void Update()
    {
        uiSign.SetActive(canTalk);
        if (canTalk && Input.GetKeyDown(KeyCode.E) && !DialogueManager.Instance.isTalking)
        {
            DialogueManager.Instance.StartRoutine(OnFinishEvent);
            if (!DialogueManager.Instance.canTalk)
            {
                canTalk = DialogueManager.Instance.canTalk;
            }
        }
    }
}