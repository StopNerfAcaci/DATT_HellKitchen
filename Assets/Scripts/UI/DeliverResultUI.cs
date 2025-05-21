using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliverResultUI : MonoBehaviour
{
    private const string POPUP = "popup";
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Color successColor;
    [SerializeField] private Color failedColor;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failedSprite;

    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {

        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        gameObject.SetActive(false);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
        anim.SetTrigger(POPUP);
        backgroundImage.color = failedColor;
        iconImage.sprite = failedSprite;
        messageText.text = "Delivery Failed!";
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, DeliveryManager.RecipeSuccessEventArgs e)
    {
        gameObject.SetActive(true);
        anim.SetTrigger(POPUP);
        backgroundImage.color = successColor;
        iconImage.sprite = successSprite;
        messageText.text = "Delivery Success!";
    }
}
