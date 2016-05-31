using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TetrisEngine;
using System.Threading;

public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;
    private InputManager inputManager;

    // predefined piece preview sprite
    public Sprite[] m_NextPieceSprite;
    public Dictionary<string, Sprite> m_MapPieceSprite;

    // predefined block type
    public GameObject[] m_BlockType;
    public Dictionary<BlockType, GameObject> m_MapBlockType;
    public GameObject[] m_BlockHintType;
    public Dictionary<BlockType, GameObject> m_MapBlockHintType;

    // gameobjects which show hold and next piece
    public GameObject[] m_NextPieceShow;
    public GameObject m_HoldPieceShow;

    // UI objects show game info
    public GameObject m_TextPendingLines;
    public GameObject m_TextRowClearType;
    public GameObject m_TextBackToBack;
    public GameObject m_TextComboCount;

    // blocks on the scene
    private GameObject[,] m_BlockView;
    // current piece
    private GameObject[] m_BlockCurrentPiece;
    // hint blocks indicate where current piece will be drop
    private GameObject[] m_BlockHint;
    // coroutine controls piece drop
    private Coroutine m_coPieceDrop;

    #region engine
    private Field m_Field;
    public Piece m_CurrentPiece = null;
    private Piece m_HoldPiece = null;
    private Queue<Piece> m_IncomingPieceQueue;
    private BlockRandomizer m_Randomizer;
    private int m_PendingLines;
    #endregion

    #region properties
    // properties control game behavior. the unit of the properties is frames. 
    public int AutoShiftDelay
    {
        get
        {
            Debug.Assert(inputManager != null);
            return inputManager.AutoShiftDelay;
        }
        set
        {
            Debug.Assert(inputManager != null);
            inputManager.AutoShiftDelay = value;
        }
    }
    public int AutoShiftInterval
    {
        get
        {
            Debug.Assert(inputManager != null);
            return inputManager.AutoShiftInterval;
        }
        set
        {
            Debug.Assert(inputManager != null);
            inputManager.AutoShiftInterval = value;
        }
    }
    public int SoftDropInterval
    {
        get
        {
            Debug.Assert(inputManager != null);
            return inputManager.SoftDropInterval;
        }
        set
        {
            Debug.Assert(inputManager != null);
            inputManager.SoftDropInterval = value;
        }
    }
    public int PieceSpawnDelay { get; set; }
    public int PieceLockDelay { get; set; }
    public int LineClearDelay { get; set; }
    // We regard a piece drop one unit during a frame as 1G. 
    // When the piece drops at the speed of 1G, the corresponding internal gravity value is 65536.
    public int PieceDropInternalGravity { get; set; }
    public const int gravityUnit = 65536;
    #endregion

    #region internal flags
    private bool isPieceSpawned = false;
    public bool IsPieceSpawned
    {
        get
        {
            return isPieceSpawned;
        }
    }
    private bool isInitialRotationPerformed = false;
    private bool isHoldPerformed = false;
    private bool isGameOver = false;
    private bool isHintShown = true;
    public bool IsHintShown
    {
        get
        {
            return isHintShown;
        }
    }

    private FrameWaitCoroutine coWaitLockDelay;
    #endregion

    // a wrapper controls block's animation
    class BlockAnimation
    {
        public static void SetFixed(ref GameObject[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].GetComponent<Animator>().SetBool("Fixed", true);
            }
        }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        inputManager = GetComponent<InputManager>();
    }
    // Use this for initialization
    void Start()
    {
        // initialize map between piece and predefined sprite
        m_MapPieceSprite = new Dictionary<string, Sprite>();
        m_MapPieceSprite.Add("IPiece", m_NextPieceSprite[0]);
        m_MapPieceSprite.Add("JPiece", m_NextPieceSprite[1]);
        m_MapPieceSprite.Add("LPiece", m_NextPieceSprite[2]);
        m_MapPieceSprite.Add("OPiece", m_NextPieceSprite[3]);
        m_MapPieceSprite.Add("SPiece", m_NextPieceSprite[4]);
        m_MapPieceSprite.Add("TPiece", m_NextPieceSprite[5]);
        m_MapPieceSprite.Add("ZPiece", m_NextPieceSprite[6]);

        // map between blockType and predefined block
        m_MapBlockType = new Dictionary<BlockType, GameObject>();
        m_MapBlockType.Add(BlockType.Grey, m_BlockType[0]);
        m_MapBlockType.Add(BlockType.Red, m_BlockType[1]);
        m_MapBlockType.Add(BlockType.Orange, m_BlockType[2]);
        m_MapBlockType.Add(BlockType.Yellow, m_BlockType[3]);
        m_MapBlockType.Add(BlockType.Green, m_BlockType[4]);
        m_MapBlockType.Add(BlockType.Cyan, m_BlockType[5]);
        m_MapBlockType.Add(BlockType.Blue, m_BlockType[6]);
        m_MapBlockType.Add(BlockType.Purple, m_BlockType[7]);
        m_MapBlockType.Add(BlockType.Bone, m_BlockType[8]);

        m_MapBlockHintType = new Dictionary<BlockType, GameObject>();
        m_MapBlockHintType.Add(BlockType.Grey, m_BlockHintType[0]);
        m_MapBlockHintType.Add(BlockType.Red, m_BlockHintType[1]);
        m_MapBlockHintType.Add(BlockType.Orange, m_BlockHintType[2]);
        m_MapBlockHintType.Add(BlockType.Yellow, m_BlockHintType[3]);
        m_MapBlockHintType.Add(BlockType.Green, m_BlockHintType[4]);
        m_MapBlockHintType.Add(BlockType.Cyan, m_BlockHintType[5]);
        m_MapBlockHintType.Add(BlockType.Blue, m_BlockHintType[6]);
        m_MapBlockHintType.Add(BlockType.Purple, m_BlockHintType[7]);
        m_MapBlockHintType.Add(BlockType.Bone, m_BlockHintType[8]);

        // public properties
        AutoShiftDelay = 14;
        AutoShiftInterval = 1;
        SoftDropInterval = 1;
        PieceDropInternalGravity = 1024;
        PieceSpawnDelay = 24;
        PieceLockDelay = 30;
        LineClearDelay = 30;

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Start the game.
    public void StartGame()
    {
        // initialize engine

        // field
        m_Field = new Field();

        // randomizer
        m_Randomizer = new History4RollsRandomizer();
        // fill the incoming piece queue.
        m_IncomingPieceQueue = new Queue<Piece>();
        for (int i = 0; i < 6; i++)
        {
            m_IncomingPieceQueue.Enqueue(m_Randomizer.GetNextPiece(m_Field));
        }

        // initialize game objects.

        m_BlockView = new GameObject[m_Field.Height, m_Field.Width];

        coWaitLockDelay = new FrameWaitCoroutine(this);
        coWaitLockDelay.WaitFrameCount = PieceLockDelay;

        // show next piece on screen
        UpdatePiecePreview();


        // start game coroutine 
        StartCoroutine(HandleGame());
        inputManager.StartHandleInput();

    }

    void UpdatePiecePreview()
    {
        // hold
        if (m_HoldPiece != null)
        {
            m_HoldPieceShow.GetComponent<SpriteRenderer>().sprite = m_MapPieceSprite[m_HoldPiece.GetType().Name];
        }
        else
        {
            m_HoldPieceShow.GetComponent<SpriteRenderer>().sprite = null;
        }
        // next
        for (int i = 0; i < m_NextPieceShow.Length; i++)
        {
            if (i < m_IncomingPieceQueue.Count)
            {
                string pieceName = m_IncomingPieceQueue.ElementAt(i).GetType().Name;
                m_NextPieceShow[i].GetComponent<SpriteRenderer>().sprite = m_MapPieceSprite[pieceName];
            }
            else
            {
                m_NextPieceShow[i].GetComponent<SpriteRenderer>().sprite = null;
            }
        }

    }

    // update the whole field
    private void UpdateField()
    {
        // destroy all blocks
        for (int i = 0; i < m_Field.Height; i++)
        {
            for (int j = 0; j < m_Field.Width; j++)
            {
                if (m_BlockView[i, j] != null)
                {
                    Destroy(m_BlockView[i, j]);
                    m_BlockView[i, j] = null;
                }
            }
        }

        // gather block info from m_field
        for (int i = 0; i < m_Field.Height; i++)
        {
            for (int j = 0; j < m_Field.Width; j++)
            {
                if (m_Field[i, j] != null)
                {
                    m_BlockView[i, j] = (GameObject)Instantiate(m_MapBlockType[m_Field[i, j].Type], new Vector3(j, i), Quaternion.identity);
                    m_BlockView[i, j].GetComponent<SpriteRenderer>().color = new Color32(192, 192, 192, 255);
                }
            }
        }
    }
    // update location of the current piece
    private void UpdatePiecePosition()
    {
        Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
        for (int i = 0; i < m_BlockCurrentPiece.Length; i++)
        {
            Vector3 newPosition = new Vector3(blockPos[i].x, blockPos[i].y);
            m_BlockCurrentPiece[i].transform.position = newPosition;
        }
    }

    // update piece drop hint position
    private void UpdateHintPosition()
    {
        if (isHintShown)
        {
            int offset = m_CurrentPiece.TryMoveDown(m_Field.Height);
            Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
            for (int i = 0; i < m_BlockHint.Length; i++)
            {
                Vector3 newPosition = new Vector3(blockPos[i].x, blockPos[i].y - offset);
                m_BlockHint[i].transform.position = newPosition;
            }
        }
    }
    public void UpdatePendingLines()
    {
        m_TextPendingLines.GetComponent<Text>().text = m_PendingLines.ToString();
    }

    public Piece GetCurrentPiece()
    {
        return m_CurrentPiece;
    }

    public bool SpawnPiece()
    {
        // 20G mode
        if (PieceDropInternalGravity >= gravityUnit * 20)
        {
            m_CurrentPiece.MoveDown(20);
        }
        Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
        m_BlockCurrentPiece = new GameObject[blockPos.Length];
        // spawn the piece on the screen
        for (int i = 0; i < blockPos.Length; i++)
        {
            GameObject block = m_MapBlockType[m_CurrentPiece.PieceStyle];
            Vector3 position = new Vector3(blockPos[i].x, blockPos[i].y);
            m_BlockCurrentPiece[i] = (GameObject)Instantiate(block, position, Quaternion.identity);
        }

        // spawn hint
        if (isHintShown)
        {
            m_BlockHint = new GameObject[blockPos.Length];
            for (int i = 0; i < blockPos.Length; i++)
            {
                GameObject block = m_MapBlockHintType[m_CurrentPiece.PieceStyle];
                Vector3 position = new Vector3(blockPos[i].x, blockPos[i].y - m_CurrentPiece.TryMoveDown(m_Field.Height));
                m_BlockHint[i] = (GameObject)Instantiate(block, position, Quaternion.identity);
            }
        }

        // check if spawn operation is successful
        return m_CurrentPiece.CheckSpawn();

    }

    IEnumerator GameOver()
    {
        Debug.Log("Game Over");
        // stop all coroutines related to game
        StopCoroutine(m_coPieceDrop);
        inputManager.StopHandleInput();

        // destroy hint blocks
        for (int i = 0; i < m_BlockHint.Length; i++)
        {
            Destroy(m_BlockHint[i]);
            m_BlockHint[i] = null;
        }

        // replace conflict blocks with current piece
        Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
        for (int i = 0; i < m_BlockCurrentPiece.Length; i++)
        {
            if (m_BlockView[blockPos[i].y, blockPos[i].x] != null)
            {
                Destroy(m_BlockView[blockPos[i].y, blockPos[i].x]);
            }
            m_BlockView[blockPos[i].y, blockPos[i].x] = m_BlockCurrentPiece[i];
        }

        // destroy blocks from bottom to top
        for (int i = 0; i < m_Field.Height; i++)
        {
            for (int j = 0; j < m_Field.Width; j++)
            {
                if (m_BlockView[i, j] != null)
                {
                    Destroy(m_BlockView[i, j]);
                    m_BlockView[i, j] = null;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;

        // clear all classes releted to current game
        m_CurrentPiece = null;
        m_HoldPiece = null;
        m_IncomingPieceQueue.Clear();
        m_Field.Clear();
        UpdatePiecePreview();

        // show game over on screen

        //GameObject gameOver = (GameObject)Instantiate(m_GameOverText, new Vector3(4.5f, 9.5f), Quaternion.identity);
        //yield return new WaitForSeconds(2f);
        //Destroy(gameOver);


        // reset game over flags
        isGameOver = false;
        yield return null;
    }

    private void CheckPieceState()
    {
        if (m_CurrentPiece.CurrentState == Piece.State.Fixing)
        {
            if (m_CurrentPiece.FixResetCount <= Piece.FixCountdownResetMaxCount)
            {
                // reset the fix countdown timer
                coWaitLockDelay.Reset();
                coWaitLockDelay.Start();
            }
            if (m_CurrentPiece.TryMoveDown())
            {
                // TODO: update fix countdown scrollbar?
            }
        }
        else if (m_CurrentPiece.CurrentState == Piece.State.Unfixed && m_CurrentPiece.FixResetCount > 0)
        {
            coWaitLockDelay.Reset();
        }

    }

    public bool DoMoveLeft()
    {
        if (isPieceSpawned && m_CurrentPiece.CurrentState != Piece.State.Fixed)
        {
            bool success = m_CurrentPiece.MoveLeft();
            if (success)
            {
                UpdatePiecePosition();
                UpdateHintPosition();
                CheckPieceState();
                return true;
            }
        }
        return false;
    }
    public bool DoMoveRight()
    {
        if (isPieceSpawned && m_CurrentPiece.CurrentState != Piece.State.Fixed)
        {
            bool success = m_CurrentPiece.MoveRight();
            if (success)
            {
                UpdatePiecePosition();
                UpdateHintPosition();
                CheckPieceState();
                return true;
            }
        }
        return false;
    }
    public bool DoSoftDrop()
    {
        if (isPieceSpawned && m_CurrentPiece.CurrentState != Piece.State.Fixed)
        {
            bool success = m_CurrentPiece.MoveDown();
            if (success)
            {
                UpdatePiecePosition();
                CheckPieceState();
                return true;
            }
        }
        return false;
    }
    public bool DoHardDrop()
    {
        if (isPieceSpawned && m_CurrentPiece.CurrentState != Piece.State.Fixed)
        {
            m_CurrentPiece.HardDrop();
            UpdatePiecePosition();
            BlockAnimation.SetFixed(ref m_BlockCurrentPiece);
            StopCoroutine(m_coPieceDrop);
            // stop the countdown timer
            coWaitLockDelay.Reset();

            // destroy hint blocks
            if (isHintShown)
            {
                for (int i = 0; i < m_BlockHint.Length; i++)
                {
                    Destroy(m_BlockHint[i]);
                    m_BlockHint[i] = null;
                }
            }

            // apply piece to the field
            Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
            for (int i = 0; i < m_BlockCurrentPiece.Length; i++)
            {
                m_BlockView[blockPos[i].y, blockPos[i].x] = m_BlockCurrentPiece[i];
                m_BlockCurrentPiece[i].GetComponent<SpriteRenderer>().color = new Color32(192, 192, 192, 255);
            }

            return true;
        }
        else
        {
            return false;
        }
    }
    public bool DoRotateCW()
    {
        if (!isPieceSpawned)
        {
            // initial rotation
            if (m_CurrentPiece.CurrentState != Piece.State.Fixed && !isInitialRotationPerformed)
            {
                isInitialRotationPerformed = true;
                return m_CurrentPiece.RotateCW();
            }
            else
            {
                return false;
            }

        }
        else // normal rotation
        {
            if (m_CurrentPiece.CurrentState != Piece.State.Fixed)
            {
                bool success = m_CurrentPiece.RotateCW();
                if (success)
                {
                    UpdatePiecePosition();
                    UpdateHintPosition();
                    CheckPieceState();
                }
                return success;
            }
            return false;
        }
    }

    public bool DoRotateCCW()
    {
        if (!isPieceSpawned)
        {
            // initial rotation
            if (m_CurrentPiece.CurrentState != Piece.State.Fixed && !isInitialRotationPerformed)
            {
                isInitialRotationPerformed = true;
                return m_CurrentPiece.RotateCCW();
            }
            else
            {
                return false;
            }
        }
        else // normal rotation
        {
            if (m_CurrentPiece.CurrentState != Piece.State.Fixed)
            {
                bool success = m_CurrentPiece.RotateCCW();
                if (success)
                {
                    UpdatePiecePosition();
                    UpdateHintPosition();
                    CheckPieceState();
                }
                return success;
            }
            return false;
        }
    }
    public bool DoHoldPiece()
    {
        if (isHoldPerformed)
        {
            return false;
        }
        if (m_CurrentPiece.CurrentState == Piece.State.Fixed)
        {
            return false;
        }
        isHoldPerformed = true;
        if (!isPieceSpawned)
        {
            // initial hold
            // Caution: initial hold is occurred after current piece is replaced by the next one (on the screen)
            if (m_HoldPiece == null)
            {
                // hold the current piece
                m_HoldPiece = m_CurrentPiece;
                m_CurrentPiece = m_IncomingPieceQueue.Dequeue();
                m_IncomingPieceQueue.Enqueue(m_Randomizer.GetNextPiece(m_Field));
            }
            else
            {
                // current piece is the next piece on the screen, so we swap hold piece and the current one here.
                Piece tempPiece;
                tempPiece = m_CurrentPiece;
                m_CurrentPiece = m_HoldPiece;
                m_HoldPiece = tempPiece;
            }
            UpdatePiecePreview();
        }
        else
        {
            // normal hold
            if (m_HoldPiece == null)
            {
                // hold the current piece
                m_HoldPiece = m_CurrentPiece;
                m_HoldPiece.Reset();
                // fetch a new piece from the incoming queue
                m_CurrentPiece = m_IncomingPieceQueue.Dequeue();
                m_IncomingPieceQueue.Enqueue(m_Randomizer.GetNextPiece(m_Field));
            }
            else
            {
                Piece tempPiece;
                // swap between hold piece and current piece
                tempPiece = m_CurrentPiece;
                m_CurrentPiece = m_HoldPiece;
                m_HoldPiece = tempPiece;
                m_HoldPiece.Reset();
            }

            // Since current piece is spawned, we destroy blocks related to current piece
            for (int i = 0; i < m_BlockCurrentPiece.Length; i++)
            {
                Destroy(m_BlockCurrentPiece[i]);
                m_BlockCurrentPiece[i] = null;
            }
            // we also destroy corresponding hint blocks.
            if (isHintShown)
            {
                for (int i = 0; i < m_BlockHint.Length; i++)
                {
                    Destroy(m_BlockHint[i]);
                    m_BlockHint[i] = null;
                }
            }
            UpdatePiecePreview();
            if (!SpawnPiece())
            {
                isGameOver = true;
            }
        }
        return true;
    }

    // add garbage row
    public void DoAddGarbageRow(int rowCount)
    {
        m_PendingLines += rowCount;
        UpdatePendingLines();
    }

    // Main logic of the game
    IEnumerator HandleGame()
    {
        while (true)
        {
            // reset flags
            isPieceSpawned = false;
            isInitialRotationPerformed = false;
            isHoldPerformed = false;

            // spawn delay
            yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(PieceSpawnDelay));

            // fetch a piece from the queue
            m_CurrentPiece = m_IncomingPieceQueue.Dequeue();
            m_IncomingPieceQueue.Enqueue(m_Randomizer.GetNextPiece(m_Field));

            // yield return here so that input coroutines can perform initial rotation/hold here.
            yield return null;

            UpdatePiecePreview();

            m_coPieceDrop = StartCoroutine(HandlePieceDrop());

            // drop the piece first
            yield return null;

            //spawn the piece
            if (!SpawnPiece())
            {
                isGameOver = true;
            }
            if (isGameOver)
            {
                //if spawn fails, the game is over.
                yield return GameOver();
                break;
            }



            isPieceSpawned = true;
            // since piece has been spawned, initial rotation should not performed
            isInitialRotationPerformed = true;

            while (m_CurrentPiece.CurrentState != Piece.State.Fixed)
            {
                if (m_CurrentPiece.CurrentState == Piece.State.Fixing)
                {

                    if (!coWaitLockDelay.IsRunning && !coWaitLockDelay.IsCompleted)
                    {
                        coWaitLockDelay.Start();
                    }

                    if (coWaitLockDelay.IsCompleted)
                    {
                        // Fix the piece onto the field.
                        m_CurrentPiece.Fix();
                        StopCoroutine(m_coPieceDrop);
                        coWaitLockDelay.Reset();
                        // update animation
                        BlockAnimation.SetFixed(ref m_BlockCurrentPiece);

                        // apply piece to the field
                        Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
                        for (int i = 0; i < m_BlockCurrentPiece.Length; i++)
                        {
                            m_BlockView[blockPos[i].y, blockPos[i].x] = m_BlockCurrentPiece[i];
                            m_BlockCurrentPiece[i].GetComponent<SpriteRenderer>().color = new Color32(192, 192, 192, 255);
                        }


                        // destroy hint blocks
                        if (isHintShown)
                        {
                            for (int i = 0; i < m_BlockHint.Length; i++)
                            {
                                Destroy(m_BlockHint[i]);
                                m_BlockHint[i] = null;
                            }
                        }
                    }
                }
                yield return null;
            }
            // now the piece is fixed, we check if related rows can be cleared
            bool isLineCleared = CheckRowClear();
            // arrange the field
            if (isLineCleared)
            {
                yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(LineClearDelay));
                UpdateField();
            }

            yield return null;

            // check pending lines
            if (m_PendingLines > 0)
            {
                m_Field.AddGarbageRow(m_PendingLines);
                m_PendingLines = 0;
                UpdatePendingLines();
                UpdateField();
            }
        }
    }

    private bool CheckRowClear()
    {
        bool isRowCleared = false;
        int rowCount = 0;
        Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
        List<int> rowID = new List<int>();
        List<int> clearRow = new List<int>();
        foreach (Point point in blockPos)
        {
            if (!rowID.Contains(point.y))
            {
                rowID.Add(point.y);
            }
        }
        // check related rows can be cleared
        foreach (int row in rowID)
        {
            if (m_Field.CheckRow(row))
            {
                rowCount++;
                isRowCleared = true;
                clearRow.Add(row);
                // destroy the corresponding lines on the screen
                for (int i = 0; i < m_Field.Width; i++)
                {
                    Destroy(m_BlockView[row, i]);
                    m_BlockView[row, i] = null;
                }
            }
        }
        // judge row clear type (single/double/tetris/t-spin/etc...)
        if (isRowCleared)
        {
            LineClearEvent clearEvent = LineClearEvent.GetLineClearEvent(rowCount, m_CurrentPiece);
            //Debug.Log("");
            Debug.Assert(clearEvent != null);
            Debug.Log(clearEvent.GetFullTypeString());
        }

        // clear row on field
        if (isRowCleared)
        {
            foreach (int row in clearRow)
            {
                m_Field.ClearRow(row);
            }
            m_Field.CollapseRows();
        }
        return isRowCleared;
    }

    // handle drop of the piece
    IEnumerator HandlePieceDrop()
    {
        while (true)
        {
            // We regard a piece drop one unit during a frame as 1G. 
            // When the piece drops at the speed of 1G, the corresponding internal gravity value is 65536.

            // how many units will drop during a drop operation.
            int dropGravity = PieceDropInternalGravity / gravityUnit == 0 ? 1 : PieceDropInternalGravity / gravityUnit;
            // drop interval, in frames.
            int dropInterval = gravityUnit / PieceDropInternalGravity == 0 ? 1 : gravityUnit / PieceDropInternalGravity;
            if (isPieceSpawned && m_CurrentPiece.CurrentState != Piece.State.Fixed)
            {
                // drop the piece
                m_CurrentPiece.MoveDown(dropGravity);

                // update to screen 
                UpdatePiecePosition();

            }
            yield return StartCoroutine(FrameWaitCoroutine.WaitForFrames(dropInterval));
        }
    }

}