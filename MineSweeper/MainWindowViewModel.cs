using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MineSweeper
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region private members
        private bool in_game = false;
        private bool first_click = true;

        private int row;
        private int col;
        private int mine_number;
        private bool both_down = false;

        private DateTime start_time;
        private double time_span;
        private string show_time = "000";
        #endregion
        //only used in class

        #region other member
        public event PropertyChangedEventHandler PropertyChanged;

        List<Rectangle> mines_set = new List<Rectangle> { };
        List<Border> borders = new List<Border> { };
        Mine[,] mines = null;
        int[,] number_show = null;
        Rectangle[,] rectangles = null;
        BlockBrush myBrush = new BlockBrush();

        Game game = new Game();

        public delegate bool AutoPlay(int x, int y);
        #endregion
        //maybe used out of class

        #region constructor
        public MainWindowViewModel()
        {
            this.row = 10;
            this.col = 10;
            this.mine_number = 15;
            mines = new Mine[row, col];
            rectangles = new Rectangle[row, col];
            number_show = game.Generate(row, col, mine_number);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    Rectangle myrectangle = new Rectangle();

                    Border border = new Border();
                    border.Child = myrectangle;
                    borders.Add(border);
                    border.Background = new SolidColorBrush(Colors.Tan);
                    border.BorderThickness = new Thickness(1);

                    mines[i, j] = mymine;
                    rectangles[i, j] = myrectangle;

                    myrectangle.Fill = Brushes.AliceBlue;
                    myrectangle.PreviewMouseRightButtonUp += new MouseButtonEventHandler(RClick);
                    myrectangle.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(LClick);
                    myrectangle.PreviewMouseDown += new MouseButtonEventHandler(MouseDown);
                    myrectangle.PreviewMouseUp += new MouseButtonEventHandler(MouseUp);

                }
            }
        }
        #endregion

        #region mouse event
        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (both_down)
            {
                Rectangle s = (Rectangle)sender;
                int x, y;                                     //position
                Mine mine = WhichMine(s, out x, out y);       //FIX LATER
                if (mine.is_cover || mine.mine_count == 0)
                    return;

                int flag_count = 0;
                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (InBorder(i, j) && mines[i, j].is_flag)
                        {
                            flag_count++;
                        }
                    }
                }

                if (flag_count != mine.mine_count)
                    return;

                LRClick(x, y);

            }
            both_down = false;
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Pressed)
            {
                both_down = true;
            }

        }

        private void LClick(object sender, MouseButtonEventArgs e)
        {


            Rectangle s = (Rectangle)sender;
            int x, y;                                     //position
            Mine mine = WhichMine(s, out x, out y);       //FIX LATER

            Console.WriteLine(sender.ToString());
            Console.WriteLine(mine.mine_count);

            if (mine.is_flag)
                return;

            if (first_click)
            {                                       //TODO start a game or modify sth ,start the timer or sth
                in_game = true;
                //first_click = false;
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();
                if (mine.is_mine)
                {
                    Restart(x, y);
                    return;
                }

            }

            if (mine.is_cover)
            {
                if (mine.is_mine)
                {
                    in_game = false;
                    s.Fill = BlockBrush.mine;
                    borders[x * row + y].Background = new SolidColorBrush(Colors.Red);
                    LoseWindow();
                    Restart();
                }
                else
                {

                    if (mine.mine_count == 0)
                    {
                        OpenEmpty(x, y);
                    }
                    else
                    {
                        OpenBlock(x, y);
                    }

                }
            }

            if (game.IsFinish(mines))
            {
                WinWindow();
                Restart();
            }

        }

        private void RClick(object sender, MouseButtonEventArgs e)
        {
            Rectangle s = (Rectangle)sender;
            int x, y;                                     //position
            Mine mine = WhichMine(s, out x, out y);       //FIX LATER

            if (mine.is_cover)
            {
                if (mine.is_flag)
                {
                    mine.is_flag = false;
                    s.Fill = Brushes.AliceBlue;
                }
                else
                {
                    mine.is_flag = true;
                    s.Fill = BlockBrush.flag;
                }
            }
        }
        #endregion

        #region block operation
        private void OpenBlock(int x, int y)
        {
            if (!mines[x, y].is_mine)
                rectangles[x, y].Fill = BlockBrush.numbers[mines[x, y].mine_count];
            else
            {
                rectangles[x, y].Fill = BlockBrush.mine;
                borders[x * row + y].Background = new SolidColorBrush(Colors.Red);
            }
            mines[x, y].is_cover = false;

        }

        private void OpenEmpty(int x, int y)
        {
            OpenBlock(x, y);

            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (InBorder(i, j))
                    {
                        if (mines[i, j].mine_count == 0 && mines[i, j].is_cover)
                        {
                            OpenEmpty(i, j);
                        }
                        else
                        {
                            OpenBlock(i, j);
                        }
                    }
                }
            }
        }

        private void LRClick(int x, int y)
        {
            bool lose = false;
            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (InBorder(i, j) && !mines[i, j].is_flag)
                    {
                        if (mines[i, j].mine_count == 0)
                            OpenEmpty(i, j);
                        else OpenBlock(i, j);
                        if (mines[i, j].is_mine)
                        {
                            lose = true;
                        }
                    }

                }
            }

            if (lose)
            {
                LoseWindow();
                Restart();
            }

            if (game.IsFinish(mines))
            {
                WinWindow();
                Restart();
            }
        }
        #endregion

        #region binding
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (first_click == true)
            {
                first_click = false;
                start_time = DateTime.Now;
                time_span = 0;
                this.Show_time = Convert.ToInt64(time_span).ToString("D3");
            }
            else
            {
                time_span = 0.001 * ((DateTime.Now - start_time).TotalMilliseconds % 1000) + (DateTime.Now - start_time).TotalMilliseconds / 1000;
                if (time_span > 999)
                {
                    this.Show_time = Convert.ToInt64(time_span - 1000).ToString("D3");
                }
                else
                {
                    this.Show_time = Convert.ToInt64(time_span).ToString("D3");
                }
                //Console.WriteLine(show_time);
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public int Row { get; set; }
        public int Col { get; set; }
        public List<Border> BorderSet { get { return borders; } }
        public string Show_time
        {
            get => show_time;
            set
            {
                show_time = value;
                OnPropertyChanged("Show_time");
            }
        }
        #endregion

        #region helpers in class
        public Mine WhichMine(Rectangle r, out int x, out int y)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (Object.ReferenceEquals(r, rectangles[i, j]))
                    {
                        x = i;
                        y = j;
                        return mines[i, j];
                    }
                }
            }
            throw new Exception();
        }

        public bool InBorder(int x, int y)
        {
            return (x >= 0 && x < row && y >= 0 && y < col);
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
                    if (InBorder(i, j))
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
                    if (InBorder(i, j) && mines[i,j].is_cover)
                    {
                        if(!simplified)
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
            foreach(Position p in result)
            {
                //Console.WriteLine("here{0}", p.x);
            }
            return result;
        }

        public void WinWindow()
        {
            string messageBoxText = "You just win !";
            string caption = "Minesweeper";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        public void LoseWindow()
        {
            string messageBoxText = "You just hit a mine!";
            string caption = "Minesweeper";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        public void Restart()                           //much more things to do
        {
            in_game = false;
            first_click = true;

            foreach (Rectangle r in rectangles)
            {
                r.Fill = Brushes.AliceBlue;
            }
            foreach (Border b in borders)
            {
                b.Background = new SolidColorBrush(Colors.Tan);
            }

            game = new Game();
            number_show = game.Generate(row, col, mine_number);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    mines[i, j] = mymine;
                }
            }

        }

        public void Restart(int x, int y)               //opt needed
        {
            while (game.GetMineCount(x, y) == -1)
            {
                game = new Game();
                number_show = game.Generate(row, col, mine_number);
            }
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    mines[i, j] = mymine;
                }
            }

            OpenBlock(x, y);
            if (game.IsFinish(mines))
            {
                WinWindow();
                Restart();
            }
        }

        #endregion

        #region AutoPlay
        public void SimpleTest(AutoPlay f)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (!mines[i, j].is_cover)
                        f(i, j);
                }
            }
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
                        if (InBorder(i, j) && mines[i, j].is_cover)
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
                LRClick(x, y);
                if (unflag_count != 0)
                    return true;
            }
            return false;
        }

        public bool ComplexFlag(int x, int y)//analyze in 3*3, opt needed
        {
            List<SolveUnit> units = new List<SolveUnit> { };
            SolveUnit center = new SolveUnit(mines[x, y].mine_count);

            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (InBorder(i, j) && !mines[i, j].is_cover)
                    {
                        SolveUnit edge = new SolveUnit(mines[i, j].mine_count);
                        for (int m = i - 1; m < i + 2; m++)
                        {
                            for (int n = j - 1; n < j + 2; n++)
                            {
                                if(InBorder(m,n) && InBorderNineByNine(m, n, x - 1, y - 1) 
                                    && mines[m,n].is_cover)
                                {
                                    edge.AddPosition(m, n);
                                }
                            }
                        }
                        units.Add(edge);

                    }
                    if (InBorder(i, j) && mines[i, j].is_cover)
                    {
                        center.AddPosition(i, j);
                    }
                }
            }

            for(int i = 0; i< units.Count; i++)
            {
                int diff = center.mine_count - units[i].mine_count;//at least diff mines
                int diff_block = center.Blocks.Count - units[i].Blocks.Count;
                if(diff == diff_block)
                {
                    center.Blocks.ExceptWith(units[i].Blocks);
                    foreach(Position p in center.Blocks)
                    {
                        mines[p.x, p.y].is_flag = true;
                        rectangles[p.x, p.y].Fill = BlockBrush.flag;
                        return true;
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
                    if (InBorder(i, j) && !mines[i, j].is_cover)
                    {
                        SolveUnit edge = new SolveUnit(mines[i, j].mine_count);
                        edge.Simplified_blocks = GetBlockSet(i, j, true);
                        edge.comfirmed_mine_count = mines[i, j].mine_count - GetInfo(i, j).flag_count;
                        units.Add(edge);
                    }
                }
            }
            Console.WriteLine("HHHHHHHHHHHHHHH {0} {1} {2}",x,y,units.Count);
            foreach(SolveUnit s in units)
            {
                s.Print();
            }
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Simplified_blocks.IsProperSupersetOf(center.Simplified_blocks) && 
                    units[i].comfirmed_mine_count == center.comfirmed_mine_count)
                {
                    Console.WriteLine("HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH");
                    center.Simplified_blocks.ExceptWith(units[i].Simplified_blocks);
                    foreach(Position p in center.Simplified_blocks)
                    {
                        OpenBlock(p.x, p.y);//win or lose
                    }
                    return true;
                }
            }


            return false;
        }

        public bool CompleteAnalyze(int x, int y)
        {
            return true;
        }

        #endregion
    }
}
