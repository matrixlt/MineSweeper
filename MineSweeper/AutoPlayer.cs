using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MineSweeper
{
    public class AutoPlayer
    {
        private int row;
        private int col;
        Mine[,] mines = null;
        Rectangle[,] rectangles = null;
        Random random = new Random(DateTime.Now.Millisecond);
        public List<Border> borders;
        public MainWindow MW;

        public delegate bool AutoPlay(int x, int y);
        public delegate bool Hint(int x, int y, bool mode = false);
        public delegate bool InBorder(int x, int y);
        public delegate void LRClick(int x, int y);
        public delegate bool OpenBlock(int x, int y);
        public delegate bool OpenEmpty(int x, int y);
        public delegate void FlagBlock(int x, int y);
        public delegate void ClickBlock(int x, int y, Mine mine);

        public InBorder inBorder;
        public LRClick lRClick;
        public OpenBlock openBlock;
        public OpenEmpty openEmpty;
        public FlagBlock flagBlock;
        public ClickBlock clickBlock;

        #region helpers
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

        private void ShowAnimation(int x, int y)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            myDoubleAnimation.AutoReverse = true;

            rectangles[x, y].BeginAnimation(Rectangle.OpacityProperty, myDoubleAnimation);
        }

        static private List<List<T>> GetAllCombination<T>(List<T> units)
        {
            if (units.Count == 0)
            {
                var result = new List<List<T>> { };
                return result;
            }
            if (units.Count == 1)
            {
                var result = new List<List<T>> { };
                result.Add(new List<T> { units[0] });
                return result;
            }
            else
            {
                T unit = units[0];
                units.Remove(unit);
                List<List<T>> result = GetAllCombination<T>(units);
                List<List<T>> others = new List<List<T>> { };
                others.Add(new List<T> { unit });

                foreach (List<T> set in result)
                {
                    var new_set = new List<T>(set);
                    new_set.Add(unit);
                    others.Add(new_set);
                }
                var r = new List<List<T>>(result.Union(others));

                return r;
            }
        }
        #endregion

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
        public bool SimpleTest(Hint f)//for old codes
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

        public bool SimpleFlag(int x, int y, bool hint_mode = false)
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
                            if (!mines[i, j].is_flag)
                            {
                                if (hint_mode)//animation
                                {
                                    ShowAnimation(i, j);
                                }
                                else flagBlock(i, j);
                            }
                        }
                    }
                }
                if (unflag_count > 0)
                    return true;
            }
            return false;
        }

        public bool SimpleClick(int x, int y, bool hint_mode = false)
        {
            BlockInfo info = GetInfo(x, y);
            int flag_count = info.flag_count;
            int unflag_count = info.unflag_count;

            if (flag_count == mines[x, y].mine_count)
            {
                if (hint_mode && unflag_count != 0)
                {
                    var block_set = GetBlockSet(x, y, true);
                    foreach (Position p in block_set)
                    {
                        ShowAnimation(p.x, p.y);
                    }
                    return true;
                }
                lRClick(x, y);
                if (unflag_count != 0)
                    return true;
            }
            return false;
        }

        public bool UncertainComplexFlag(int x, int y, bool hint_mode = false)//analyze in 3*3, opt needed
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
                    //CheckSet(center.Blocks);
                    foreach (Position p in center.Blocks)
                    {
                        if (!mines[p.x, p.y].is_flag)
                        {
                            if (hint_mode)
                                ShowAnimation(p.x, p.y);
                            else flagBlock(p.x, p.y);
                            test = true;
                        }
                    }
                    return test;
                }
            }
            return false;

        }

        public bool ComplexFlag(int x, int y, bool hint_mode = false)//analyze in 3*3
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
                            if (hint_mode)
                                ShowAnimation(p.x, p.y);
                            else flagBlock(p.x, p.y);
                        }
                        return true;
                    }
                    else
                    {
                        center.Simplified_blocks.UnionWith(units[i].Simplified_blocks);
                    }

                }
            }


            return false;
        }

        public bool ComplexClick(int x, int y, bool hint_mode = false)//analyze in 3*3+,simplified and comfirmed
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
                        if (hint_mode)
                        {
                            ShowAnimation(p.x, p.y);
                        }
                        else
                        {
                            if (mines[p.x, p.y].mine_count != 0)
                            {
                                if (openBlock(p.x, p.y))
                                    return true;//win or lose
                            }
                            else
                            {
                                if (openEmpty(p.x, p.y))
                                    return true;
                            }
                        }


                    }
                    return true;
                }
            }


            return false;
        }

        public bool CompleteAnalyze(int x, int y, bool hint_mode = false)//analyze in 5*5+,simplified and comfirmed,test needed
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


            if (units.Count == 0)
                return false;
            var combinations = GetAllCombination(units);//get all combinatons
            foreach (List<SolveUnit> set in combinations)
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
                        goto end;
                    }
                    real_mine_count += su.comfirmed_mine_count;
                }
                //CheckSet(solve);
                if (solve.IsProperSubsetOf(center.Simplified_blocks))//properset
                {
                    int block_diff = center.Simplified_blocks.Count - solve.Count;
                    int mine_diff = center.comfirmed_mine_count - real_mine_count;
                    //Console.WriteLine("{0} {1}", block_diff, mine_diff);
                    if (mine_diff == 0)
                    {
                        center.Simplified_blocks.ExceptWith(solve);
                        foreach (Position p in center.Simplified_blocks)
                        {
                            if (hint_mode)
                            {
                                ShowAnimation(p.x, p.y);
                            }
                            else
                            {
                                if (mines[p.x, p.y].mine_count == 0)
                                {
                                    if (openEmpty(p.x, p.y))
                                        return true;
                                }

                                else
                                {
                                    if (openBlock(p.x, p.y))
                                        return true;
                                }
                            }
                        }
                        return true;
                    }

                    if (mine_diff > 0 && mine_diff == block_diff)
                    {
                        center.Simplified_blocks.ExceptWith(solve);
                        //CheckSet(center.Simplified_blocks);
                        foreach (Position p in center.Simplified_blocks)
                        {
                            if (hint_mode)
                                ShowAnimation(p.x, p.y);
                            else flagBlock(p.x, p.y);
                        }
                        return true;
                    }
                }

                else if (center.Simplified_blocks.IsProperSubsetOf(solve))//superset
                {
                    int block_diff = solve.Count - center.Simplified_blocks.Count;
                    int mine_diff = real_mine_count - center.comfirmed_mine_count;
                    //Console.WriteLine("{0} {1}", block_diff, mine_diff);
                    if (mine_diff == 0)
                    {
                        CheckSet(solve);
                        CheckSet(center.Simplified_blocks);
                        solve.ExceptWith(center.Simplified_blocks);
                        foreach (Position p in solve)
                        {
                            if (hint_mode)
                            {
                                ShowAnimation(p.x, p.y);
                            }
                            else
                            {
                                if (mines[p.x, p.y].mine_count == 0)
                                {
                                    if (openEmpty(p.x, p.y))
                                        return true;
                                }

                                else
                                {
                                    if (openBlock(p.x, p.y))
                                        return true;
                                }
                            }

                        }
                        return true;
                    }

                    if (mine_diff > 0 && mine_diff == block_diff)
                    {
                        solve.ExceptWith(center.Simplified_blocks);
                        //CheckSet(center.Simplified_blocks);
                        foreach (Position p in solve)
                        {
                            if (hint_mode)
                                ShowAnimation(p.x, p.y);
                            else flagBlock(p.x, p.y);
                        }
                        return true;
                    }
                }

                else if (!solve.SetEquals(center.Simplified_blocks))//not properset, not equal
                {
                    int mine_diff = real_mine_count - center.comfirmed_mine_count;
                    if (mine_diff > 0)
                    {
                        var new_set = new HashSet<Position>(solve);
                        new_set.ExceptWith(center.Simplified_blocks);
                        if (new_set.Count != 0)
                        {
                            int block_diff = new_set.Count;
                            if (block_diff == mine_diff)
                            {
                                foreach (Position p in new_set)
                                {
                                    if (hint_mode)
                                        ShowAnimation(p.x, p.y);
                                    else flagBlock(p.x, p.y);
                                }
                                return true;
                            }

                        }
                    }
                    else if (mine_diff < 0)
                    {
                        //CheckSet(solve);
                        //CheckSet(center.Simplified_blocks);
                        var new_set = new HashSet<Position>(center.Simplified_blocks);
                        new_set.ExceptWith(solve);
                        int block_diff = new_set.Count;
                        if (block_diff == -mine_diff)
                        {
                            foreach (Position p in new_set)
                            {
                                if (hint_mode)
                                    ShowAnimation(p.x, p.y);
                                else flagBlock(p.x, p.y);
                            }
                            return true;
                        }
                    }

                }
                end:;
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

        public bool RandomClick()
        {
            var all_possible = new List<Position> { };
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    if (mines[x, y].is_cover && !mines[x, y].is_flag)
                    {
                        all_possible.Add(new Position(x, y));
                    }
                }
            }

            int choose = random.Next(0, all_possible.Count);
            if (all_possible.Count == 0)
            {
                return true;
            }
            else
            {
                if (mines[all_possible[choose].x, all_possible[choose].y].mine_count == 0)
                {
                    if (openEmpty(all_possible[choose].x, all_possible[choose].y))
                        return true;
                }

                else
                {
                    if (openBlock(all_possible[choose].x, all_possible[choose].y))
                        return true;
                }
            }

            if (mines[all_possible[choose].x, all_possible[choose].y].is_mine)
                return true;

            return false;

        }

        public void SimpleHint(Hint f)
        {
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    if (!mines[x, y].is_cover && mines[x, y].mine_count != 0 && f(x, y, true))
                        return;
                }
            }
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
