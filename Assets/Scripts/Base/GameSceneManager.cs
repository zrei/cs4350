using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : Singleton<GameSceneManager>
{

    public void LoadBattleScene()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }
    
    public void UnloadBattleScene()
    {
        SceneManager.UnloadSceneAsync(1);
    }
}
