using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TetrisEngine
{
    public class LineClearEvent
    {
        private static LineClearEvent m_PrevLineClearEvent;
        public static int ComboCount { get; set; }

        public const string singleLine = "Single ";
        public const string doubleLine = "Double ";
        public const string tripleLine = "Triple ";
        public const string tetris = "Tetris ";
        public const string tSpinSingle = "T-Spin Single ";
        public const string tSpinDouble = "T-Spin Double ";
        public const string tSpinTriple = "T-Spin Triple ";
        public const string backToBack = "Back-to-Back";

        public bool IsBackToBack { get; protected set; }
        public int LineClearCount { get; protected set; }
        protected int basicScore = 0;
        protected int basicLineAttack = 0;
        protected int basicPendingLinesDecrement = 0;
        // reward for performing combo in battle mode
        protected readonly static int[] comboLineAttackReward = { 0, 0, 1, 1, 2, 2, 2, 3, 3, 4, 5 };
        // reward for performing combo in other modes
        protected readonly static int[] comboScoreReward = { 0, 0, 100, 100, 200, 200, 200, 300, 300, 400, 500 };

        public LineClearEvent()
        {
            IsBackToBack = false;
        }

        public int GetScore()
        {
            int score = basicScore;
            score = IsBackToBack ? score * 3 / 2 : score;
            return score + comboScoreReward[ComboCount >= comboScoreReward.Length ? comboScoreReward.Length - 1 : ComboCount];
        }
        public int GetAttackLines()
        {
            int lines = basicLineAttack;
            lines = IsBackToBack ? lines * 3 / 2 : lines;
            return lines + comboLineAttackReward[ComboCount >= comboLineAttackReward.Length ? comboLineAttackReward.Length - 1 : ComboCount];
        }
        public int GetPendingLinesDecrement()
        {
            return basicPendingLinesDecrement;
        }
        public virtual string GetTypeString()
        {
            return "";
        }
        public virtual string GetFullTypeString()
        {
            return "";
        }

        public static LineClearEvent GetLineClearEvent(int lineCount, Piece currentPiece)
        {
            LineClearEvent lineClearEvent = null;
            bool isTSpin = false;
            // check if T-Spin occurs
            if (currentPiece.GetType().Name == "TPiece")
            {
                isTSpin = false;
                // center block of the T block is the 2nd one
                Point center = currentPiece.CurrentBlockPos[1];
                Field field = currentPiece.ParentField;
                if (currentPiece.isLastOperationRotation)
                {
                    // check corners around rotation center
                    int count = 0;
                    if (!field.IsSpaceAvailable(center.x - 1, center.y + 1)) count++; // left-top
                    if (!field.IsSpaceAvailable(center.x + 1, center.y + 1)) count++; // right-top
                    if (!field.IsSpaceAvailable(center.x + 1, center.y - 1)) count++; // right-bottom
                    if (!field.IsSpaceAvailable(center.x - 1, center.y - 1)) count++; // left-bottom
                    if (count >= 3)
                    {
                        isTSpin = true;
                    }
                }
            }
            // normal
            if (lineCount == 1)
            {
                lineClearEvent = new SingleLine();
            }
            if (lineCount == 2)
            {
                lineClearEvent = new DoubleLine();
            }
            if (lineCount == 3)
            {
                lineClearEvent = new TripleLine();
            }
            if (lineCount == 4)
            {
                lineClearEvent = new TetrisLine();
            }
            // t-spin
            if (lineCount == 1 && isTSpin)
            {
                lineClearEvent = new TSpinSingle();
            }
            if (lineCount == 2 && isTSpin)
            {
                lineClearEvent = new TSpinDouble();
            }
            if (lineCount == 3 && isTSpin)
            {
                lineClearEvent = new TSpinTriple();
            }

            // other situations
            if (lineClearEvent == null)
            {
                lineClearEvent = new LineClearEvent();
            }

            // check back-to-back
            bool isBackToBack = false;
            if (m_PrevLineClearEvent != null)
            {
                isBackToBack = m_PrevLineClearEvent.GetTypeString() == tetris ||
                m_PrevLineClearEvent.GetTypeString() == tSpinSingle ||
                m_PrevLineClearEvent.GetTypeString() == tSpinDouble ||
                m_PrevLineClearEvent.GetTypeString() == tSpinTriple;
            }

            if ((lineClearEvent.GetTypeString() == tetris ||
                lineClearEvent.GetTypeString() == tSpinSingle ||
                lineClearEvent.GetTypeString() == tSpinDouble ||
                lineClearEvent.GetTypeString() == tSpinTriple) && isBackToBack)
            {
                lineClearEvent.IsBackToBack = true;
            }
            lineClearEvent.LineClearCount = lineCount;
            // record current line clear type
            m_PrevLineClearEvent = lineClearEvent;

            return lineClearEvent;
        }
    }

    class SingleLine : LineClearEvent
    {
        public SingleLine()
        {
            basicLineAttack = 0;
            basicScore = 100;
            basicPendingLinesDecrement = 1;
        }
        public override string GetTypeString()
        {
            return singleLine;
        }
        public override string GetFullTypeString()
        {
            return GetTypeString() + (ComboCount > 1 ? (" " + ComboCount.ToString() + " Combo") : "");
        }
    }
    class DoubleLine : LineClearEvent
    {
        public DoubleLine()
        {
            basicLineAttack = 1;
            basicScore = 200;
            basicPendingLinesDecrement = 2;
        }
        public override string GetTypeString()
        {
            return doubleLine;
        }
        public override string GetFullTypeString()
        {
            return GetTypeString() + (ComboCount > 1 ? (" " + ComboCount.ToString() + " Combo") : "");
        }
    }
    class TripleLine : LineClearEvent
    {
        public TripleLine()
        {
            basicLineAttack = 2;
            basicScore = 400;
            basicPendingLinesDecrement = 3;
        }
        public override string GetTypeString()
        {
            return tripleLine;
        }
        public override string GetFullTypeString()
        {
            return GetTypeString() + (ComboCount > 1 ? (" " + ComboCount.ToString() + " Combo") : "");
        }
    }
    class TetrisLine : LineClearEvent
    {
        public TetrisLine()
        {
            basicLineAttack = 4;
            basicScore = 800;
            basicPendingLinesDecrement = 4;
        }
        public override string GetTypeString()
        {
            return tetris;
        }
        public override string GetFullTypeString()
        {
            return IsBackToBack ? GetTypeString() + backToBack : GetTypeString() + (ComboCount > 1 ? (" " + ComboCount.ToString() + " Combo") : "");
        }
    }
    class TSpinSingle : LineClearEvent
    {
        public TSpinSingle()
        {
            basicLineAttack = 2;
            basicScore = 300;
            basicPendingLinesDecrement = 2;
        }
        public override string GetTypeString()
        {
            return tSpinSingle;
        }
        public override string GetFullTypeString()
        {
            return IsBackToBack ? GetTypeString() + backToBack : GetTypeString() + (ComboCount > 1 ? (" " + ComboCount.ToString() + " Combo") : "");
        }
    }
    class TSpinDouble : LineClearEvent
    {
        public TSpinDouble()
        {
            basicLineAttack = 4;
            basicScore = 600;
            basicPendingLinesDecrement = 4;
        }
        public override string GetTypeString()
        {
            return tSpinDouble;
        }
        public override string GetFullTypeString()
        {
            return IsBackToBack ? GetTypeString() + backToBack : GetTypeString() + (ComboCount > 1 ? (" " + ComboCount.ToString() + " Combo") : "");
        }
    }
    class TSpinTriple : LineClearEvent
    {
        public TSpinTriple()
        {
            basicLineAttack = 6;
            basicScore = 1200;
            basicPendingLinesDecrement = 6;
        }
        public override string GetTypeString()
        {
            return tSpinTriple;
        }
        public override string GetFullTypeString()
        {
            return IsBackToBack ? GetTypeString() + backToBack : GetTypeString() + (ComboCount > 1 ? (" " + ComboCount.ToString() + " Combo") : "");
        }
    }
}
