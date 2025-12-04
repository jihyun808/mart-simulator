// SceneHistory.cs
public static class SceneHistory
{
    public static string LastSceneName { get; set; } = string.Empty;

    public static void Clear()
    {
        LastSceneName = string.Empty;
    }

    public static bool HasHistory()
    {
        return !string.IsNullOrEmpty(LastSceneName);
    }
}