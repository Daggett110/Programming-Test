/* Third try at making a player. This one is going to scoot
 * around instead of hopping. Hopping would look great, but
 * we don't have time. Besides, now it's much simpler to
 * transition to a roll/ball form.
 * */

using System.Collections;
using UnityEngine;

public class HeisenballCharacterController : MonoBehaviour {

    [Header("Player Movement")]
    public float movementSpeed = 5f;        // The speed at which our character should move in capsule form
    public float ballMovementSpeed = 1f;    // The force we should apply to our ball to make it roll
    public float returnToCapsuleSpeed = 2f; // How long it should take us to return to the upright Rotation before playing the capsule animation
    
    Rigidbody playerRB;                     // Reference to the players Rigidbody
    Vector3 movement;                       // Making this variable a part of the controller scope allows us to save from making new Vector3 every frame evading garbage collection
    Transform cam;                          // Reference to camera so movement is always relative to camera position
    Vector3 camForward;                     // Used in scaling the movement vector to allow our player to go in the proper direction
    Animator anim;                          // Reference to the Animator. Using super simple animations to make the player feel better
    bool isBall = false;                    // Flag if we are currently in ball form
    bool returnedToCapsule = true;          // Flag to know when to give capsule controls back to player and animate the growth

    [Header("Jumping")]
    public float jumpForce = 1f;            // The upwards force we apply to our character when jumping
    public bool jumpInProgress = true;      // Flag if we currently doing a jump
    public float distToGround = 0.1f;       // The distance from the our ground-checking raycast will travel to look for ground

    // Input Values. These are updated by Unity in Update(), but we want to run physics 
    // in FixedUpdate(), so we'll grab the input we need every frame and pass it to FixedUpdate()
    bool jumpQueued = false;

    public bool tempBool;

    // Set up reference at the beginning of the game
    private void Start ()
    {
        // Establish and necessary references
        playerRB = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        cam = Camera.main.transform;
	}

    // Gets jump input
    private void Update()
    {
        // Unfortunately, input events are updated in Update() in Unity. So if we want to get GetButtonDown() or
        // GetButtonUp(). We are only tracking the jumpin this manner because if we used GetButton, we would
        // simply miss jump inputs, if we simply used GetButtonDown(), we . We would be doing everything in Update, 
        // but the physics system operates in FixedUpdate(). We phrase it this way because Update typically runs
        // faster than 
        if (!jumpQueued && !jumpInProgress)
        {
            jumpQueued = Input.GetButtonDown("Jump");
        }
    }

    // Runs the controls for the character
    private void FixedUpdate()
    {
        // First, lets deal with any transistions we need to do
        // If we are entering or exiting the ball form ..
        if (Input.GetButton("Ball Form") && !isBall)
        {
            BecomeBall();
        }
        // .. otherwise, if we have met all the criteria for turning into a capsule
        else if (!Input.GetButton("Ball Form") && isBall && EnoughHeadroom())
        {
            StartCoroutine(BecomeCapsule());
        }

        GetMovement();
        
        // If we are in Capsule Form ..
        if (!isBall && returnedToCapsule)
        {
            HandleCapsuleMovement();
            HandleJumping();
        }
        else // if we are currently in ball form
        {
            playerRB.AddForce(movement * ballMovementSpeed);
        }

        // We want to reset jump input at the end of each fixed update frame, to make sure we only jump when we need too
        jumpQueued = false;


    }

    // Turns input into movement direction and handles movement animation
    private void GetMovement()
    {
        // Get Input from Left stick. We can do
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Scale the movement vector to match the direction of our camera
        camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        movement = (vertical * camForward + horizontal * cam.right);

        // Set the animation param to match the movement (we do this in FixedUpdate() instead of Update() because the animator is set to match physics
        anim.SetBool("Moving", movement != Vector3.zero);
        anim.SetFloat("MovementSpeed", movement.magnitude);
    }

    // Returns if the ceiling is high enough for us to return to a capsule form
    private bool EnoughHeadroom()
    {
        return !Physics.Raycast(transform.position, Vector3.up, 1.5f);
    }

    // Handles starting a jump and applying the force
    private void HandleJumping()
    {
        // If we are on the ground, and are attempting to jump ..
        if (IsGrounded() && jumpQueued && !jumpInProgress)
        {
            // .. Jump!
            anim.SetTrigger("Jump");
            jumpInProgress = true;
        }
        // .. otherwise, if we are at the end of the jump animation ..
        else if (jumpInProgress && anim.GetCurrentAnimatorStateInfo(0).IsName("TransitionToHang"))
        {
            // Apply upwards force to the player when it appears we should in the animation
            playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            // Flag that we are not waiting for a jump force anymore
            jumpInProgress = false;
        }
    }

    // Returns whether we are on the ground or still falling
    private bool IsGrounded()
    {
        // First, Raycast Downward to see if we are on the ground
        bool grounded = Physics.Raycast(transform.position, -Vector3.up, distToGround);
        // let our animator know whether we are on the ground or not
        anim.SetBool("Grounded", grounded);

        return grounded;
    }
    
    private void HandleCapsuleMovement()
    {
        // Move the character controller to the new position based on our movement speed
        playerRB.MovePosition(playerRB.position + movement * Time.fixedDeltaTime * movementSpeed);

        // Check so that the player doesn't rotate to origin when there is no input
        if (movement != Vector3.zero)
        {
            // Set player direction to movement direction
            playerRB.MoveRotation(Quaternion.LookRotation(movement));
        }
    }

    private void BecomeBall()
    {
        // .. Start the animation and free our rotation axes
        if(!anim.GetCurrentAnimatorStateInfo(0).IsName("EnterBallForm") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("BallForm") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("ExitBallForm"))
        anim.SetTrigger("Start Ball");
        playerRB.constraints = RigidbodyConstraints.None;
        isBall = true;
    }

    private IEnumerator BecomeCapsule()
    {
        // Set our flag so we know we are no longer in ball form
        isBall = false;

        // Constrain the axes so the player doesn't fall over
        playerRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Flag that we are not yet in capsule form
        returnedToCapsule = false;

        // If we are not back to an upright rotation ..
        if (playerRB.rotation != Quaternion.identity)
        {

            // Note the rotation we started at when exiting ball form
            Quaternion startingRot = playerRB.rotation;

            // Set up the params measuring return progress
            float startTime = Time.time;
            float journeyLength = Quaternion.Angle(startingRot, Quaternion.identity);

            // Until we are at the proper capsule rotation ..
            while (((Time.time - startTime) * returnToCapsuleSpeed) / journeyLength <= 1)
            {
                // Slerp us to it based on the speed
                playerRB.rotation = Quaternion.Slerp(
                    startingRot,
                    Quaternion.identity,
                    ((Time.time - startTime) * returnToCapsuleSpeed) / journeyLength);
                // .. and proceed forward a frame
                yield return null;
            }

            // When finished, Ensure our player is back at the desired rotation
            playerRB.rotation = Quaternion.identity;
        }
        
        // resume player controls
        returnedToCapsule = true;

        // Set the animator to return to the capsule
        // if statement stops issues related to releasing the Ball form button at different times
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("EnterBall") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("BallForm") || (anim.IsInTransition(0) && anim.GetAnimatorTransitionInfo(0).anyState))
        {
            anim.SetTrigger("Exit Ball");
        }
    }
}
