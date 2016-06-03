using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TetrisEngine;

public class UIManager : MonoBehaviour
{
    public enum State { Idle, OnSelectMode, OnGameRunning, OnBattleWait };
    public State CurrentState { get; private set; }

    public static UIManager instance = null;

    public GameObject m_PendingLinesPanel;
    public GameObject m_RowClearTypePanel;
    public GameObject m_TextLines;
    public GameObject m_TextScore;
    public GameObject m_TextLevel;

    public GameObject m_PendingLinesPanelOpponent;
    public GameObject m_RowClearTypePanelOpponent;

    public GameObject m_GameModePanel;

    public GameObject m_TextGameStatus;
    private Coroutine coShowStatusText;

    public GameObject m_FieldFrame;

    private readonly static Color32 red = new Color32(255, 0, 0, 255);
    private readonly static Color32 blue = new Color32(0, 0, 255, 255);
    private readonly static Color32 white = new Color32(255, 255, 255, 255);

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    // Use this for initialization
    void Start()
    {
        CurrentState = State.Idle;
        SetGameModePanelVisible(false);
        ShowStatusText(-1);
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState != State.OnGameRunning)
        {
            if (Input.GetButtonDown("Rotate CCW"))
            {
                DoSubmit();
            }
            if (Input.GetButtonDown("Rotate CW"))
            {
                DoCancel();
            }
        }
    }

    public void DoSubmit()
    {
        if (CurrentState == State.Idle)
        {
            SetSelectModeState();
        }
        else if (CurrentState == State.OnSelectMode)
        {
            SetGameModePanelVisible(false);
            int currentMenuIndex = m_GameModePanel.GetComponent<GameMenu>().GetCurrentSelectionIndex();
            switch (currentMenuIndex)
            {
                case 0:
                    // normal
                    GameManager.instance.StartGame();
                    break;
                default:
                    return;
                    break;
            }
            CurrentState = State.OnGameRunning;
        }
    }

    public void DoCancel()
    {

    }

    public void OnMenuSelectionChanged()
    {
        int currentIndex = m_GameModePanel.GetComponent<GameMenu>().GetCurrentSelectionIndex();
        switch (currentIndex)
        {
            case 1:
                // advance
                SetFieldFrameColor(blue);
                break;
            case 2:
                // sudden
                SetFieldFrameColor(red);
                break;
            default:
                SetFieldFrameColor(white);
                break;
        }
    }

    public void SetIdleState()
    {
        SetFieldFrameColor(white);
        SetStatusText("Press Any Button");
        SetGameModePanelVisible(false);
        ShowStatusText(-1);
        CurrentState = State.Idle;
    }
    public void SetSelectModeState()
    {
        StopShowStatusText();
        SetStatusTextVisible(false);
        SetGameModePanelVisible(true);
        CurrentState = State.OnSelectMode;
    }
    public void SetBattleWaitState()
    {

    }

    public void SetPendingLines(int lines)
    {
        //m_TextPendingLines.GetComponent<Text>().text = lines.ToString();
        m_PendingLinesPanel.transform.Find("TextPendingLines").gameObject.GetComponent<Text>().text = lines.ToString();
    }
    public void SetLines(int lines)
    {
        m_TextLines.GetComponent<Text>().text = lines.ToString();
    }
    public void SetScore(int score)
    {
        m_TextScore.GetComponent<Text>().text = score.ToString();
    }
    public void SetLevel(int level)
    {
        m_TextLevel.GetComponent<Text>().text = level.ToString();
    }
    public void SetRowClearType(string type)
    {
        m_RowClearTypePanel.transform.Find("TextLineClearType").gameObject.GetComponent<Text>().text = type;
    }
    public void SetBackToBack(bool isBackToBack)
    {
        m_RowClearTypePanel.transform.Find("TextBacktoBack").gameObject.GetComponent<Text>().text = isBackToBack ? "Back-To-Back" : "";
    }
    public void SetComboCount(int comboCount)
    {
        m_RowClearTypePanel.transform.Find("TextComboCount").gameObject.GetComponent<Text>().text = comboCount > 1 ? comboCount.ToString() + "combo" : "";
    }
    public void SetGameModePanelVisible(bool isVisible)
    {
        m_GameModePanel.GetComponent<GameMenu>().SetVisible(isVisible);
    }
    public void SetStatusText(string text)
    {
        m_TextGameStatus.GetComponent<Text>().text = text;
    }
    public void SetStatusTextVisible(bool isVisible)
    {
        m_TextGameStatus.GetComponent<CanvasGroup>().alpha = isVisible ? 1 : 0;
        m_TextGameStatus.GetComponent<CanvasGroup>().interactable = isVisible;
    }
    public void SetFieldFrameColor(Color32 color)
    {
        m_FieldFrame.GetComponent<SpriteRenderer>().color = color;
    }


    public void ShowStatusText(int showTimes)
    {
        coShowStatusText = StartCoroutine(HandleShowStatusText(showTimes));
    }
    public void StopShowStatusText()
    {
        StopCoroutine(coShowStatusText);
    }

    IEnumerator HandleShowStatusText(int showTimes)
    {
        for (int i = 0; i < showTimes || showTimes == -1; i++)
        {
            SetStatusTextVisible(true);
            yield return new WaitForSeconds(1f);
            SetStatusTextVisible(false);
            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }

    public void ShowRowClearType(LineClearEvent clearEvent)
    {
        StartCoroutine(HandleShowRowClearMessage(clearEvent));
    }
    IEnumerator HandleShowRowClearMessage(LineClearEvent clearEvent)
    {
        SetRowClearType(clearEvent.GetTypeString());
        SetBackToBack(clearEvent.IsBackToBack);
        SetComboCount(LineClearEvent.ComboCount);
        yield return new WaitForSeconds(0.5f);
        SetRowClearType("");
        SetBackToBack(false);
        SetComboCount(0);
        yield return null;
    }
}
