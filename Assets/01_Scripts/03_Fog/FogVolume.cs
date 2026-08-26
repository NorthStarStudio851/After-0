using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class FogVolume : MonoBehaviour
{

    [Header("Terrain")]
    [SerializeField] private Terrain terrain;

    [Header("Placement")]
    [SerializeField] private float height = 3f;
    [SerializeField] private float margin = 40f;


   private MeshFilter meshFilter;
   private Mesh quad;

   private void OnEnable()
   {
    meshFilter = GetComponent<MeshFilter>();
    BuildQuad();
    FitToTerrain();
   }

private void OnDisable()
{
    if(quad == null) return;

    if(Application.isPlaying) Destroy(quad);
    else DestroyImmediate(quad);

    quad = null;
}

        [ContextMenu("Fit to terrain")]
   public void FitToTerrain()
   {
    if(terrain == null) terrain = Terrain.activeTerrain;

    Vector3 origin = terrain != null ? terrain.transform.position : Vector3.zero;
    Vector3 size   = terrain != null ? terrain.terrainData.size :new Vector3(255f, 0f, 255f);

    float width = Mathf.Max(size.x, size.z) + margin * 2f;

    transform.position = new Vector3(
        origin.x + size.x * 0.5f,
        origin.y + height,
        origin.z + size.z * 0.5f);

        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(width, 1f, width);
   }

 //Unity's built in quad faces +Z, which would need rotaing, and a rotated transform
   //makes the world-space maths in the shader harder to read
   private void BuildQuad()
   {
        quad= new Mesh
        {
                name = "Fog Plane",
                hideFlags = HideFlags.DontSave
        };

        quad.SetVertices(new[]
        {
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3(-0.5f, 0f,  0.5f),
            new Vector3( 0.5f, 0f,  0.5f),
            new Vector3( 0.5f, 0f, -0.5f)
        });

        quad.SetNormals(new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up });
        quad.SetUVs(0, new[]
        {
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(1f, 1f), new Vector2(1f, 0f )
        });

        quad.SetTriangles(new[] {0, 1, 2, 0, 2, 3 }, 0);
        quad.RecalculateBounds();

        meshFilter.sharedMesh = quad;
   }

   private void OnValidate()
   {
    height = Mathf.Max(0f, height);
    margin = Mathf.Max(0f, margin);

    if( isActiveAndEnabled && meshFilter != null) FitToTerrain();

   }
}

  

  

