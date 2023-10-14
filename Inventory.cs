using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AB5893
{
    public class Inventory : MonoBehaviour
    {
        [Header("Image Version")]
        [SerializeField] private GameObject itemListLeftParent;
        [SerializeField] private GameObject itemListRightParent;
        [SerializeField] private GameObject itemPrefab;

        [Header("Debugging")]
        [SerializeField] private PlayerData playerData;

        private void OnEnable()
        {
            playerData = DataManager.instance.playerData;

            int itemsShown = 0;

            for(int i = 0; i < playerData.inventory.Count; i++)
            {
                if(playerData.inventory[i].amount <= 0)
                {
                    // skip
                }

                else
                {
                    if (itemsShown < 9)
                    {
                        GameObject newItem = Instantiate(itemPrefab, itemListLeftParent.transform);

                        newItem.GetComponentInChildren<Image>().sprite = playerData.inventory[i].sprite;
                        newItem.GetComponentInChildren<Image>().SetNativeSize();

                        newItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = playerData.inventory[i].name;
                        newItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "x" + playerData.inventory[i].amount;
                    }

                    else
                    {
                        GameObject newItem = Instantiate(itemPrefab, itemListRightParent.transform);

                        newItem.GetComponentInChildren<Image>().sprite = playerData.inventory[i].sprite;
                        newItem.GetComponentInChildren<Image>().SetNativeSize();

                        newItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = playerData.inventory[i].name;
                        newItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "x" + playerData.inventory[i].amount;
                    }

                    itemsShown++;
                }
            }
        }

        private void OnDisable()
        {
            foreach(Transform child in itemListLeftParent.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in itemListRightParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
