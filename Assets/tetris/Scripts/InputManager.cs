using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    // parameters for Delayed Auto Shift system
    public int AutoShiftDelay { get; set; }
    public int AutoShiftInterval { get; set; }
    public int SoftDropInterval { get; set; }


    private Coroutine coHandlePieceMove;
    private Coroutine coHandleOtherInputs;

    // Input handle priority: Hard Drop > Left/Right Move > Soft Drop
    #region Input Handlers
    IEnumerator HandlePieceMove()
    {
        while (true)
        {
            if (Input.GetAxisRaw("Horizontal") < 0)
            {
                yield return StartCoroutine(HandleLeftMoveInput());
            }
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                yield return StartCoroutine(HandleRightMoveInput());
            }
            if (Input.GetAxisRaw("Soft Drop") < 0)
            {
                yield return HandleSoftDropInput();
            }

            yield return null;
        }
    }
    IEnumerator HandleOtherInputs()
    {
        while (true)
        {
            if (Input.GetButtonDown("Hard Drop"))
            {
                GameManager.instance.DoHardDrop();
            }
            if (Input.GetButtonDown("Rotate CW"))
            {
                GameManager.instance.DoRotateCW();
            }
            if (Input.GetButtonDown("Rotate CCW"))
            {
                GameManager.instance.DoRotateCCW();
            }
            if (Input.GetButton("Hold Piece"))
            {
                GameManager.instance.DoHoldPiece();
            }
            // initial rotation
            if (Input.GetButton("Rotate CW") && !GameManager.instance.IsPieceSpawned)
            {
                GameManager.instance.DoRotateCW();
            }
            if (Input.GetButton("Rotate CCW") && !GameManager.instance.IsPieceSpawned)
            {
                GameManager.instance.DoRotateCCW();
            }
            yield return null;
        }
    }

    IEnumerator HandleLeftMoveInput()
    {
        GameManager.instance.DoMoveLeft();

        yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(AutoShiftDelay));
        //yield return new WaitForSeconds(AutoShiftDelay);
        while (Input.GetAxisRaw("Horizontal") < 0)
        {
            GameManager.instance.DoMoveLeft();
            yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(AutoShiftInterval));
            //yield return new WaitForSeconds(AutoShiftSpeed);
        }
    }
    IEnumerator HandleRightMoveInput()
    {
        GameManager.instance.DoMoveRight();

        yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(AutoShiftDelay));
        //yield return new WaitForSeconds(AutoShiftDelay);
        while (Input.GetAxisRaw("Horizontal") > 0)
        {
            GameManager.instance.DoMoveRight();
            yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(AutoShiftInterval));
            //yield return new WaitForSeconds(AutoShiftSpeed);
        }
    }
    IEnumerator HandleSoftDropInput()
    {
        GameManager.instance.DoSoftDrop();
        yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(SoftDropInterval));
    }
    #endregion

    // Use this for initialization
    void Start()
    {
        // setup default parameters for DAS
        AutoShiftDelay = 14;
        AutoShiftInterval = 1;
        SoftDropInterval = 1;

    }

    public void StartHandleInput()
    {
        // start coroutines which handle input operations
        coHandlePieceMove = StartCoroutine(HandlePieceMove());
        coHandleOtherInputs = StartCoroutine(HandleOtherInputs());
    }
    public void StopHandleInput()
    {
        StopCoroutine(coHandlePieceMove);
        StopCoroutine(coHandleOtherInputs);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
