using System;
using System.Collections;
using System.Text;

namespace TetrisEngine
{
    public class Field
    {
        private BlockChip[,] m_field;

        private int m_Width = 10;
        private int m_Height = 22;

        public int Width
        {
            get
            {
                return m_Width;
            }
        }
        public int Height
        {
            get
            {
                return m_Height;
            }
        }

        public Field()
        {
            m_field = new BlockChip[m_Height, m_Width];
        }
        public Field(int width, int height)
        {
            this.m_Width = width;
            this.m_Height = height;
            m_field = new BlockChip[height, width];
        }
        /// <summary>
        /// returns block information at specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>returns a reference of BlockChip. if nothing here, returns null.</returns>
        public BlockChip GetBlockAt(int x, int y)
        {
            return m_field[x, y];
        }
        public BlockChip this[int x, int y]
        {
            get
            {
                return GetBlockAt(x, y);
            }
            set
            {
                m_field[x, y] = value;
            }
        }
        /// <summary>
        /// check if specified location is available(is null).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsSpaceAvailable(int x, int y)
        {
            // out of range
            if (x < 0 || x >= m_Width || y < 0 || y >= m_Height)
            {
                return false;
            }

            return m_field[y, x] == null;
        }
        /// <summary>
        /// Check if a specified row can be cleared
        /// </summary>
        /// <param name="row">row id</param>
        /// <returns>Returns true if a specified row is full (can be cleared), false if not</returns>
        public bool CheckRow(int row)
        {
            if (row < 0 || row >= m_Height)
            {
                return false;
            }
            for (int i = 0; i < m_Width; i++)
            {
                if (m_field[row, i] == null)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Clear a specified row
        /// </summary>
        /// <param name="row">row id</param>
        /// <returns>Returns true if succeed, false if failed</returns>
        public bool ClearRow(int row)
        {
            if(row <0|| row>=m_Height)
            {
                return false;
            }
            // clear the row
            for (int i = 0; i < m_Width; i++)
            {
                m_field[row, i] = null;
            }
            // move down the upper block
            for (int rowID = row; rowID < m_Height - 1; rowID++)
            {
                for (int i = 0; i < Width; i++)
                {
                    m_field[rowID, i] = m_field[rowID + 1, i];
                }
            }
            return true;
        }
    }
}

