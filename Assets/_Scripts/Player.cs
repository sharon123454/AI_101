using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private GameObject _ShotPrefab;
    [SerializeField] private Transform _ShootingPoint;
    [SerializeField] private Transform _Visual;
    [SerializeField] private float _Speed = 10f, _RotationSpeed = 180f;
    [SerializeField] private float _MinDisToRotate = 0.5f;

    private Vector3 mouseWorldPosition;

    private void Awake()
    {
        if (Instance && Instance != this)
            Destroy(gameObject);

        Instance = this;
    }

    void Update()
    {
        Vector3 playerInput = new Vector3(/*UnityEngine.Input.GetAxisRaw("Horizontal")*/0, UnityEngine.Input.GetAxisRaw("Vertical"), 0);

        Controller.MoveObjInDirection(transform, playerInput, _Speed);

        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Controller.Rotate(transform, mouseWorldPosition, _RotationSpeed, _MinDisToRotate);

        if (Input.GetMouseButtonDown(0))
            Controller.Attack(transform, _ShootingPoint, _ShotPrefab);
    }

    public Vector3 GetPlayerPosition() { return transform.position; }

}