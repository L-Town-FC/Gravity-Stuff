using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravitySwitchState : PlayerBaseState
{

    //TODO: Convert Coutine into function that can be run in update

    public PlayerGravitySwitchState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    :base (currentContext, playerStateFactory){ }

    static float gravityChangeSpeed = 10f;
    bool isGravityChanging = false;
    bool applyRotation = false;
    Vector3 playerLookPoint;
    float bodyAngleBetween;
    float playerCamToFromAngle;
    float incrementor = 0f; //holds how many degrees player has turned already
    float increment = gravityChangeSpeed * Time.fixedDeltaTime;
    float maxChange = 90f; //degrees players axis will change by

    public override void EnterState()
    {
        Debug.Log("Flipping Gravity");
        isGravityChanging = true;
        ctx._playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it
        ctx._disableGravity = true;
    }
    public override void UpdateState()
    {
        if (applyRotation)
        {

        }
        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }
    public override void InitializeSubState()
    {

    }
    public override void CheckSwitchState()
    {

    }

    void PrecalculateRotation()
    {
        //Purpose: This IEnumerator's purpose is to alter which direction gravity is for the player. To do this, a new gravity direction is chosen by some player means
        //Gravity and player controls are disabled to make the players movement more predictable when this IEnumerator is triggered
        //The player is rotated so that their players up is opposite the new gravity direction
        //This IEnumerator also attempts to make the player look at the same spot throughout the transition to make the transition less jarring
        ctx._playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it
        ctx._disableGravity = true;



        //playerStateManager.currentPlayerState = playerStateManager.PlayerState.flipping; //sets globals player state to flipping gravity so the player cant do other actions while flipping
        playerStateManager.playerStatesQueue.Enqueue(playerStateManager.PlayerState.flipping);

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
        //float bodyAngleBetween = Vector3.SignedAngle(ctx.transform.forward, projectedLookAngle, ctx._up);
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
        //float playerCamToFromAngle = Vector3.SignedAngle(ctx._playerCam.forward, playerLookDirection, playerCamRotationAxis);

        ctx.transform.rotation = origRotation;
        ctx._playerCam.rotation = camRoration;
        applyRotation = true;
    }

    //TODO: Split this into two more functions. One for the first while, one for the second while

    void ApplyRotation()
    {
        float incrementor = 0f; //holds how many degrees player has turned already
        float increment = gravityChangeSpeed * Time.fixedDeltaTime;
        float maxChange = 90f; //degrees players axis will change by

        //the players yaw and cameras pitch rates are based on the players axis turning rate.The faster the turning rate, the larger the increment, and the faster everything else occurs
        while (incrementor < maxChange)
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
            yield return null;
        }

        //because quaternions make no sense this needed to be added because the player would regularly be slightly off alignment with a cardinal direction
        //if not perfectly aligned with different cardinal unit vectors the rotation doesnt complete fully. this is a workaround that checks the remaining angle and completes it
        //is barely noticable but something to look at when bored one day
        float remainingAngle = Vector3.Angle(ctx.transform.up, ctx._up);
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            ctx.transform.Rotate(ctx._rotationAxis, remainingAngle / count, Space.World);
            yield return null;
        }

        //final adujustment because quaternions are not to be trusted
        //may need to switch this to truncation as rounding to int may be too jarring
        float x = Mathf.Round(ctx.transform.localEulerAngles.x);
        float y = Mathf.Round(ctx.transform.localEulerAngles.y);
        float z = Mathf.Round(ctx.transform.localEulerAngles.z);

        ctx.transform.localEulerAngles = new Vector3(x, y, z);

        ctx._playerCameraScript.enabled = true;
        ctx._disableGravity = false;
        isGravityChanging = false;
    }

    IEnumerator SettingGravity(Vector3 _rotationAxis)
    {
        //Purpose: This IEnumerator's purpose is to alter which direction gravity is for the player. To do this, a new gravity direction is chosen by some player means
        //Gravity and player controls are disabled to make the players movement more predictable when this IEnumerator is triggered
        //The player is rotated so that their players up is opposite the new gravity direction
        //This IEnumerator also attempts to make the player look at the same spot throughout the transition to make the transition less jarring
        ctx._playerCameraScript.enabled = false; //disables players ability to move their camera around during gravity flip. this ensures that player cant screw up coroutine by moving during it
        ctx._disableGravity = true;

        

        //playerStateManager.currentPlayerState = playerStateManager.PlayerState.flipping; //sets globals player state to flipping gravity so the player cant do other actions while flipping
        playerStateManager.playerStatesQueue.Enqueue(playerStateManager.PlayerState.flipping);

        //trying to make it so player is looking at same spot after rotation as before
        //grabs the location of the point that the player is looking at. if the point is significantly far away, a point a set distance away is used instead
        Vector3 playerLookPoint;
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

        ctx.transform.Rotate(_rotationAxis, 90f, Space.World);

        Vector3 playerLookDirection = playerLookPoint - ctx._playerCam.position;
        Vector3 projectedLookAngle = Vector3.ProjectOnPlane(playerLookDirection, ctx._up);
        float bodyAngleBetween = Vector3.SignedAngle(ctx.transform.forward, projectedLookAngle, ctx._up);
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
        float playerCamToFromAngle = Vector3.SignedAngle(ctx._playerCam.forward, playerLookDirection, playerCamRotationAxis);

        ctx.transform.rotation = origRotation;
        ctx._playerCam.rotation = camRoration;

        float incrementor = 0f; //holds how many degrees player has turned already
        float increment = gravityChangeSpeed * Time.fixedDeltaTime;
        float maxChange = 90f; //degrees players axis will change by

        //the players yaw and cameras pitch rates are based on the players axis turning rate.The faster the turning rate, the larger the increment, and the faster everything else occurs
        while (incrementor < maxChange)
        {
            if (incrementor + increment > 90f)
            {
                increment = maxChange - incrementor;
            }
            Debug.DrawLine(ctx._playerCam.position, playerLookPoint, Color.green);
            Debug.DrawRay(ctx._playerCam.position, ctx._playerCam.forward * 20f, Color.red);
            ctx.transform.Rotate(_rotationAxis * increment, Space.World); //aligns transform.up with new calculated up
            ctx.transform.Rotate(Vector3.up * bodyAngleBetween * increment / maxChange, Space.Self); //rotates player body locally to face towards previous looked at spot
            ctx._playerCam.Rotate(Vector3.right * playerCamToFromAngle * increment / maxChange, Space.Self); //adjusts camera to look at previously looked at spot

            incrementor += increment;
            yield return null;
        }

        //because quaternions make no sense this needed to be added because the player would regularly be slightly off alignment with a cardinal direction
        //if not perfectly aligned with different cardinal unit vectors the rotation doesnt complete fully. this is a workaround that checks the remaining angle and completes it
        //is barely noticable but something to look at when bored one day
        float remainingAngle = Vector3.Angle(ctx.transform.up, ctx._up);
        int count = 15;
        for (int i = 0; i < count; i++)
        {
            ctx.transform.Rotate(_rotationAxis, remainingAngle / count, Space.World);
            yield return null;
        }

        //final adujustment because quaternions are not to be trusted
        //may need to switch this to truncation as rounding to int may be too jarring
        float x = Mathf.Round(ctx.transform.localEulerAngles.x);
        float y = Mathf.Round(ctx.transform.localEulerAngles.y);
        float z = Mathf.Round(ctx.transform.localEulerAngles.z);

        ctx.transform.localEulerAngles = new Vector3(x, y, z);

        ctx._playerCameraScript.enabled = true;
        ctx._disableGravity = false;

        //playerStateManager.currentPlayerState = playerStateManager.PlayerState._default; //sets globals player state to flipping gravity so the player cant do other actions while flipping
        //playerStateManager.playerStatesQueue.Enqueue(playerStateManager.PlayerState._default);

        //StopCoroutine("SettingGravity");

        yield return null;
    }
}
