using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    class SolveUnit
    {
        public int mine_count;
        HashSet<Tuple<int, int>>  blocks = new HashSet<Tuple<int, int>>{};

        public SolveUnit(int count)
        {
            mine_count = count;
        }

        public HashSet<Tuple<int, int>> Blocks { get => blocks;}

        public void AddTuple(int x, int y)
        {
            blocks.Add(new Tuple<int, int>(x,y));
        }

    }
}
