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

        public int row;
        public int col;
        private int mine_number;
        private bool both_down = false;

        private int height;
        private int width;

        private bool first_interval = true;
        private DateTime start_time;
        private int time_span;
        private double total_time;
        private string show_time = "000";
        private int elapse_time;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private int count_flag = 0;
        private string left_mine;
        #endregion

        #region other member
        public event PropertyChangedEventHandler PropertyChanged;

        List<Border> borders = new List<Border> { };
        Mine[,] mines = null;
        public int[,] distribution = null;
        Rectangle[,] rectangles = null;
        BlockBrush myBrush = new BlockBrush();

        Game game = new Game();

        public delegate bool AutoPlay(int x, int y);
        public AutoPlayer player;
        #endregion

        #region constructor
        public MainWindowViewModel()
        {
            Distribution = game.Generate(10, 15, 15);
            this.row = game.row;
            this.col = game.col;
            this.mine_number = game.mine_number;
            this.Left_mine = (this.mine_number - this.Count_flag).ToString("D3");

            Ininitialize(game);
            Height = 25 * Row;
            Width = 25 * Col;
            player = new AutoPlayer(row, col, Mines, Rectangles);
            player.inBorder = InBorder;
            player.lRClick = LRClick;
            player.openBlock = OpenBlock;
            player.openEmpty = OpenEmpty;
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
                        if (InBorder(i, j) && Mines[i, j].is_flag)
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
                first_click = false;
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
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
                    borders[x * col + y].Background = new SolidColorBrush(Colors.Red);
                    dispatcherTimer.Stop();
                    total_time = 0;
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

            if (game.IsFinish(Mines))
            {
                dispatcherTimer.Stop();
                total_time = 0.001 * ((DateTime.Now - start_time).TotalMilliseconds % 1000) + (DateTime.Now - start_time).TotalMilliseconds / 1000;
                Left_mine = "000";
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
                    this.Count_flag -= 1;
                    this.Left_mine = (this.mine_number - this.Count_flag).ToString("D3");
                    mine.is_flag = false;
                    s.Fill = Brushes.AliceBlue;
                    //Console.WriteLine(this.Left_mine);
                }
                else
                {
                    this.Count_flag += 1;
                    this.Left_mine = (this.mine_number - this.Count_flag).ToString("D3");
                    mine.is_flag = true;
                    s.Fill = BlockBrush.flag;
                    //Console.WriteLine(this.Left_mine);
                }
            }
        }
        #endregion

        #region block operation
        public bool OpenBlock(int x, int y)
        {
            if (!Mines[x, y].is_mine)
                Rectangles[x, y].Fill = BlockBrush.numbers[Mines[x, y].mine_count];
            else
            {
                Rectangles[x, y].Fill = BlockBrush.mine;
                borders[x * col + y].Background = new SolidColorBrush(Colors.Red);
                LoseWindow();
                Restart();
                return true;
            }
            Mines[x, y].is_cover = false;
            if (game.IsFinish(Mines))
            {
                this.Left_mine = "000";
                WinWindow();
                Restart();
                return true;
            }
            return false;
        }

        private void OpenEmpty(int x, int y)//opt needed
        {
            OpenBlock(x, y);

            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (InBorder(i, j))
                    {
                        if (Mines[i, j].mine_count == 0 && Mines[i, j].is_cover)
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

        public void LRClick(int x, int y)//in or lose in it
        {
            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (InBorder(i, j) && !Mines[i, j].is_flag)
                    {
                        if (Mines[i, j].mine_count == 0)
                            OpenEmpty(i, j);
                        else
                        {
                            if (OpenBlock(i, j)) return;
                        }
                    }

                }
            }
        }
        #endregion

        #region binding
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (first_interval == true)
            {
                first_interval = false;
                start_time = DateTime.Now;
                time_span = 0;
                this.Show_time = Convert.ToInt64(time_span).ToString("D3");
            }
            else
            {
                elapse_time = (int)(DateTime.Now - start_time).TotalSeconds - time_span;
                if (elapse_time >= 1)
                {
                    time_span = elapse_time + time_span;
                    if (time_span > 999)
                    {
                        this.Show_time = Convert.ToInt64(time_span - 1000).ToString("D3");
                    }
                    else
                    {
                        this.Show_time = Convert.ToInt64(time_span).ToString("D3");
                    }
                }
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

        public int Row
        {
            get { return row; }
            set
            {
                row = value;
                OnPropertyChanged("Row");
            }
        }
        public int Col
        {
            get { return col; }
            set
            {
                col = value;
                OnPropertyChanged("Col");
            }
        }
        public List<Border> BorderSet
        {
            get { return borders; }
            set
            {
                borders = value;
                OnPropertyChanged("BorderSet");
            }
        }
        public string Show_time
        {
            get => show_time;
            set
            {
                show_time = value;
                OnPropertyChanged("Show_time");
            }
        }

        public Mine[,] Mines { get => mines; set => mines = value; }
        public Rectangle[,] Rectangles { get => rectangles; set => rectangles = value; }
        public int[,] Distribution { get => distribution; set => distribution = value; }
        public int Height
        {
            get => height; set
            {
                height = value;
                OnPropertyChanged("Height");
            }
        }
        public int Width
        {
            get => width; set
            {
                width = value;
                OnPropertyChanged("Width");

            }
        }

        public string Left_mine
        {
            get => left_mine;
            set
            {
                left_mine = value;
                OnPropertyChanged("Left_mine");
            }
        }
        public int Count_flag { get => count_flag; set => count_flag = value; }
        #endregion

        #region helpers in class
        public void Ininitialize(Game game)
        {
            Mines = new Mine[game.row, game.col];
            Rectangles = new Rectangle[game.row, game.col];
            List<Border> newBorderSet = new List<Border> { };
            for (int i = 0; i < game.row; i++)
            {
                for (int j = 0; j < game.col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    Rectangle myrectangle = new Rectangle();

                    Border border = new Border();
                    border.Child = myrectangle;
                    newBorderSet.Add(border);
                    border.Background = new SolidColorBrush(Colors.Tan);
                    border.BorderThickness = new Thickness(1);

                    Mines[i, j] = mymine;
                    Rectangles[i, j] = myrectangle;

                    myrectangle.Fill = Brushes.AliceBlue;
                    myrectangle.PreviewMouseRightButtonUp += new MouseButtonEventHandler(RClick);
                    myrectangle.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(LClick);
                    myrectangle.PreviewMouseDown += new MouseButtonEventHandler(MouseDown);
                    myrectangle.PreviewMouseUp += new MouseButtonEventHandler(MouseUp);

                }
            }
            BorderSet = newBorderSet;
        }

        public Mine WhichMine(Rectangle r, out int x, out int y)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (Object.ReferenceEquals(r, Rectangles[i, j]))
                    {
                        x = i;
                        y = j;
                        return Mines[i, j];
                    }
                }
            }
            throw new Exception();
        }

        public bool InBorder(int x, int y)
        {
            return (x >= 0 && x < row && y >= 0 && y < col);
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

        public void Restart()                           //much more things to do,such as frash AutoPlayer
        {
            in_game = false;
            first_click = true;
            first_interval = true;

            dispatcherTimer.Stop();
            this.Show_time = "000";

            foreach (Rectangle r in Rectangles)
            {
                r.Fill = Brushes.AliceBlue;
            }
            foreach (Border b in borders)
            {
                b.Background = new SolidColorBrush(Colors.Tan);
            }

            game = new Game();
            this.Count_flag = 0;
            this.Left_mine = (this.mine_number - this.Count_flag).ToString("D3");
            Distribution = game.Generate(row, col, mine_number);
            player.SetProperties(row, col, Mines, Rectangles);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    Mines[i, j] = mymine;
                }
            }

        }

        public void Restart(int x, int y)               //opt needed
        {
            while (game.GetMineCount(x, y) == -1)
            {
                game = new Game();
                Distribution = game.Generate(row, col, mine_number);
            }
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    Mines[i, j] = mymine;
                }
            }
            if (Mines[x, y].mine_count != 0)
                OpenBlock(x, y);
            else OpenEmpty(x, y);

            player.SetProperties(row, col, Mines, Rectangles);

            if (game.IsFinish(Mines))
            {
                Left_mine = "000";
                WinWindow();
                Restart();
            }
        }

        public void Restart(int[,] distribution)
        {
            in_game = false;
            first_click = true;
            first_interval = true;

            dispatcherTimer.Stop();
            this.Show_time = "000";

            game = new Game(distribution);
            Row = game.row;
            Col = game.col;
            this.Count_flag = 0;
            this.Left_mine = (this.mine_number - this.Count_flag).ToString("D3");

            Ininitialize(game);
            Height = 25 * Row;
            Width = 25 * Col;
            player.SetProperties(row, col, Mines, Rectangles);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    Mines[i, j] = mymine;
                }
            }

        }
        #endregion
    }
}
