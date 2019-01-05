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
        public static void Save(int row, int col, int[,] is_mine, string path)
        {
            path += @"\test.txt";
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(row.ToString()+" "+col.ToString());
                    for(int i = 0; i< row; i++)
                    {
                        for(int j = 0; j < col; j++)
                        {
                            sw.Write(is_mine[i,j].ToString() + " ");
                            Console.WriteLine(is_mine[i, j]);
                        }
                        
                        sw.WriteLine();
                    }
                }
            }
        }
    }
}
