using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum Type { LookOut, Guard, Hunter }
public enum StateMachine { Idle, OnAlert, Scavenge, Patrol, Chase, Ensnare }
public class AI_Controller : MonoBehaviour
{
    public Type _type;
    public StateMachine _currentState;
    public Transform _Visual;

    [Header("Attack Parameters")]
    public GameObject _ShotPrefab;
    public Transform _ShootingPoint;
    public float _ShootingRange = 10f;
    public float _CooldownTime = 0.5f;
    public bool _isRememberingPlayer;

    [Header("Movement Parameters")]
    public float _speed = 5f;
    public float _chaseSpeedMulti = 1.25f;
    public float _rotationSpeed = 90f;
    public float _minDisToRotate = 0.2f;

    [Header("Partol Parameters")]
    public AI_Detector _playerDetector;
    public float _PatrolTime = 5f;
    public float _PatrolIdleTime = 3f;
    public Transform[] _PatrolDirections;

    [Header("State Timers")]
    public float _TimeToLosePlayer = 3;

    #region Merchant Try
    //enum ItemType { one, two, three }
    //struct Item
    //{
    //    ItemType type;
    //    bool stackable;
    //    int amount;
    //}
    #endregion

    private WaitForSeconds _waitXSexonds;
    private Transform _targetTransform;
    private Vector3 _moveDirection;
    private Vector3 _destination;
    private float _cooldownTimer;
    private bool _canFire = true;

    private void Start()
    {
        _playerDetector.DetectedInRange += playerDetector_SetTarget;
        _waitXSexonds = new WaitForSeconds(_TimeToLosePlayer);
    }

    private void Update()
    {
        if (_targetTransform && Vector2.Distance(transform.position, _targetTransform.position) < _ShootingRange
            && _currentState != StateMachine.Ensnare && _currentState != StateMachine.Scavenge
            && _currentState != StateMachine.Patrol && _currentState != StateMachine.Idle)
        {
            Attack();
        }

        switch (_type)
        {
            case Type.LookOut:
                LookOutStateMachine();
                break;
            case Type.Guard:
                GuardStateMachine();
                break;
            case Type.Hunter:
                HunterStateMachine();
                break;
            default:
                break;
        }
    }

    private void HunterStateMachine()
    {
        switch (_currentState)
        {
            case StateMachine.Idle:
                if (_targetTransform && Vector2.Distance(transform.position, _targetTransform.position) < _ShootingRange)
                {
                    SetState(StateMachine.Chase);
                }
                break;
            case StateMachine.Chase:
                if (_targetTransform)
                {
                    Controller.Rotate(_Visual, _targetTransform.position, _rotationSpeed, _minDisToRotate);
                    Controller.MoveToTarget(transform, _targetTransform.position, _speed * _chaseSpeedMulti);
                }
                else
                {
                    SetState(StateMachine.Idle);
                }
                break;
            case StateMachine.OnAlert:
            case StateMachine.Scavenge:
            case StateMachine.Patrol:
            case StateMachine.Ensnare:
            default:
                Debug.Log($"{_type} doesn't implament {_currentState}");
                break;
        }
    }
    private void GuardStateMachine()
    {
        switch (_currentState)
        {
            case StateMachine.Patrol:
                if (_targetTransform)//if target found
                {
                    SetState(StateMachine.Chase);
                }

                //add changing destination loop
                Controller.MoveObjInDirection(transform, _moveDirection, _speed);
                Controller.Rotate(_Visual, _destination, _rotationSpeed, _minDisToRotate);
                break;
            case StateMachine.Chase:
                if (_targetTransform)
                {
                    Controller.Rotate(_Visual, _targetTransform.position, _rotationSpeed, _minDisToRotate);
                    Controller.MoveToTarget(transform, _targetTransform.position, _speed * _chaseSpeedMulti);
                }
                else
                {
                    SetState(StateMachine.Patrol);
                }
                break;
            case StateMachine.Idle:
            case StateMachine.OnAlert:
            case StateMachine.Scavenge:
            case StateMachine.Ensnare:
            default:
                Debug.Log($"{_type} doesn't implament {_currentState}");
                break;
        }
    }
    private void LookOutStateMachine()
    {
        switch (_currentState)
        {
            case StateMachine.Idle:
                if (_targetTransform)
                {
                    SetState(StateMachine.OnAlert);
                }
                break;
            case StateMachine.OnAlert:
                if (_targetTransform)
                {
                    Controller.Rotate(_Visual, _targetTransform.position, _rotationSpeed, _minDisToRotate);
                }
                else
                {
                    SetState(StateMachine.Idle);
                }
                break;
            case StateMachine.Scavenge:
            case StateMachine.Patrol:
            case StateMachine.Chase:
            case StateMachine.Ensnare:
            default:
                Debug.Log($"{_type} doesn't implament {_currentState}");
                break;
        }
    }
    public void SetDestination(Vector3 Destination) { _destination = Destination; }

    public void playerDetector_SetTarget(object sender, Transform targetInArea)
    {
        if (_isRememberingPlayer && !targetInArea) //some AI can remember the player even out of range
            return;

        _targetTransform = targetInArea;
    }

    private void Attack()
    {
        if (_canFire)
        {
            _canFire = false;
            Controller.Attack(_Visual, _ShootingPoint, _ShotPrefab);
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

    private void SetState(StateMachine newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case StateMachine.Idle:
                _moveDirection = Vector3.zero;
                SetDestination(transform.position);
                StartCoroutine(LosePlayer());
                break;
            case StateMachine.Patrol:
            case StateMachine.OnAlert:
            case StateMachine.Scavenge:
            case StateMachine.Chase:
            case StateMachine.Ensnare:
            default:
                //Debug.Log("State change not implamented");
                break;
        }
    }
    private IEnumerator LosePlayer()
    {
        yield return _waitXSexonds;
        _targetTransform = null;
    }

    private IEnumerator Patrol()
    {
        if (_PatrolDirections.Length > 0)
            for (int i = 0; i < _PatrolDirections.Length; i++)
            {
                if (_currentState == StateMachine.Patrol)
                {
                    _moveDirection = (_PatrolDirections[i].position - transform.position).normalized;
                    _destination = _PatrolDirections[i].position + transform.position;
                }

                yield return null;

                if (_currentState == StateMachine.Patrol)
                {
                    _moveDirection = Vector3.zero;
                    _destination = transform.position;
                }

                yield return null;
            }
    }

}