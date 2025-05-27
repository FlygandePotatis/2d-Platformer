using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Input = UnityEngine.Input;



[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(StaminaBar))]



public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeedX = 0.325f;
    bool fastXMovementFixedUpdate = false;
    float moveSpeedXScalar = 1f;
    [SerializeField] float runningMoveSpeedX = 1.4f;
    float velocityXSmoothing;
    [SerializeField] float accelerationTimeAirborn = 0.1f;
    [SerializeField] float accelerationTimeGrounded = 0.05f;



    [SerializeField] float jumpingHeight = 4f;
    [SerializeField] float lenghtOfJump = 8f;
    bool hasJumped = false;
    float jumpBufferTimeMidAir = 1000;
    [SerializeField] float midAirJumpBufferTime = 0.1f;
    float jumpBufferTimeFromGround = 1000;
    [SerializeField] float groundJumpBufferTime = 0.08f;
    bool hasJumpedAndNotLanded = false;
    [SerializeField] float velocityWhenReleasingSpaceScalar = 0.62f;
    bool velocityWhenReleasingSpaceScalarBool = false;
    bool FixedUpdateJump = false;



    bool wallSliding;
    public float wallSlideSpeedMax = 0.025f;
    public float wallStickTime = 0.25f;
    float timeToWallUnStick;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    int wallDirectionX = 1;

    bool FixedWallSlidingJump = false;



    [SerializeField] float floatyJumpGravity = 0.45f;
    [SerializeField] float sJumpGravity = 1.6f;
    [SerializeField] float normalJumpGravity = 0.83f;
    [SerializeField] float releaseSpaceJumpGravity = 0.61f;
    [SerializeField] float wallLeapingJumpGravity = 0.60f;
    float gravityConstant = 1f;
    bool sFalling = false;
    float gravity;



    [SerializeField] float maxFallSpeedDownwards = 0.10f;
    [SerializeField] float peakOfJumpVelocityMax = 0.05f;
    [SerializeField] float peakOfJumpVelocityConstant = 0.8f;
    float peakGravityConstant = 1f;



    bool slidingDownSlopeSidewaysJump = false;



    Vector2 input;



    Vector3 velocity;



    Controller2D controller;
    StaminaBar staminaBar;



    bool makeASmallJump = false;



    bool is_jumping_currently = false;



    bool canMakeADoubleJump = false;
    bool fixedDoubleJump = false;
    [SerializeField] float doubleJumpVelocityConstant = 0.8f;
    [SerializeField] float doubleJumpStaminaDrain = 18f;



    public SpriteRenderer spriteOfPlayer;
    float directionOfPlayerInput = 0;



    private Animator animator;



    public enum PlayerAnimationState {noTransitionPlayerState, idle, running, jumping, falling, transitionBetweenJumpingAndFalling, wallSlide, meeleAttack, slidingDownSteepSlope};



    PlayerAnimationState playerState;



    PlayerAnimationState oldPlayerState = PlayerAnimationState.falling;



    bool hitStopActivated = false;



    public Transform meeleAttackPointLeft;
    public Transform meeleAttackPointRight;
    public float meeleAttackRange = 0.5f;
    public float meleeAttackDamage = 20f;
    public float meeleAttackHitStopTime = 0.5f;
    public float meeleAttackHitStopTimeUponDeath = 2f;
    
    public float timeBetweenMeleeAttacks = 2f;
    float nextMeleeAttackTime = 0f;

    float meleeAttackBufferTimePassed;
    public float meleeAttackBufferTime = 0.1f;

    public LayerMask enemyLayers;
    Collider2D[] meeleHitEnemies;



    bool enemyUponDeath = false;



    float jumpingBufferTimePassed = 0f;



    void Start()
    {
        controller = GetComponent<Controller2D>();
        staminaBar = GetComponent<StaminaBar>();



        animator = GetComponent<Animator>();



        playerState = PlayerAnimationState.idle;



        meleeAttackBufferTimePassed = meleeAttackBufferTime;
    }



    private void FixedUpdate()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        wallDirectionX = (controller.collisions.left) ? -1 : 1;



        float targetVelocityX = input.x * moveSpeedX * moveSpeedXScalar;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborn);



        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0 && controller.collisions.youCanClimb)
        {
            wallSliding = true;


            
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }



            if (timeToWallUnStick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;


                if(input.x != wallDirectionX && input.x != 0)
                {
                    timeToWallUnStick -= 0.02f;
                }
                else
                {
                    timeToWallUnStick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnStick = wallStickTime;
            }
        }



        if (wallSliding || controller.collisions.below)
        {
            is_jumping_currently = false;
        }

        if (is_jumping_currently && Mathf.Abs(velocity.y) < peakOfJumpVelocityMax)
        {
            peakGravityConstant = peakOfJumpVelocityConstant;
        }
        else
        {
            peakGravityConstant = 1f;
        }



        gravity = peakGravityConstant * gravityConstant * (-8) * jumpingHeight * Mathf.Pow(moveSpeedX, 2) / Mathf.Pow(lenghtOfJump, 2);



        if (fastXMovementFixedUpdate)
        {
            moveSpeedXScalar = runningMoveSpeedX;
        }
        else
        {
            moveSpeedXScalar = 1f;
        }



        velocity.y += gravity;



        Jump();



        FallingVelocityScalar();



        if (velocity.y < 0)
        {
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeedDownwards);
        }



        controller.Move(velocity, input);



        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity;
            }
            else
            {
                velocity.y = 0f;
            }
        }
    }



    private void Update()
    {
        directionOfPlayerInput = Input.GetAxisRaw("Horizontal");



        if (directionOfPlayerInput > 0)
        {
            spriteOfPlayer.flipX = false;
        }
        if (directionOfPlayerInput < 0)
        {
            spriteOfPlayer.flipX = true;
        }



        PlayerCombatFunction();



        JumpingFunction();



        FastXMovemvent();



        PlayerAnimationStateAndUpdate();
    }



    private void Jump()
    {
        if (FixedUpdateJump)
        {
            velocity.y = 4 * jumpingHeight * moveSpeedX / lenghtOfJump;
            gravityConstant = floatyJumpGravity;
            hasJumped = true;
            hasJumpedAndNotLanded = true;
            FixedUpdateJump = false;



            if (!Input.GetKey(KeyCode.Space))
            {
                makeASmallJump = true;
            }



            is_jumping_currently = true;
        }



        if (slidingDownSlopeSidewaysJump)
        {
            velocity.y = 4 * jumpingHeight * moveSpeedX / lenghtOfJump * controller.collisions.slopeNormal.y;
            velocity.x = 4 * jumpingHeight * moveSpeedX / lenghtOfJump * controller.collisions.slopeNormal.x;
            gravityConstant = floatyJumpGravity;
            hasJumped = true;
            hasJumpedAndNotLanded = true;
            slidingDownSlopeSidewaysJump = false;



            is_jumping_currently = true;
        }



        if (FixedWallSlidingJump)
        {
            if (wallDirectionX == input.x)
            {
                velocity.x = -wallDirectionX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (input.x == 0)
            {
                velocity.x = -wallDirectionX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                gravityConstant = wallLeapingJumpGravity;
                velocity.x = -wallDirectionX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
            FixedWallSlidingJump = false;



            is_jumping_currently = true;
        }



        if (fixedDoubleJump)
        {
            velocity.y = doubleJumpVelocityConstant * 4 * jumpingHeight * moveSpeedX / lenghtOfJump;
            gravityConstant = floatyJumpGravity;
            hasJumped = true;
            hasJumpedAndNotLanded = true;
            fixedDoubleJump = false;



            if (!Input.GetKey(KeyCode.Space))
            {
                makeASmallJump = true;
            }



            is_jumping_currently = true;
        }
    }



    private void FallingVelocityScalar()
    {
        if (velocityWhenReleasingSpaceScalarBool)
        {
            velocity.y *= velocityWhenReleasingSpaceScalar;
            velocityWhenReleasingSpaceScalarBool = false;
        }
    }



    void PlayerCombatFunction()
    {
        meleeAttackBufferTimePassed += Time.unscaledDeltaTime;



        if (Time.unscaledTime >= nextMeleeAttackTime && meleeAttackBufferTimePassed < meleeAttackBufferTime && Time.timeScale != 0)
        {
            MeeleAttack();
            nextMeleeAttackTime = Time.unscaledTime + timeBetweenMeleeAttacks;
            meleeAttackBufferTimePassed = meleeAttackBufferTime;
        }
        else if (Input.GetKeyDown(KeyCode.J) && Time.unscaledTime >= nextMeleeAttackTime && Time.timeScale != 0)
        {
            MeeleAttack();
            nextMeleeAttackTime = Time.unscaledTime + timeBetweenMeleeAttacks;
        }
        else if(Input.GetKeyDown(KeyCode.J))
        {
            meleeAttackBufferTimePassed = 0;
        }
    }


    void MeeleAttack()
    {
        playerState = PlayerAnimationState.meeleAttack;


        
        if (spriteOfPlayer.flipX)
        {
            meeleHitEnemies = Physics2D.OverlapCircleAll(meeleAttackPointRight.position, meeleAttackRange, enemyLayers); 
        }
        else
        {
            meeleHitEnemies = Physics2D.OverlapCircleAll(meeleAttackPointLeft.position, meeleAttackRange, enemyLayers);
        }



        foreach (Collider2D enemy in meeleHitEnemies)
        {
            enemy.GetComponent<EnemyHealthScript>().enemyTakeDamageByPlayer(meleeAttackDamage);
        }



        //buffer for attack
    }



    private void OnDrawGizmosSelected()
    {
        if (spriteOfPlayer.flipX)
        {
            Gizmos.DrawWireSphere(meeleAttackPointRight.position, meeleAttackRange);
        }
        else
        {
            Gizmos.DrawWireSphere(meeleAttackPointLeft.position, meeleAttackRange);
        }
    }



    public void meleeAttackHitStop()
    {
        enemyUponDeath = false;

        foreach (Collider2D enemy in meeleHitEnemies)
        {
            if(enemy.gameObject.layer == LayerMask.NameToLayer("DeadEnemies"))
            {
                enemyUponDeath = true;
            }
        }



        if (meeleHitEnemies != null && meeleHitEnemies.Length > 0 && enemyUponDeath == true)
        {
            HitStop(meeleAttackHitStopTimeUponDeath);
        }
        else if (meeleHitEnemies != null && meeleHitEnemies.Length > 0)
        {
            HitStop(meeleAttackHitStopTime);
        }
    }



    public void EndSpecialAnimation()
    {
        playerState = PlayerAnimationState.idle;
    }



    private void JumpingFunction()
    {
        if (FixedUpdateJump || slidingDownSlopeSidewaysJump || FixedWallSlidingJump || fixedDoubleJump)
        {
            jumpingBufferTimePassed += Time.unscaledDeltaTime;
        }
        else
        {
            jumpingBufferTimePassed = 0f;
        }



        NormalJump();



        MidAirBufferJump();



        GroundBufferJump();



        DoubleJump();
    }



    private void NormalJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (wallSliding)
            {
                FixedWallSlidingJump = true;
            }
            if (controller.collisions.below)
            {
                if (controller.collisions.slidingDownMaxSlope)
                {
                    if(input.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                    {
                        slidingDownSlopeSidewaysJump = true;
                    }
                }
                else
                {
                    FixedUpdateJump = true;
                }
            }
        }



        if (Input.GetKey(KeyCode.LeftControl))
        {
            sFalling = true;
        }
        else
        {
            sFalling = false;
        }



        if (sFalling)
        {
            gravityConstant = sJumpGravity;
        }
        else if (velocity.y == 0f || velocity.y < 0f)
        {
            gravityConstant = normalJumpGravity;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && hasJumped)
        {
            gravityConstant = releaseSpaceJumpGravity;
            velocityWhenReleasingSpaceScalarBool = true;
        }
        else if (makeASmallJump && hasJumped)
        {
            gravityConstant = releaseSpaceJumpGravity;
            velocityWhenReleasingSpaceScalarBool = true;
            makeASmallJump = false;
            hasJumped = false;
        }



        if (Input.GetKeyUp(KeyCode.Space))
        {
            hasJumped = false;
        }
    }



    private void MidAirBufferJump()
    {
        jumpBufferTimeMidAir += 1 * Time.deltaTime;



        if (Input.GetKeyDown(KeyCode.Space) && !controller.collisions.below)
        {
            jumpBufferTimeMidAir = 0;
        }



        if (controller.collisions.below && jumpBufferTimeMidAir < midAirJumpBufferTime && !controller.collisions.slidingDownMaxSlope)
        {
            FixedUpdateJump = true;
        }
    }



    private void GroundBufferJump()
    {
        if (hasJumpedAndNotLanded && jumpBufferTimeFromGround > 0.05f && controller.collisions.below)
        {
            hasJumpedAndNotLanded = false;
        }



        jumpBufferTimeFromGround += 1 * Time.deltaTime;



        if (controller.collisions.below)
        {
            jumpBufferTimeFromGround = 0;
        }



        if (Input.GetKeyDown(KeyCode.Space) && !controller.collisions.below && !hasJumpedAndNotLanded && jumpBufferTimeFromGround < groundJumpBufferTime)
        {
            FixedUpdateJump = true;
        }
    }



    void DoubleJump()
    {
        if (controller.collisions.below || wallSliding)
        {
            canMakeADoubleJump = true;
        }



        if (Input.GetKeyDown(KeyCode.Space) && !wallSliding && !controller.collisions.below && jumpBufferTimeFromGround >= groundJumpBufferTime && canMakeADoubleJump)
        {
            if (staminaBar.currentStamina >= doubleJumpStaminaDrain)
            {
                fixedDoubleJump = true;
                canMakeADoubleJump = false;
                staminaBar.TakeStamina(doubleJumpStaminaDrain);
            }
        }
    }



    private void FastXMovemvent()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            fastXMovementFixedUpdate = true;
        }
        else
        {
            fastXMovementFixedUpdate = false;
        }
    }



    void PlayerAnimationStateAndUpdate()
    {
        if (playerState != PlayerAnimationState.meeleAttack)
        {
            if (controller.collisions.below && Mathf.Abs(directionOfPlayerInput) > 0 && Mathf.Abs(velocity.x) != 0)
            {
                playerState = PlayerAnimationState.running;
            }
            else
            {
                playerState = PlayerAnimationState.idle;
            }



            if (!controller.collisions.below && velocity.y > 0.01f)
            {
                playerState = PlayerAnimationState.jumping;
            }
            else if (!controller.collisions.below && velocity.y < -0.01f)
            {
                playerState = PlayerAnimationState.falling;
            }
            else if (!controller.collisions.below)
            {
                playerState = PlayerAnimationState.transitionBetweenJumpingAndFalling;
            }



            if (wallSliding && !controller.collisions.below)
            {
                playerState = PlayerAnimationState.wallSlide;
            }



            if (controller.collisions.slidingDownMaxSlope && controller.collisions.below)
            {
                playerState = PlayerAnimationState.slidingDownSteepSlope;
            }
        }



        if (playerState != oldPlayerState)
        {
            animator.SetInteger("playerState", (int)playerState);
            oldPlayerState = playerState;
        }
        else
        {
            playerState = PlayerAnimationState.noTransitionPlayerState;
            animator.SetInteger("playerState", (int)playerState);



            if (oldPlayerState == PlayerAnimationState.meeleAttack)
            {
                playerState = oldPlayerState;
            }
        }
    }



    public void HitStop(float duration)
    {
        if (hitStopActivated)
        {
            return;
        }
        else
        {
            Time.timeScale = 0f;
            //AudioListener.pause = true;
            //maybe take away ability to do inputs while paused - så inte inputs görs medan hitstopen är aktiverad
            StartCoroutine(HitStopTime(duration));
        }
    }



    IEnumerator HitStopTime(float duration)
    {
        hitStopActivated = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        //AudioListener.pause = false;
        hitStopActivated = false;
        //om du vill första fienden efter pausen eller göra något efter hitstopen är klar kan du göra det här eller kalla en annan funktion
        //buffer inputs ksk???
    }
}