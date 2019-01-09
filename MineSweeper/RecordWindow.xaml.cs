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
    /// RecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RecordWindow : Window
    {
        private Record record;
        public RecordWindow()
        {
            InitializeComponent();
        }

        public RecordWindow(Record record)
        {
            InitializeComponent();
            this.DataContext = this;
            this.record = record;

        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            record.Reset();
            beginer.Text = "999";
            intermediate.Text = "999";
            expert.Text = "999";
        }

        public int Beginner_best { get => record.Beginner_best; }
        public int Intermediate_best { get => record.Intermediate_best; }
        public int Expert_best { get => record.Expert_best; }
    }
}
