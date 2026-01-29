using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 플레이어 Stun 시 화면 암전 효과
/// Stun 시작: 점점 어두워짐 (Fade Out)
/// Stun 종료: 즉시 밝아짐 (Fade In)
/// </summary>
public class StunScreenEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image blackScreen; // UI Image (검은색)

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 1f;  // 암전 시간
    [SerializeField] private float fadeInDuration = 0.3f; // 복구 시간

    private Coroutine currentFade;

    private void Start()
    {
        SetupBlackScreen();
    }

    private void SetupBlackScreen()
    {
        // 자동으로 Black Screen 생성 (없으면)
        if (blackScreen == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Stun Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // 최상위
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            GameObject screenObj = new GameObject("Black Screen");
            screenObj.transform.SetParent(canvas.transform, false);

            blackScreen = screenObj.AddComponent<Image>();
            blackScreen.color = new Color(0, 0, 0, 0); // 투명 시작
            blackScreen.raycastTarget = false;

            RectTransform rect = screenObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        // 초기 상태: 완전 투명
        SetAlpha(0);
    }

    /// <summary>Stun 시작 (외부에서 호출)</summary>
    public void StartStun(float stunDuration)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(StunSequence(stunDuration));
    }

    private IEnumerator StunSequence(float stunDuration)
    {
        // 1. 암전 (Fade Out)
        yield return FadeOut();

        // 2. 기절 시간 대기 (완전 검은 화면)
        float remainingTime = stunDuration - fadeOutDuration;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // 3. 복구 (Fade In)
        yield return FadeIn();
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1); // 완전 불투명
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0); // 완전 투명
    }

    private void SetAlpha(float alpha)
    {
        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            color.a = alpha;
            blackScreen.color = color;
        }
    }
}