using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
        private bool? last_win = null;
        private GameState game_state = GameState.NotStart;
        private bool test_mode = false;
        private bool cheat_mode = false;

        private int row;
        private int col;
        private int mine_number;
        private bool both_down = false;

        private int mine_size = 25;
        private int height_margin = 150;
        private int width_margin = 75;
        private int height;
        private int width;
        private int main_height;
        private int main_width;

        private bool first_interval = true;
        private DateTime start_time;
        private int time_span;
        private string show_time = "000";
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private int count_flag = 0;
        private string left_mine;

        private Random random = new Random(DateTime.Now.Millisecond + 97);
        #endregion

        #region other member
        public event PropertyChangedEventHandler PropertyChanged;

        List<Border> borders = new List<Border> { };
        Mine[,] mines = null;
        public int[,] distribution = null;
        Rectangle[,] rectangles = null;
        BlockBrush myBrush = new BlockBrush();

        Game game = null;

        public AutoPlayer player;

        public Record record = null;

        #endregion

        #region constructor
        public MainWindowViewModel()
        {
            game = new Game();
            game.Random = random;
            Distribution = game.Generate(16, 30, 99);
            game.game_type = GameType.Expert;
            Row = game.row;
            Col = game.col;
            Mine_number = game.mine_number;
            Left_mine = (Mine_number - Count_flag).ToString("D3");

            Ininitialize(game);
            Height = mine_size * Row;
            Width = mine_size * Col;
            Main_height = Height + height_margin;
            Main_width = Width + width_margin;

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            player = new AutoPlayer(row, col, Mines, Rectangles);
            player.inBorder = InBorder;
            player.lRClick = LRClick;
            player.openBlock = OpenBlock;
            player.flagBlock = FlagBlock;
            player.borders = BorderSet;
            player.getGameState = GetGameState;

            record = new Record();
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

            if (mine.is_flag || !mine.is_cover)
                return;

            OpenBlock(x, y);

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
                    Count_flag -= 1;
                    Left_mine = (Mine_number - Count_flag).ToString("D3");
                    mine.is_flag = false;
                    s.Fill = Brushes.AliceBlue;
                    //Console.WriteLine(this.Left_mine);
                }
                else
                {
                    this.Count_flag += 1;
                    Left_mine = (Mine_number - Count_flag).ToString("D3");
                    FlagBlock(x, y);
                    //Console.WriteLine(this.Left_mine);
                }
            }
        }
        #endregion

        #region block operation
        public void OpenBlock(int x, int y)
        {
            if (Game_state == GameState.Win || Game_state == GameState.Lose)//no move
                return;
            if (first_click)
            {                                       //TODO start a game or modify sth ,start the timer or sth
                In_game = true;
                first_click = false;
                Game_state = GameState.On;

                dispatcherTimer.Start();
                if (mines[x, y].is_mine)
                {
                    Restart(x, y);                  //to ensure not hit mine at first move
                    return;                   //not right, but temp
                }

            }

            Mines[x, y].is_cover = false;
            if (!Mines[x, y].is_mine)
            {
                if (!test_mode)
                    Rectangles[x, y].Fill = BlockBrush.numbers[Mines[x, y].mine_count];
            }
            else
            {
                Rectangles[x, y].Fill = BlockBrush.mine;
                borders[x * col + y].Background = new SolidColorBrush(Colors.Red);
                LoseGame();
                Game_state = GameState.Lose;
                //Restart();
                return;
            }

            if (Mines[x, y].Is_blank)
            {
                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (InBorder(i, j) && Mines[i, j].is_cover)
                        {
                            OpenBlock(i, j);
                        }
                    }
                }
            }


            if (game.IsFinish(Mines))
            {
                Left_mine = "000";
                WinGame();
                Game_state = GameState.Win;
                //Console.WriteLine("test {0} {1}", x, y);
                //Restart();
                return;
            }

            return;
        }

        public void LRClick(int x, int y)//win or lose in it
        {
            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (InBorder(i, j) && !Mines[i, j].is_flag)
                    {
                        OpenBlock(i, j);
                        if (Game_state == GameState.Lose || Game_state == GameState.Win)
                            return;
                    }

                }
            }
        }

        public void FlagBlock(int x, int y)//one way, just flag, not include unflag
        {
            if (Game_state == GameState.Win || Game_state == GameState.Lose)//no move
                return;

            Mines[x, y].is_flag = true;
            if (test_mode)
                return;
            Rectangles[x, y].Fill = BlockBrush.flag;
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
                Show_time = Convert.ToInt64(time_span).ToString("D3");
            }
            else
            {
                int elapse_time = (int)(DateTime.Now - start_time).TotalSeconds - time_span;
                if (elapse_time >= 1)
                {
                    time_span = elapse_time + time_span;
                    if (time_span <= 999)
                    {
                        Show_time = Convert.ToInt64(time_span).ToString("D3");
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
        //LZS adding Mine_number
        public int Mine_number
        {
            get { return mine_number; }
            set
            {
                mine_number = value;
                OnPropertyChanged("Mine_number");
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

        public int Main_height
        {
            get => main_height; set
            {
                main_height = value;
                OnPropertyChanged("Main_height");
            }
        }

        public int Main_width
        {
            get => main_width; set
            {
                main_width = value;
                OnPropertyChanged("Main_width");
            }
        }

        public bool In_game { get => in_game; set => in_game = value; }
        public bool? Last_win { get => last_win; set => last_win = value; }
        public bool Test_mode { get => test_mode; set => test_mode = value; }

        public int Count_flag { get => count_flag; set => count_flag = value; }
        public GameType Type { get => game.game_type; set => game.game_type = value; }
        public bool Cheat_mode { get => cheat_mode; set => cheat_mode = value; }
        public bool Is_Finished { get => game.IsFinish(Mines); }
        public GameState Game_state { get => game_state; set => game_state = value; }
        public GameState GetGameState()
        {
            return Game_state;
        }
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

        public void WinGame()
        {
            dispatcherTimer.Stop();

            Last_win = true;
            if (Test_mode)
                return;
            string messageBoxText = "You just win !";
            if (!Cheat_mode && time_span < record.GetRecord(game.game_type))
            {
                record.SetRecord(game.game_type, time_span);
                messageBoxText += "And you just break a record!";
            }


            messageBoxText += "\n Do you want to save this game?";
            string caption = "Minesweeper";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Information;

            var result = MessageBox.Show(messageBoxText, caption, button, icon);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveAndLoad.Save(Row, Col, Distribution, SaveAndLoad.path +
                        "Minesweeper" + DateTime.Now.ToString().Replace(":", "-").Replace(" ", "-").Replace("/", "-") + ".txt");
                    break;
                case MessageBoxResult.No:
                    break;
            }

        }

        public void LoseGame()
        {
            dispatcherTimer.Stop();

            Last_win = false;
            if (Test_mode)
                return;
            SaveAndLoad.Save(Row, Col, Distribution, SaveAndLoad.path + Game.id.ToString() + ".txt");
            string messageBoxText = "You just hit a mine!";
            messageBoxText += "\n Do you want to save this game?";
            string caption = "Minesweeper";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Information;

            var result = MessageBox.Show(messageBoxText, caption, button, icon);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveAndLoad.Save(Row, Col, Distribution, SaveAndLoad.path +
                        "Minesweeper" + DateTime.Now.ToString().Replace(":", "-").Replace(" ", "-").Replace("/", "-") + ".txt");
                    break;
                case MessageBoxResult.No:
                    break;
            }

        }

        #endregion

        #region restart a game
        public void Restart()                           //much more things to do,such as frash AutoPlayer
        {
            In_game = false;
            first_click = true;
            Game_state = GameState.NotStart;
            first_interval = true;

            dispatcherTimer.Stop();
            Show_time = "000";

            foreach (Rectangle r in Rectangles)
            {
                r.Fill = Brushes.AliceBlue;
            }
            foreach (Border b in borders)
            {
                b.Background = new SolidColorBrush(Colors.Tan);
            }
            Cheat_mode = false;
            GameType temp = game.game_type;             //fix later
            game = new Game();
            game.game_type = temp;


            game.Random = random;
            Count_flag = 0;
            Left_mine = (Mine_number - Count_flag).ToString("D3");

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
            GameType temp = game.game_type;                 //fix later

            while (game.GetMineCount(x, y) == -1)
            {
                game = new Game();
                game.Random = random;
                Distribution = game.Generate(row, col, mine_number);
            }
            game.game_type = temp;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    Mines[i, j] = mymine;
                }
            }

            OpenBlock(x, y);


            player.SetProperties(row, col, Mines, Rectangles);

            if (game.IsFinish(Mines))
            {
                Left_mine = "000";
                WinGame();
                Restart();
            }
        }

        public void Restart(int[,] distribution)
        {
            In_game = false;
            first_click = true;
            Game_state = GameState.NotStart;
            first_interval = true;

            dispatcherTimer.Stop();
            Show_time = "000";

            game = new Game(distribution);
            game.Random = random;
            Row = game.row;
            Col = game.col;
            Mine_number = game.mine_number;
            Count_flag = 0;
            Left_mine = (Mine_number - Count_flag).ToString("D3");

            Ininitialize(game);
            Height = mine_size * Row;
            Width = mine_size * Col;
            Main_height = Height + height_margin;
            Main_width = Width + width_margin;
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

        public void Restart(int row, int col, int mine_count)
        {
            In_game = false;
            first_click = true;
            Game_state = GameState.NotStart;
            first_interval = true;

            dispatcherTimer.Stop();
            Show_time = "000";

            game = new Game();
            game.Random = random;
            Distribution = game.Generate(row, col, mine_count);
            Row = game.row;
            Col = game.col;
            Mine_number = game.mine_number;
            Count_flag = 0;
            Left_mine = (Mine_number - Count_flag).ToString("D3");

            Ininitialize(game);
            Height = mine_size * Row;
            Width = mine_size * Col;
            Main_height = Height + height_margin;
            Main_width = Width + width_margin;
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
