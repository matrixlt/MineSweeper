using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    public class Record
    {
        private int beginner_best = 999;
        private int intermediate_best = 999;
        private int expert_best = 999;
        private string path;
        public Record()
        {
            String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Minesweeper\\";
            System.IO.Directory.CreateDirectory(path);
            path += "record.txt";
            this.path = path;
            if (!File.Exists(path))
            {
                Reset();
            }
            else
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    int count = 0;
                    bool test = true;
                    string s;
                    while ((s = sr.ReadLine()) != null && count < 3)
                    {
                        if (count == 0)
                            test = test && Int32.TryParse(s, out beginner_best);
                        if (count == 1)
                            test = test && Int32.TryParse(s, out intermediate_best);
                        if (count == 2)
                            test = test && Int32.TryParse(s, out expert_best);

                        count++;
                    }

                    if (test && count == 3)
                        Console.WriteLine("Load Record Successful");
                    else
                        Console.WriteLine("Load Record Failed");
                }
            }
        }

        public int Beginner_best
        {
            get => beginner_best; set
            {
                beginner_best = value;
                Set();

            }
        }
        public int Intermediate_best
        {
            get => intermediate_best; set
            {
                intermediate_best = value;
                Set();
            }
        }
        public int Expert_best
        {
            get => expert_best; set
            {
                expert_best = value;
                Set();
            }
        }

        public void Reset()
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("999");
                sw.WriteLine("999");
                sw.WriteLine("999");
            }
            beginner_best = 999;
            intermediate_best = 999;
            expert_best = 999;
        }

        public void Set()
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(beginner_best);
                sw.WriteLine(intermediate_best);
                sw.WriteLine(expert_best);
            }
        }

        public int GetRecord(GameType type)
        {
            if (type == GameType.Beginner)
                return Beginner_best;
            if (type == GameType.Intermediate)
                return Intermediate_best;
            if (type == GameType.Expert)
                return Expert_best;

            return -1;
        }

        public void SetRecord(GameType type, int time)
        {
            if (type == GameType.Beginner)
                Beginner_best = time;
            if (type == GameType.Intermediate)
                Intermediate_best = time;
            if (type == GameType.Expert)
                Expert_best = time;
        }
    }
}
