using AidenK.CodeManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLoading : MonoBehaviour
{
    [SerializeField] FloatVariable Health;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(1);
    }
}
