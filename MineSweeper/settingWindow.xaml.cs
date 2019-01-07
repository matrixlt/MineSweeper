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
using System.Windows.Shapes;

namespace MineSweeper
{
    /// <summary>
    /// settingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        private int rows;
        private int cols;
        private int mines;
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void ConfirmButton(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private int Rows
        {
            get { return int.Parse(rowsSetting.Text);}
            set { rowsSetting.Text = Convert.ToString(value); }
        }

        private int Cols
        {
            get { return int.Parse(rowsSetting.Text);}
            set { rowsSetting.Text = Convert.ToString(value); }
        }
        
        private int Mines
        {
            get { return int.Parse(minesSetting.Text); }
            set { minesSetting.Text = Convert.ToString(value); }
        }

        public SettingWindow(int row, int col, int mine_number)
        {
            InitializeComponent();
            Rows = row;
            Cols = col;
            Mines = mine_number;
        }
    }
}
