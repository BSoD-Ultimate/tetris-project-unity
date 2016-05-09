using System;
using System.Collections.Generic;
using System.Text;

namespace TetrisEngine
{
    public enum BlockType { Empty, Grey, Red, Orange, Yellow, Green, Cyan, Blue, Purple, Bone };
    public class BlockChip
    {
        public BlockType Type { get; set; }
        public bool IsHidden { get; set; }
        public BlockChip()
        {
            Reset();
        }
        public void Reset()
        {
            Type = BlockType.Grey;
            IsHidden = false;
        }
    }
}
