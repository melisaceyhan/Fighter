using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MySceneManager;

public class GameOverScene : MonoBehaviour
{
    public float timer = 3;
   
    void Start() 
    {
        StartCoroutine("LoadScene");
    }
    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(timer);
        MySceneManager.GetInstance().RequestLevelLoad(SceneType.main, "intro");
    }
        
    
}
