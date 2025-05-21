using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_SOUND_EFFECT_VOLUME = "SoundEffVolume";
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioClipSO audioClipSO;

    private float volume = 1f;
    private void Awake()
    {
        Instance = this;
        volume = PlayerPrefs.GetFloat(PLAYER_SOUND_EFFECT_VOLUME,1f);
    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.OnAnyPickSomething += Player_OnPickSomething;
        BaseCounter.OnAnyObjectPlaceHere += BaseCounter_OnAnyObjectPlaceHere;
        TrashCounter.OnAnyObjectTrash += TrashCounter_OnAnyObjectTrash;
    }

    private void TrashCounter_OnAnyObjectTrash(object sender, EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipSO.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlaceHere(object sender, EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(audioClipSO.objectDrop, baseCounter.transform.position);
    }

    private void Player_OnPickSomething(object sender, EventArgs e)
    {
        Player player = sender as Player;
        PlaySound(audioClipSO.objectPickup, player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipSO.chop,cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        DeliverCounter deliveryCounter = DeliverCounter.Instance;
        PlaySound(audioClipSO.deliveryFail,deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        DeliverCounter deliveryCounter = DeliverCounter.Instance;
        PlaySound(audioClipSO.deliverySuccess, deliveryCounter.transform.position);
    }

    public void PlaySound(AudioClip[] audioclipArray, Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(audioclipArray[UnityEngine.Random.Range(0,audioclipArray.Length)], position, volumeMultiplier*volume);
    }
    private void PlaySound(AudioClip audioclip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioclip,position,volume);
    }
    public void PlayFootstepSound(Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipSO.footstep,position, volume);
    }
    public void ChangeVolume()
    {
        volume += .1f;
        if(volume > 1f)
        {
            volume = 0f;
        }
        PlayerPrefs.SetFloat(PLAYER_SOUND_EFFECT_VOLUME, volume);
        PlayerPrefs.Save();
    }
    public float GetVolume()
    {
        return volume;
    }
}
