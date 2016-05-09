using UnityEngine;
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

    // gameobjects which show hold and next piece
    public GameObject[] m_NextPieceShow;
    public GameObject m_HoldPieceShow;

    // blocks on the scene
    private GameObject[,] m_BlockView;
    // current piece
    private GameObject[] m_BlockCurrentPiece;

    // coroutine controls piece drop
    private Coroutine m_coPieceDrop;

    #region engine
    private Field m_Field;
    public Piece m_CurrentPiece = null;
    private Piece m_HoldPiece = null;
    private Queue<Piece> m_IncomingPieceQueue;
    private BlockRandomizer m_Randomizer;
    #endregion

    #region properties
    public float AutoShiftDelay
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
    public float AutoShiftInterval
    {
        get
        {
            Debug.Assert(inputManager != null);
            return inputManager.AutoShiftSpeed;
        }
        set
        {
            Debug.Assert(inputManager != null);
            inputManager.AutoShiftSpeed = value;
        }
    }
    public float SoftDropInterval
    {
        get
        {
            Debug.Assert(inputManager != null);
            return inputManager.SoftDropSpeed;
        }
        set
        {
            Debug.Assert(inputManager != null);
            inputManager.SoftDropSpeed = value;
        }
    }
    public float PieceDropInterval { get; set; }
    public int PieceDropGravity { get; set; }
    public float PieceSpawnDelay { get; set; }
    public float PieceLockDelay { get; set; }
    public float LineClearDelay { get; set; }
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
    private WaitCoroutine coWaitPieceFix;
    #endregion

    // a wrapper controls block's animation
    class BlockAnimation
    {
        public static void SetUnfixed(ref GameObject[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].GetComponent<Animator>().SetBool("TouchGround", false);
            }
        }
        public static void SetFixing(ref GameObject[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                //if(!blocks[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("red_block_fixing"))
                //{
                    blocks[i].GetComponent<Animator>().SetBool("TouchGround", true);
               // }
            }
        }
        public static void ResetFixing(ref GameObject[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].GetComponent<Animator>().SetTrigger("CountdownReset");
            }
        }
        public static void SetFixed(ref GameObject[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                //if (!blocks[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("red_block_fixed"))
                //{
                    blocks[i].GetComponent<Animator>().SetBool("Fixed", true);
                //}
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

        // public properties
        AutoShiftDelay = 0.25f;
        AutoShiftInterval = 0.015f;
        SoftDropInterval = 0.01f;
        PieceDropInterval = 0.5f;
        PieceDropGravity = 1;
        PieceSpawnDelay = 0.4f;
        PieceLockDelay = 0.5f;
        LineClearDelay = 0.5f;

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

        coWaitPieceFix = new WaitCoroutine(this);

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
            m_HoldPieceShow.GetComponent<SpriteRenderer>().sprite = m_MapPieceSprite[m_HoldPiece.ToString()];
        }
        else
        {
            m_HoldPieceShow.GetComponent<SpriteRenderer>().sprite = null;
        }
        // next
        for (int i = 0; i < m_NextPieceShow.Length; i++)
        {
            string pieceName = m_IncomingPieceQueue.ElementAt(i).GetType().Name;
            m_NextPieceShow[i].GetComponent<SpriteRenderer>().sprite = m_MapPieceSprite[pieceName];
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

    public Piece GetCurrentPiece()
    {
        return m_CurrentPiece;
    }

    public bool SpawnPiece()
    {
        Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
        m_BlockCurrentPiece = new GameObject[blockPos.Length];
        // spawn the piece on the screen
        for (int i = 0; i < blockPos.Length; i++)
        {
            GameObject block = m_MapBlockType[m_CurrentPiece.PieceStyle];
            Vector3 position = new Vector3(blockPos[i].x, blockPos[i].y);
            m_BlockCurrentPiece[i] = (GameObject)Instantiate(block, position, Quaternion.identity);
        }

        // check if spawn operation is successful
        return m_CurrentPiece.CheckSpawn();

    }

    void GameOver()
    {
        Debug.Log("Game Over");
    }

    private void CheckPieceState()
    {
        if (m_CurrentPiece.CurrentState == Piece.State.Fixing)
        {
            if (m_CurrentPiece.FixResetCount <= Piece.FixCountdownResetMaxCount)
            {
                // reset the fix countdown timer
                coWaitPieceFix.Stop();
                coWaitPieceFix.Reset();
                coWaitPieceFix.Start(PieceLockDelay);
                // reset the animation
                BlockAnimation.ResetFixing(ref m_BlockCurrentPiece);
            }
            if (m_CurrentPiece.TryMoveDown())
            {
                BlockAnimation.SetUnfixed(ref m_BlockCurrentPiece);
            }
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
                CheckPieceState();
                return true;
            }
        }
        return false;
    }
    //public bool DoSoftDrop()
    //{

    //}
    public bool DoHardDrop()
    {
        if (isPieceSpawned && m_CurrentPiece.CurrentState != Piece.State.Fixed)
        {
            m_CurrentPiece.HardDrop();
            UpdatePiecePosition();
            BlockAnimation.SetFixed(ref m_BlockCurrentPiece);
            StopCoroutine(m_coPieceDrop);
            // stop the countdown timer
            coWaitPieceFix.Stop();
            coWaitPieceFix.Reset();

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
            if (!isInitialRotationPerformed)
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
            if (!isInitialRotationPerformed)
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
                    CheckPieceState();
                }
                return success;
            }
            return false;
        }
    }
    //public bool DoHoldPiece()
    //{

    //}
    // Main logic of the game
    IEnumerator HandleGame()
    {
        while (true)
        {
            // reset flags
            isPieceSpawned = false;
            isInitialRotationPerformed = false;
            isHoldPerformed = false;

            // fetch a piece from the queue
            m_CurrentPiece = m_IncomingPieceQueue.Dequeue();
            m_IncomingPieceQueue.Enqueue(m_Randomizer.GetNextPiece(m_Field));

            yield return new WaitForSeconds(PieceSpawnDelay);

            UpdatePiecePreview();

            m_coPieceDrop = StartCoroutine(HandlePieceDrop());

            //spawn the piece
            if (!SpawnPiece())
            {
                //if spawn fails, the game is over.
                GameOver();
                break;
            }

            isPieceSpawned = true;

            while (m_CurrentPiece.CurrentState != Piece.State.Fixed)
            {
                if (m_CurrentPiece.CurrentState == Piece.State.Fixing)
                {
                    // update animation
                    BlockAnimation.SetFixing(ref m_BlockCurrentPiece);

                    if (!coWaitPieceFix.IsRunning && !coWaitPieceFix.IsCompleted)
                    {
                        coWaitPieceFix.Start(PieceLockDelay);
                    }

                    if (coWaitPieceFix.IsCompleted)
                    {
                        // Fix the piece onto the field.
                        m_CurrentPiece.Fix();
                        StopCoroutine(m_coPieceDrop);
                        coWaitPieceFix.Reset();
                        // update animation
                        BlockAnimation.SetFixed(ref m_BlockCurrentPiece);

                        // apply piece to the field.
                        Point[] blockPos = m_CurrentPiece.CurrentBlockPos;
                        for (int i = 0; i < m_BlockCurrentPiece.Length; i++)
                        {
                            m_BlockView[blockPos[i].y, blockPos[i].x] = m_BlockCurrentPiece[i];
                        }

                        // check if related lines can be cleared
                        bool isLineCleared = false;
                        foreach (Point point in blockPos)
                        {
                            if (m_Field.CheckRow(point.y))
                            {
                                isLineCleared = true;
                                m_Field.ClearRow(point.y);
                                // destroy the corresponding lines on the screen
                                for (int i = 0; i < m_Field.Width; i++)
                                {
                                    Destroy(m_BlockView[point.y, i]);
                                    m_BlockView[point.y, i] = null;
                                }
                            }
                        }
                        // arrange the field
                        if (isLineCleared)
                        {
                            yield return new WaitForSeconds(LineClearDelay);
                            UpdateField();
                        }


                    }
                }

                yield return null;
            }

            yield return null;
        }
    }

    // handle drop of the piece
    IEnumerator HandlePieceDrop()
    {
        while (true)
        {
            if (isPieceSpawned && m_CurrentPiece.CurrentState != Piece.State.Fixed)
            {
                // drop the piece
                m_CurrentPiece.MoveDown(PieceDropGravity);

                // update to screen 
                UpdatePiecePosition();

            }
            yield return new WaitForSeconds(PieceDropInterval);
        }
    }

}

class WaitCoroutine
{
    private bool isRunning = false;
    private bool isCompleted = false;

    private MonoBehaviour m_AttachedScript = null;
    private Coroutine m_WaitCoroutine = null;

    public WaitCoroutine(MonoBehaviour script)
    {
        m_AttachedScript = script;
    }

    public void Start(float waitForSeconds)
    {
        isCompleted = false;
        m_WaitCoroutine = m_AttachedScript.StartCoroutine(HandleWait(waitForSeconds));
    }

    public void Stop()
    {
        if (m_WaitCoroutine != null)
        {
            m_AttachedScript.StopCoroutine(m_WaitCoroutine);
        }
        isRunning = false;
    }
    public void Reset()
    {
        isRunning = false;
        isCompleted = false;
    }
    public bool IsRunning
    {
        get
        {
            return isRunning;
        }

    }
    public bool IsCompleted
    {
        get
        {
            return isCompleted;
        }
    }
    private IEnumerator HandleWait(float waitForSeconds)
    {
        isCompleted = false;
        isRunning = true;
        yield return new WaitForSeconds(waitForSeconds);
        isCompleted = true;
        isRunning = false;
        yield return null;
    }
}