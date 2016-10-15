using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace PaintFPMariuszKonior
{
    public partial class StyleSettings : Window
    {
        Color currColor = Colors.Black;
        double penWidth =  2;
        double penHeight = 2;

        public StyleSettings()
        {
            InitializeComponent();
            createGridOfColor();
        }

        private void createGridOfColor()
        {
            PropertyInfo[] props = typeof(Brushes).GetProperties(BindingFlags.Public |
                                                  BindingFlags.Static);
            foreach (PropertyInfo p in props)
            {
                Button b = new Button();
                b.Background = (SolidColorBrush)p.GetValue(null, null);
                b.Foreground = Brushes.Transparent;
                b.BorderBrush = Brushes.Transparent;
                b.Click += new RoutedEventHandler(SelectHecColor);
                this.ugColors.Children.Add(b);
            }
        }

        private void SelectHecColor(object sender, RoutedEventArgs e)
        {
            SolidColorBrush sb = (SolidColorBrush)(sender as Button).Background;
            currColor = sb.Color;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public DrawingAttributes DrawingAttributes
        {
            set
            {
                //chkPressure.IsChecked = value.IgnorePressure;
                //chkHighlight.IsChecked = value.IsHighlighter;
                penWidth = value.Width;
                penHeight = value.Height;
                currColor = value.Color;
            }
            get
            {
                DrawingAttributes drawattr = new DrawingAttributes();
                //drawattr.IgnorePressure = (bool)chkPressure.IsChecked;
                drawattr.Width = penWidth;
                drawattr.Height = penHeight;
                //drawattr.IsHighlighter = (bool)chkHighlight.IsChecked;
                drawattr.Color = currColor;
                return drawattr;
            }
        }
    }
}