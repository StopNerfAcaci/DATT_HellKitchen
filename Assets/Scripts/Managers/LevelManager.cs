
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<TargetLevel> level;
    private void Awake()
    {
        foreach(TargetLevel t in level)
        {
            
            t.button.onClick.AddListener(() =>
            {
                Debug.Log("Scene to load: " + t.sceneName);
                Loader.Load(t.sceneName);
            });
        }
        for (int i = 0; i < level.Count; i++)
        {
            level[i].button.onClick.AddListener(() =>
            {
               
                
            });
        }
    }
}
[System.Serializable]
public class TargetLevel
{
    public Button button;
    public Loader.Scene sceneName;
}
