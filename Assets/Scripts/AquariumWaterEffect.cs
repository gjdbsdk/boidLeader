using UnityEngine;

public class AquariumWaterEffect : MonoBehaviour
{
    [Header("빛 일렁임 설정")]
    public Light[] waterLights;        // Point Light 3~4개 연결
    public float minIntensity = 300f;  // 최소 밝기
    public float maxIntensity = 700f;  // 최대 밝기
    public float waveSpeed = 1.5f;     // 일렁이는 속도
    public float moveRange = 2.0f;     // 조명이 좌우/앞뒤로 흔들리는 폭

    [Header("물결 표면 흐름 설정")]
    public float scrollSpeedX = 0.03f;
    public float scrollSpeedY = 0.02f;

    private Renderer waterRenderer;
    private Vector3[] initialPositions; // 각 조명의 원래 위치 저장

    void Start()
    {
        waterRenderer = GetComponent<Renderer>();

        // 시작할 때 각 조명의 원래 위치를 기억해 둠
        if (waterLights != null)
        {
            initialPositions = new Vector3[waterLights.Length];
            for (int i = 0; i < waterLights.Length; i++)
            {
                if (waterLights[i] != null)
                {
                    initialPositions[i] = waterLights[i].transform.localPosition;
                }
            }
        }
    }

    void Update()
    {
        if (waterLights != null)
        {
            for (int i = 0; i < waterLights.Length; i++)
            {
                if (waterLights[i] != null)
                {
                    // 1. 밝기 일렁임
                    float t = 0.5f + Mathf.Sin(Time.time * waveSpeed + i) / 2.0f;
                    waterLights[i].intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

                    // 2. 조명 위치를 살짝씩 흔들어서 바닥에 비치는 빛이 물결치게 연출
                    float offsetX = Mathf.Sin(Time.time * waveSpeed + i) * moveRange;
                    float offsetZ = Mathf.Cos(Time.time * waveSpeed * 0.8f + i) * moveRange;
                    waterLights[i].transform.localPosition = initialPositions[i] + new Vector3(offsetX, 0f, offsetZ);
                }
            }
        }

        if (waterRenderer != null)
        {
            Vector2 offset = new Vector2(Time.time * scrollSpeedX, Time.time * scrollSpeedY);
            waterRenderer.material.SetTextureOffset("_BaseMap", offset);
            waterRenderer.material.SetTextureOffset("_BumpMap", offset);
        }
    }
}