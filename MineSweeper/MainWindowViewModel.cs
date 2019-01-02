using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private int mine_number;

        List<Rectangle> mines_set = new List<Rectangle> { };
        List<Border> borders = new List<Border> { };

        Mine[,] mines = null;
        int[,] number_show = null;
        NumberBrush myBrush = new NumberBrush();

        Game game = new Game();

        public MainWindowViewModel()
        {
            this.row = 10;
            this.col = 10;
            this.mine_number = 15;
            mines = new Mine[row, col];
            number_show =  game.Generate(row, col, mine_number);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i,j));
                    Rectangle mine = new Rectangle();

                    Border border = new Border();
                    border.Child = mine;
                    borders.Add(border);
                    border.Background = new SolidColorBrush(Colors.Tan);
                    border.BorderThickness = new Thickness(1);

                    mines[i, j] = mymine;
                    mines_set.Add(mine);
                    mine.Fill = Brushes.AliceBlue;
                    mine.MouseLeftButtonUp += new MouseButtonEventHandler(click);
                }
            }
        }

        private void OpenBlock(int x, int y)
        {
            mines_set[x * row + y].Fill = NumberBrush.numbers[mines[x,y].mine_count];
            mines[x, y].is_cover = false;
        }

        private void OpenEmpty(int x, int y)
        {
            OpenBlock(x, y);

            for (int i = x - 1; i < x + 2; i++)
            {
                for(int j = y - 1; j < y + 2; j++)
                {
                    if (i >= 0 && i < row && j >= 0 && j < col)
                    {
                        if(mines[i,j].mine_count == 0 && mines[i,j].is_cover)
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

        private void click(object sender, MouseButtonEventArgs e)
        {
            

            Rectangle s = (Rectangle)sender;
            int x, y;                                     //position
            Mine mine = WhichMine(s,out x, out y);       //FIX LATER
            
            Console.WriteLine(sender.ToString());
            Console.WriteLine(mine.mine_count);

            if (first_click)
            {                                       //TODO start a game or modify sth ,start the timer or sth
                in_game = true;
                first_click = false;
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
                    LostWindow();
                    Restart();
                }
                else
                {
                    
                    if(mine.mine_count == 0)
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

        public int Row { get; set; }
        public int Col { get; set; }
        public List<Rectangle> MinesSet { get { return mines_set; } }
        public List<Border> BorderSet { get { return borders; } }

        public Mine WhichMine(Rectangle r, out int x, out int y)
        {
            for(int i = 0; i < mines_set.Count; i++)
            {
                if (Object.ReferenceEquals(r, mines_set[i]))
                {
                    Console.WriteLine("{0} {1} {2}",i, i / row, i % row);
                    x = i / row;
                    y = i % row;
                    return mines[i / row, i % row];
                }
            }

            throw new Exception();
        }

        public void WinWindow()
        {
            string messageBoxText = "You just win !";
            string caption = "Minesweeper";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        public void LostWindow()
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

            foreach (Rectangle r in mines_set)
            {
                r.Fill = Brushes.AliceBlue;
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
            while(game.GetMineCount(x,y) == -1)
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
    }
}
