using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Player/Player")]
public class Player : MonoBehaviour
{
    [Header("General parameters")]
    [Tooltip("The player's health value"), Range(0f, 100f)]
    [SerializeField] float health = 50f;

    // - - -

    [Header("Jump parameters")]
    [Tooltip("Whether or not the player can jump")]
    [SerializeField] bool canJump = false;

    [Tooltip("Whether or not fall damage is applied")]
    [SerializeField] bool hasFallDamage = false;

    [Space(10)]

    [Tooltip("The jump height of the player, in game units")]
    [SerializeField] float jumpHeight = 10f;

    [Tooltip("The delay in milliseconds from input to jump")]
    [SerializeField] float jumpDelayMS = 15f;

    [Tooltip("The delay in milliseconds from input to jump")]
    [SerializeField] float coyoteTimeMS = 100f;

    // - - -

    [Header("Move parameters")]
    [Tooltip("Whether the player is currently sprinting or not")]
    [SerializeField] bool isSprinting = false;

    [Tooltip("The move speed of the player")]
    [SerializeField] float moveSpeed = 10f;

    [Tooltip("How much faster the sprint of the player is"), Range(1f, 5f)]
    [SerializeField] float sprintMulti = 1.5f;
}
