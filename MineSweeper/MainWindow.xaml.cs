﻿using System;
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
        int delay = 500;
        public MainWindow()
        {
            VM = new MainWindowViewModel();
            SL = new SaveAndLoad();
            InitializeComponent();
            DataContext = VM;

        }

        #region Game
        private void Restart_Click(object sender, RoutedEventArgs e)//maybe should be in other place
        {
            VM.Restart();
        }

        private void ChooseGame(object sender, RoutedEventArgs e)
        {
            if (sender == beginner)
            {
                VM.Restart(8, 8, 10);
                VM.Type = GameType.Beginner;
                VM.Cheat_mode = false;
            }
            if (sender == intermediate)
            {
                VM.Restart(16, 16, 40);
                VM.Type = GameType.Intermediate;
                VM.Cheat_mode = false;
            }

            if (sender == expert)
            {
                VM.Restart(16, 30, 99);
                VM.Type = GameType.Expert;
                VM.Cheat_mode = false;
            }

        }

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

        private void ShowRecord(object sender, RoutedEventArgs e)
        {
            RecordWindow recordWindow = new RecordWindow(VM.record);
            if (recordWindow.ShowDialog() == true)
            {
                recordWindow.Close();
            }
        }

        #endregion

        #region AutoPlay

        private void SimpleFlag(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.SimpleTest(VM.player.SimpleFlag);
        }

        private void SimpleClick(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.SimpleTest(VM.player.SimpleClick);
        }

        private void ComplexFlag(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.SimpleTest(VM.player.ComplexFlag);
        }

        private void UncertainComplexFlag(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.SimpleTest(VM.player.UncertainComplexFlag);
        }

        private void ComplexClick(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.SimpleTest(VM.player.ComplexClick);
        }

        private void CompleteAnalyze(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.SimpleTest(VM.player.CompleteAnalyze);
        }

        private void SealedBlock(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.SealedBlock();
        }

        private void RandomClick(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.player.RandomClick();
        }

        #endregion

        #region AutoTest
        private void SimpleTest(object sender, RoutedEventArgs e)//rare bug??
        {
            VM.Cheat_mode = true;
            SimpleSolve();
        }

        private void ComplexTest(object sender, RoutedEventArgs e)//bug not fixed
        {
            VM.Cheat_mode = true;
            ComplexSolve();
        }

        private async void AutoTest(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            if (!VM.In_game)
            {
                if (VM.player.RandomClick())
                    return;
                else await Task.Delay(delay); 
            }
                
            while (VM.In_game)
            {
                ComplexSolve();
                await Task.Delay(delay);
                if (VM.In_game)
                {
                    if (VM.player.RandomClick())
                        return;
                    else await Task.Delay(delay);
                }
                    
            }
        }

        private void AutoTest100(object sender, RoutedEventArgs e)
        {
            VM.Cheat_mode = true;
            VM.Test_mode = true;
            int win = 0;
            int lose = 0;
            for (int i = 0; i < 100; i++)
            {
                if (!VM.In_game)
                    VM.player.RandomClick();
                while (VM.In_game)
                {
                    SimpleSolve();
                    ComplexSolve();
                    SimpleSolve();
                    if (VM.In_game)
                        VM.player.RandomClick();

                }
                if (VM.Last_win == true)
                {
                    win++;
                }
                else if (VM.Last_win == false)
                {
                    lose++;
                }
                Console.WriteLine("COUNT {0}", i);
            }
            VM.Test_mode = false;
            Console.WriteLine("WIN {0}   LOSE {1}", win, lose);
        }

        private async void SimpleSolve()
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
                {
                    await Task.Delay(delay);
                    count_flag++;
                }
                    

                while (click)
                {
                    click = VM.player.SimpleTest(VM.player.SimpleClick);
                    if (click)
                    {
                        await Task.Delay(delay);
                        count_click++;
                    }
                }
                i++;
                if (record_click == count_click && record_flag == count_flag)
                    break;
            }
        }

        private async void ComplexSolve()
        {
            bool test = false;
            while (true)
            {
                Console.WriteLine("0");
                SimpleSolve();
                {
                    if (VM.Is_Finished)
                        return;
                    await Task.Delay(delay);
                }
                Console.WriteLine("1{0}", test);
                test = test || VM.player.SimpleTest(VM.player.ComplexClick);
                if (test)
                {
                    if (VM.Is_Finished)
                        return;
                    await Task.Delay(delay);
                }

                Console.WriteLine("2{0}", test);
                test = test || VM.player.SimpleTest(VM.player.ComplexFlag);
                if (test)
                    await Task.Delay(delay);
                Console.WriteLine("3{0}", test);
                test = test || VM.player.SimpleTest(VM.player.UncertainComplexFlag);
                if (test)
                    await Task.Delay(delay);
                Console.WriteLine("4{0}", test);
                test = test || VM.player.SimpleTest(VM.player.CompleteAnalyze);
                if (test)
                {
                    if (VM.Is_Finished)
                        return;
                    await Task.Delay(delay);
                }
                    
                Console.WriteLine("5{0}", test);
                if (!test)
                { return; }
                else { test = false; }

            }
        }
        #endregion

        #region File
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
                SaveAndLoad.Save(VM.Row, VM.Col, VM.distribution, filename);
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

        #region Hint
        private void HintSimpleFlag(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleHint(VM.player.SimpleFlag);
        }

        private void HintSimpleClick(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleHint(VM.player.SimpleClick);
        }

        private void HintComplexClick(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleHint(VM.player.ComplexClick);
        }

        private void HintComplexFlag(object sender, RoutedEventArgs e)//test neeeded
        {
            VM.player.SimpleHint(VM.player.ComplexFlag);
        }

        private void HintUncertainComplexFlag(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleHint(VM.player.UncertainComplexFlag);
        }

        private void HintCompleteAnalyze(object sender, RoutedEventArgs e)
        {
            VM.player.SimpleHint(VM.player.CompleteAnalyze);
        }
        #endregion
    }
}
