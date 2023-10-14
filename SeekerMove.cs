using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// NOTE! touch swipe is not yet added into the script since there has been no build made with the new movement system
// code for the swiping has been taken from here: https://forum.unity.com/threads/swipe-in-all-directions-touch-and-mouse.165416/

public class SeekerMove : MonoBehaviour
    , IPointerDownHandler
{
    [Header("Seeker Data")]
    [Tooltip("Seeker index, used to gather right data from Data Manager. NEEDS TO BE SET MANUALLY!")]
    [SerializeField] private int seekerIndex;
    [Tooltip("Seeker's data from Data Manager. Fetched automatically during game play.")]
    [SerializeField] private SeekerData data;
    [SerializeField] private float moveSpeed; // this will later be replaced with data manager values

    [Header("Debugging for Movement")]
    [Tooltip("Has movement cooldown passed?")]
    [SerializeField] private bool canBeMoved;
    [Tooltip("Is this seeker currently selected for moving?")]
    [SerializeField] private bool isSelected;

    [Header("Needed Components")]
    [Tooltip("Selection effect instantiated in the scene. SHOULD BE LEFT EMPTY.")]
    [SerializeField] private GameObject selectionEffect;
    [Tooltip("Prefab that will be instantiated for this seeker during gameplay. NEEDS TO BE SET MANUALLY!")]
    [SerializeField] private GameObject selectionEffectPrefab;
    [Tooltip("Cooldown meter of this seeker. NEEDS TO BE SET MANUALLY!")]
    [SerializeField] private Image cooldownMeter;

    [Header("Mouse Swipe Debugging")]
    [Tooltip("Current slot in which seeker is standing.")]
    [SerializeField] private Collider2D currentSlot;
    [Tooltip("Point where touch was pressed down.")]
    [SerializeField] private Vector2 firstPressPoint;
    [Tooltip("Point where touch was released.")]
    [SerializeField] private Vector2 secondPressPoint;
    [Tooltip("Direction of movement.")]
    [SerializeField] private Vector2 currentSwipe;

    private void Start()
    {
        data = SeekerDataManager.manager.seekers[seekerIndex - 1];

        moveSpeed = data.moveSpeed;
    }

    private void Update()
    {
        // if movement cooldown has passed and seeker has been selected for moving,
        // activate mouse swipe detection
        if(canBeMoved && isSelected)
        {
            MouseSwipe();
        }
    }

    public void StartMovementCheck()
    {
        currentSlot = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Slots"));

        StartCoroutine(MovementCheck());
    }

    IEnumerator MovementCheck()
    {
        cooldownMeter.fillAmount = 0f;

        float updateFillAmount = 1 / (moveSpeed * 100);

        while (cooldownMeter.fillAmount != 1)
        {
            cooldownMeter.fillAmount += updateFillAmount;

            yield return new WaitForSeconds(0.01f);
        }

        canBeMoved = true;

        yield return null;
    }

    private void MouseSwipe()
    {
        // register the point where mouse was pressed down
        if (Input.GetMouseButtonDown(0))
        {
            firstPressPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        if (Input.GetMouseButtonUp(0))
        {
            // register the point where mouse was released
            secondPressPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            // calculate the direction using the point where mouse was pressed down and released
            currentSwipe = new Vector2(secondPressPoint.x - firstPressPoint.x, secondPressPoint.y - firstPressPoint.y);
            currentSwipe.Normalize();

            // if swiped up and there is room for moving, move to next row
            if(currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f && transform.position.y < -1)
            {
                currentSlot.enabled = false;
                RaycastHit2D targetLocation = Physics2D.Raycast(transform.position, Vector2.up, 1f, LayerMask.GetMask("Slots"));
                currentSlot.enabled = true;

                if (targetLocation.collider.GetComponent<StartSlot>().isOccupied == false)
                {                  
                    transform.position = new Vector2(transform.position.x, transform.position.y + 1f);

                    GetComponent<LayerOrderer>().SetLayerOrder();

                    targetLocation.collider.GetComponent<StartSlot>().SetOccupied(gameObject);
                    currentSlot.GetComponent<StartSlot>().SetUnoccupied();
                    currentSlot = targetLocation.collider;

                    canBeMoved = false;
                    isSelected = false;

                    Destroy(selectionEffect);
                    selectionEffect = null;

                    StartCoroutine(MovementCheck());
                }
            }

            // if swiped down and there is room for moving, move to next row
            if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f && transform.position.y > -5)
            {
                currentSlot.enabled = false;
                RaycastHit2D targetLocation = Physics2D.Raycast(transform.position, -Vector2.up, 1f, LayerMask.GetMask("Slots"));
                currentSlot.enabled = true;

                if (targetLocation.collider.GetComponent<StartSlot>().isOccupied == false)
                {
                    transform.position = new Vector2(transform.position.x, transform.position.y - 1f);

                    GetComponent<LayerOrderer>().SetLayerOrder();

                    targetLocation.collider.GetComponent<StartSlot>().SetOccupied(gameObject);
                    currentSlot.GetComponent<StartSlot>().SetUnoccupied();
                    currentSlot = targetLocation.collider;

                    canBeMoved = false;
                    isSelected = false;

                    Destroy(selectionEffect);
                    selectionEffect = null;

                    StartCoroutine(MovementCheck());
                }
            }

            // if swiped right and there is room for moving, move to next row
            if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f && transform.position.x < 2)
            {
                currentSlot.enabled = false;
                RaycastHit2D targetLocation = Physics2D.Raycast(transform.position, Vector2.right, 1f, LayerMask.GetMask("Slots"));
                currentSlot.enabled = true;

                if (targetLocation.collider.GetComponent<StartSlot>().isOccupied == false)
                {
                    transform.position = new Vector2(transform.position.x + 1f, transform.position.y);

                    targetLocation.collider.GetComponent<StartSlot>().SetOccupied(gameObject);
                    currentSlot.GetComponent<StartSlot>().SetUnoccupied();
                    currentSlot = targetLocation.collider;

                    canBeMoved = false;
                    isSelected = false;

                    Destroy(selectionEffect);
                    selectionEffect = null;

                    StartCoroutine(MovementCheck());
                }
            }

            // if swiped left and there is room for moving, move to next row
            if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f && transform.position.x > -2)
            {
                currentSlot.enabled = false;
                RaycastHit2D targetLocation = Physics2D.Raycast(transform.position, -Vector2.right, 1f, LayerMask.GetMask("Slots"));
                currentSlot.enabled = true;

                if (targetLocation.collider.GetComponent<StartSlot>().isOccupied == false)
                {
                    transform.position = new Vector2(transform.position.x - 1f, transform.position.y);

                    targetLocation.collider.GetComponent<StartSlot>().SetOccupied(gameObject);
                    currentSlot.GetComponent<StartSlot>().SetUnoccupied();
                    currentSlot = targetLocation.collider;

                    canBeMoved = false;
                    isSelected = false;

                    Destroy(selectionEffect);
                    selectionEffect = null;

                    StartCoroutine(MovementCheck());
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // if movement cooldown has passed and seeker is not yet selected, select seeker for moving
        if (canBeMoved && !isSelected)
        {
            isSelected = true;

            selectionEffect = Instantiate(selectionEffectPrefab, transform.GetChild(1).position, Quaternion.identity, transform.GetChild(1));
        }

        // if movement cooldown has passed and seeker is already selected, unselect seeker for moving
        else if (canBeMoved && isSelected)
        {
            isSelected = false;

            Destroy(selectionEffect);
            selectionEffect = null;
        }
    }
}
