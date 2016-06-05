using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TetrisEngine;

namespace GamePlayMode
{
    class NormalMode : GameMode
    {
        private int m_Level;
        private int m_Lines;
        private int m_Score;

        private FrameCounter m_frameCounter;

        public NormalMode()
        {
            m_Level = 1;
            m_Lines = 0;
            m_Score = 0;
            m_frameCounter = new FrameCounter();
        }
        public override IEnumerator HandleGameMode()
        {
            yield return null;
        }
        public override void OnFrameUpdated()
        {

        }
        public override void OnGameStart()
        {
            SetLevel(m_Level);
            UpdateGamePanel();
        }
        public override void OnPieceSpawned()
        {
            m_frameCounter.Reset();
            m_frameCounter.Start();
        }
        public override void OnPieceFixed()
        {
            int frameCount = m_frameCounter.Count();
            float seconds = frameCount / 60.0f;
            int scoreAdd = 40 - 4 * (int)seconds;
            m_Score += scoreAdd <= 0 ? 0 : scoreAdd * m_Level;
            UpdateGamePanel();
        }
        public override void OnLineClear(LineClearEvent clearEvent)
        {
            m_Lines += clearEvent.LineClearCount;
            m_Score += clearEvent.GetScore() * m_Level;

            // check level

            int newLevel = m_Lines / 10 + 1;
            if(newLevel > m_Level)
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
                    GameManager.instance.PieceDropInternalGravity = 1024;
                    GameManager.instance.PieceSpawnDelay = 30;
                    GameManager.instance.AutoShiftDelay = 14;
                    GameManager.instance.PieceLockDelay = 30;
                    GameManager.instance.LineClearDelay = 40;
                    break;
                case 2:
                    GameManager.instance.PieceDropInternalGravity = 2048;
                    break;
                case 3:
                    GameManager.instance.PieceDropInternalGravity = 3072;
                    break;
                case 4:
                    GameManager.instance.PieceDropInternalGravity = 4096;
                    break;
                case 5:
                    GameManager.instance.PieceDropInternalGravity = 8192;
                    break;
                case 6:
                    GameManager.instance.PieceDropInternalGravity = 12288;
                    break;
                case 7:
                    GameManager.instance.PieceDropInternalGravity = 16384;
                    break;
                case 8:
                    GameManager.instance.PieceDropInternalGravity = 32768;
                    break;
                case 9:
                    GameManager.instance.PieceDropInternalGravity = 49152;
                    break;
                case 10:
                    // 1G
                    GameManager.instance.PieceDropInternalGravity = 65536;
                    break;
                case 11:
                    GameManager.instance.PieceDropInternalGravity = 131072;
                    break;
                case 12:
                    GameManager.instance.PieceDropInternalGravity = 196608;
                    break;
                case 13:
                    GameManager.instance.PieceDropInternalGravity = 262144;
                    break;
                case 14:
                    GameManager.instance.PieceDropInternalGravity = 327680;
                    break;
                case 15:
                    // 20G
                    GameManager.instance.PieceDropInternalGravity = 1310720;
                    GameManager.instance.AutoShiftDelay = 8;
                    GameManager.instance.LineClearDelay = 25;
                    break;
                case 16:
                    GameManager.instance.PieceDropInternalGravity = 1310720;
                    GameManager.instance.LineClearDelay = 16;
                    break;
                case 17:
                    GameManager.instance.PieceDropInternalGravity = 1310720;
                    GameManager.instance.PieceSpawnDelay = 16;
                    GameManager.instance.LineClearDelay = 12;
                    break;
                case 18:
                    GameManager.instance.PieceDropInternalGravity = 1310720;
                    GameManager.instance.PieceSpawnDelay = 12;
                    GameManager.instance.LineClearDelay = 6;
                    GameManager.instance.PieceLockDelay = 17;
                    break;
                case 19:
                    GameManager.instance.PieceDropInternalGravity = 1310720;
                    GameManager.instance.PieceSpawnDelay = 6;
                    GameManager.instance.LineClearDelay = 6;
                    GameManager.instance.PieceLockDelay = 17;
                    break;
                case 20:
                    GameManager.instance.PieceDropInternalGravity = 1310720;
                    GameManager.instance.PieceSpawnDelay = 4;
                    GameManager.instance.LineClearDelay = 6;
                    GameManager.instance.PieceLockDelay = 15;
                    break;
            }
            m_Level = level;
        }
    }
}
