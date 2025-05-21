using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        LevelSelectScene,
        Level_1,
        Level_2,
        Level_3,
        Level_4,
        Level_5,
        Level_6,
        LoadingScene,
        LobbyScene,
        CharacterSelectScene
    }
    public static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }  
    public static void Load2(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public static void LoaderCallBack()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
}
