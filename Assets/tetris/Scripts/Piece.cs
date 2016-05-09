using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TetrisEngine
{
    public abstract class Piece
    {
        public enum State { Unfixed, Fixing, Fixed };
        public State CurrentState { get; protected set; }
        // indicates current shape of the piece.
        // Zero = spawn shape 
        // Right = shape resulting from a clockwise rotation ("right") from spawn 
        // Left = shape resulting from a counter-clockwise ("left") rotation from spawn 
        // Inverse = shape resulting from 2 successive rotations in either direction from spawn
        public enum Shape { Zero, Right, Inverse, Left };
        public Shape CurrentShape { get; protected set; }
        // block type(color) used by current piece.
        protected BlockType m_blockType;
        public BlockType PieceStyle
        {
            get
            {
                return m_blockType;
            }
        }
        // the field which the piece belongs to.
        protected Field m_BindingField;
        // position of all blocks the piece contains.
        protected Point[] m_BlockPos;
        public Point[] CurrentBlockPos
        {
            get
            {
                return CloneBlockPos(m_BlockPos);
            }
        }
        // a dictionary used to perform rotation action
        protected Dictionary<Shape, Dictionary<Shape, Point[]>> m_RotationData;
        // a set of array organized by Dictionary used to move piece into available space when rotating piece.
        protected Dictionary<Shape, Dictionary<Shape, Point[]>> m_WallkickTestOffset;
        // when the block is under fixing state(cannot move down), 
        // this number is used to count times to reset fix countdown timer by moving/rotating blocks.
        protected int m_FixCountdownResetCount;
        public int FixResetCount
        {
            get
            {
                return m_FixCountdownResetCount;
            }
        }
        // default value
        protected static int m_FixCountdownResetMaxCount = 8;
        public static int FixCountdownResetMaxCount
        {
            get
            {
                return m_FixCountdownResetMaxCount;
            }
            set
            {
                m_FixCountdownResetMaxCount = value;
            }
        }

        public Piece(Field bindingField)
        {
            m_BindingField = bindingField;
            CurrentState = State.Unfixed;
            CurrentShape = Shape.Zero;
            m_blockType = BlockType.Empty;
            m_FixCountdownResetCount = 0;

            m_RotationData = new Dictionary<Shape, Dictionary<Shape, Point[]>>();
            m_WallkickTestOffset = new Dictionary<Shape, Dictionary<Shape, Point[]>>();
        }

        protected Point[] CloneBlockPos(Point[] block)
        {
            Point[] clone = new Point[block.Length];
            for (int i = 0; i < m_BlockPos.Length; i++)
            {
                clone[i] = (Point)block[i].Clone();
            }
            return clone;
        }

        protected abstract void SetRotationData();
        protected abstract void SetWallKickData();

        protected abstract void SetSpawnPos();

        public void Reset()
        {
            m_FixCountdownResetCount = 0;
            SetSpawnPos();
        }
        #region PieceBehavior

        // check if the piece can be successfully spawned
        public bool CheckSpawn()
        {
            Debug.Assert(m_BlockPos != null);
            foreach (Point point in m_BlockPos)
            {
                if (!m_BindingField.IsSpaceAvailable(point.x, point.y))
                {
                    return false;
                }
            }
            return true;
        }

        // check if new position can hold the piece
        protected bool IsPlaceAvailable(Point[] pos)
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (!m_BindingField.IsSpaceAvailable(pos[i].x, pos[i].y))
                {
                    return false;
                }
            }
            return true;
        }

        public bool MoveLeft()
        {
            Debug.Assert(m_BlockPos != null);

            Point[] newPos = CloneBlockPos(m_BlockPos);

            foreach (Point point in newPos)
            {
                point.x--;
            }

            if (IsPlaceAvailable(newPos))
            {
                m_BlockPos = newPos;
                if (CurrentState == State.Fixing)
                {
                    m_FixCountdownResetCount++;
                }
                if (TryMoveDown())
                {
                    CurrentState = State.Unfixed;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool MoveRight()
        {
            Debug.Assert(m_BlockPos != null);

            Point[] newPos = CloneBlockPos(m_BlockPos);
            foreach (Point point in newPos)
            {
                point.x++;
            }

            if (IsPlaceAvailable(newPos))
            {
                m_BlockPos = newPos;
                if (CurrentState == State.Fixing)
                {
                    m_FixCountdownResetCount++;
                }
                if (TryMoveDown())
                {
                    CurrentState = State.Unfixed;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool TryMoveDown()
        {
            Debug.Assert(m_BlockPos != null);

            Point[] newPos = CloneBlockPos(m_BlockPos);
            foreach (Point point in newPos)
            {
                point.y--;
            }

            if (IsPlaceAvailable(newPos))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool MoveDown()
        {
            Debug.Assert(m_BlockPos != null);

            Point[] newPos = CloneBlockPos(m_BlockPos);
            foreach (Point point in newPos)
            {
                point.y--;
            }

            if (IsPlaceAvailable(newPos))
            {
                m_BlockPos = newPos;
                CurrentState = State.Unfixed;
                return true;
            }
            else
            {
                CurrentState = State.Fixing;
                return false;
            }
        }
        // move the piece more than one chip at one time.
        // returns actual rows moved
        public int MoveDown(int stepCount)
        {
            bool moveSucceed = false;
            for (int i = 0; i < stepCount; i++)
            {
                moveSucceed = MoveDown();
                if (!moveSucceed)
                {
                    return i;
                }
            }
            return stepCount;
        }

        public void HardDrop()
        {
            MoveDown(m_BindingField.Height);
            Fix();
        }

        public virtual bool RotateCW()
        {
            Shape newShape = (Shape)((int)(CurrentShape + 1) % 4);

            Point[] rotationData = m_RotationData[CurrentShape][newShape];
            Point[] wallkickData = m_WallkickTestOffset[CurrentShape][newShape];

            Debug.Assert(rotationData.Length == m_BlockPos.Length);

            Point[] newPos = CloneBlockPos(m_BlockPos);

            // do rotation
            for (int i = 0; i < newPos.Length; i++)
            {
                newPos[i] += rotationData[i];
            }

            if (IsPlaceAvailable(newPos))
            {
                // apply rotation result and new shape to the old one.
                CurrentShape = newShape;
                m_BlockPos = newPos;
                if (CurrentState == State.Fixing)
                {
                    m_FixCountdownResetCount++;
                }
                if (TryMoveDown())
                {
                    CurrentState = State.Unfixed;
                }
                return true;
            }

            // basic rotation fails, try move piece according to wall kick data.

            foreach (Point offset in wallkickData)
            {
                Point[] wallkickPos = CloneBlockPos(newPos);
                // apply wall kick data
                for (int i = 0; i < wallkickPos.Length; i++)
                {
                    wallkickPos[i] += offset;
                }
                if (IsPlaceAvailable(wallkickPos))
                {
                    CurrentShape = newShape;
                    m_BlockPos = wallkickPos;
                    if (CurrentState == State.Fixing)
                    {
                        m_FixCountdownResetCount++;
                    }
                    if (TryMoveDown())
                    {
                        CurrentState = State.Unfixed;
                    }
                    return true;
                }
            }

            // rotation fails
            return false;
        }
        public virtual bool RotateCCW()
        {
            Shape newShape = (Shape)((int)(CurrentShape - 1) % 4);

            Point[] rotationData = m_RotationData[CurrentShape][newShape];
            Point[] wallkickData = m_WallkickTestOffset[CurrentShape][newShape];

            Debug.Assert(rotationData.Length == m_BlockPos.Length);

            Point[] newPos = CloneBlockPos(m_BlockPos);

            // do rotation
            for (int i = 0; i < newPos.Length; i++)
            {
                newPos[i] += rotationData[i];
            }

            if (IsPlaceAvailable(newPos))
            {
                // apply rotation result and new shape to the old one.
                CurrentShape = newShape;
                m_BlockPos = newPos;
                if (CurrentState == State.Fixing)
                {
                    m_FixCountdownResetCount++;
                }
                if (TryMoveDown())
                {
                    CurrentState = State.Unfixed;
                }
                return true;
            }

            // basic rotation fails, try move piece according to wall kick data.

            foreach (Point offset in wallkickData)
            {
                Point[] wallkickPos = CloneBlockPos(newPos);
                // apply wallkick data
                for (int i = 0; i < wallkickPos.Length; i++)
                {
                    wallkickPos[i] += offset;
                }
                if (IsPlaceAvailable(wallkickPos))
                {
                    CurrentShape = newShape;
                    m_BlockPos = wallkickPos;
                    if (CurrentState == State.Fixing)
                    {
                        m_FixCountdownResetCount++;
                    }
                    if (TryMoveDown())
                    {
                        CurrentState = State.Unfixed;
                    }
                    return true;
                }
            }

            // rotation fails
            return false;
        }

        // fix the piece onto the field
        public void Fix()
        {
            Debug.Assert(m_BlockPos != null);

            foreach (Point point in m_BlockPos)
            {
                BlockChip newChip = new BlockChip();
                newChip.Type = m_blockType;
                m_BindingField[point.y, point.x] = newChip;
            }

            CurrentState = State.Fixed;
        }

        #endregion
    }


    /// <summary>
    /// J Piece
    /// Block Position :
    /// Zero:           Right:          Inverse:        Left:
    /// -------         -------         -------         -------
    /// |3    |         |  0 3|         |     |         |  2  |
    /// |0 1 2|         |  1  |         |2 1 0|         |  1  |
    /// |     |         |  2  |         |    3|         |3 0  |
    /// -------         -------         -------         -------
    /// </summary>
    class JPiece : Piece
    {
        public JPiece(Field bindingField)
            : base(bindingField)
        {
            m_blockType = BlockType.Orange;
            SetSpawnPos();
            SetRotationData();
            SetWallKickData();
        }
        // set the piece's initial state and location.
        protected override void SetSpawnPos()
        {
            m_BlockPos = new Point[4];
            int hCenter = (m_BindingField.Width + 1) / 2 - 1;// center of a row in the field
            int top = m_BindingField.Height - 3; // top line of the field
            m_BlockPos[0] = new Point(hCenter - 1, top - 1);
            m_BlockPos[1] = new Point(hCenter, top - 1);
            m_BlockPos[2] = new Point(hCenter + 1, top - 1);
            m_BlockPos[3] = new Point(hCenter - 1, top + 1);
        }
        /// <summary>
        /// Zero:           Right:          Inverse:        Left:           Zero:
        /// -------         -------         -------         -------         -------
        /// |3    |         |  0 3|         |     |         |  2  |         |3    |
        /// |0 1 2|         |  1  |         |2 1 0|         |  1  |         |0 1 2|
        /// |     |         |  2  |         |    3|         |3 0  |         |     |
        /// -------         -------         -------         -------         -------
        /// </summary>
        protected override void SetRotationData()
        {
            // rotation data
            Point[] ZeroToRight = { new Point(1, 1), new Point(0, 0), new Point(-1, -1), new Point(2, 0) };
            Point[] RightToZero = { new Point(-1, -1), new Point(0, 0), new Point(1, 1), new Point(-2, 0) };
            Point[] RightToInverse = { new Point(1, -1), new Point(0, 0), new Point(-1, 1), new Point(0, -2) };
            Point[] InverseToRight = { new Point(-1, 1), new Point(0, 0), new Point(1, -1), new Point(0, 2) };
            Point[] InverseToLeft = { new Point(-1, -1), new Point(0, 0), new Point(1, 1), new Point(-2, 0) };
            Point[] LeftToInverse = { new Point(1, 1), new Point(0, 0), new Point(-1, -1), new Point(2, 0) };
            Point[] LeftToZero = { new Point(-1, 1), new Point(0, 0), new Point(1, -1), new Point(0, 2) };
            Point[] ZeroToLeft = { new Point(1, -1), new Point(0, 0), new Point(-1, 1), new Point(0, -2) };

            Dictionary<Shape, Point[]> ZeroTransform = new Dictionary<Shape, Point[]>();
            ZeroTransform.Add(Shape.Right, ZeroToRight);
            ZeroTransform.Add(Shape.Left, ZeroToLeft);
            Dictionary<Shape, Point[]> RightTransform = new Dictionary<Shape, Point[]>();
            RightTransform.Add(Shape.Zero, RightToZero);
            RightTransform.Add(Shape.Inverse, RightToInverse);
            Dictionary<Shape, Point[]> InverseTransform = new Dictionary<Shape, Point[]>();
            InverseTransform.Add(Shape.Right, InverseToRight);
            InverseTransform.Add(Shape.Left, InverseToLeft);
            Dictionary<Shape, Point[]> LeftTransform = new Dictionary<Shape, Point[]>();
            LeftTransform.Add(Shape.Zero, LeftToZero);
            LeftTransform.Add(Shape.Inverse, LeftToInverse);

            m_RotationData.Add(Shape.Zero, ZeroTransform);
            m_RotationData.Add(Shape.Right, RightTransform);
            m_RotationData.Add(Shape.Inverse, InverseTransform);
            m_RotationData.Add(Shape.Left, LeftTransform);

        }
        protected override void SetWallKickData()
        {
            // wall kick data
            Point[] ZeroToRight = { new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2) };
            Point[] RightToZero = { new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) };
            Point[] RightToInverse = { new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) };
            Point[] InverseToRight = { new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2) };
            Point[] InverseToLeft = { new Point(1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) };
            Point[] LeftToInverse = { new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, 2) };
            Point[] LeftToZero = { new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, 2) };
            Point[] ZeroToLeft = { new Point(-1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) };

            Dictionary<Shape, Point[]> ZeroOffset = new Dictionary<Shape, Point[]>();
            ZeroOffset.Add(Shape.Right, ZeroToRight);
            ZeroOffset.Add(Shape.Left, ZeroToLeft);
            Dictionary<Shape, Point[]> RightOffset = new Dictionary<Shape, Point[]>();
            RightOffset.Add(Shape.Zero, RightToZero);
            RightOffset.Add(Shape.Inverse, RightToInverse);
            Dictionary<Shape, Point[]> InverseOffset = new Dictionary<Shape, Point[]>();
            InverseOffset.Add(Shape.Right, InverseToRight);
            InverseOffset.Add(Shape.Left, InverseToLeft);
            Dictionary<Shape, Point[]> LeftOffset = new Dictionary<Shape, Point[]>();
            LeftOffset.Add(Shape.Zero, LeftToZero);
            LeftOffset.Add(Shape.Inverse, LeftToInverse);

            m_WallkickTestOffset.Add(Shape.Zero, ZeroOffset);
            m_WallkickTestOffset.Add(Shape.Right, RightOffset);
            m_WallkickTestOffset.Add(Shape.Inverse, InverseOffset);
            m_WallkickTestOffset.Add(Shape.Left, LeftOffset);
        }
    }

    /// <summary>
    /// L Piece
    /// Block Position :
    /// Zero:           Right:          Inverse:        Left:
    /// -------         -------         -------         -------
    /// |    3|         |  0  |         |     |         |3 2  |
    /// |0 1 2|         |  1  |         |2 1 0|         |  1  |
    /// |     |         |  2 3|         |3    |         |  0  |
    /// -------         -------         -------         -------
    /// </summary>
    class LPiece : Piece
    {
        public LPiece(Field bindingField)
            : base(bindingField)
        {
            m_blockType = BlockType.Orange;
            SetSpawnPos();
            SetRotationData();
            SetWallKickData();
        }
        // set the piece's initial state and location.
        protected override void SetSpawnPos()
        {
            m_BlockPos = new Point[4];
            int hCenter = (m_BindingField.Width + 1) / 2 - 1;// center of a row in the field
            int top = m_BindingField.Height - 3; // top line of the field
            m_BlockPos[0] = new Point(hCenter - 1, top - 1);
            m_BlockPos[1] = new Point(hCenter, top - 1);
            m_BlockPos[2] = new Point(hCenter + 1, top - 1);
            m_BlockPos[3] = new Point(hCenter + 1, top);
        }
        /// <summary>
        /// Zero:           Right:          Inverse:        Left:           Zero:
        /// -------         -------         -------         -------         -------
        /// |    3|         |  0  |         |     |         |3 2  |         |    3|
        /// |0 1 2|         |  1  |         |2 1 0|         |  1  |         |0 1 2|
        /// |     |         |  2 3|         |3    |         |  0  |         |     |
        /// -------         -------         -------         -------         -------
        /// </summary>
        protected override void SetRotationData()
        {
            // rotation data
            Point[] ZeroToRight = { new Point(1, 1), new Point(0, 0), new Point(-1, -1), new Point(0, -2) };
            Point[] RightToZero = { new Point(-1, -1), new Point(0, 0), new Point(1, 1), new Point(0, 2) };
            Point[] RightToInverse = { new Point(1, -1), new Point(0, 0), new Point(-1, 1), new Point(-2, 0) };
            Point[] InverseToRight = { new Point(-1, 1), new Point(0, 0), new Point(1, -1), new Point(2, 0) };
            Point[] InverseToLeft = { new Point(-1, -1), new Point(0, 0), new Point(1, 1), new Point(0, 2) };
            Point[] LeftToInverse = { new Point(1, 1), new Point(0, 0), new Point(-1, -1), new Point(0, -2) };
            Point[] LeftToZero = { new Point(-1, 1), new Point(0, 0), new Point(1, -1), new Point(2, 0) };
            Point[] ZeroToLeft = { new Point(1, -1), new Point(0, 0), new Point(-1, 1), new Point(-2, 0) };

            Dictionary<Shape, Point[]> ZeroTransform = new Dictionary<Shape, Point[]>();
            ZeroTransform.Add(Shape.Right, ZeroToRight);
            ZeroTransform.Add(Shape.Left, ZeroToLeft);
            Dictionary<Shape, Point[]> RightTransform = new Dictionary<Shape, Point[]>();
            RightTransform.Add(Shape.Zero, RightToZero);
            RightTransform.Add(Shape.Inverse, RightToInverse);
            Dictionary<Shape, Point[]> InverseTransform = new Dictionary<Shape, Point[]>();
            InverseTransform.Add(Shape.Right, InverseToRight);
            InverseTransform.Add(Shape.Left, InverseToLeft);
            Dictionary<Shape, Point[]> LeftTransform = new Dictionary<Shape, Point[]>();
            LeftTransform.Add(Shape.Zero, LeftToZero);
            LeftTransform.Add(Shape.Inverse, LeftToInverse);

            m_RotationData.Add(Shape.Zero, ZeroTransform);
            m_RotationData.Add(Shape.Right, RightTransform);
            m_RotationData.Add(Shape.Inverse, InverseTransform);
            m_RotationData.Add(Shape.Left, LeftTransform);

        }
        protected override void SetWallKickData()
        {
            // wall kick data
            Point[] ZeroToRight = { new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2) };
            Point[] RightToZero = { new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) };
            Point[] RightToInverse = { new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) };
            Point[] InverseToRight = { new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2) };
            Point[] InverseToLeft = { new Point(1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) };
            Point[] LeftToInverse = { new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, 2) };
            Point[] LeftToZero = { new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, 2) };
            Point[] ZeroToLeft = { new Point(-1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) };

            Dictionary<Shape, Point[]> ZeroOffset = new Dictionary<Shape, Point[]>();
            ZeroOffset.Add(Shape.Right, ZeroToRight);
            ZeroOffset.Add(Shape.Left, ZeroToLeft);
            Dictionary<Shape, Point[]> RightOffset = new Dictionary<Shape, Point[]>();
            RightOffset.Add(Shape.Zero, RightToZero);
            RightOffset.Add(Shape.Inverse, RightToInverse);
            Dictionary<Shape, Point[]> InverseOffset = new Dictionary<Shape, Point[]>();
            InverseOffset.Add(Shape.Right, InverseToRight);
            InverseOffset.Add(Shape.Left, InverseToLeft);
            Dictionary<Shape, Point[]> LeftOffset = new Dictionary<Shape, Point[]>();
            LeftOffset.Add(Shape.Zero, LeftToZero);
            LeftOffset.Add(Shape.Inverse, LeftToInverse);

            m_WallkickTestOffset.Add(Shape.Zero, ZeroOffset);
            m_WallkickTestOffset.Add(Shape.Right, RightOffset);
            m_WallkickTestOffset.Add(Shape.Inverse, InverseOffset);
            m_WallkickTestOffset.Add(Shape.Left, LeftOffset);
        }
    }
}
