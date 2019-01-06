using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    class Game
    {
        public static int id = 0;
        public bool is_over;
        public bool win;

        public int row;
        public int col;
        public int mine_number;

        public int[,] distribution = null;


        public Game()
        {
            id += 1;
            is_over = false;
            win = false;
        }

        public static void Shuffle(int[] list)
        {
            Random random = new Random();
            int n = list.Length;

            for (int i = list.Length - 1; i > 1; i--)
            {
                int rnd = random.Next(i + 1);

                int value = list[rnd];
                list[rnd] = list[i];
                list[i] = value;
            }
        }

        public int[,] Generate(int row, int col, int mine_number)
        {
            this.row = row;
            this.col = col;
            this.mine_number = mine_number;

            int[] temp = new int[row * col];
            distribution = new int[row, col];

            for (int i = 0; i < row * col; i++)
            {
                if (i < mine_number)
                {
                    temp[i] = 1;
                }
                else
                {
                    temp[i] = 0;
                }
            }

            Shuffle(temp);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    distribution[i, j] = temp[i * col + j];
                }
            }

            return distribution;
        }

        public int GetMineCount(int i, int j)
        {
            if (i < 0 || i >= row || j < 0 || j >= col)
            {
                throw new Exception();
            }

            if (distribution[i, j] == 1)
            {
                return -1; //is mine
            }

            else//not mine
            {
                int count = 0;
                for (int m = i - 1; m < i + 2; m++)
                {
                    for (int n = j - 1; n < j + 2; n++)
                    {
                        count += IsMine(m, n);
                    }
                }
                return count;
            }
        }

        public int IsMine(int i, int j)
        {
            if (i < 0 || i >= row || j < 0 || j >= col)
            {
                return 0;
            }
            else
            {
                return distribution[i, j];
            }
        }

        public bool IsFinish(Mine[,] mines)// can be opt
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (distribution[i, j] == 0 && mines[i, j].is_cover)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
