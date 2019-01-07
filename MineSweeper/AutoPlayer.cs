using Combinatorics.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MineSweeper
{
    public class AutoPlayer
    {
        private int row;
        private int col;
        Mine[,] mines = null;
        Rectangle[,] rectangles = null;

        public delegate bool AutoPlay(int x, int y);
        public delegate bool InBorder(int x, int y);
        public delegate void LRClick(int x, int y);
        public delegate bool OpenBlock(int x, int y);
        public delegate void OpenEmpty(int x, int y);

        public InBorder inBorder;
        public LRClick lRClick;
        public OpenBlock openBlock;
        public OpenEmpty openEmpty;

        public AutoPlayer(int row, int col, Mine[,] mines, Rectangle[,] rectangles)
        {
            SetProperties(row, col, mines, rectangles);
        }

        public void SetProperties(int row, int col, Mine[,] mines, Rectangle[,] rectangles)
        {
            this.row = row;
            this.col = col;
            this.mines = mines;
            this.rectangles = rectangles;
        }

        public bool InBorderNineByNine(int x, int y, int x_low, int y_low)
        {
            return (x >= x_low && x < x_low + 3 && y >= y_low && y < y_low + 3);
        }

        private BlockInfo GetInfo(int x, int y)//3*3
        {
            int flag = 0;
            int unflag = 0;
            int block = 0;
            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (inBorder(i, j))
                    {
                        block++;
                        if (mines[i, j].is_cover)
                        {
                            if (mines[i, j].is_flag)
                            { flag++; }
                            else { unflag++; }
                        }
                    }
                }
            }

            return new BlockInfo(mines[x, y].mine_count, flag, unflag, block);

        }

        private HashSet<Position> GetBlockSet(int x, int y, bool simplified = false)
        {
            var result = new HashSet<Position> { };
            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (inBorder(i, j) && mines[i, j].is_cover)
                    {
                        if (!simplified)
                            result.Add(new Position(i, j));
                        else
                        {
                            if (!mines[i, j].is_flag)
                            {
                                result.Add(new Position(i, j));
                            }
                        }
                    }
                }
            }
            foreach (Position p in result)
            {
                //Console.WriteLine("here{0}", p.x);
            }
            return result;
        }

        #region AutoPlay
        public bool SimpleTest(AutoPlay f)
        {
            bool effective = false;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (!mines[i, j].is_cover)
                    {
                        bool test = f(i, j);
                        effective = test || effective;
                    }

                }
            }
            return effective;
        }

        public bool SimpleFlag(int x, int y)
        {
            BlockInfo info = GetInfo(x, y);
            int flag_count = info.flag_count;
            int unflag_count = info.unflag_count;


            if (unflag_count + flag_count == mines[x, y].mine_count)
            {
                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (inBorder(i, j) && mines[i, j].is_cover)
                        {
                            mines[i, j].is_flag = true;
                            rectangles[i, j].Fill = BlockBrush.flag;
                        }
                    }
                }
                if (unflag_count > 0)
                    return true;
            }
            return false;
        }

        public bool SimpleClick(int x, int y)
        {
            BlockInfo info = GetInfo(x, y);
            int flag_count = info.flag_count;
            int unflag_count = info.unflag_count;

            if (flag_count == mines[x, y].mine_count)
            {
                lRClick(x, y);
                if (unflag_count != 0)
                    return true;
            }
            return false;
        }

        public bool UncertainComplexFlag(int x, int y)//analyze in 3*3, opt needed
        {
            List<SolveUnit> units = new List<SolveUnit> { };
            SolveUnit center = new SolveUnit(mines[x, y].mine_count);

            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (inBorder(i, j) && !mines[i, j].is_cover         //in border and a uncovered number
                        && mines[i, j].mine_count != 0 && (x != i || y != j))//number not 0 and block not center
                    {
                        SolveUnit edge = new SolveUnit(mines[i, j].mine_count);
                        for (int m = i - 1; m < i + 2; m++)
                        {
                            for (int n = j - 1; n < j + 2; n++)
                            {
                                if (inBorder(m, n) && InBorderNineByNine(m, n, x - 1, y - 1)
                                    && mines[m, n].is_cover)
                                {
                                    edge.AddPosition(m, n);
                                }
                            }
                        }
                        units.Add(edge);

                    }
                    if (inBorder(i, j) && mines[i, j].is_cover)
                    {
                        center.AddPosition(i, j);
                    }
                }
            }

            for (int i = 0; i < units.Count; i++)
            {
                int diff = center.mine_count - units[i].mine_count;//at least diff mines
                int diff_block = center.Blocks.Count - units[i].Blocks.Count;

                if (diff == diff_block && diff > 0)
                {
                    bool test = false;
                    center.Blocks.ExceptWith(units[i].Blocks);
                    CheckSet(center.Blocks);
                    foreach (Position p in center.Blocks)
                    {
                        if (!mines[p.x, p.y].is_flag)
                        {
                            mines[p.x, p.y].is_flag = true;
                            rectangles[p.x, p.y].Fill = BlockBrush.flag;
                            test = true;
                        }
                    }
                    return test;
                }
            }
            return false;

        }

        public bool ComplexFlag(int x, int y)//analyze in 3*3
        {
            List<SolveUnit> units = new List<SolveUnit> { };
            SolveUnit center = new SolveUnit(mines[x, y].mine_count);

            center.Simplified_blocks = GetBlockSet(x, y, true);
            center.comfirmed_mine_count = mines[x, y].mine_count - GetInfo(x, y).flag_count;

            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (inBorder(i, j) && !mines[i, j].is_cover && mines[i, j].mine_count != 0)
                    {
                        SolveUnit edge = new SolveUnit(mines[i, j].mine_count);
                        edge.Simplified_blocks = GetBlockSet(i, j, true);
                        edge.comfirmed_mine_count = mines[i, j].mine_count - GetInfo(i, j).flag_count;
                        units.Add(edge);
                    }
                }
            }

            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Simplified_blocks.IsProperSubsetOf(center.Simplified_blocks) &&
                    units[i].comfirmed_mine_count < center.comfirmed_mine_count)
                {
                    int mine_diff = center.comfirmed_mine_count - units[i].comfirmed_mine_count;
                    center.Simplified_blocks.ExceptWith(units[i].Simplified_blocks);
                    int block_diff = center.Simplified_blocks.Count;
                    if (mine_diff == block_diff)
                    {
                        foreach (Position p in center.Simplified_blocks)
                        {
                            mines[p.x, p.y].is_flag = true;
                            rectangles[p.x, p.y].Fill = BlockBrush.flag;
                        }
                    }
                    else
                    {
                        center.Simplified_blocks.UnionWith(units[i].Simplified_blocks);
                    }

                }
            }


            return false;
        }

        public bool ComplexClick(int x, int y)//analyze in 3*3+,simplified and comfirmed
        {
            List<SolveUnit> units = new List<SolveUnit> { };
            SolveUnit center = new SolveUnit(mines[x, y].mine_count);

            center.Simplified_blocks = GetBlockSet(x, y, true);
            center.comfirmed_mine_count = mines[x, y].mine_count - GetInfo(x, y).flag_count;

            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (inBorder(i, j) && !mines[i, j].is_cover && mines[i, j].mine_count != 0)
                    {
                        SolveUnit edge = new SolveUnit(mines[i, j].mine_count);
                        edge.Simplified_blocks = GetBlockSet(i, j, true);
                        edge.comfirmed_mine_count = mines[i, j].mine_count - GetInfo(i, j).flag_count;
                        units.Add(edge);
                    }
                }
            }

            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Simplified_blocks.IsProperSubsetOf(center.Simplified_blocks) &&
                    units[i].comfirmed_mine_count == center.comfirmed_mine_count)
                {
                    center.Simplified_blocks.ExceptWith(units[i].Simplified_blocks);
                    foreach (Position p in center.Simplified_blocks)
                    {
                        if (mines[p.x, p.y].mine_count != 0)
                        {
                            if (openBlock(p.x, p.y))
                                return true;//win or lose
                        }
                        else
                        {
                            openEmpty(p.x, p.y);
                        }

                    }
                    return true;
                }
            }


            return false;
        }

        public bool CompleteAnalyze(int x, int y)//analyze in 5*5+,simplified and comfirmed,test needed
        {
            List<SolveUnit> units = new List<SolveUnit> { };
            SolveUnit center = new SolveUnit(mines[x, y].mine_count);

            center.Simplified_blocks = GetBlockSet(x, y, true);
            center.comfirmed_mine_count = mines[x, y].mine_count - GetInfo(x, y).flag_count;

            for (int i = x - 2; i < x + 3; i++)
            {
                for (int j = y - 2; j < y + 3; j++)
                {
                    if (inBorder(i, j) && !mines[i, j].is_cover            // in border and is a uncovered number 
                        && mines[i, j].mine_count != 0 && (x != i || y != j)//number is not 0 and is not center(x,y)
                        )
                    {
                        SolveUnit edge = new SolveUnit(mines[i, j].mine_count);
                        edge.Simplified_blocks = GetBlockSet(i, j, true);
                        edge.comfirmed_mine_count = mines[i, j].mine_count - GetInfo(i, j).flag_count;
                        if (edge.comfirmed_mine_count != 0)                  //not a empty condition
                            units.Add(edge);
                    }
                }
            }
            for (int i = 1; i < units.Count + 1; i++)
            {
                Combinations<SolveUnit> combinations = new Combinations<SolveUnit>(units, i);//get all combination
                foreach (IList<SolveUnit> set in combinations)
                {
                    HashSet<Position> solve = new HashSet<Position> { };
                    int before = 0;
                    int after = 0;
                    int real_mine_count = 0;
                    foreach (SolveUnit su in set)
                    {
                        before = solve.Count;
                        solve.UnionWith(su.Simplified_blocks);//union
                        after = solve.Count;
                        if (after - before != su.Simplified_blocks.Count)//intersect not empty
                        {
                            break;
                        }
                        real_mine_count += su.comfirmed_mine_count;
                    }
                    CheckSet(solve);
                    if (solve.IsProperSubsetOf(center.Simplified_blocks))//properset
                    {
                        int block_diff = center.Simplified_blocks.Count - solve.Count;
                        int mine_diff = center.comfirmed_mine_count - real_mine_count;
                        Console.WriteLine("{0} {1}", block_diff, mine_diff);
                        if (mine_diff == 0)
                        {
                            center.Simplified_blocks.ExceptWith(solve);
                            foreach (Position p in center.Simplified_blocks)
                            {
                                if (mines[p.x, p.y].mine_count == 0)
                                {
                                    openEmpty(p.x, p.y);
                                    return true;
                                }

                                else
                                {
                                    if (openBlock(p.x, p.y))
                                        return true;
                                }

                            }
                        }

                        if (mine_diff > 0 && mine_diff == block_diff)
                        {
                            center.Simplified_blocks.ExceptWith(solve);
                            //CheckSet(center.Simplified_blocks);
                            foreach (Position p in center.Simplified_blocks)
                            {
                                mines[p.x, p.y].is_flag = true;
                                rectangles[p.x, p.y].Fill = BlockBrush.flag;
                            }
                            return true;
                        }
                    }
                    else if (!solve.SetEquals(center.Simplified_blocks))//not properset, not equal
                    {
                        int mine_diff = real_mine_count - center.comfirmed_mine_count;
                        if (mine_diff > 0)
                        {
                            solve.ExceptWith(center.Simplified_blocks);
                            if (solve.Count != 0)
                            {
                                int block_diff = solve.Count;
                                if (block_diff == mine_diff)
                                {
                                    foreach (Position p in solve)
                                    {
                                        mines[p.x, p.y].is_flag = true;
                                        rectangles[p.x, p.y].Fill = BlockBrush.flag;
                                    }
                                    return true;
                                }

                            }
                        }
                        else if (mine_diff < 0)
                        {
                            center.Simplified_blocks.ExceptWith(solve);
                            int block_diff = center.Simplified_blocks.Count;
                            if (block_diff == -mine_diff)
                            {
                                foreach (Position p in center.Simplified_blocks)
                                {
                                    mines[p.x, p.y].is_flag = true;
                                    rectangles[p.x, p.y].Fill = BlockBrush.flag;
                                }
                                return true;
                            }
                        }

                    }

                }
            }
            return false;
        }

        public bool SealedBlock()
        {
            int mine_count = 0;
            int flag_count = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (mines[i, j].is_mine)
                        mine_count++;
                    if (mines[i, j].is_flag)
                        flag_count++;
                }
            }

            if (mine_count == flag_count)
            {
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        if (mines[i, j].is_cover && !mines[i, j].is_flag)
                            if (openBlock(i, j))
                                return true;
                    }
                }
                return true;
            }

            return false;
        }
        #endregion

        #region debug
        private void CheckSet(HashSet<Position> h)
        {
            foreach (Position p in h)
            {
                Console.WriteLine("check: {0} {1}", p.x, p.y);
            }
            Console.WriteLine();
        }
        #endregion
    }
}
