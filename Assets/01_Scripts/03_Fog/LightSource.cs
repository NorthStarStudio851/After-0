using UnityEngine;

public enum PoleKind
{
        TorchPole = 0,
        LightPole = 1
}

public class LightSource : MonoBehaviour
{
   [Header("Kind")]
   [SerializeField] private PoleKind kind = PoleKind.TorchPole;

   [Header("Radius (metres)")]
   [SerializeField] private float torchPoleRadius = 21f;
   [SerializeField] private float lightPoleRadius = 35f;

   public float Radius => kind == PoleKind.LightPole ? lightPoleRadius : torchPoleRadius;

   private void OnDrawGizmosSelected()
   {
    Gizmos.color = kind == PoleKind.LightPole
    ? new Color(0.49f, 0.78f, 1f, 0.9f)
    : new Color(1f, 0.85f, 0.35f, 0.9f);

    Gizmos.DrawWireSphere(transform.position, Radius);
   }
}
