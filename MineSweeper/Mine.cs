using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MineSweeper
{
    public class Mine
    {
        public int mine_count;
        public bool is_flag;
        public bool is_cover;
        public bool is_mine;

        public Mine(int count)
        {
            mine_count = count;
            is_cover = true;
            is_flag = false;

            if (count == -1)             //-1 is mine
            {
                is_mine = true;
            }
            else
            {
                is_mine = false;
            }

        }

    }
}
