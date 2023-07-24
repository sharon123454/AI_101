using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _BulletSpeed = 70f;
    [SerializeField] private float _timeTillDeath = 2f;

    private float _timer;

    private void Start()
    {
        _timer = _timeTillDeath;
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer < 0)
            Destroy(gameObject);

        Controller.MoveObjInDirection(transform, Vector3.up, _BulletSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

}