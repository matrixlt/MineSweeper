using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MineSweeper
{
    public class MainWindowViewModel
    {
        private bool in_game = false;
        private bool first_click = true;

        private int row;
        private int col;


        List<Rectangle> mines_set = new List<Rectangle> { };
        Mine[,] mines = null;
        int[,] number_show = null;
        NumberBrush myBrush = new NumberBrush();

        Game game = new Game();

        public MainWindowViewModel()
        {
            this.row = 10;
            this.col = 10;
            mines = new Mine[row, col];
            number_show =  game.Generate(row, col, 5);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i,j));
                    Rectangle mine = new Rectangle();
                    mines[i, j] = mymine;
                    mines_set.Add(mine);
                    mine.Fill = Brushes.AliceBlue;
                    mine.MouseLeftButtonUp += new MouseButtonEventHandler(click);
                }
            }
        }

        private void click(object sender, MouseButtonEventArgs e)
        {
            if (first_click)
            {
                in_game = true;
                first_click = false;        //TODO start a game or modify sth ,start the timer or sth
            }

            Rectangle s = (Rectangle)sender;
            Mine mine = WhichMine(s);       //FIX LATER

            Console.WriteLine(sender.ToString());
            Console.WriteLine(mine.mine_count);

            if (mine.is_cover)
            {
                if (mine.is_mine)
                {
                    in_game = false;
                    string messageBoxText = "You just hit a mine!";
                    string caption = "Minesweeper";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Information;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                    Restart();
                }
                else
                {
                    s.Fill = NumberBrush.numbers[mine.mine_count];
                    mine.is_cover = false;
                }
            }

            if (game.IsFinish(mines))
            {
                string messageBoxText = "You just win !";
                string caption = "Minesweeper";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Information;
                MessageBox.Show(messageBoxText, caption, button, icon);
                Restart();
            }
            
        }

        public int Row { get; set; }
        public int Col { get; set; }
        public List<Rectangle> MinesSet { get { return mines_set; } }

        public Mine WhichMine(Rectangle r)
        {
            for(int i = 0; i < mines_set.Count; i++)
            {
                if (Object.ReferenceEquals(r, mines_set[i]))
                {
                    Console.WriteLine("{0} {1} {2}",i, i / row, i % row);
                    return mines[i / row, i % row];
                }
            }

            throw new Exception();
        }

        public void Restart()                           //much more things to do
        {
            in_game = false;
            first_click = true;

            foreach (Rectangle r in mines_set)
            {
                r.Fill = Brushes.AliceBlue;
            }

            game = new Game();
            number_show = game.Generate(row, col, 5);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i, j));
                    mines[i, j] = mymine;
                }
            }

        }
    }
}
