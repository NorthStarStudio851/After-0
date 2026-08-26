using UnityEngine;

[ExecuteAlways]
public class FogViewer : MonoBehaviour
{
    private static readonly int ViewerProperty = Shader.PropertyToID("_FogViewer");

    [Header("Radius (metres)")]
    [SerializeField] private float dayRadius   = 13f;
    [SerializeField] private float nightRadius = 6.5f;
    [SerializeField] private float changeSpeed = 1.5f;

    [Header("Time of day")]
    [Range(0f, 1f)]
    [SerializeField] private float nightAmount;

    private float currentRadius;

    private void OnEnable()
    {
        currentRadius = TargetRadius();
        Push();
    }
    private void OnDisable()
    {
        //Leaving aa stable bubble behind would light in the middle of nowhere
    Shader.SetGlobalVector(ViewerProperty, Vector4.zero);
    }

    private void LateUpdate()
    {
         float wanted = TargetRadius();

         currentRadius = Application.isPlaying
         ? Mathf.MoveTowards(currentRadius, wanted, changeSpeed * Time.deltaTime) : wanted;

         Push();
    }

    public float TargetRadius() => Mathf.Lerp(dayRadius, nightRadius, nightAmount);

    private void Push()
    {
        Vector3 p = transform.position;
        Shader.SetGlobalVector(ViewerProperty, new Vector4(p.x, p.y, p.z, currentRadius));
    } 
    private void OnValidate()
    {
        dayRadius = Mathf.Max(0f, dayRadius);
        nightRadius = Mathf.Max(0f, nightRadius);
        changeSpeed = Mathf.Max(0.01f, changeSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.35f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, currentRadius > 0f ? currentRadius : dayRadius);
    }
   
}