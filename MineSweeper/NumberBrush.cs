using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MineSweeper
{
    class NumberBrush
    {
        static public List<VisualBrush> numbers = new List<VisualBrush> { };
        static public List<Color> colors = new List<Color> { Colors.White,
                                                             Colors.Blue,Colors.Green,Colors.Red,
                                                             Colors.Navy,Colors.Brown,Colors.DarkSeaGreen,
                                                             Colors.Black,Colors.Gray};
        public NumberBrush()
        {
            for(int i = 0; i < 9; i++)
            {
                var my_brush = new VisualBrush();
                StackPanel aPanel = new StackPanel();
                TextBlock someText = new TextBlock();
                someText.Text = i.ToString();
                FontSizeConverter fSizeConverter = new FontSizeConverter();
                someText.FontSize = (double)fSizeConverter.ConvertFromString("10pt");
                someText.Margin = new Thickness(10);
                someText.Foreground = new SolidColorBrush(colors[i]);
                if(i!=0)
                    aPanel.Children.Add(someText);
                my_brush.Visual = aPanel;
                numbers.Add(my_brush);
            }
        }
    }
}
