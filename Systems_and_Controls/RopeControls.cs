using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RopeControls : MonoBehaviour
{
    //Character Components
    private Rigidbody2D rb2d;
    private Animator animator;
    private PlayerControlor playerControlor;

    public Dictionary<string, GameObject> segmentHashtable = new Dictionary<string, GameObject>();
    private float xOffset = 0.0f;
    public GameObject previousParent;

    //Climbing and swinging speed
    float climbingSpeed = 1.5f;
    float slippingSpeed = 1.5f;
    public float horizontalSpeed = 3.0f;

    //Slip down and slip up switches
    bool slipsDownABit = false;
    bool slipsUpABit = false;

    //Enable to be able to move horizontally
    bool canMoveHorizontally = true;

    //Enable to apply force on the carrier (overrides the above)
    bool canShakeCarrier = false;
    int climbingSwitch = 0;
    int slippingSwitch = 0;

    float directionToFace = 1;

    public bool lastRopeSegment = false;

    private float rawHorizontalAxis;
    private Vector2 movement;
    private GameObject parentRopeSegment;

    private bool isOffsetting;
    private bool isRotating;

    private void Start()
    {
        //Initialization;
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerControlor = GetComponent<PlayerControlor>();
    }

    //Movement is done and changed on FixedUpdate
    //Update only is used to detect single button press (Jump button) as FixedUpdate may miss it
    private void Update()
    {
        //All Input values are static variables from Controls class;
        if (Controls.jump)
        {
            stopCoroutine();
            transform.parent.gameObject.GetComponent<Rope>().JumpOff(rawHorizontalAxis, gameObject);
        }
    }

    public void stopCoroutine()
    {
        StopAllCoroutines();
        isOffsetting = false;
        isRotating = false;
    }

    //Coroutine that moves the player from the position where it collides with the rope to actually being with his hands on the rope
    public IEnumerator SetOffset(float xOffset, float duration)
    {
        isOffsetting = true;
        float init = transform.localPosition.x;
        float end = xOffset;
        for (float t = 0.0f; t < duration; t += 0.01f)
        {
            transform.localPosition = new Vector3(Mathf.Lerp(transform.localPosition.x, end, t / duration), transform.localPosition.y, transform.localPosition.z);
            yield return new WaitForSeconds(0.01f);
        }
        transform.localPosition = new Vector3(xOffset, transform.localPosition.y, transform.localPosition.z);
        isOffsetting = false;
    }

    //If rope is rotated or while moving through differently rotated rope segments, smoothly rotate the character
    public IEnumerator SetRotation(bool right, float duration)
    {
        isRotating = true;

        //End is 0 or 180 due to character being a child of the rope segment it's attached to
        float end = 0;
        if (!right)
            end = 180;
        for (float t = 0.0f; t < duration; t += 0.01f)
        {
            transform.localEulerAngles = new Vector3(Mathf.LerpAngle(transform.localEulerAngles.x, 0, t / duration), end,
                Mathf.LerpAngle(transform.localEulerAngles.z, 0, t / duration));
            yield return new WaitForSeconds(0.01f);
        }
        transform.localEulerAngles = new Vector3(0, end, 0);
        isRotating = false;
    }

    private void FixedUpdate()
    {
        //OLD VERSION: Snap to position and rotation of the rope segment.
        //Visible snapping and rotation changes; Changed for a smooth lerp coroutine approach
        /*if (GetComponent<PlayerControlor>().right)
            transform.localEulerAngles = new Vector3(0, 0, 0);
        else if (GetComponent<PlayerControlor>().right)
            transform.localEulerAngles = new Vector3(0, 180, 0);*/

        /*Vector3 p = transform.localPosition;
        p.x = xOffset;
        transform.localPosition = p;*/

        //Smoothly adapting position and rotation when character collider hits a segment rope collider
        if(!isOffsetting)
            StartCoroutine(SetOffset(xOffset, 0.2f));

        if (!isRotating)
            StartCoroutine(SetRotation(GetComponent<PlayerControlor>().right, 0.7f));


        //Raw and Smooth Inputs;
        var rawVerticalAxis = Input.GetAxisRaw("Vertical");
        var smoothVerticalAxis = Input.GetAxis("Vertical");

        var smoothHorizontalAxis = Controls.horizontal;

        //While swinging on the rope, do not allow the player go move vertically;
        if (smoothHorizontalAxis != 0)
        {
            rawVerticalAxis = 0;
            smoothVerticalAxis = 0;
        }

        //VERTICAL MOVEMENT
        //Setting switches and changing animation accordingly via Animator
        if (smoothVerticalAxis > 0.1)
        {
            //Climbing up
            if (!lastRopeSegment)
            {
                animator.SetBool("climbup", true);
                animator.SetBool("climbidle", false);
            }
            else
            {
                animator.SetBool("climbup", false);
                animator.SetBool("climbidle", true);
            }
            climbingSwitch = 1;
            slippingSwitch = 0;
        }
        else if (smoothVerticalAxis < -0.1)
        {
            //Sliding down
            climbingSwitch = 0;
            slippingSwitch = 1;
        }
        else
        {
            //Hanging Still
            animator.SetBool("climbup", false);
            animator.SetBool("climbidle", true);
            climbingSwitch = 0;
            slippingSwitch = 0;
        }

        //Move Vertically
        if (rawVerticalAxis != 0 || (smoothVerticalAxis < 0 && slipsDownABit) || (smoothVerticalAxis > 0 && slipsUpABit))
        {
          //Don't allow the character to go any higher than a certain rope segment (configurable per Rope in Rope Class)
          if (!lastRopeSegment || (lastRopeSegment && rawVerticalAxis < 0))
              transform.Translate(Vector3.up * Mathf.Pow(climbingSpeed * smoothVerticalAxis, climbingSwitch) * Mathf.Pow(slippingSpeed * smoothVerticalAxis, slippingSwitch) * Time.deltaTime);
        }


        //VERTICAL MOVEMENT
        if (smoothHorizontalAxis != 0 && canMoveHorizontally)
        {
            //Add force to the segment to which the character is attached;
            parentRopeSegment = transform.parent.gameObject;
            parentRopeSegment.GetComponent<Rigidbody2D>().AddRelativeForce(Vector3.right * horizontalSpeed * smoothHorizontalAxis);

            //All segments are numbered 0 - MaxSegmentNo
            int result = int.Parse(parentRopeSegment.name);
            GameObject parentRope = transform.parent.parentRopeSegment;
            string ropeparent = parentRope.name;
            RopeCreator ropeCreator = parentRope.GetComponent<RopeCreator>();

            Rope ropeSegment = parentRopeSegment.GetComponent<Rope>();

            //Double Checking that Rope was created with a RopeCreator
            //Add a smaller force to both closest segments to better simulate a continous object being pushed
            //Some segments may be deactivated through RopeCreator through ropeGrappleFactor - See RopeCreator.cs
            if (ropeCreator != null)
            {
                if (ropeSegment.segmentNumber != 1)
                    GameObject.Find(ropeparent + "/" + (result - ropeCreator.ropeGrappleFactor).ToString()).GetComponent<Rigidbody2D>().AddRelativeForce(Vector3.right * horizontalSpeed / 2 * smoothHorizontalAxis);
                if (!ropeSegment.lastRopeSegment)
                    GameObject.Find(ropeparent + "/" + (result + ropeCreator.ropeGrappleFactor).ToString()).GetComponent<Rigidbody2D>().AddRelativeForce(Vector3.right * horizontalSpeed / 2 * smoothHorizontalAxis);
            }
        }

        //Add Swinging Animations via Animator
        if((smoothHorizontalAxis > 0 && playerControlor.right) ||
          (smoothHorizontalAxis < 0 && !playerControlor.right))
        {
            animator.SetInteger("swing", 1);
        }

        else if ((smoothHorizontalAxis < 0 && playerControlor.right) ||
          (smoothHorizontalAxis > 0 && !playerControlor.right))
        {
            animator.SetInteger("swing", 0);
        }

        else{
            animator.SetInteger("swing", 0);
        }

        //Shake
        if (canShakeCarrier)
            parentRopeSegment.GetComponent<Rigidbody2D>().AddRelativeForce(Vector3.right * smoothHorizontalAxis * 10);

        if (rawHorizontalAxis != 0)
            directionToFace = rawHorizontalAxis;


    }

    //Functions used by ControlManager.cs, that deals with Rope Segment Entering, Exiting and Jumping
    //Adds and removes segment to know which segment we are on, and to confirm that we are exiting and entering as we should
    public void RegisterSegment(GameObject segment)
    {
        segmentHashtable.Add(segment.name, segment.gameObject);
    }

    public void UnregisterSegment(GameObject segment)
    {
        segmentHashtable.Remove(segment.name);
    }


    public void SetXOffest(float offset)
    {
        xOffset = offset;
    }

    public void SetUpController(float newClimbingSpeed, float newSlippingSpeed, float newHorizontalSpeed, bool slipUp , bool slipDown, bool moveHorizontally,bool shake)
    {
        //if a passed value is 0, then ignore it
        if (newClimbingSpeed != 0.0)
            climbingSpeed = newClimbingSpeed;
        if (newSlippingSpeed != 0.0)
            slippingSpeed = newSlippingSpeed;
        if (newHorizontalSpeed != 0.0)
            horizontalSpeed = newHorizontalSpeed;
        slipsDownABit = slipUp;
        slipsUpABit = slipDown;
        canMoveHorizontally = moveHorizontally;
        canShakeCarrier = shake;

        //override canMoveHorizontally if needed
        if (canShakeCarrier)
            canMoveHorizontally = false;
    }


}
