using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StageRequirement
{
    public string itemName;    // 요구 아이템 이름
    public int requiredCount;  // 필요한 개수
}

public class Stage1Data : MonoBehaviour
{
    public List<StageRequirement> requirements; // 스테이지 요구 목록
}
