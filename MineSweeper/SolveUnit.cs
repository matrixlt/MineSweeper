using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    struct Position
    {
        public int x;
        public int y;
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    struct BlockInfo
    {
        public int mine_count;
        public int flag_count;
        public int unflag_count;
        public int block_count;

        public BlockInfo(int mc,int fc, int uc, int bc)
        {
            mine_count = mc;
            flag_count = fc;
            unflag_count = uc;
            block_count = bc;
        }
    }

    class SolveUnit
    {
        public int mine_count;
        public bool comfirm = false;
        public bool simplified = false;
        public int comfirmed_mine_count;
        HashSet<Position> blocks = new HashSet<Position> { };
        HashSet<Position> simplified_blocks = new HashSet<Position> { };
        public SolveUnit(int count)
        {
            mine_count = count;
        }

        public HashSet<Position> Blocks { get => blocks; set => blocks = value; }
        internal HashSet<Position> Simplified_blocks { get => simplified_blocks; set => simplified_blocks = value; }

        public void AddPosition(int x, int y)
        {
            blocks.Add(new Position(x, y));
        }

        public void Print()
        {
            foreach(Position p in Simplified_blocks)
            {
                Console.WriteLine("p:{0} {1}", p.x, p.y);
            }
            Console.WriteLine();
        }
    }
}
