using UnityEngine;
using System.Collections;
using Debug = System.Diagnostics.Debug;

public class Flock : MonoBehaviour 
{
    public LayerMask obstacleLayer;       // Obstacle 레이어 지정
    public float obstacleDetectRange = 6f; // 기둥을 미리 감지할 거리
    public float obstacleAvoidForce = 40f; // 기둥에서 튕겨 나가는 힘

    public float minSpeed = 100.0f;         // movement speed of the flock
    public float turnSpeed = 20.0f;         // rotation speed of the flock
    public float randomFreq = 20.0f;        // 얼마나 randomPush값을 업데이트할지 -> flock의 미세한 움직임 결정    

    public float randomForce = 20.0f;       // flock의 미세한 움직임의 힘 결정
    public float toOriginForce = 20.0f;     // cohesion관련 변수
    public float toOriginRange = 100.0f;    // 재정렬 매개변수, 무리의 확산 범위 결정

    public float gravity = 2.0f;            

    public float avoidanceRadius = 400.0f;  // 개별 flock들 간의 거리 반경
    public float avoidanceForce = 20.0f;

    public float followVelocity = 4.0f;
    public float followRadius = 40.0f;      // Minimum Follow distance to the leader

    private Transform origin;               // Parent transform, FlockController
    private Vector3 velocity;               // Velocity of the flock
    // velocity : 속도 : 벡터(sx, sy, sz) / speed : 속력 : 스칼라 / v * dt = (sx * dt, sy * dt, sz * dt) : 변위 : 벡터 / speed * dt : 거리 : 스칼라
    private Vector3 normalizedVelocity; // 절댓값 -> 방향만 남김, 거리는 없앰 (단위벡터)
    private Vector3 randomPush;             // Random push value
    private Vector3 originPush;
    private Transform[] objects;            // Flock objects in the group
    // ___[] : 리스트 -> 무리 내 다른 flock들을 담을 리스트
    private Flock[] otherFlocks;       // Unity Flocks in the group
    private Transform transformComponent;   // My transform (현재 스크립트가 적용될 flock의)
    public float randomFreqInterval; // 추가됨

    void Start ()
    {
        // FlockController를 따라가게 하되, 그룹화되어 하나의 개체처럼 움직이지는 않게 함
        randomFreqInterval = 1.0f / randomFreq; // -> 역수를 취함, 주기(시간) = 1/주파수 ; 1/20s
        origin = transform.parent; // FlockController, 부모자식관계는 transform으로 접근
        transformComponent = transform; // 현재 flock 위치
        Flock[] tempFlocks = null;
        if (transform.parent){
            tempFlocks = transform.parent.GetComponentsInChildren<Flock>(); // FlockController 내부 모든 Flock(자신 포함)을 다 가져옴
        }
        Debug.Assert(tempFlocks != null, nameof(tempFlocks) + " != null"); 

        objects = new Transform[tempFlocks.Length];
        otherFlocks = new Flock[tempFlocks.Length];

        for (int i = 0 ; i < tempFlocks.Length ; i++){ // 10번 (flock 개수만큼) 반복
            objects[i] = tempFlocks[i].transform;
            otherFlocks[i] = tempFlocks[i];
        }
        transform.parent = null; // 부모(FlockController) & 자식(flock)관계 끊어내기
        // 시험 출제! -> 독립적으로 움직이는 것처럼 보이게 하기 위해 (따라서 가긴 하지만, 완전히 똑같이 가진 않음)

        // 시험 출제!
        StartCoroutine(UpdateRandom()); // IEnumerator 인터페이스 형태의 다른 함수를 전달받음
        // 동시에 여러 개의 작업을 진행 / 시간을 지연시켜 작업을 수행할 때 사용
        minSpeed *= Random.Range(0.8f, 1.2f);
    }

    // 시험 출제!
    // subroutine(NoCoroutine)              VS      coroutine
    // return                               VS      yield return (yield : 양보)
    // 시간 예약 시 단일 작업(기다림) 수행    VS      시간 예약, 다른 작업과 함께 수행

    IEnumerator UpdateRandom () // IEnumerator 인터페이스 형태의 함수 ; yield return형태로 반환해야 함
    {
        while(true) // 무한루프 ; 게임이 종료될 때 까지 실행됨, but coroutine이라서 다른 행동과 동시에 실행될 수 있음!
        {
            randomPush = Random.insideUnitSphere * randomForce; // Flock이 흔들릴 때 임의의 방향 설정
            // Random.insideUniSphere : 반지름이 1인 단위 원 내에서 원점 -> 구 내부의 임의의 한 점인 벡터 (길이 0~1, 방향 0~360)
            yield return new WaitForSeconds(randomFreqInterval + Random.Range(-randomFreqInterval / 2.0f, randomFreqInterval / 2.0f));
            // 0.05 +- 1/2*0.05
            // randomPush를 무작위 시간동안 적용시킴
        }
    }

