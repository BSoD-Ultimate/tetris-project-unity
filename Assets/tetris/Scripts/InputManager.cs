using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    // parameters for Delayed Auto Shift system
    public float AutoShiftDelay { get; set; }
    public float AutoShiftSpeed { get; set; }
    public float SoftDropSpeed { get; set; }

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
            if (Input.GetAxisRaw("Hold Piece") > 0)
            {
                Debug.Log("Hold performed");
            }
            yield return null;
        }
    }

    IEnumerator HandleLeftMoveInput()
    {
        //Debug.Log("Left move performed");
        GameManager.instance.DoMoveLeft();
        yield return new WaitForSeconds(AutoShiftDelay);
        while (Input.GetAxisRaw("Horizontal") < 0)
        {
            //Debug.Log("Left move performed by DAS");
            GameManager.instance.DoMoveLeft();
            yield return new WaitForSeconds(AutoShiftSpeed);
        }
        yield return null;
    }
    IEnumerator HandleRightMoveInput()
    {
        //Debug.Log("Right move performed");
        GameManager.instance.DoMoveRight();
        yield return new WaitForSeconds(AutoShiftDelay);
        while (Input.GetAxisRaw("Horizontal") > 0)
        {
            //Debug.Log("Right move performed by DAS");
            GameManager.instance.DoMoveRight();
            yield return new WaitForSeconds(AutoShiftSpeed);
        }
        yield return null;
    }
    IEnumerator HandleSoftDropInput()
    {
        Debug.Log("Soft Drop performed");
        yield return new WaitForSeconds(SoftDropSpeed);
    }
    #endregion

    // Use this for initialization
    void Start()
    {
        // setup default parameters for DAS
        AutoShiftDelay = 0.25f;
        AutoShiftSpeed = 0.05f;
        SoftDropSpeed = 0.01f;

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
