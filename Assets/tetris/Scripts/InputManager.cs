using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    class KeyBindings
    {
        public KeyCode keyLeftMove { get; set; }
        public KeyCode keyRightMove { get; set; }
        public KeyCode keySoftDrop { get; set; }
        public KeyCode keyHardDrop { get; set; }
        public KeyCode keyRotateCW { get; set; }
        public KeyCode keyRotateCCW { get; set; }
        public KeyCode keyHold { get; set; }
    }
    private KeyBindings m_keyBindings = null;

    // parameters for Delayed Auto Shift system
    public float AutoShiftDelay { get; set; }
    public float AutoShiftSpeed { get; set; }
    public float SoftDropSpeed { get; set; }

    private bool isDoingMove = false;

    private Coroutine coHandleInputs;

    #region Game Controls
    // set game control keys
    public void SetLeftMoveKey(KeyCode key)
    {
        m_keyBindings.keyLeftMove = key;
    }
    public void SetRightMoveKey(KeyCode key)
    {
        m_keyBindings.keyRightMove = key;
    }
    public void SetSoftDropKey(KeyCode key)
    {
        m_keyBindings.keySoftDrop = key;
    }
    public void SetHardDropKey(KeyCode key)
    {
        m_keyBindings.keyHardDrop = key;
    }
    public void SetRotateCWKey(KeyCode key)
    {
        m_keyBindings.keyRotateCW = key;
    }
    public void SetRotateCCWKey(KeyCode key)
    {
        m_keyBindings.keyRotateCCW = key;
    }
    public void SetHoldKey(KeyCode key)
    {
        m_keyBindings.keyHold = key;
    }
    // default controls
    public void SetDefaultControls()
    {
        m_keyBindings.keyLeftMove = KeyCode.LeftArrow;
        m_keyBindings.keyRightMove = KeyCode.RightArrow;
        m_keyBindings.keySoftDrop = KeyCode.DownArrow;
        m_keyBindings.keyHardDrop = KeyCode.UpArrow;
        m_keyBindings.keyRotateCW = KeyCode.Z;
        m_keyBindings.keyRotateCCW = KeyCode.X;
        m_keyBindings.keyHold = KeyCode.LeftShift;
    }
    #endregion

    // Input handle priority: Hard Drop > Left/Right Move > Soft Drop
    #region Input Handlers
    IEnumerator HandleInputs()
    {
        while (true)
        {
            if (Input.GetKeyDown(m_keyBindings.keyLeftMove))
            {
                StartCoroutine(HandleLeftMoveInput());
            }
            if (Input.GetKeyDown(m_keyBindings.keyRightMove))
            {
                StartCoroutine(HandleRightMoveInput());
            }
            if(Input.GetKeyDown(m_keyBindings.keySoftDrop))
            {

            }
            if(Input.GetKeyDown(m_keyBindings.keyHardDrop))
            {
                GameManager.instance.DoHardDrop();
            }
            if (Input.GetKeyDown(m_keyBindings.keyRotateCW))
            {
                //Debug.Log("RotateCW performed");
                GameManager.instance.DoRotateCW();
            }
            if (Input.GetKeyDown(m_keyBindings.keyRotateCCW))
            {
                Debug.Log("RotateCCW performed");
                GameManager.instance.DoRotateCCW();
            }
            if (Input.GetKeyDown(m_keyBindings.keyHold))
            {
                Debug.Log("Hold performed");
            }

            // initial rotation & hold
            if (Input.GetKey(m_keyBindings.keyRotateCW) && !GameManager.instance.IsPieceSpawned)
            {
                GameManager.instance.DoRotateCW();
            }
            if (Input.GetKey(m_keyBindings.keyRotateCCW) && !GameManager.instance.IsPieceSpawned)
            {
                Debug.Log("RotateCCW performed");
            }
            yield return null;
        }
    }

    IEnumerator HandleLeftMoveInput()
    {
        //Debug.Log("Left move performed");
        GameManager.instance.DoMoveLeft();
        yield return new WaitForSeconds(AutoShiftDelay);
        while (Input.GetKey(m_keyBindings.keyLeftMove))
        {
            //Debug.Log("Left move performed by DAS");
            GameManager.instance.DoMoveLeft();
            yield return new WaitForSeconds(AutoShiftSpeed);
        }
        isDoingMove = false;
        yield return null;
    }
    IEnumerator HandleRightMoveInput()
    {
        //Debug.Log("Right move performed");
        GameManager.instance.DoMoveRight();
        yield return new WaitForSeconds(AutoShiftDelay);
        while (Input.GetKey(m_keyBindings.keyRightMove))
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
        while (Input.GetKey(m_keyBindings.keySoftDrop))
        {
            yield return new WaitForSeconds(SoftDropSpeed);
            Debug.Log("Soft Drop performed by DAS");
        }
        yield return null;
    }
    IEnumerator HandleOtherInputs()
    {
        yield return null;
    }
    #endregion

    // Use this for initialization
    void Start()
    {
        // setup default controls
        // TODO: add read custom control settings from file 
        m_keyBindings = new KeyBindings();
        SetDefaultControls();

        // setup default parameters for DAS
        AutoShiftDelay = 0.25f;
        AutoShiftSpeed = 0.015f;
        SoftDropSpeed = 0.01f;

    }

    public void StartHandleInput()
    {
        // start coroutines which handle input operations
        coHandleInputs = StartCoroutine(HandleInputs());
    }
    public void StopHandleInput()
    {
        StopCoroutine(coHandleInputs);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
