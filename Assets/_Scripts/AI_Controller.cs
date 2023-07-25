using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum Type { LookOut, Guard, Hunter }
public enum StateMachine { Idle, OnAlert, Scavenge, Patrol, Chase, Ensnare }
public class AI_Controller : MonoBehaviour
{
    public Type _type;
    public StateMachine _currentState;
    public AI_Detector _playerDetector;
    public Transform _Visual;

    [Header("Attack Parameters")]
    public GameObject _ShotPrefab;
    public Transform _ShootingPoint;
    public float _ShootingRange = 10f;
    public float _CooldownTime = 0.5f;
    public bool _isRememberingTarget;

    [Header("Movement Parameters")]
    public float _speed = 5f;
    public float _chaseSpeedMulti = 1.25f;
    public float _rotationSpeed = 90f;
    public float _minDisToRotate = 0.2f;

    [Header("Partol Parameters")]
    public int _CurrentPatrolPoint;
    public float _PatrolIdleTime = 3f;
    public float _StopDistanceFromDestination = 1;
    public Transform[] _PatrolDestinations;

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

    private WaitForSeconds _timeToForgetTarget;
    [SerializeField] private Transform _targetTransform;
    private Coroutine _resetPlayer;
    private Vector3 _destination;
    private float _cooldownTimer;
    private float _patrolTimer;
    private bool _canFire = true;

    private void Start()
    {
        _timeToForgetTarget = new WaitForSeconds(_TimeToLosePlayer);
        _playerDetector.DetectedInRange += playerDetector_SetTarget;
        switch (_type)
        {
            case Type.LookOut:
                break;
            case Type.Guard:
                SetState(StateMachine.Patrol);
                break;
            case Type.Hunter:
                break;
        }

    }
    private void Update()
    {
        if (IsAbleToAttack())
            Attack();

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
                Debug.Log($"{_type} State Machine isn't implamented.");
                break;
        }
    }

    //State Machines
    private void HunterStateMachine()
    {
        switch (_currentState)
        {
            case StateMachine.Idle:
                if (_targetTransform)
                {
                    SetState(StateMachine.Chase);
                }
                break;
            case StateMachine.Chase:
                if (_targetTransform)
                {
                    Controller.Rotate(_Visual, _targetTransform.position, _rotationSpeed, _minDisToRotate);
                    Controller.MoveToTarget(transform, _targetTransform.position, _speed * _chaseSpeedMulti);

                    if (Vector2.Distance(transform.position, _targetTransform.position) > _ShootingRange)
                            _resetPlayer = StartCoroutine(ForgetTarget());
                }
                else
                    SetState(StateMachine.Idle);
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
            case StateMachine.Idle:
                _patrolTimer -= Time.deltaTime;

                if (_targetTransform)
                    SetState(StateMachine.Chase);

                if (_patrolTimer < 0)
                    SetState(StateMachine.Patrol);

                break;
            case StateMachine.Patrol:
                if (_targetTransform)
                    SetState(StateMachine.Chase);

                if (_PatrolDestinations.Length > 0)
                {
                    if (Vector2.Distance(transform.position, _PatrolDestinations[_CurrentPatrolPoint].position) < _StopDistanceFromDestination)
                    {
                        if (_CurrentPatrolPoint >= _PatrolDestinations.Length - 1)
                            _CurrentPatrolPoint = 0;
                        else
                            _CurrentPatrolPoint++;

                        SetState(StateMachine.Idle);
                    }

                    Controller.MoveToTarget(transform, _destination, _speed);
                    Controller.Rotate(_Visual, _destination, _rotationSpeed, _minDisToRotate);
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
                    SetState(StateMachine.Patrol);
                }
                break;
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

                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _ShootingRange * 5);
                    foreach (Collider2D collider in colliders)
                        if (collider)
                        {
                            AI_Controller aIInRange = collider.gameObject.GetComponent<AI_Controller>();

                            if (aIInRange)
                                if (aIInRange._type == Type.Hunter)
                                    aIInRange.playerDetector_SetTarget(this, _targetTransform);
                        }
                }
                break;
            case StateMachine.OnAlert:
                if (_targetTransform)
                {
                    Controller.Rotate(_Visual, _targetTransform.position, _rotationSpeed, _minDisToRotate);

                    if (Vector2.Distance(transform.position, _targetTransform.position) > _ShootingRange)
                        _resetPlayer = StartCoroutine(ForgetTarget());

                }
                else
                    SetState(StateMachine.Idle);
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

    //Setters
    private void SetState(StateMachine newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case StateMachine.Idle:
                SetDestination(transform.position);
                if (_PatrolDestinations.Length > 0)
                    _patrolTimer = _PatrolIdleTime;
                break;
            case StateMachine.Patrol:
                if (_PatrolDestinations.Length > 0)
                    SetDestination(_PatrolDestinations[_CurrentPatrolPoint].position);
                break;
            case StateMachine.OnAlert:
            case StateMachine.Scavenge:
            case StateMachine.Chase:
            case StateMachine.Ensnare:
            default:
                //Debug.Log("State change not implamented");
                break;
        }
    }
    private void SetDestination(Vector3 Destination) { _destination = Destination; }

    //Attack Logic
    private void Attack()
    {
        _canFire = false;
        Controller.Attack(_Visual, _ShootingPoint, _ShotPrefab);
        _cooldownTimer = _CooldownTime;
        StartCoroutine(CooldownAttack());
    }
    private bool IsAbleToAttack()
    {
        if (_targetTransform && _canFire && Vector2.Distance(transform.position, _targetTransform.position) < _ShootingRange
    && _currentState != StateMachine.Ensnare && _currentState != StateMachine.Scavenge
    && _currentState != StateMachine.Patrol && _currentState != StateMachine.Idle)
        {
            return true;
        }
        return false;
    }
    private IEnumerator CooldownAttack()
    {
        while (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
            yield return null;
        }
        _canFire = true;
    }

    //Coroutines
    private IEnumerator ForgetTarget()
    {
        yield return _timeToForgetTarget;

        _targetTransform = null;
        _resetPlayer = null;
    }

    //Events
    public void playerDetector_SetTarget(object sender, Transform targetInArea)
    {
        if (_isRememberingTarget && !targetInArea) //some AI can remember the player even out of range
            return;

        _targetTransform = targetInArea;
    }

}