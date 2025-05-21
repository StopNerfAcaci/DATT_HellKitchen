using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryMangerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Image timerImage;
    private float totalTime;
    private float startTime;
    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
        if(DeliveryManager.Instance != null)
            totalTime = DeliveryManager.Instance.expireTime;
    }
    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;
        foreach (Transform t in iconContainer)
        {
            if (t == iconTemplate) continue;
            Destroy(t);
        }
        foreach (KitchenObjectSO kitchenObjectSO in recipeSO.kitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
        }
    }
    private void Update()
    {
        startTime += Time.deltaTime;
        timerImage.fillAmount = startTime / totalTime;
    }
}
