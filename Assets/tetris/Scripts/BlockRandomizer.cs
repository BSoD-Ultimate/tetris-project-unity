using System;
using System.Collections.Generic;
using System.Text;

namespace TetrisEngine
{
    abstract class BlockRandomizer
    {
        protected const int m_maxValue = 7;
        protected Dictionary<int, char> m_blockBindings;
        protected Random m_random;

        public BlockRandomizer()
        {
            m_blockBindings = new Dictionary<int, char>();
            m_blockBindings.Add(0, 'I');
            m_blockBindings.Add(1, 'J');
            m_blockBindings.Add(2, 'L');
            m_blockBindings.Add(3, 'O');
            m_blockBindings.Add(4, 'S');
            m_blockBindings.Add(5, 'T');
            m_blockBindings.Add(6, 'Z');

            m_random = new Random();
        }
        public BlockRandomizer(int seed)
            : this()
        {
            m_random = new Random(seed);
        }

        protected abstract char GetNext();
        public Piece GetNextPiece(Field field)
        {
            char nextPiece = GetNext();
            switch (nextPiece)
            {
                case 'I':
                    return new IPiece(field);
                case 'J':
                    return new JPiece(field);
                case 'L':
                    return new LPiece(field);
                case 'O':
                    return new OPiece(field);
                case 'S':
                    return new SPiece(field);
                case 'T':
                    return new TPiece(field);
                case 'Z':
                    return new ZPiece(field);
            }
            return null;
        }
    }
    class History4RollsRandomizer : BlockRandomizer
    {
        private Queue<char> m_blockHistory = new Queue<char>();
        private bool m_isFirstPiece = true;

        public History4RollsRandomizer()
            : base()
        {
            m_blockHistory.Enqueue('S');
            m_blockHistory.Enqueue('S');
            m_blockHistory.Enqueue('Z');
            m_blockHistory.Enqueue('Z');
        }
        public History4RollsRandomizer(int seed)
            : base(seed)
        {
            m_blockHistory.Enqueue('S');
            m_blockHistory.Enqueue('S');
            m_blockHistory.Enqueue('Z');
            m_blockHistory.Enqueue('Z');
        }
        protected override char GetNext()
        {
            int nextBlockValue=m_random.Next(m_maxValue);
            if(m_isFirstPiece)
            {
                while (m_blockBindings[nextBlockValue]=='S' ||
                    m_blockBindings[nextBlockValue]=='Z' ||
                    m_blockBindings[nextBlockValue]=='O' )
                {
                    nextBlockValue = m_random.Next(m_maxValue);
                }
                m_isFirstPiece = false;
            }

            while(m_blockHistory.Contains(m_blockBindings[nextBlockValue]))
            {
                nextBlockValue = m_random.Next(m_maxValue);
            }

            m_blockHistory.Dequeue();
            m_blockHistory.Enqueue(m_blockBindings[nextBlockValue]);

            return m_blockBindings[nextBlockValue];
        }
    }
    class MemorylessRandomizer : BlockRandomizer
    {
        public MemorylessRandomizer() : base()
        {

        }
        public MemorylessRandomizer(int seed) : base(seed)
        {

        }
        protected override char GetNext()
        {
            return m_blockBindings[m_random.Next(m_maxValue)];
        }
    }
    /// <summary>
    /// only generates L Piece, for test
    /// </summary>
    class TestRandomizer : BlockRandomizer
    {
        public TestRandomizer() : base()
        {

        }
        protected override char GetNext()
        {
            return m_blockBindings[2];
        }
    }
}
