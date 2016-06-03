using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameMenu : MonoBehaviour
{

    // bad hard-coded code, need further improvements!

    public GameObject m_MenuTitle;
    public GameObject[] m_MenuItems;
    public bool IsVisible { get; private set; }
    private UIManager uiManager;

    private int m_CurrentSelection;
    private static Color32 white = new Color32(255, 255, 255, 255);
    private static Color32 yellow = new Color32(255, 255, 0, 255);

    public void SetVisible(bool isVisible)
    {
        IsVisible = isVisible;
        gameObject.GetComponent<CanvasGroup>().alpha = isVisible ? 1 : 0;
        gameObject.GetComponent<CanvasGroup>().interactable = isVisible;
    }

    public int GetCurrentSelectionIndex()
    {
        return m_CurrentSelection;
    }
    public GameObject GetCurrentSelection()
    {
        return m_MenuItems[m_CurrentSelection];
    }
    public string GetCurrentSelectionText()
    {
        return m_MenuItems[m_CurrentSelection].GetComponent<Text>().text;
    }
    void OnSelectionChange()
    {
        foreach (GameObject item in m_MenuItems)
        {
            item.GetComponent<Text>().color = white;
        }

        m_MenuItems[m_CurrentSelection].GetComponent<Text>().color = yellow;

        uiManager.OnMenuSelectionChanged();
    }
    // Use this for initialization
    void Start()
    {
        m_CurrentSelection = 0;
        uiManager = UIManager.instance;
        OnSelectionChange();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsVisible)
        {
            if (Input.GetButtonDown("Soft Drop"))
            {
                m_CurrentSelection = (m_CurrentSelection + 1) % m_MenuItems.Length;
                OnSelectionChange();
            }
            if (Input.GetButtonDown("Hard Drop"))
            {
                m_CurrentSelection = (m_CurrentSelection - 1 + m_MenuItems.Length) % m_MenuItems.Length;
                OnSelectionChange();
            }
        }
    }
}
