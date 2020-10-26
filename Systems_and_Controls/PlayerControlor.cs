using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControlor : MonoBehaviour
{
    // Public Variables;
    public float speed;
    public float jumpForce;
    public float runSpeedFactor;

    public Transform feetPos;
    public float checkRadius;
    public LayerMask whatIsGround;
    public LayerMask whatIsCollidable;

    public float gravity;
    public float toDialogueSpeed;

    private bool jumpPrepare = false;
    public bool isFalling = false;
    public bool onAir = false;
    public float timeToFallAnimation = 3f;

    private Rigidbody2D rb2d;
    private Collision2D c2d;
    private Animator animator;

    Vector3 current = new Vector3(0, 1, 0);

    private float runningFactor;

    public bool grounded;

    private float surfaceAngle;
    private float moveHorizontal;

    public bool right;

    public bool jumpOffRope = false;

    public bool colliding;
    public bool collidingAll;

    public Vector3 centerToFeet;
    public float yVarToOriginal = 0;

    public float framesBetweenLandWalk = 7;

    private bool fallToWalkTransition;
    private int fallTransitionFrames;
    private bool forwardJump;

    public bool sceneHasRope;

    private void Start()
    {
        //Initialization;
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        grounded = false;
        jumpPrepare = false;
        isFalling = false;
        right = true;

        previousPos = transform.position.x;
    }

    //Simple collider that is true when in contact with any collider (maybe not needed anymore, but used as safety net)
    void OnCollisionStay2D(Collision2D other)
    {
        collidingAll = true;
    }

    //Is player colliding with something 'vertical'
    void isColliding()
    {
        // Raycast is sent to the direction in which the player is facing.
        Physics2D.queriesHitTriggers = false;
        Vector3 direction = Vector3.zero;
        if(right)
            direction = new Vector3(1, 0, 0);
        else
            direction = new Vector3(-1, 0, 0);

        //Two raycasts are sent, from the feet and from the center of the player
        RaycastHit2D[] allCollidersFeet = Physics2D.RaycastAll(new Vector3(transform.position.x, transform.position.y - 2.5f, 0), direction, 0.4f, whatIsCollidable);
        RaycastHit2D[] allCollidersCenter = Physics2D.RaycastAll(transform.position, direction, 0.4f, whatIsCollidable);

        bool isCollidingFeet = false;
        bool isCollidingCenter = false;

        //If the angle between the normal and the direction of raycast is larger than 140º, we claim it to be a vertical wall
        for (int i = 0; i < allCollidersFeet.Length; i++)
        {
            if (allCollidersFeet[i].collider.name != gameObject.name
             && allCollidersFeet[i].collider.name != "Stairs")
            {
                if(Vector2.Angle(allCollidersFeet[i].normal, direction) > 140)
                    isCollidingFeet = true;
            }
        }

        for (int i = 0; i < allCollidersCenter.Length; i++)
        {
            if (allCollidersCenter[i].collider.name != gameObject.name
            && allCollidersCenter[i].collider.name != "Stairs")
            {
                if(Vector2.Angle(allCollidersCenter[i].normal, direction) > 140)
                    isCollidingCenter = true;
            }
        }

        //If either the feet or chest areas are in contact with a wall, than it is colliding
        if (isCollidingFeet || isCollidingCenter)
            colliding = true;
        else
            colliding = false;
        Physics2D.queriesHitTriggers = true;
    }

    int k = 0;

    //See if player's hands will hit rope to smooth animation between jump and grab rope
    public void findRope()
    {
        // Raycast is sent to the direction in which the player is facing.
        Vector3 direction = rb2d.velocity;

        //Two raycasts are sent from the lower arm and the hand
        RaycastHit2D[] allCollidersRope = Physics2D.RaycastAll(new Vector3(transform.position.x, transform.position.y+0.65f, 0), direction, 5.5f, whatIsCollidable);
        RaycastHit2D[] allCollidersRope2 = Physics2D.RaycastAll(new Vector3(transform.position.x, transform.position.y + 0.3f, 0), direction, 5.5f, whatIsCollidable);

        bool hasRope = false;

        //If running and at a certain distance X or if walking and at a distance Y, then initiate PreClimb animation
        //Otherwise do not change the animation as the player will not collide with the rope
        for (int i = 0; i < allCollidersRope.Length; i++)
        {
            if (allCollidersRope[i].collider.name != gameObject.name)
            {
                if (allCollidersRope[i].collider.tag == "Rope")
                {
                    hasRope = true;
                    k = 0;
                    if (((right && rb2d.velocity.x > speed * 1.8f) || (!right && rb2d.velocity.x < -speed * 1.8f)) && allCollidersRope[i].distance < 5f)
                        animator.SetBool("preclimb", true);

                    else if (((right && rb2d.velocity.x > 0) || (!right && rb2d.velocity.x < 0)) && allCollidersRope[i].distance < 3f)
                        animator.SetBool("preclimb", true);

                    else
                        animator.SetBool("preclimb", false);
                }
            }
        }

      // Same as above for second raycast
        for (int i = 0; i < allCollidersRope2.Length; i++)
        {
            if (allCollidersRope2[i].collider.name != gameObject.name)
            {
                if (allCollidersRope2[i].collider.tag == "Rope")
                {
                    hasRope = true;
                    k = 0;
                    if (((right && rb2d.velocity.x > speed*1.8f) || (!right && rb2d.velocity.x < -speed*1.8f)) && allCollidersRope2[i].distance < 5f)
                        animator.SetBool("preclimb", true);
                    else if (((right && rb2d.velocity.x > 0) || (!right && rb2d.velocity.x < 0)) && allCollidersRope2[i].distance < 3f)
                        animator.SetBool("preclimb", true);
                    else
                    {
                        animator.SetBool("preclimb", false);
                    }
                }
            }
        }

        //Since the player has freedom of movement, it is possible to believe that he is hitting the rope and then doesn't
        //For that, if the player has seen two frames of not hitting a rope, then revert back the animation to fall
        if(!hasRope)
        {
            k++;
            if (k == 2)
            {
                animator.SetBool("preclimb", false);
                k = 0;
            }
        }

    }

    //Player Move To Certain Place (Generally for actions, dialogues and observations)
    public IEnumerator moveTo(Vector3 targetPosition, GameObject gameObject)
    {
        rb2d.velocity = new Vector2(rb2d.velocity.x/2, rb2d.velocity.y / 2);
        //Rotates player if needed according to him and the target positions
        if (transform.position.x - targetPosition.x >0)
        {
            if (right)
            {
                transform.Rotate(new Vector3(0, 180, 0));
                right = !right;
            }

        }
        else if (transform.position.x - targetPosition.x <0)
        {
            if (!right)
            {
                transform.Rotate(new Vector3(0, 180, 0));
                right = !right;
            }

        }

        //Reset possible jump.
        animator.SetBool("jumpbool", false);
        animator.SetBool("fall", false);
        jumpPrepare = false;
        isFalling = false;

        //Disables the controlor, as from this point on, the commands are only reactivated at the end of the dialogue
        GetComponent<PlayerControlor>().enabled = false;
        targetPosition.y += yVarToOriginal;

        //MovesTowards the target position while walking until a certain distance
        while (Mathf.Abs(transform.position.x - targetPosition.x) > 0.1)
        {
            animator.SetInteger("speed", 1);
            animator.SetBool("jumpbool", false);
            animator.SetBool("fall", false);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, toDialogueSpeed * Time.deltaTime);
            yield return null;
        }

        //When arriving to place go back to idle animation (will be TALK animation) and rotate again if needed
        animator.SetInteger("speed", 0);


        if (gameObject.transform.position.x > this.transform.position.x)
        {
            transform.localRotation = new Quaternion(0, 0, 0, 0);
            right = true;
        }

        else
        {
            transform.localRotation = new Quaternion(0, 180, 0, 0);
            right = false;
        }

    }

    private float speedBeforeJump = -2;

    //Simple Jump function
    public void Jump(float jumpFactor)
    {
        if (grounded)
        {
            rb2d.velocity = new Vector2(moveHorizontal * 1.4f, jumpForce*jumpFactor);
            speedBeforeJump = Controls.horizontal;
        }
    }

    //Jump function (to be called by the Animator)
    public void JumpAnimationEvent(float jumpFactor)
    {
        if (grounded)
        {
            if (moveHorizontal != 0)
            {
                jumpFactor = 0.9f;
            }

            rb2d.velocity = new Vector2(moveHorizontal * 1.4f, jumpForce * jumpFactor);
            speedBeforeJump = Controls.horizontal;
            if (moveHorizontal != 0)
            {
                forwardJump = true;
                runningFactor = 1.5f;
            }
        }

    }

    //Reset after fall (cancel Jump Prepare (possibly redundant) and isFalling (Which will cancel the fall animation)
    public void endFall()
    {
        fallToWalkTransition = false;
        isFalling = false;
        jumpPrepare = false;
        speedBeforeJump = 2;
    }

    private bool canEndFallBool = false;

    public void canEndFall()
    {
        canEndFallBool = true;
        //runningFactor = 1;
    }

    float previousPos;
    float nextPos;
    Vector3 ray;

    //Most movement is controlled by FixedUpdate
    private void FixedUpdate()
    {
        nextPos = transform.localPosition.x;

        if (sceneHasRope)
        {
            if (!grounded && !animator.GetBool("climb"))
                findRope();
        }

        //Horizontal Movement. Now is working for every circumstance where player is not in a rope.
        //Change jumpOffRope to grounded to make it work only when grounded (that is, on air speed does not depend on actions during on air)
        if (!jumpOffRope)
        {
            if (!forwardJump)
            {
                //If running, there is a quick acceleration (very simple) until reaching max speed
                if (Controls.run)
                {
                    if (runningFactor < runSpeedFactor)
                        runningFactor *= 1.13f;

                    if (runningFactor > runSpeedFactor)
                        runningFactor = runSpeedFactor;
                }
                else
                    runningFactor = 1;
            }


            //Using built-in RigidBody moving mechanics.
            //Inputs relate keyboard arrows to translation in said axis;
            //Moving mechanics depend on vertical collisions or not
            /*if (collidingAll && !grounded)
                    moveHorizontal = 0;
            else
                moveHorizontal = Controls.horizontal * speed * runningFactor;*/

            if(collidingAll)
            {
                //If colliding with something while on air, do not move
                if (!grounded)
                    moveHorizontal = 0;

                //Doesn't allow walking towards a wall when hitting the wall
                else if(colliding && ((right && Controls.horizontal > 0) || (!right && Controls.horizontal < 0)))
                {
                    animator.SetBool("waitfall", false);
                    moveHorizontal = 0;
                }

                //ACTUAL HORIZONTAL MOVEMENT.
                //If it is colliding with something, is grounded and not colliding with a vertical wall, then move
                else //if(colliding && !animator.GetBool("jumpbool"))
                    moveHorizontal = Controls.horizontal * speed * runningFactor;
            }
            //If it's not colliding with anything, also move (remove this to disable having control of the character while on air)
            else
                moveHorizontal = Controls.horizontal * speed * runningFactor;


            //Checks if the speed during jump is equal to the speed before jump. If the player releases the side key, he loses the momentum of speed
            if (Controls.horizontal == speedBeforeJump)
                rb2d.velocity = new Vector2(moveHorizontal * 1.0f, rb2d.velocity.y);

            else
            {
                rb2d.velocity = new Vector2(moveHorizontal, rb2d.velocity.y);
                speedBeforeJump = -2;
            }

            //Flip Character
            if (moveHorizontal < -0.1 && right)
            {
                Quaternion originalRot = transform.rotation;
                transform.rotation = originalRot * Quaternion.Euler(0, 180, 0);
                right = false;
            }

            else if (moveHorizontal > 0.1 && !right)
            {
                right = true;
                Quaternion originalRot = transform.rotation;
                transform.rotation = originalRot * Quaternion.Euler(0, 180, 0);
            }

            //Set Animation:
            //Speed = 2: Run Animation
            //Speed = 1: Walk Animation
            //Speed = 0: Idle Animation

            if (moveHorizontal != 0 && runningFactor > 1 && Mathf.Abs(nextPos-previousPos) > 0.005 && !forwardJump)
                animator.SetInteger("speed", 2);
            else if ((moveHorizontal != 0 && runningFactor == 1 && Mathf.Abs(nextPos - previousPos) > 0) || forwardJump)
                animator.SetInteger("speed", 1);
            else
                animator.SetInteger("speed", 0);

        }
        previousPos = nextPos;

        //Counts number of frames (in FixedUpdate) after someone got grounded after a fall to transition to walk/run
        if (fallToWalkTransition)
            fallTransitionFrames++;
    }

    //Update controls mostly collisions (RayCasts) and single button induced actions, such as Jumping
    private void Update()
    {
        isColliding();

        if (animator.GetBool("fall"))
            animator.SetBool("fall2", true);
        else
            animator.SetBool("fall2", false);

        //Vertical Movement
        grounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, whatIsGround);

        if (!animator.GetBool("jumpbool") && !animator.GetBool("waitfall"))
            forwardJump = false;

        if (grounded)
        {
            animator.SetBool("waitfall", false);
            animator.SetBool("grounded", true);
            animator.SetBool("offRope", false);
            //Every time the player is grounded (except when preparing a jump), jump animation is off
            if (!jumpPrepare)
            {
                jumpOffRope = false;
                animator.SetBool("jumpbool", false);
            }

            //JUMP
            //When grounded and space is hit, cancel Fall animation, start jump animation and prepare jump
            //if (Input.GetKeyDown(KeyCode.Space) && !GetComponent<ControlManager>().onLedge) //When On Ledge, can't use Space for jump
            if (Controls.jump && !GetComponent<ControlManager>().onLedge && !animator.GetBool("jumpbool")) //When On Ledge, can't use Space for jump
            {
                isFalling = false;
                jumpPrepare = true;
                animator.SetBool("jumpbool", true);

                //Jump when still is done through animation event (as there's a pre-jump, so a tiny delay between key and jump)
                //When walking or running, jump is immediate for best responsiveness
                if (animator.GetInteger("speed") > 0)
                {
                    if (Controls.run)// && runningFactor > 1)
                        Jump(1); //Jump while running is done with maximum jump height and same horizontal speed
                    else
                    {
                        Jump(0.7f); //Jump while walking is done with 3/4 of jump height
                        forwardJump = true;
                        runningFactor = 1.75f; //Part of the momentum is transferred to horizontal speed while jumping
                    }
                }
            }
        }

        //When is falling, use a raycast that predicts the time it'll take for the player to hit ground
        if (isFalling)
        {
            //Use the velocity as the direction vector for the raycast
            Physics2D.queriesHitTriggers = false;
            ray = rb2d.velocity;

            //Determine angle of velocity and send raycast
            float theta = Mathf.Atan(ray.y / ray.x);

            RaycastHit2D hit = Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y - 2.5f, 0), ray, 100, whatIsGround);

            //Predict time to hit (through y = y0 + v0t + 1/2gt^2)
            float timeToHit = (ray.y + Mathf.Sqrt(ray.y * ray.y + 2 * Mathf.Abs(Mathf.Sin(theta)) * hit.distance * gravity * -Physics2D.gravity.y)) / (gravity * -Physics2D.gravity.y);

            //If time to hit is inferior to a certain public number, cancel idle fall animation to initiate fall animation
            if (hit != null && !grounded)
            {
                if (ray.y != 0 && hit.distance != 0)
                {

                    //if (Mathf.Sqrt(Mathf.Abs(Mathf.Sin(theta)) * hit.distance) < timeToFallAnimation && hit.distance != 0)
                    if (timeToHit < timeToFallAnimation && hit.distance != 0)
                        animator.SetBool("waitfall", false);
                    else
                        animator.SetBool("waitfall", true);

                    animator.SetBool("fall", true);
                }
            }

            /*else if (ray != Vector3.zero)
                animator.SetBool("waitfall", true);*/

            Physics2D.queriesHitTriggers = true;
        }

        //If not grounded, player is not in a jump prepare
        else if (!grounded)
        {
            jumpPrepare = false;
            animator.SetBool("grounded", false);
        }


        //Transition from Fall to Ground. This transition may occur when, while Falling, he is grounded and walking/running
        if (grounded && isFalling && Mathf.Abs(rb2d.velocity.x) > 0.1f) //&& canEndFallBool)
        {
            //When first hits the ground fallToWalkTransition becomes true, to give some extra frames between land and transition
            if (!fallToWalkTransition)
            {
                fallTransitionFrames = 0;
                fallToWalkTransition = true;
                GetComponent<ControlManager>().footsteps(0.75f);
            }

            if (fallToWalkTransition)
            {
                if (fallTransitionFrames == framesBetweenLandWalk - 2)
                    runningFactor = 1;
                //If fallToWalkTransition is true, frames are counted in FixedUpdate, and if over a certain number, transition is activated
                else if (fallTransitionFrames > framesBetweenLandWalk)
                {
                    forwardJump = false;
                    isFalling = false;
                    fallToWalkTransition = false; //fallToWalkTransition is also set to false on endJump() if it happens first;
                }
            }
        }

            //He is onAir if he is not grounded, or during the grounded parts of animation, such as JumpPrepare and IsFalling
        if (!grounded || jumpPrepare || isFalling)
            onAir = true;
        else
            onAir = false;


        if (rb2d.velocity.y < -0.2 && !grounded)
        {
            //If not yet falling, start falling animation (cancel jump prepare)
            if (!isFalling)
            {
                isFalling = true;
                //animator.SetBool("waitfall", true);
                //animator.SetBool("fall", true);
                jumpPrepare = false;
            }
        }

        //If grounded or with neutral to positive velocity cancel falling.
        else
        {
            if (!isFalling)
            {
                canEndFallBool = false;
                animator.SetBool("fall", false);
            }
        }
    }


    void OnCollisionExit2D(Collision2D other)
    {
        collidingAll = false;
    }
}
