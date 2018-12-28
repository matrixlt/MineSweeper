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
        public Rectangle graph = new Rectangle();
        public int mine_count;
        public int state;

        public Mine()
        {

        }

        public Mine(int state)
        {
            this.state = state;
        }
    }
}
