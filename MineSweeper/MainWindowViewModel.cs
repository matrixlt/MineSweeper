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
        List<Rectangle> mines = new List<Rectangle> { };
        Mine[,] game = null;

        public MainWindowViewModel()
        {
            this.row = 20;
            this.col = 20;
            game = new Mine[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Mine mymine = new Mine();
                    Rectangle mine = mymine.graph;
                    game[i, j] = mymine;
                    mines.Add(mine);
                    mine.Fill = Brushes.AliceBlue;
                    mine.MouseLeftButtonUp += new MouseButtonEventHandler(click);
                }
            }
        }

        private void click(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(sender.ToString());
            Rectangle s = (Rectangle)sender;
            if (s.Fill != System.Windows.Media.Brushes.SkyBlue)
                s.Fill = System.Windows.Media.Brushes.SkyBlue;
            else
                s.Fill = System.Windows.Media.Brushes.Brown;

        }
        public int Row { get; set; }
        public int Col { get; set; }
        public List<Rectangle> Mines { get { return mines; } }
    }
}
