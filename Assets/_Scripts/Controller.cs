using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static void MoveObjInDirection(Transform obj, Vector3 direction, float speed)
    {
        if (!obj || direction == Vector3.zero) { return; }

        Vector3 moveDirection = new Vector3(direction.x, direction.y, 0);
        obj.Translate(moveDirection * Time.deltaTime * speed);
    }

    public static void MoveObjToPosition(Transform obj, Vector3 position, float speed)
    {
        if (!obj || position == obj.position) { return; }

        Vector3 moveDirection = (position - obj.position).normalized;
        obj.Translate(moveDirection * Time.deltaTime * speed);
    }

    public static void Rotate(Transform obj, Vector3 destination, float rotationSpeed, float stopFromDestination)
    {
        if (Vector3.Distance(destination, obj.position) < stopFromDestination) { return; }

        destination.z = 0;
        Vector3 dirNormalized = destination - obj.position;

        float zRotateAngle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg - 90;

        Quaternion desiredRotation = Quaternion.Euler(0, 0, zRotateAngle);

        obj.rotation = Quaternion.RotateTowards(obj.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }

    public static void Attack(Transform firingUnit, Transform shootingPoint, GameObject shotPrefab)
    {
        if (shootingPoint && shotPrefab)
        {
            GameObject bulletGO = Instantiate(shotPrefab, shootingPoint.position, firingUnit.rotation);
            bulletGO.layer = firingUnit.gameObject.layer;
        }
    }

    public static void TakeDamage(Transform obj, Transform shootingPoint, GameObject shotPrefab)
    {

    }

}