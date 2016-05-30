using UnityEngine;
using System.Collections;

public delegate void WaitAction(object param);

public class FrameWaitCoroutine
{
    private MonoBehaviour m_AttachedScript = null;
    private FrameCounter m_Counter;
    private Coroutine m_WaitCoroutine = null;
    public bool IsRunning { get; private set; }
    public bool IsCompleted
    {
        get
        {
            return GetCount() >= WaitFrameCount;
        }
    }
    public WaitAction Action { get; set; }
    public int WaitFrameCount { get; set; }

    public FrameWaitCoroutine(MonoBehaviour script)
    {
        m_Counter = new FrameCounter();
        m_AttachedScript = script;
        IsRunning = false;
    }
    public int GetCount()
    {
        return m_Counter.Count();
    }
    public void Start(object param = null)
    {
        m_WaitCoroutine = m_AttachedScript.StartCoroutine(Wait(param));
    }
    public void Stop()
    {
        if (m_WaitCoroutine != null)
        {
        	m_AttachedScript.StopCoroutine(m_WaitCoroutine);
        }
        IsRunning = false;
    }
    public void Reset()
    {
        Stop();
        m_Counter.Reset();
    }

    public IEnumerator Wait(object param = null)
    {
        IsRunning = true;
        m_Counter.Reset();
        m_Counter.Start();
        while (m_Counter.Count() < WaitFrameCount)
        {
            if (Action != null)
            {
                Action(param);
            }
            yield return null;
        }
        IsRunning = false;
        yield return null;
    }


    public static IEnumerator WaitForFrames(int frameCount, WaitAction action = null, object param = null)
    {
        FrameCounter counter = new FrameCounter();
        counter.Reset();
        counter.Start();
        while (counter.Count() < frameCount)
        {
            if (action != null)
            {
                action(param);
            }
            yield return null;
        }
    }
}
