/// <summary>
/// 씬 이동 기록 관리 (설정 메뉴에서 돌아올 때 사용)
/// Static 클래스로 씬 간 데이터 유지
/// </summary>
public static class SceneHistory
{
    public static string LastSceneName { get; set; } = string.Empty;

    /// <summary>기록 초기화</summary>
    public static void Clear()
    {
        LastSceneName = string.Empty;
    }

    /// <summary>저장된 기록이 있는지 확인</summary>
    public static bool HasHistory()
    {
        return !string.IsNullOrEmpty(LastSceneName);
    }
}