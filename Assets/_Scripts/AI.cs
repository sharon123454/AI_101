using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum StateMachine { Idle, Alert, Scavenge, Patrol, Chase, Ensnare }
public class AI : MonoBehaviour
{
    [SerializeField] private GameObject _ShotPrefab;
    [SerializeField] private Transform _ShootingPoint;
    [SerializeField] private Transform _Visual;
    [SerializeField] private DetectionCollider _DetectionCollider;
    [SerializeField] private float _CooldownTime = 0.5f;
    [SerializeField] private float _Speed = 5f, _ChaseSpeedMulti = 1.25f;
    [SerializeField] private float _RotationSpeed = 90f, _MinDisToRotate = 0.2f;
    [SerializeField] private float _PatrolTime = 1.5f, _PatrolIdleTime = 1f;
    [Range(0.05f, 10f)]
    [SerializeField] private float _MinChaceRange = 1f;
    [SerializeField] private Vector3[] _PatrolDirections;

    enum ItemType { one, two, three }
    struct Item
    {
        ItemType type;
        bool stackable;
        int amount;
    }

    private WaitForSeconds _patrolTime, _patrolIdleTime;
    private Transform _targetTransform;
    [SerializeField] private StateMachine _currentState;
    private Vector3 _moveDirection;
    private Vector3 _destination;
    private float _cooldownTimer;
    [SerializeField] private bool _canFire;

    private void Start()
    {
        _DetectionCollider.PlayerDetectedInRange += DetectionCollider_PlayerDetectedInRange;
        _patrolTime = new WaitForSeconds(_PatrolTime);
        _patrolIdleTime = new WaitForSeconds(_PatrolIdleTime);
    }

    private void DetectionCollider_PlayerDetectedInRange(object sender, Player player)
    {
        if (player)
        {
            _targetTransform = player.transform;
            _canFire = true;
        }
        else
        {
            if (_currentState != StateMachine.Chase)
            {
                _targetTransform = null;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetState(StateMachine.Idle);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetState(StateMachine.Patrol);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetState(StateMachine.Chase);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SetState(StateMachine.Alert);

        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer <= 0)
                _canFire = true;
        }

        switch (_currentState)
        {
            case StateMachine.Idle:
                if (_targetTransform)
                    Controller.Rotate(transform, _targetTransform.position, _RotationSpeed, _MinDisToRotate);
                break;
            case StateMachine.Alert:
                break;
            case StateMachine.Scavenge:
                break;
            case StateMachine.Patrol:
                if (!_targetTransform)
                {
                    Controller.MoveObjInDirection(transform, _moveDirection, _Speed);
                    Controller.Rotate(transform, _destination, _RotationSpeed, _MinDisToRotate);
                }
                else
                {
                    Controller.Rotate(transform, _targetTransform.position, _RotationSpeed, _MinDisToRotate);
                    if (_canFire)
                    {
                        Controller.Attack(_targetTransform, _ShootingPoint, _ShotPrefab);
                        _canFire = false;
                        _cooldownTimer = _CooldownTime;
                    }
                }
                break;
            case StateMachine.Chase:
                if (_targetTransform)
                {
                    Controller.Rotate(transform, _targetTransform.position, _RotationSpeed * 10, _MinDisToRotate);

                    if (Vector3.Distance(transform.position, _targetTransform.position) > _MinChaceRange)
                    {
                        Controller.MoveObjToPosition(transform, _targetTransform.position, _Speed * _ChaseSpeedMulti);
                    }

                    if (_canFire)
                    {
                        Controller.Attack(_targetTransform, _ShootingPoint, _ShotPrefab);
                        _canFire = false;
                        _cooldownTimer = _CooldownTime;
                    }
                }
                break;
            case StateMachine.Ensnare:
                break;
            default:
                break;
        }
    }

    public void SetDestination(Vector3 Destination) { _destination = Destination; }

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