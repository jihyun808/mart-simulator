using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StageRequirement
{
    public string itemName;    // 요구 아이템 이름
    public int requiredCount;  // 필요한 개수
}

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class Stage1Data : ScriptableObject
{
    public List<StageRequirement> requirements = new List<StageRequirement>();
}
