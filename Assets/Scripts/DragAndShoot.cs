using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DragAndShoot : MonoBehaviour
{
    // public objects are implemented from scene
    public LineRenderer jointRenderer = default;
    public Rigidbody wallBody;
    public Canvas gameOverPanel;
    public Canvas demoFinishedPanel;
    public Slider levelProgress;

    // private objects controlled by scripts
    private Vector3 mousePressDownPos;
    private Vector3 mouseReleasePos;

    private Rigidbody rb;
    private RigidbodyConstraints originalConstraints;
    private JointRendererCollider jrCollider;
    private SpringJoint springJoint;
    private GameObject endPortal;

    private bool isShoot;
    private bool portalCreated;
    private float portalScale;


    void Start()
    {
        // Game reset timeScale
        Time.timeScale = 1;
        
        // Get the required objects in the scene
        rb = GetComponent<Rigidbody>();
        springJoint = GetComponent<SpringJoint>();
        jrCollider = GetComponentInChildren<JointRendererCollider>();

        /*
         We need to save the constraints for the ball (Freeze positions and rotations)
         The constraints make the ball not move to the left or right 
         (freeze all rotations and positions but y axis)
        */
        originalConstraints = rb.constraints;
        CalculateJointRenderer();

    }


    // Make the connection correct between ball and wall
    private void CalculateJointRenderer()
    {
        jointRenderer.SetPosition(0, transform.position);
        jointRenderer.SetPosition(1,
            new Vector3(wallBody.transform.position.x,
            transform.position.y, wallBody.transform.position.z));
    }

    #region PreviousVersion
    // Check the touch release position
    /*private void OnMouseUp()
    {
        mouseReleasePos = Input.mousePosition;
        if ((mouseReleasePos.y - mousePressDownPos.y) < 0)
        {
            Shoot(mouseReleasePos - mousePressDownPos);
        }
    }*/

    // Add force to the object to bend the connection
    /*void OnMouseDrag()
    {
        float firstPos = mousePressDownPos.y;
        float res = (firstPos - Input.mousePosition.y);
        if (res > 0)
            rb.AddForce(0, Mathf.Min(-res, 400), 0, ForceMode.Force);

        // Spring effect
        jointRenderer.SetPosition(0, transform.position);
    }*/

    #endregion



    // If the conditions are set, shoot the ball
    private float forceMultiplier = 3;
    void Shoot(Vector3 Force)
    {
        // If the ball is already shot, return
        if (isShoot) { return; }

        // Else apply force to the ball to make it jump
        rb.AddForce(new Vector3(1, Mathf.Min(-Force.y,400), 1) * forceMultiplier);
        isShoot = true;
        Destroy(springJoint);

    }

    


    void Update()
    {
        // Region InputCheck
        #region InputCheck

        // Add force to the object to bend the connection
        if (Input.GetMouseButton(0))
        {
            float firstPos = mousePressDownPos.y;
            float res = (firstPos - Input.mousePosition.y);
            if (res > 0)
                rb.AddForce(0, Mathf.Min(-res, 400), 0, ForceMode.Force);

            // Spring effect
            jointRenderer.SetPosition(0, transform.position);
        }

        // Check the touch release position
        if (Input.GetMouseButtonUp(0))
        {
            mouseReleasePos = Input.mousePosition;
            // Minimum force required set
            if ((mouseReleasePos.y - mousePressDownPos.y) < -150)
            {
                Shoot(mouseReleasePos - mousePressDownPos);
            }
        }

        #endregion


        // Region trigger control
        #region TRIGGERCONTROL
        // If the player touches the screen, check conditions
        if (Input.GetMouseButtonDown(0))
        {
            // Get the touch position for touch dragging
            mousePressDownPos = Input.mousePosition;

            // Needed conditions for attaching ball to wall
            if (isShoot && !springJoint &&
                (jrCollider.currentSituation != Situations.GREY_TRIGGERED) &&
                (jrCollider.currentSituation != Situations.END_LEVEL) &&
                (jrCollider.currentSituation != Situations.NOT_TRIGGERED))
            {
                // If the player hits the red obstacle, game ends
                if (jrCollider.currentSituation == Situations.RED_TRIGGERED)
                {
                    gameOverPanel.gameObject.SetActive(true);
                    Time.timeScale = 0;
                }

                // Else, create SpringJoint
                springJoint = rb.gameObject.AddComponent<SpringJoint>();
                springJoint.connectedBody = wallBody;
                springJoint.spring = 350;
                jointRenderer.enabled = true;
                CalculateJointRenderer();
                isShoot = false;
            }

            // If the player hits the grey obstacle, do not attach it to wall and make the ball go down
            if (jrCollider.currentSituation == Situations.GREY_TRIGGERED)
            {
                rb.constraints = RigidbodyConstraints.FreezePositionY;
                rb.constraints = originalConstraints;
            }

        }

        // If SpringJoint created, attach it to the ball
        if (springJoint)
        {
            jointRenderer.SetPosition(0, transform.position);
            jointRenderer.enabled = true;
        }

        // If the ball is shot, remove the SpringJoint
        if (isShoot)
        {
            jointRenderer.enabled = false;
        }


        #endregion

        levelProgress.value = transform.position.y / (wallBody.position.y*2);



        // Region end game portal
        #region ENDGAMEPORTAL
        // CREATE PORTAL WITH LERP WHEN PLAYER PASS THE FINISH LINE

        // Create the portal only once per level
        if (jrCollider.currentSituation == Situations.END_LEVEL && !portalCreated)
        {
            portalCreated = true;
            endPortal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Rigidbody endRigid=endPortal.AddComponent<Rigidbody>();
            endRigid.useGravity = false;
            endRigid.constraints = RigidbodyConstraints.FreezeAll;
            endPortal.GetComponent<CapsuleCollider>().isTrigger = true;
            endPortal.tag="endPortal";
        }

        // Lerping the portal
        if (portalCreated && portalScale < 1)
        {
            endPortal.transform.position = new Vector3(0, 60, 0);
            endPortal.GetComponent<CapsuleCollider>().radius = 0.15f;
            endPortal.GetComponent<CapsuleCollider>().center = new Vector3(0, 0, 0);
            endPortal.transform.localScale = new Vector3(
                Mathf.Lerp(0, 2, portalScale),
                0.1f,
                Mathf.Lerp(0, 2, portalScale)
            );
            portalScale += 0.75f * Time.deltaTime;
        }

        #endregion

    }

    // Game contains only one scene, you can use the prefabs to create new levels
    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }

    // Ground check
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            gameOverPanel.gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        if (other.CompareTag("endPortal"))
        {
            demoFinishedPanel.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
