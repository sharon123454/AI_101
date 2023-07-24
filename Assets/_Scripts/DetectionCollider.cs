using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class DetectionCollider : MonoBehaviour
{
    public event EventHandler<Player> PlayerDetectedInRange;

    private Player detectedPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (detectedPlayer = collision.GetComponent<Player>())
            PlayerDetectedInRange?.Invoke(this, detectedPlayer);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>())
            PlayerDetectedInRange?.Invoke(this, null);
    }

}