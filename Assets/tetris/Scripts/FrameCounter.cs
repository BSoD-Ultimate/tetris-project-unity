using UnityEngine;
using System.Collections;

public class FrameCounter
{
    private int m_FrameCountStart;

    public FrameCounter()
    {
        m_FrameCountStart = -1;
    }

    public void Start()
    {
        m_FrameCountStart = Time.frameCount;
    }

    public void Reset()
    {
        m_FrameCountStart = -1;
    }

    public int Count()
    {
        return m_FrameCountStart != -1 ? Time.frameCount - m_FrameCountStart : 0;
    }
}
