using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class DungeonEntrance : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestScriptableObject dungeonQuest;

    [Header("Needed References")]
    [SerializeField] private GameObject dungeonEnteringPreventedUI;

    [Header("Config")]
    [SerializeField] private string goToScene;
    [SerializeField] private int storybookSectionIndex;
    [Tooltip("Tick if a quest is started when entering this dungeon")]
    [SerializeField] private bool activateQuestProgressTracking;

    private string questId;
    private QuestState currentQuestState;
    private Quest currentQuest;

    private void Start()
    {
        if(dungeonQuest != null)
        {
            questId = dungeonQuest.id;
            StartCoroutine(QuestProgressCheckDelay());
        }

        currentQuest = QuestManager.instance.GetQuestById(questId);

        currentQuestState = currentQuest.state;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // if dungeon quest can be started, start it and move to storybook according to given info
            if(currentQuestState == QuestState.CAN_START)
            {
                if (activateQuestProgressTracking)
                {
                    GameEventsManager.instance.questEvents.StartQuest(questId);
                }

                StorybookHandler.instance.SetNewStorybookData(storybookSectionIndex, goToScene, false);
                SceneManager.LoadScene("Storybook");
            }

            // if dungeon quest is already in progress, return to dungeon through storybook scene
            else if (currentQuestState == QuestState.IN_PROGRESS)
            {
                StorybookHandler.instance.SetNewStorybookData(storybookSectionIndex, goToScene, false);
                SceneManager.LoadScene("Storybook");
            }

            // if quest is not in progress or possible to start, show entrance prevented UI
            else
            {
                if(currentQuest != null)
                {
                    dungeonEnteringPreventedUI.SetActive(true);
                    dungeonEnteringPreventedUI.GetComponent<DungeonEnteringPreventedUI>().SetUIContent(currentQuest);
                }
            }
        }
    }

    private void QuestStateChange(Quest quest)
    {
        // only update the quest state if this point has the corresponding quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            Debug.Log("Quest with id: " + questId + " updated to state: " + currentQuestState);
        }
    }
}
