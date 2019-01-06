using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MineSweeper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel VM;
        SaveAndLoad SL;
        public MainWindow()
        {
            VM = new MainWindowViewModel();
            SL = new SaveAndLoad();
            InitializeComponent();
            DataContext = VM;

        }

        private void Restart_Click(object sender, RoutedEventArgs e)//maybe should be in other place
        {
            VM.Restart();
        }

        private void SimpleFlag(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleTest(VM.player.SimpleFlag);
        }

        private void SimpleClick(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleTest(VM.player.SimpleClick);
        }

        private void ComplexFlag(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleTest(VM.player.ComplexFlag);
        }

        private void UncertainComplexFlag(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleTest(VM.player.UncertainComplexFlag);
        }

        private void ComplexClick(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleTest(VM.player.ComplexClick);
        }

        private void CompleteAnalyze(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleTest(VM.player.CompleteAnalyze);
        }

        private void SimpleTest(object sender, RoutedEventArgs e)//rare bug??
        {
            bool click = true;
            bool flag = true;
            int count_click = 0;
            int count_flag = 0;
            int record_click = 0;
            int record_flag = 0;
            int i = 0;
            while (true)
            {
                click = true;
                record_click = count_click;
                record_flag = count_flag;

                flag = VM.player.SimpleTest(VM.player.SimpleFlag);
                if (flag)
                    count_flag++;

                while (click)
                {
                    click = VM.player.SimpleTest(VM.player.SimpleClick);
                    if (click)
                        count_click++;
                }
                i++;
                if (record_click == count_click && record_flag == count_flag)
                    break;


            }
        }

        private void ComplexTest(object sender, RoutedEventArgs e)
        {

        }

        private void SealedBlock(object sender, RoutedEventArgs e)
        {
            VM.player.SealedBlock();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            DateTime time = DateTime.Now;
            dlg.FileName = "Minesweeper" + time.ToString().Replace(":", "-").Replace(" ", "-").Replace("/", "-");
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Minesweeper (.txt)|*.txt";


            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                SaveAndLoad.Save(VM.row, VM.col, VM.distribution, filename);
            }


        }

        private void Load(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Minesweeper documents (.txt)|*.txt";
            int[,] distribution = null;

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                distribution = SaveAndLoad.Load(filename);
            }
            int row = distribution.GetLength(0);
            int col = distribution.GetLength(1);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Console.Write(distribution[i, j]);
                }
                Console.WriteLine();
            }
            VM.Restart(distribution);


        }

    }
}
