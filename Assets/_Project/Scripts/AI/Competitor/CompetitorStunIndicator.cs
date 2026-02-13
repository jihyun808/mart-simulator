using UnityEngine;

public class CompetitorStunIndicator : MonoBehaviour
{
    [SerializeField] private GameObject stunIcon; // 아이콘만 연결

    private void Awake()
    {
        if (stunIcon != null)
            stunIcon.SetActive(false);
    }

    public void Show()
    {
        if (stunIcon != null)
            stunIcon.SetActive(true);
    }

    public void Hide()
    {
        if (stunIcon != null)
            stunIcon.SetActive(false);
    }
}
