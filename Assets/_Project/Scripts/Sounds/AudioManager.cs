using UnityEngine;

/// <summary>
/// 게임 효과음 중앙 관리 시스템
/// 싱글톤 패턴으로 어디서든 AudioManager.Instance.PlaySFX() 호출 가능
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("AI Sound Effects")]
    [SerializeField] private AudioClip aiWalkSound;
    [SerializeField] private AudioClip aiRunSound;

    [Header("Game State Sound Effects")]
    [SerializeField] private AudioClip gameClearSound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Player Sound Effects")]
    [SerializeField] private AudioClip stunSound;

    [Header("Item Sound Effects")]
    [SerializeField] private AudioClip objectBreakSound;
    [SerializeField] private AudioClip itemAddSound;
    [SerializeField] private AudioClip itemRemoveSound;

    [Header("Cashier Sound Effects")]
    [SerializeField] private AudioClip cashierRejectSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>효과음 재생 (한 번만)</summary>
    public void PlaySFX(SFXType type)
    {
        AudioClip clip = GetClip(type);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>루프 효과음 재생 (AI 걷기/뛰기용)</summary>
    public void PlayLoopSFX(SFXType type)
    {
        AudioClip clip = GetClip(type);
        if (clip != null && sfxSource.clip != clip)
        {
            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
    }

    /// <summary>루프 효과음 정지</summary>
    public void StopLoopSFX()
    {
        if (sfxSource.isPlaying && sfxSource.loop)
        {
            sfxSource.Stop();
            sfxSource.loop = false;
        }
    }

    private AudioClip GetClip(SFXType type)
    {
        switch (type)
        {
            case SFXType.AIWalk:
                return aiWalkSound;
            case SFXType.AIRun:
                return aiRunSound;
            case SFXType.GameClear:
                return gameClearSound;
            case SFXType.GameOver:
                return gameOverSound;
            case SFXType.Stun:
                return stunSound;
            case SFXType.ObjectBreak:
                return objectBreakSound;
            case SFXType.ItemAdd:
                return itemAddSound;
            case SFXType.ItemRemove:
                return itemRemoveSound;
            case SFXType.CashierReject:
                return cashierRejectSound;
            default:
                return null;
        }
    }

    /// <summary>마스터 볼륨 조절</summary>
    public void SetVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}

public enum SFXType
{
    AIWalk,
    AIRun,
    GameClear,
    GameOver,
    Stun,
    ObjectBreak,
    ItemAdd,
    ItemRemove,
    CashierReject
}