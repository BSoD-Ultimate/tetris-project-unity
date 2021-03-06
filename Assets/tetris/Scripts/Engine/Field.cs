﻿using System;
using System.Collections;
using System.Text;
using System.Diagnostics;

namespace TetrisEngine
{
    public class Field
    {
        private BlockChip[,] m_field;

        private int m_Width = 10;
        private int m_Height = 22;

        private Random m_Random;

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
            m_Random = new Random();
        }
        public Field(int width, int height)
        {
            this.m_Width = width;
            this.m_Height = height;
            m_field = new BlockChip[height, width];
            m_Random = new Random();
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
            if (row < 0 || row >= m_Height)
            {
                return false;
            }
            // clear the row
            for (int i = 0; i < m_Width; i++)
            {
                m_field[row, i] = null;
            }

            return true;
        }
        private bool IsRowEmpty(int row)
        {
            for (int i = 0; i < m_Width; i++)
            {
                if (m_field[row, i] != null)
                {
                    return false;
                }
            }
            return true;
        }
        public bool CollapseRows()
        {
            // find empty rows from top to bottom
            for (int row = m_Height - 2; row >= 0; row--)
            {
                if (IsRowEmpty(row))
                {
                    // move down upper blocks
                    for (int rowID = row; rowID < m_Height - 1; rowID++)
                    {
                        for (int i = 0; i < Width; i++)
                        {
                            m_field[rowID, i] = m_field[rowID + 1, i];
                        }
                    }
                }
            }
            return true;
        }
        // clear all blocks.
        public void Clear()
        {
            for (int i = 0; i < m_Height; i++)
            {
                for (int j = 0; j < m_Width; j++)
                {
                    m_field[i, j] = null;
                }
            }
        }

        // Add garbage rows in the field
        public void AddGarbageRow(int rowCount, bool[] garbageRowStyle = null)
        {
            // out of range
            if (rowCount > m_Height)
            {
                rowCount = m_Height;
            }
            BlockChip[,] newField = new BlockChip[m_Height, m_Width];
            // copy original field data to new field
            for (int i = 0; i < m_Height - rowCount; i++)
            {
                for (int j = 0; j < m_Width; j++)
                {
                    newField[i + rowCount, j] = m_field[i, j];
                }
            }

            // make garbage row
            if (garbageRowStyle != null)
            {
                Debug.Assert(garbageRowStyle.Length == this.Width);
            }
            else
            {
                // default
                garbageRowStyle = new bool[m_Width];
                for(int i=0;i<m_Width;i++)
                {
                    garbageRowStyle[i] = true;
                }
                garbageRowStyle[m_Random.Next(0, 10)] = false;
            }

            // fill garbage row
            for(int i=0;i<rowCount;i++)
            {
                for(int j=0;j<m_Width;j++)
                {
                    if(garbageRowStyle[j])
                    {
                        newField[i, j] = new BlockChip();
                        newField[i, j].Type = BlockType.Grey;
                    }
                }
            }
            m_field = newField;
        }
    }
}

