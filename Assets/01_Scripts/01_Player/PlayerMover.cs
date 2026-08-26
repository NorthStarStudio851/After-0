using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMover : MonoBehaviour
{
   [Header("Movement")]
   [SerializeField] private float moveSpeed = 6f;
   [SerializeField] private float gravity   = -20f;

    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;


   private CharacterController controller;
   private float verticalVelocity;

   private void Awake()
   {
    controller = GetComponent<CharacterController>();

    //Pointing this at the player himself makes every direction relative ti his own facing,
    //which turns steering into an endless circle
    if(cameraPivot == null || cameraPivot == transform)
    {
        if(Camera.main != null)
            {
                cameraPivot = Camera.main.transform;
            }
        else
            {
                Debug.LogWarning("PlayerMover: no camera pivot, movement falls back to world axes.", this);
            }
         }
    }

void Update()
    {
       Vector3 direction = CameraRelative(ReadInput());
        if(controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion = direction * moveSpeed + Vector3.up * verticalVelocity;
       controller.Move(motion * Time.deltaTime); 
    }

    private Vector2 ReadInput()
    {
        Vector2 input = Vector2.zero;

        if(Keyboard.current != null)
        {
                if(Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
                if(Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
                if(Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
                if(Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
        }
            if(input == Vector2.zero && Gamepad.current != null)
            {
                input = Gamepad.current.leftStick.ReadValue();
            }

            return Vector2.ClampMagnitude(input, 1f);
    }


  private Vector3 CameraRelative(Vector2 input)
   {
    if(cameraPivot == null) return new Vector3(input.x, 0f, input.y);

    Vector3 forward = cameraPivot.forward;
    Vector3 right   = cameraPivot.right;

    forward.y = 0f;
    right.y   = 0f;

    return forward.normalized * input.y + right.normalized * input.x;  
   }
    
}
