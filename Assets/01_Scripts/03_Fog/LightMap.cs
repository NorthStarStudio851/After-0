using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LightMap : MonoBehaviour
{
    private static readonly int MapProperty = Shader.PropertyToID("_LightMap");
    private static readonly int BoundsProperty = Shader.PropertyToID("_LightMapBounds");

    public static LightMap Instance {get; private set;}

    [Header("Terrain")]
    [SerializeField] private Terrain terrain;

    [Header("Texture")]
    [Tooltip("Pixels across the whole terrain. 128 over 255m is about 2m per pixel.")]
    [SerializeField] private int resolution = 128;

    [Tooltip("How much of the outer radius fades out, 0 = hard edge")]
    [Range(0f, 0.6f)]
    [SerializeField] private float edgeSoftness = 0.18f;

    private readonly List<LightSource> sources = new List<LightSource>();

    private Texture2D map;
    private byte[] levels;

    private Vector3 origin;
    private float worldSize = 255f;
    private bool dirty;

    private void OnEnable()
    {
        Instance = this;

        ResolveTerrain();
        BuildTexture();

        sources.AddRange(FindObjectsByType<LightSource>(FindObjectsSortMode.None));

        dirty = true;
    }

    private void OnDisable()
    {
        if(Instance == this) Instance = null;

        sources.Clear();

    if(map == null) return;

    if(Application.isPlaying) Destroy(map);
    else DestroyImmediate(map);

    map = null;
    levels = null;
    }

    private void LateUpdate()
    {
        if(!dirty) return;

        dirty = false;
        Rebuild();
    }

    private void ResolveTerrain()
    {
        if(terrain == null) terrain = Terrain.activeTerrain;

        if(terrain != null)
        {
            Vector3 size = terrain.terrainData.size;
            origin = terrain.transform.position;
            worldSize = Mathf.Max(size.x, size.z);
        }
        else
        {
            origin = Vector3.zero;
            worldSize = 255f;
        }
    }
    private void BuildTexture()
    {
        resolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(resolution), 32, 512);

        if(map != null && map.width == resolution) return;

        if(map != null)
        {
            if(Application.isPlaying) Destroy(map);
            else DestroyImmediate(map);
        }

        //R8 is one byte per pixel. RGBA would cost four times as much for three channels nothind ever reads.

        map = new Texture2D(resolution, resolution, TextureFormat.R8, false, true)
        {
            name = "Light Map", 
            wrapMode = TextureWrapMode.Clamp, 
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave
        };

        levels = new byte[resolution * resolution];
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        if(map == null || levels ==null) BuildTexture();
        if(map == null)  return;

        ResolveTerrain();

        System.Array.Clear(levels, 0, levels.Length);

        for(int i = 0; i < sources.Count; i++) Stamp(sources[i]);

        map.SetPixelData(levels, 0);
        map.Apply(false, false);

        PushToShaders(); 
    }


private void Stamp(LightSource source)
{
if (source == null || !source.isActiveAndEnabled) return;

float radius = source.Radius;
if(radius <= 0f) return;

float metresPerPixel = worldSize / resolution;
Vector3 centre = source.transform.position;

float localX = centre.x - origin.x;
float localZ = centre.z - origin.z;

//Only the box around this pole, never the whole texture. With fifty poles difference is twenty thousand checks against eight hundred thousand

int minX = Mathf.Max(0, Mathf.FloorToInt((localX - radius) / metresPerPixel));
int maxX = Mathf.Min( resolution - 1,Mathf.CeilToInt((localX + radius)/metresPerPixel));
int minY = Mathf.Max(0, Mathf.FloorToInt((localZ - radius) / metresPerPixel));
int maxY = Mathf.Min( resolution - 1,Mathf.CeilToInt((localZ + radius)/metresPerPixel));

for(int y = minY; y <= maxY; y++)
{
    float pz = (y + 0.5f) * metresPerPixel - localZ;

    for(int x = minX; x <= maxX; x++ )
    {
        float px = (x + 0.5f) * metresPerPixel - localX;

        float value = Falloff(Mathf.Sqrt(px * px + pz * pz) / radius);
        if(value <= 0f ) continue;

        int index = y * resolution + x;
        byte written = (byte)( value * 255f);

        //Circles overlap constantly, so the brightest one wins
        if (written > levels[index]) levels[index] = written;
             }

        }

    }
    // 1 in the middle, easing to 0 across the outer edgeSoftness of the radius
    private float Falloff(float normalised)
    {
        if(normalised >= 1f) return 0f;
        if(edgeSoftness <= 0.001f) return 1f;

        float t = Mathf.Clamp01((1f - normalised) / edgeSoftness);
        return t * t * (3f -2f *t);
    }

    private void PushToShaders()
    {
        if (map == null) return;

        Shader.SetGlobalTexture(MapProperty, map);

        //xy = terrain corner, z = size in metres, w =1/size so the shader skips a divide
        Shader.SetGlobalVector(BoundsProperty,
        new Vector4(origin.x, origin.z, worldSize, 1f / Mathf.Max(0.01f, worldSize)));
    }

    private void OnValidate()
    {
        resolution =Mathf.Clamp(resolution, 32,512);
        dirty = true;
    }

    private void OnDrawGizmosSelected()
    {
        ResolveTerrain();
         Gizmos.color = new Color(1f, 0.58f, 0.35f, 0.6f);
         Vector3 centre = origin + new Vector3(worldSize * 0.5f, 0f, worldSize * 0.5f);
         Gizmos.DrawWireCube(centre, new Vector3(worldSize, 0.1f, worldSize));
    }
}
