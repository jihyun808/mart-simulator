using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OutlineEffect : MonoBehaviour
{
    [Header("Outline")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0.001f, 0.2f)] private float outlineWidth = 0.04f;

    private Material outlineMaterial;
    private GameObject outlineObject;

    private void Awake()
    {
        CreateOutlineMaterial();
        CreateOutlineMesh();
    }

    private void CreateOutlineMaterial()
    {
        Shader shader = Shader.Find("Custom/URP/OutlineUnlit");
        if (shader == null)
        {
            Debug.LogError("[OutlineEffect] Shader not found!");
            return;
        }

        outlineMaterial = new Material(shader);
        outlineMaterial.SetColor("_BaseColor", outlineColor);
        
        // ⭐ 렌더큐 변경
        outlineMaterial.renderQueue = (int)RenderQueue.Geometry + 1;
    }

    private void CreateOutlineMesh()
    {
        if (outlineMaterial == null) return;

        var originalMF = GetComponent<MeshFilter>();
        var originalMesh = originalMF.sharedMesh;
        if (originalMesh == null)
        {
            Debug.LogError("[OutlineEffect] Original mesh is null.");
            return;
        }

        outlineObject = new GameObject("Outline_Mesh");
        outlineObject.transform.SetParent(transform, false);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * (1f + outlineWidth);

        var outlineMF = outlineObject.AddComponent<MeshFilter>();
        outlineMF.sharedMesh = originalMesh;

        var outlineMR = outlineObject.AddComponent<MeshRenderer>();
        outlineMR.sharedMaterial = outlineMaterial;
        outlineMR.shadowCastingMode = ShadowCastingMode.Off;
        outlineMR.receiveShadows = false;

        outlineObject.layer = gameObject.layer;

        // ⭐ 기본적으로 켜두기 (필요시 나중에 끄기)
        outlineObject.SetActive(true);
    }

    public void SetOutlineActive(bool active)
    {
        if (outlineObject != null)
            outlineObject.SetActive(active);
    }

    private void OnDestroy()
    {
        if (outlineMaterial != null) Destroy(outlineMaterial);
    }
}