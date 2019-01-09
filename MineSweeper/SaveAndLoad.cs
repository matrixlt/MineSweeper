using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    class SaveAndLoad
    {
        public static String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Minesweeper\\";
        public static void Save(int row, int col, int[,] is_mine, string path)
        {
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(row.ToString() + " " + col.ToString());
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < col; j++)
                        {
                            sw.Write(is_mine[i, j].ToString() + " ");
                            Console.WriteLine(is_mine[i, j]);
                        }

                        sw.WriteLine();
                    }
                }
            }
        }

        public static int[,] Load(string path)
        {
            int row = 0;
            int col = 0;
            bool test = true;
            int[,] result = null;
            int count = 0;
            using (StreamReader sr = File.OpenText(path))
            {
                while (true)
                {
                    string s;
                    if (row == 0)
                    {
                        s = sr.ReadLine();
                        if (s == null)
                            throw new Exception();
                        test = test && Int32.TryParse(s.Split(' ')[0], out row);
                        test = test && Int32.TryParse(s.Split(' ')[1], out col);
                        if (!test || row == 0 || col == 0)
                        {
                            throw new Exception();
                        }
                        result = new int[row, col];
                    }
                    else
                    {
                        if (count == row)
                            return result;
                        s = sr.ReadLine();
                        if (s == null)
                            throw new Exception();
                        String[] numbers = s.Split(' ');
                        for (int i = 0; i < col; i++)
                        {
                            test = Int32.TryParse(numbers[i], out result[count, i]);
                        }
                        count++;
                    }
                }
            }
        }
    }
}
