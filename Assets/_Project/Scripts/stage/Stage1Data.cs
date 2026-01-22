using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스테이지 클리어 조건 (아이템 이름 + 필요 개수)
/// </summary>
[System.Serializable]
public class StageRequirement
{
    public string itemName;
    public int requiredCount;
}

/// <summary>
/// 스테이지 데이터 ScriptableObject
/// Assets/Create/Game/Stage Data로 생성
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class Stage1Data : ScriptableObject
{
    public List<StageRequirement> requirements = new List<StageRequirement>();
}