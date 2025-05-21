using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color chosenColor;
    [SerializeField] private Loader.Scene scene;

    private void Start()
    {
        image.color = defaultColor;
        GetComponent<Button>().onClick.AddListener(() =>
        {
            OnClick();
        });
        LevelSelectMenu.Instance.OnLevelSelectedChanged += LevelSelectMenu_OnLevelSelectedChanged;

        if(scene == Loader.Scene.Level_1)
        {
            image.color = chosenColor;
        }
        // Check initial state
        UpdateVisualState();
    }

    private void LevelSelectMenu_OnLevelSelectedChanged(object sender, LevelSelectMenu.LevelSelectedChangedEventArgs e)
    {
        // Update visual state based on the received scene
        if (e.selectedScene == scene)
        {
            image.color = chosenColor;
        }
        else
        {
            image.color = defaultColor;
        }
    }

    private void UpdateVisualState()
    {
        // Update color based on current selection
        if (LevelSelectMenu.Instance.GetChosenScene() == scene)
        {
            image.color = chosenColor;
        }
        else
        {
            image.color = defaultColor;
        }
    }

    public void OnClick()
    {
        LevelSelectMenu.Instance.SetChosenScene(scene);
    }
    private void OnDestroy()
    {
        // Clean up event subscription
        if (LevelSelectMenu.Instance != null)
        {
            LevelSelectMenu.Instance.OnLevelSelectedChanged -= LevelSelectMenu_OnLevelSelectedChanged;
        }
    }
}
