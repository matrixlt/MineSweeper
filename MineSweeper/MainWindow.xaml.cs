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
        #region first menu one step operation
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

        private void SealedBlock(object sender, RoutedEventArgs e)
        {
            VM.player.SealedBlock();
        }
        #endregion

        #region second menu two
        private void SimpleTest(object sender, RoutedEventArgs e)//rare bug??
        {
            SimpleSolve();
        }

        private void ComplexTest(object sender, RoutedEventArgs e)//bug not fixed
        {
            bool test = false;
            while (true)
            {
                Console.WriteLine("0");
                SimpleSolve();
                Console.WriteLine("1{0}", test);
                test = test || VM.player.SimpleTest(VM.player.ComplexClick);
                Console.WriteLine("2{0}", test);
                test = test || VM.player.SimpleTest(VM.player.ComplexFlag);
                Console.WriteLine("3{0}", test);
                test = test || VM.player.SimpleTest(VM.player.UncertainComplexFlag);
                Console.WriteLine("4{0}", test);
                test = test || VM.player.SimpleTest(VM.player.CompleteAnalyze);
                Console.WriteLine("5{0}", test);
                if (!test)
                { return; }
                else { test = false; }

            }
        }

        private void SimpleSolve()
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
        #endregion

        #region LZS adding settingWindow
        private void SettingClick(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow(VM.Row, VM.Col, VM.Mine_number);
            if (settingWindow.ShowDialog() == true)
            {
                VM.Col = int.Parse(settingWindow.colsSetting.Text);
                VM.Row = int.Parse(settingWindow.rowsSetting.Text);
                VM.Mine_number = int.Parse(settingWindow.minesSetting.Text);
                VM.Height = 25 * VM.Row;
                VM.Width = 25 * VM.Col;
                settingWindow.Close();
                VM.Restart(VM.Row, VM.Col, VM.Mine_number); //need to change the Distribution[,]
            }
        }


        #endregion

        #region third menu 
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
            else return;
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
        #endregion
    }
}
