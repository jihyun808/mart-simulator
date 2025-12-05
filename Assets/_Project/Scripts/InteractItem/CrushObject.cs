// CrushObject.cs
using UnityEngine;

public class CrushObject : MonoBehaviour
{
    [Header("Replacement Settings")]
    [SerializeField] private Sprite replacementSprite;
    [SerializeField] private bool destroyOriginal = true;
    [SerializeField] private float imageHeightOffset = 0.01f;
    [SerializeField] private Vector2 imageSize = new Vector2(0.03f, 0.03f);

    [Header("Image Settings")]
    [SerializeField] private Color imageColor = Color.white;
    [SerializeField] private int sortingOrder = 0;
    [SerializeField] private string sortingLayer = "Default";

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private string groundTag = "Ground";

    private bool hasReplaced = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasReplaced) return;

        if (IsGround(collision.gameObject))
        {
            ReplaceWithSprite(collision.GetContact(0).point);
        }
    }

    private bool IsGround(GameObject obj)
    {
        bool isGroundLayer = ((1 << obj.layer) & groundLayer) != 0;
        bool hasGroundTag = string.IsNullOrEmpty(groundTag) || obj.CompareTag(groundTag);
        
        return isGroundLayer && hasGroundTag;
    }

    private void ReplaceWithSprite(Vector3 hitPoint)
    {
        if (replacementSprite == null) return;

        hasReplaced = true;

        GameObject imageObject = new GameObject("ReplacementImage");
        imageObject.transform.position = hitPoint + Vector3.up * imageHeightOffset;
        imageObject.transform.rotation = Quaternion.Euler(90, 0, 0);

        SpriteRenderer spriteRenderer = imageObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = replacementSprite;
        spriteRenderer.color = imageColor;
        spriteRenderer.sortingLayerName = sortingLayer;
        spriteRenderer.sortingOrder = sortingOrder;

        imageObject.transform.localScale = new Vector3(imageSize.x, imageSize.y, 1f);
        
        AIController[] ais = FindObjectsOfType<AIController>();
        foreach (var ai in ais)
        {
            PlayerSuspicionDetector detector = ai.GetComponent<PlayerSuspicionDetector>();
            if (detector != null)
            {
                detector.OnObjectBreak();
            }
        }

        if (destroyOriginal)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}