using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MineSweeper
{
    public class MainWindowViewModel
    {
        private bool in_game;
        private int row;
        private int col;
        List<Rectangle> mines_set = new List<Rectangle> { };
        Mine[,] mines = null;
        int[,] number_show = null;
        NumberBrush myBrush = new NumberBrush();

        Game game = new Game();

        public MainWindowViewModel()
        {
            this.row = 20;
            this.col = 20;
            mines = new Mine[row, col];
            number_show =  game.Generate(row, col, 100);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine(game.GetMineCount(i,j));
                    Rectangle mine = mymine.graph;
                    mines[i, j] = mymine;
                    mines_set.Add(mine);
                    mine.Fill = Brushes.AliceBlue;
                    mine.MouseLeftButtonUp += new MouseButtonEventHandler(click);
                }
            }
        }

        private void click(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(sender.ToString());
            Rectangle s = (Rectangle)sender;
            Mine mine = WhichMine(s);
            Console.WriteLine(mine.mine_count);
            if(mine.mine_count > 0)
                s.Fill = NumberBrush.numbers[mine.mine_count];
            
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
                    return mines[i / row, i % row];
                }
            }

            throw new Exception();
        }
    }
}
