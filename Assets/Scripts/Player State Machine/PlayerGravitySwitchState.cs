using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravitySwitchState : PlayerBaseState
{
    public PlayerGravitySwitchState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    :base (currentContext, playerStateFactory){ }

    static float gravityChangeSpeed = 90f; //rate at which gravity is flipped in degrees per second. Arbitrarily picked. but 90 means it takes 1 second for flip. smaller number means slower rotation
    Vector3 playerLookPoint; //location where the player is looking before gravity is flipped
    float bodyAngleBetween; //angle the player needs to turn in order to be facing the target it was previously looking at
    float playerCamToFromAngle; //angle between where the camera is looking and the players neutral angle
    float incrementor = 0f; //holds how many degrees player has turned already
    float increment = gravityChangeSpeed * Time.fixedDeltaTime;
    float maxChange = 90f; //degrees players axis will change by
    float remainingAngle; //extra correction angle
    int counter = 0; //how many correction steps have been applied
    int count = 15; //number of steps to apply correction angle. Number chosen arbitrarily but the larger the number, the slower and smoother the final correction looks

    //used to track the progress of the gravity changing script
    GravityChangeSteps gravityChangeStep = GravityChangeSteps.initial;
    enum GravityChangeSteps { initial, corrections, done, reset}

    public override void EnterState()
    {
        ResetParameters();
        ctx._playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it
        ctx._disableGravity = true;
        PrecalculateRotation();
    }
    public override void UpdateState()
    {
        if (gravityChangeStep == GravityChangeSteps.initial)
        {
            InitialRotation();
        }else if(gravityChangeStep == GravityChangeSteps.corrections)
        {
            Corrections();
        }else if(gravityChangeStep == GravityChangeSteps.done)
        {
            ctx._playerCameraScript.enabled = true;
            ctx._disableGravity = false;
            gravityChangeStep = GravityChangeSteps.reset;
        }

        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
        Debug.Log("here");
        ctx._playerCameraScript.enabled = true;
        ctx._disableGravity = false;
    }
    public override void InitializeSubState()
    {

    }
    public override void CheckSwitchState()
    {
        if(gravityChangeStep == GravityChangeSteps.reset)
        {
            gravityChangeStep = GravityChangeSteps.initial;
            SwitchState(factory.Idle());
        }
    }

    //precalculates the rotation required to rotate the player so they are aligned with their newly desired up direction
    void PrecalculateRotation()
    {
        //Purpose: Is to pre-calculate the players new gravity direction
        //Gravity and player controls are disabled to make the players movement more predictable
        //The player is rotated so that their players up is opposite the new gravity direction
        ctx._playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it
        ctx._disableGravity = true;

        //trying to make it so player is looking at same spot after rotation as before
        //grabs the location of the point that the player is looking at. if the point is significantly far away, a point a set distance away is used instead
        //Vector3 playerLookPoint;
        RaycastHit hit;
        if (Physics.Raycast(ctx._playerCam.position, ctx._playerCam.forward, out hit, 60f))
        {
            playerLookPoint = hit.point;
        }
        else
        {
            playerLookPoint = ctx._playerCam.position + ctx._playerCam.forward * 60f;
        }

        //need to precalculate this value and then precalculate how much the player needs to rotate to look at target
        Quaternion origRotation = ctx.transform.rotation;
        Quaternion camRoration = ctx._playerCam.rotation;

        ctx.transform.Rotate(ctx._rotationAxis, 90f, Space.World);

        Vector3 playerLookDirection = playerLookPoint - ctx._playerCam.position;
        Vector3 projectedLookAngle = Vector3.ProjectOnPlane(playerLookDirection, ctx._up);
        bodyAngleBetween = Vector3.SignedAngle(ctx.transform.forward, projectedLookAngle, ctx._up);

        ctx.transform.Rotate(Vector3.up, bodyAngleBetween, Space.Self);

        //stops the player from completely rotating around in order to look at object when flipping gravity back or forward
        //may disable this
        if (bodyAngleBetween > 150f)
        {
            bodyAngleBetween -= 180f;
        }
        else if (bodyAngleBetween < -150f)
        {
            bodyAngleBetween += 180f;
        }

        //calculates angle between playerCam and previous look position
        playerLookDirection = playerLookPoint - ctx._playerCam.position;
        Vector3 playerCamRotationAxis = ctx._playerCam.right;
        playerCamToFromAngle = Vector3.SignedAngle(ctx._playerCam.forward, playerLookDirection, playerCamRotationAxis);

        ctx.transform.rotation = origRotation;
        ctx._playerCam.rotation = camRoration;
    }

    //Applies the precalcuated quaternion angles to the player over time
    void InitialRotation()
    {
        if(incrementor < maxChange)
        {
            if (incrementor + increment > 90f)
            {
                increment = maxChange - incrementor;
            }
            Debug.DrawLine(ctx._playerCam.position, playerLookPoint, Color.green);
            Debug.DrawRay(ctx._playerCam.position, ctx._playerCam.forward * 20f, Color.red);
            ctx.transform.Rotate(ctx._rotationAxis * increment, Space.World); //aligns transform.up with new calculated up
            ctx.transform.Rotate(Vector3.up * bodyAngleBetween * increment / maxChange, Space.Self); //rotates player body locally to face towards previous looked at spot
            ctx._playerCam.Rotate(Vector3.right * playerCamToFromAngle * increment / maxChange, Space.Self); //adjusts camera to look at previously looked at spot

            incrementor += increment;
        }
        else
        {
            //because quaternion multiplication is weird af, there will be some remaining distance to rotate. This is calculated here to be used in the corrections phase
            remainingAngle = Vector3.Angle(ctx.transform.up, ctx._up);
            gravityChangeStep = GravityChangeSteps.corrections;
        }
    }

    //initial rotation is usually off by a few degrees because quaternion multiplication depends on the order
    //When precalculating the order is different than when they are actually applied so these final corrections are done to account for that
    void Corrections()
    {
        if(counter < count)
        {
            //rotates the final amount
            ctx.transform.Rotate(ctx._rotationAxis, remainingAngle / count, Space.World);
            counter++;
        }
        else
        {
            //final adujustment because quaternions are not to be trusted
            //may need to switch this to truncation as rounding to int may be too jarring
            float x = Mathf.Round(ctx.transform.localEulerAngles.x);
            float y = Mathf.Round(ctx.transform.localEulerAngles.y);
            float z = Mathf.Round(ctx.transform.localEulerAngles.z);

            ctx.transform.localEulerAngles = new Vector3(x, y, z);
            gravityChangeStep = GravityChangeSteps.done;
        }
    }

    //resetting parameters used in this script so they dont impact the next time the script is ran
    void ResetParameters()
    {
        bodyAngleBetween = 0f;
        playerCamToFromAngle = 0f;
        playerLookPoint = Vector3.zero;
        incrementor = 0f;
        remainingAngle = 0f;
        counter = 0;
    }
}
