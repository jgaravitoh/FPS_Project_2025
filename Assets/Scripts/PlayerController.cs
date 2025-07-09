using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("----------- Mouse/Camera Variables -----------")]
    [SerializeField] private Transform viewPoint;
    [SerializeField] private bool invertVerticalMouseBool = false;
    private int verticalMouseDirection = -1;
    private float mouseSentitivity = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;
    private Camera cam;

    [Header("----------- Movement Variables -----------")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float activeMoveSpeed;

    [Header("----------- Jump Movement Variables -----------")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityMod = 2.5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] Transform groundCheckPoint;
    private bool isGrounded;
    


    private Vector3 moveDir, movement;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Camera stuff
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSentitivity;    
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z); // horizontal rotation 

        verticalRotStore = Mathf.Clamp(verticalRotStore + mouseInput.y, -60f, 60f);
        viewPoint.rotation = Quaternion.Euler(Mathf.Clamp( verticalRotStore * verticalMouseDirection, -60f ,60f), // vertical rotation (View Point)
                                                           viewPoint.rotation.eulerAngles.y,
                                                           viewPoint.rotation.eulerAngles.z);

        // Movement stuff
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"),0f,Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift)) { activeMoveSpeed = runSpeed; }
                                        else { activeMoveSpeed = walkSpeed; }
            
        float yVel = movement.y;
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
        movement.y = yVel;

        if (characterController.isGrounded) { movement.y = 0f; }
        
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

        if (Input.GetButtonDown("Jump") && isGrounded) { movement.y = jumpForce; }

        movement.y += (Physics.gravity.y * Time.deltaTime * gravityMod);

        characterController.Move( movement * Time.deltaTime );

        if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; }
        else if (Cursor.lockState == CursorLockMode.None) { if (Input.GetMouseButtonDown(0)) { Cursor.lockState = CursorLockMode.Locked;  } }
    }
    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }
    private void FixedUpdate()
    {
        Mouse_SetYInvert(invertVerticalMouseBool);
    }

    /// <summary>
    /// Sets the vertical camera movement direction based on the given boolean.
    /// </summary>
    /// <param name="invertVertical">
    /// If true, the vertical mouse movement will be inverted (e.g., moving the mouse up will look down).
    /// If false, the movement will be normal (not inverted).
    /// </param>
    public void Mouse_SetYInvert(bool invertVertical) { verticalMouseDirection = invertVertical ? 1 : -1; /* I know it looks weird... But for the movement not to be inverted, we have to invert it. */ }
    public bool Mouse_GetYinvert(){ return invertVerticalMouseBool; }
}