    void Update ()
    { 
        // Internal variables
	    float speed = velocity.magnitude; // velocity의 크기 == 속력 == |속도|
        // *중요 : 평균을 낼 때는 "평균을 담을 변수를 0으로 초기화"하고, for문 내에서 덧셈 후 길이로 나눔
        Vector3 avgVelocity = Vector3.zero; // 평균 속도 변수, "(0.0f, 0.0f, 0.0f)로 초기화"
        Vector3 avgPosition = Vector3.zero; // 평균 위치 변수, "(0.0f, 0.0f, 0.0f)로 초기화"
        float count = 0; // 분모가 될 변수
        float f = 0.0f;
        float d = 0.0f;
        Vector3 myPosition = transformComponent.position; // 현재 flock 위치
        Vector3 forceV;
        Vector3 toAvg;
        Vector3 wantedVel;
        
        // 앞쪽에 Obstacle 레이어(기둥, 지형)가 감지되면 표면의 반대 방향(hit.normal)으로 회피
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleDetectRange, obstacleLayer))
        {
            // 기둥 표면에서 바깥쪽으로 밀어내는 힘 추가
            velocity += hit.normal * obstacleAvoidForce * Time.deltaTime;
        }

        for(int i = 0;i<objects.Length;i++) // object : 나를 포함한 FlockController의 자식들
        {
            Transform boidTransform = objects[i]; // 10개를 일단 가져옴
            if (boidTransform != transformComponent){ // 본인을 제외한 flock들만 계산하기 위한 if문
                Vector3 otherPosition = boidTransform.position; // 다른 flock 하나의 위치 
                // 평균 계산 코드
                avgPosition += otherPosition;
                count++;
                forceV = myPosition - otherPosition; // 다른 flock의 위치 -> 현재 flock위치의 벡터
                float directionMagnitude = forceV.magnitude; // 다른 flock과 현재 flock 사이의 거리
                float forceMagnitude = 0.0f; 
                if(directionMagnitude < followRadius){ // followRadius : 40
                    if (directionMagnitude < avoidanceRadius){ // avoidanceRadius : 50 -> 30으로 바꾸던가 해야함
                        // forceMagnitude의 쓰임 A
                        // Separation 수행
                        forceMagnitude = 1.0f - (directionMagnitude / avoidanceRadius); // 멀어지는 힘 계산
                        // f = 1 - d / 50
                        // d = 0 -> f = 1
                        // d = 50 -> f = 0 반비례관계

                        if (directionMagnitude > 0){ // 길이 값 ; 음수일 수 없음, === directionMagnitude != 0
                            avgVelocity += (forceV / directionMagnitude) * forceMagnitude * avoidanceForce; // separation 실행
                            // forceV / direction : 상대방->현재 Flock으로의 단위벡터 설정
                            // 상대방과 현재 flock이 너무 가까움 -> 상대방->현재Flock방향으로 avoidanceForce만큼 현재 Flock이 이동하게 함
                        } // 여러 벡터를 다 더해 방향 계산
                    }
                    // forceMagnitude의 쓰임 B
                    forceMagnitude = directionMagnitude / followRadius; 
                    Flock tempOtherBoid = otherFlocks[i];
                    avgVelocity += followVelocity * forceMagnitude * tempOtherBoid.normalizedVelocity; // alignment 실행
                    // tempOtherBoid.normalizedVelocity : 다른 Flock의 이동 방향
                }
            }
        }
        
        if(count > 0) // 현재 Flock 말고도 다른 Flock이 존재함
        {
            avgVelocity /= count; // 평균 속도 계산, 이전까지의 avgVelocity는 단순 함계상태
            toAvg = (avgPosition / count) - myPosition; // 현재 위치 -> 무리 평균 위치, Cohesion 실행
        }	
        else
            toAvg = Vector3.zero; // Flock이 나뿐이라면 계산 불필요
        
        //Directional Vector to the leader
	    forceV = origin.position -  myPosition; // 현재 위치 -> FlockController 벡터
        d = forceV.magnitude;   // FlockController의 방향 단위벡터
        f = d / toOriginRange;

        //Calculate the velocity of the flock to the leader
        if(d > 0) {
            originPush = f  * toOriginForce * forceV / f;
            // originPush = (forceV / d) * f * toOriginForce;
            
        }
        if(speed < minSpeed && speed > 0)
        {
		    velocity = (velocity / speed) * minSpeed;
            // 방향(단위벡터) * 최소속력
        }
        
        wantedVel = velocity;
		
        wantedVel -= wantedVel * Time.deltaTime;
        // 이전 영향 제거
        wantedVel += randomPush * Time.deltaTime;
        // 힘 (가속도)
        wantedVel += originPush * Time.deltaTime;
        // cohesion 계산
        wantedVel += avgVelocity * Time.deltaTime;
        // separation, alignment 계산
        wantedVel += gravity * Time.deltaTime * toAvg.normalized;
        // 만유인력(FlockController을 향해 가는 힘)

        velocity = Vector3.RotateTowards(velocity, wantedVel, turnSpeed * Time.deltaTime, 100.00f);
        transformComponent.rotation = Quaternion.LookRotation(velocity);
        transformComponent.Translate(velocity * Time.deltaTime, Space.World);

        normalizedVelocity = velocity.normalized; // 단위벡터로 변환
    }
}