using UnityEngine;
using System;

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
    private Vector3 moveDir, movement;

    [Header("----------- Jump Movement Variables -----------")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityMod = 2.5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] Transform groundCheckPoint;
    private bool isGrounded;

    [Header("----------- Shooting Mechanic Variables -----------")]
    [SerializeField] private GameObject bulletImpact;
    public float timeBetweenShots = 0.1f;
    private float shotCounter;

    public float maxHeat = 10f, heatPerShot = 1f, coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] allGuns;
    private int selectedGun;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat;

        SwitchGun();
    }

    // Update is called once per frame
    void Update()
    {
        #region Camera stuff
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSentitivity;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z); // horizontal rotation 

        verticalRotStore = Mathf.Clamp(verticalRotStore + mouseInput.y, -60f, 60f);
        viewPoint.rotation = Quaternion.Euler(Mathf.Clamp(verticalRotStore * verticalMouseDirection, -60f, 60f), // vertical rotation (View Point)
                                                           viewPoint.rotation.eulerAngles.y,
                                                           viewPoint.rotation.eulerAngles.z);
        #endregion

        #region Movement stuff
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift)) { activeMoveSpeed = runSpeed; }
        else { activeMoveSpeed = walkSpeed; }

        float yVel = movement.y;
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
        movement.y = yVel;

        if (characterController.isGrounded) { movement.y = 0f; }

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

        if (Input.GetButtonDown("Jump") && isGrounded) { movement.y = jumpForce; }

        movement.y += (Physics.gravity.y * Time.deltaTime * gravityMod);

        characterController.Move(movement * Time.deltaTime);
        #endregion

        #region Shooting stuff
        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            if (Input.GetMouseButton(0))
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0) { Shoot(); }
            }
            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;

            if (heatCounter <= 0) { overHeated = false;
                UIController.instance.overheatedMessage.gameObject.SetActive(false);
            }
        }
        if (heatCounter < 0) { heatCounter = 0; }

        UIController.instance.weaponTempSlider.value = heatCounter;

        // MOSUE SCROLLING THE AVAILABLE GUNS
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0) { selectedGun++; selectedGun = (int)nfmod((float)selectedGun, (float)allGuns.Length); SwitchGun(); }
        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0) { selectedGun--; selectedGun = (int)nfmod((float)selectedGun, (float)allGuns.Length); SwitchGun(); }


        #endregion

        #region Handle mouse state for the game
        if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; }
        else if (Cursor.lockState == CursorLockMode.None) { if (Input.GetMouseButtonDown(0)) { Cursor.lockState = CursorLockMode.Locked; } }
        #endregion

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

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Destroy(bulletImpactObject, 10f);
        }
        shotCounter = timeBetweenShots;

        heatCounter += heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }
    }
    private void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
    }

    float nfmod(float a, float b) { return a - b * (float)Math.Floor(a / b); }
}
