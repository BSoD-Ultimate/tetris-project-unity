using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TetrisEngine;

namespace GamePlayMode
{
    class AdvanceMode : GameMode
    {
        private int m_Level;
        private int m_Lines;
        private int m_Score;
        private int m_GarbageRowCount;

        private int m_AddGarbageRowInterval;// in frames

        public AdvanceMode()
        {
            m_Level = 1;
            m_Lines = 0;
            m_Score = 0;
            m_GarbageRowCount = 0;
            m_AddGarbageRowInterval = 600;
        }

        public override IEnumerator HandleGameMode()
        {
            while (true)
            {
                yield return GameManager.instance.StartCoroutine(FrameWaitCoroutine.WaitForFrames(m_AddGarbageRowInterval));
                GameManager.instance.DoAddPendingLines(1);
                m_GarbageRowCount++;
            }
        }
        public override void OnFrameUpdated()
        {

        }
        public override void OnGameStart()
        {
            GameManager.instance.PieceDropInternalGravity = 1024;
            GameManager.instance.PieceSpawnDelay = 10;
            GameManager.instance.AutoShiftDelay = 14;
            GameManager.instance.PieceLockDelay = 30;
            GameManager.instance.LineClearDelay = 6;
            SetLevel(m_Level);

            UpdateGamePanel();
        }
        public override void OnPieceSpawned()
        {

        }
        public override void OnPieceFixed()
        {

        }
        public override void OnLineClear(LineClearEvent clearEvent)
        {
            m_Lines += clearEvent.LineClearCount;
            m_Score += clearEvent.GetAttackLines();

            // update level
            int newLevel = m_GarbageRowCount / 10 + 1;
            if (newLevel > m_Level)
            {
                SetLevel(newLevel);
                m_Level = newLevel;
            }

            UpdateGamePanel();
        }
        public override void OnGameOver()
        {

        }
        private void UpdateGamePanel()
        {
            UIManager.instance.SetLines(m_Lines);
            UIManager.instance.SetScore(m_Score);
            UIManager.instance.SetLevel(m_Level);
        }
        private void SetLevel(int level)
        {
            switch (level)
            {
                default:
                    break;
                case 1:
                    m_AddGarbageRowInterval = 600;
                    break;
                case 2:
                    m_AddGarbageRowInterval = 570;
                    break;
                case 3:
                    m_AddGarbageRowInterval = 480;
                    break;
                case 4:
                    m_AddGarbageRowInterval = 450;
                    break;
                case 5:
                    m_AddGarbageRowInterval = 420;
                    break;
                case 6:
                    m_AddGarbageRowInterval = 330;
                    break;
                case 7:
                    m_AddGarbageRowInterval = 300;
                    break;
                case 8:
                    m_AddGarbageRowInterval = 250;
                    break;
                case 9:
                    m_AddGarbageRowInterval = 100;
                    break;
                case 10:
                    m_AddGarbageRowInterval = 60;
                    break;
            }
            m_Level = level;
        }
    }
}
