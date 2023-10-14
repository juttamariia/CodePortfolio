using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CollectionSeekerPanel : MonoBehaviour
    , IPointerClickHandler
{
    [Header("General Info")]
    [Tooltip("Data of the seeker. Fetched automatically during game play.")]
    public SeekerData data;
    [Tooltip("Index of the seeker. NEEDS TO BE SET MANUALLY!")]
    public int seekerIndex;
    [Tooltip("Is this seeker currently selected for combat team?")]
    public bool selected;

    [Header("Needed Components")]
    [Tooltip("Egg prefab that will spawn when seeker is unlocked.")]
    [SerializeField] private GameObject unlockEgg;
    [SerializeField] private GameObject spriteMask;

    [Header("Don't Mind These - For Other Scripts Only")]
    public float shardMeterFillAmount;

    // private info, no need to show in inspector
    private GraphicManager graphicData;
    private Collection collection;
    private GameObject seekerInfo;
    private GameObject seekerPanel;
    private GameObject infoButton;
    private GameObject seekerSprite;
    private GameObject seekerName;
    private GameObject shardsMeter;
    private GameObject levelMeter;
    private GameObject notificationIcon;
    private Animator anim;
    private Animator anim1;
    private bool eggShowing;
    private bool characterShown;
    private SeekerDataManager dataManager;

    private void Awake()
    {

    }

    void Start()
    {
        dataManager = SeekerDataManager.manager;
        data = SeekerDataManager.manager.seekers[seekerIndex - 1];
        collection = dataManager.transform.GetChild(1).GetComponent<Collection>();
        graphicData = dataManager.transform.GetChild(0).GetComponent<GraphicManager>();
        seekerInfo = collection.transform.GetChild(1).gameObject;
        seekerPanel = transform.GetChild(0).gameObject;
        infoButton = transform.GetChild(2).gameObject;
        seekerSprite = transform.GetChild(1).GetChild(0).gameObject;
        seekerName = transform.GetChild(3).gameObject;
        shardsMeter = transform.GetChild(4).gameObject;
        levelMeter = transform.GetChild(5).gameObject;
        notificationIcon = transform.GetChild(6).gameObject;

        ShowPanelInfo();
        CheckNotifications();
    }

    public void ShowPanelInfo()
    {
        seekerName.GetComponent<TextMeshProUGUI>().text = "" + data.name;

        // if seeker is unlocked, show info like this
        if (data.isUnlocked)
        {
            if (!characterShown)
            {
                characterShown = true;
                seekerSprite.GetComponent<Image>().color = new Color(255f, 255f, 255f, 255f);
                seekerPanel.GetComponent<Image>().color = graphicData.typeColors[data.primaryType - 1];
                infoButton.SetActive(true);
                levelMeter.SetActive(true);
            }

            // set up level meter
            SetMeterValue(levelMeter, data.levelXP, data.levelXPNeeded);
            levelMeter.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "" + data.levelXP + " / " + data.levelXPNeeded;

            // set up shards meter
            SetMeterValue(shardsMeter, data.starShards, data.shardsNeeded);
            shardsMeter.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "" + data.starShards + " / " + data.shardsNeeded;
            shardMeterFillAmount = shardsMeter.transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount;
        }

        else
        {
            seekerPanel.GetComponent<Image>().color = graphicData.typeColors[14];
            infoButton.SetActive(false);
            levelMeter.SetActive(false);

            // set up shards meter
            SetMeterValue(shardsMeter, data.starShards, data.shardsNeededForUnlock);
            shardsMeter.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "" + data.starShards + " / " + data.shardsNeededForUnlock;
            shardMeterFillAmount = shardsMeter.transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount;
        }
    }

    public void CheckNotifications()
    {
        if(data.attributePointsLeftToCollect) // add conditions for star up: shards and coins
        {
            notificationIcon.SetActive(true);
        }

        else
        {
            notificationIcon.SetActive(false);
        }

        if (data.readyToBeUnlocked)
        {
            if (!eggShowing)
            {
                eggShowing = true;
                SeekerReadyToBeUnlocked();
            }
            
        }
    }

    private void SetMeterValue(GameObject meter, int currentValue, int maxValue)
    {
        float currentAmount = currentValue / maxValue;

        meter.transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = currentAmount;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (data.isUnlocked)
        {
            // if seeker has not been selected and there is still room for more selected seekers, select this seeker
            if (!selected && collection.selectedSeekersAmount < collection.maxSelectedSeekersAmount)
            {
                selected = true;
                collection.SelectSeeker(seekerIndex, data, this);
            }

            // if seeker has already been selected, unselect this seeker
            else if (selected)
            {
                selected = false;
                collection.UnselectSeeker(seekerIndex);
            }

            // if there is no more room for any seekers, vibrate player's phone
            else if (collection.selectedSeekersAmount >= collection.maxSelectedSeekersAmount)
            {
                Handheld.Vibrate();
                anim = GameObject.Find("Seeker3").GetComponent<Animator>();
                anim1 = GameObject.Find("Seeker4").GetComponent<Animator>();
                anim.SetBool("isFull", true);
                anim1.SetBool("isFull", true);
                StartCoroutine(EggToTrue());
            }
        }

        if (data.readyToBeUnlocked)
        {
            seekerSprite.SetActive(true);
            GameObject egg = transform.GetChild(7).gameObject;
            Destroy(egg);

            seekerInfo.SetActive(true);
            seekerInfo.GetComponent<CollectionSeekerInfo>().ResetInfo(data, seekerIndex, gameObject);
            collection.infoOpen = true;
            collection.ShowNoButtons();

            // hide selected seekers so that they don't show up in info
            foreach (GameObject selectedObject in collection.selectedSeekersPositions)
            {
                selectedObject.SetActive(false);
            }
        }
    }

    private void SeekerReadyToBeUnlocked()
    {
        seekerSprite.SetActive(false);
        Instantiate(unlockEgg, transform.GetChild(7).position, Quaternion.identity, transform.GetChild(7));
    }

    public void OpenInfo()
    {
        seekerInfo.SetActive(true);
        seekerInfo.GetComponent<CollectionSeekerInfo>().ResetInfo(data, seekerIndex, gameObject);
        collection.infoOpen = true;
        collection.ShowNoButtons();
        spriteMask.SetActive(false);
        
        // hide selected seekers so that they don't show up in info
        foreach(GameObject selectedObject in collection.selectedSeekersPositions)
        {
            selectedObject.SetActive(false);
        }
    }
    IEnumerator EggToTrue()
    {
        yield return new WaitForSeconds(0.5f);

        anim.SetBool("isFull", false);
        anim1.SetBool("isFull", false);
    }
}
