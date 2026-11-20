using UnityEngine;
using System.Collections;

public class FlockController : MonoBehaviour
{
    public Vector3 bound;
    public float speed = 100.0f;
    public float targetReachedRadius = 10.0f; // 목표 지점에 도착했는지 판단기준

    private Vector3 initialPosition;
    private Vector3 nextMovementPoint;

    void Start()
    {
        initialPosition = transform.position; // 현재 위치
        CalculateNextMovementPoint();
    }

    void CalculateNextMovementPoint()
{
    float posX = Random.Range(initialPosition.x - bound.x, initialPosition.x + bound.x);
    float posY = Random.Range(initialPosition.y - bound.y, initialPosition.y + bound.y);
    float posZ = Random.Range(initialPosition.z - bound.z, initialPosition.z + bound.z);

    nextMovementPoint = new Vector3(posX, posY, posZ); 
}

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime); // 방향벡터 * 거리 -> 이동 (직진 먼저)
        transform.rotation = Quaternion.Slerp(transform.rotation, 
            Quaternion.LookRotation(nextMovementPoint - transform.position), // 현재 위치에서 다음 위치로 이동하는 벡터
            1.0f * Time.deltaTime); // 회전 나중에
        // -> Update()는 계속 수행되기때문에 사실상 회전->직진 === 직진->회전

        if (Vector3.Distance(nextMovementPoint, transform.position) <= targetReachedRadius){ // 도착함
            CalculateNextMovementPoint(); // 다음위치 계산 및 설정
        }
    }
}
