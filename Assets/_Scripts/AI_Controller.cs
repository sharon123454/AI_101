using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum StateMachine { Idle, Alert, Scavenge, Patrol, Chase, Ensnare }
public class AI_Controller : MonoBehaviour
{
    [SerializeField] private Transform _Visual;
    [Header("Attack Parameters")]
    [SerializeField] private GameObject _ShotPrefab;
    [SerializeField] private Transform _ShootingPoint;
    [SerializeField] private AI_Detector _weaponDetector;
    [SerializeField] private float _CooldownTime = 0.5f;

    [Header("Movement Parameters")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _chaseSpeedMulti = 1.25f;
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private float _minDisToRotate = 0.2f;

    [Header("Partol Parameters")]
    [SerializeField] private AI_Detector _playerDetector;
    [SerializeField] private float _PatrolTime = 5f;
    [SerializeField] private float _PatrolIdleTime = 3f;
    [SerializeField] private Vector3[] _PatrolDirections;
    [SerializeField] private StateMachine _currentState;

    #region Merchant Try
    //enum ItemType { one, two, three }
    //struct Item
    //{
    //    ItemType type;
    //    bool stackable;
    //    int amount;
    //}
    #endregion

    private WaitForSeconds _patrolTime, _patrolIdleTime;
    private Vector3 _moveDirection;
    private Vector3 _destination;
    private float _cooldownTimer;

    [SerializeField] private bool _isRememberingPlayer;

    private Transform _targetTransform;
    private bool _canFire = true;

    private void Start()
    {
        _patrolTime = new WaitForSeconds(_PatrolTime);
        _patrolIdleTime = new WaitForSeconds(_PatrolIdleTime);
        _playerDetector.DetectedInRange += playerDetector_SetTarget;
        _weaponDetector.DetectedInRange += weaponDetector_TryToAttack;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case StateMachine.Idle:
                break;
            case StateMachine.Scavenge:
                break;
            case StateMachine.Patrol:
                if (!_targetTransform)
                {
                    Controller.MoveObjInDirection(transform, _moveDirection, _speed);
                    Controller.Rotate(transform, _destination, _rotationSpeed, _minDisToRotate);
                }
                else
                {
                    Controller.Rotate(transform, _targetTransform.position, _rotationSpeed, _minDisToRotate);
                }
                break;
            case StateMachine.Chase:
                if (_targetTransform)
                {
                    Controller.Rotate(transform, _targetTransform.position, _rotationSpeed * 10, _minDisToRotate);
                    Controller.MoveObjToPosition(transform, _targetTransform.position, _speed * _chaseSpeedMulti);
                }
                break;
            case StateMachine.Ensnare:
                break;
            default:
                break;
        }
    }

    public void playerDetector_SetTarget(object sender, Transform targetInArea)
    {
        if (_isRememberingPlayer && !targetInArea) //some AI can remember the player even out of range
            return;

        _targetTransform = targetInArea;
    }
    public void weaponDetector_TryToAttack(object sender, Transform targetInArea)
    {
        if (_canFire && _targetTransform && targetInArea)
        {
            Controller.Attack(transform, _ShootingPoint, _ShotPrefab);
            _canFire = false;
            _cooldownTimer = _CooldownTime;
            StartCoroutine(CooldownWeapon());
        }
    }
    private IEnumerator CooldownWeapon()
    {
        while (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
            yield return null;
        }
        _canFire = true;
    }

    public void SetDestination(Vector3 Destination) { _destination = Destination; }

    public void ChageToState(int stateID)
    {

    }

    public void SetState(StateMachine newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case StateMachine.Idle:
                _moveDirection = Vector3.zero;
                SetDestination(transform.position);
                break;
            case StateMachine.Alert:
                break;
            case StateMachine.Scavenge:
                break;
            case StateMachine.Patrol:
                StopCoroutine(Patrol());
                StartCoroutine(Patrol());
                break;
            case StateMachine.Chase:
                break;
            case StateMachine.Ensnare:
                break;
            default:
                Debug.Log("State not implamented");
                break;
        }
    }

    private IEnumerator Patrol()
    {
        for (int i = 0; i < _PatrolDirections.Length; i++)
        {
            if (_currentState == StateMachine.Patrol)
            {
                _moveDirection = (_PatrolDirections[i] - transform.position).normalized;
                _destination = _PatrolDirections[i] + transform.position;
            }

            yield return _patrolTime;

            if (_currentState == StateMachine.Patrol)
            {
                _moveDirection = Vector3.zero;
                _destination = transform.position;
            }

            yield return _patrolIdleTime;
        }
    }

}