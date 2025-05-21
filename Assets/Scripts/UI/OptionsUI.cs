using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance;
    [SerializeField] private Button SoundEffectBtn;
    [SerializeField] private Button MusicBtn;
    [SerializeField] private Button CloseBtn;
    [SerializeField] private TextMeshProUGUI soundEffText;
    [SerializeField] private TextMeshProUGUI musicText;

    private void Awake()
    {
        Instance = this;
        SoundEffectBtn.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        MusicBtn.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        CloseBtn.onClick.AddListener(() =>
        {
            Hide();
        });
    }
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalGameUnpaused += KitchenGameManager_OnGameUnPaused;
        UpdateVisual();
        Hide();
    }

    private void KitchenGameManager_OnGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual()
    {
        soundEffText.text = "Sound Effects: "+ Mathf.Round(SoundManager.Instance.GetVolume()*10f);
        musicText.text = "Music : "+ Mathf.Round(MusicManager.Instance.GetVolume()*10f);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
