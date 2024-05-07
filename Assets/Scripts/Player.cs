using AidenK.CodeManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct PlayerInfo
{
    [SerializeField]
    private FloatVariable Health;

    public int Ammo;
    public float speed;
}


public class Player : MonoBehaviour
{
    [SerializeField]
    private FloatVariable Health;

    public int Ammo;
    public float speed;
}
