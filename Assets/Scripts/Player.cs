using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Player/Player")]
public class Player : MonoBehaviour
{
    public float health = 1f;
    [SerializeField] float speed = 1f;
    [SerializeField, HideInInspector] float damage = 1f;
}
