using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Ink;

namespace PaintFPMariuszKonior
{
    public partial class MainWindow : Window
    {
        Color currColor = Colors.Black;
        DrawMethod drawMethod = new DrawMethod();
        TranslateMethod translateMethod = new TranslateMethod();
        ColorsAndTools colorsAndTools = new ColorsAndTools();
        private double PositionX, PositionY, PositionXLast, PositionYLast;
        private bool blockLine = false;
        Image image = new Image();
        CutOut cutOutType = CutOut.circle;
        InkCanvas[] backUp = new InkCanvas[3];
        

        public MainWindow()
        {
            InitializeComponent();
            ApplicationCommands.Close.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            DefaultValues();
        }


        public void DefaultValues()
        {
            labelSpaceHorizontal.Visibility = Visibility.Hidden;
            labelSpaceVertical.Visibility = Visibility.Hidden;
            hSpacingVal.Text = "10";
            vSpacingVal.Text = "10";
            cutOutWidthVal.Text = "10";
            marginVal.Text = "10";
            amountVal.Text = "1";
            upTunnelVal.Text = "10";
            downTunnelVal.Text = "10";
            sealVal.Text = "10";
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                zoomSlider.Value += 0.1;
            }
            else
            {
                zoomSlider.Value -= 0.1;
            }
        }

        #region Tools

        private void Ink(object sender, RoutedEventArgs e)
        {
            canvasMain.EditingMode = InkCanvasEditingMode.Ink;
        }

        private double getOpacity(bool Opacity)
        {
            if (Opacity)
                return 0.0;
            return 1;
        }

        private void Select(object sender, RoutedEventArgs e)
        {
            canvasMain.EditingMode = InkCanvasEditingMode.Select;
            ClearSelect();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            canvasMain.Select(canvasMain.Strokes);
            ClearSelect();
        }        

        private void EraseByPoint(object sender, RoutedEventArgs e)
        {
            canvasMain.EditingMode = InkCanvasEditingMode.EraseByPoint;
        }

        private void EraseByStroke(object sender, RoutedEventArgs e)
        {
            canvasMain.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            colorsAndTools.ChangeColor(canvasMain);
        }

        private void ColorInk(object sender, RoutedEventArgs e)
        {
            colorsAndTools.ColorInk(canvasMain, ColorInkSelect);
        }

        #endregion

        #region Save $ Open

        private void OpenFile(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = dialog.Filter = "Bitmap files (*.jpg; *.jpeg; *.gif; *.bmp; *.tiff)|*.jpg; *.jpeg; *.gif; *.bmp; *.tiff";

            if ((bool)dialog.ShowDialog(this))
            {
                //canvasMain.Strokes.Clear();              
                try
                {
                    using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        BitmapFrame frame = BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                        int scale = 4;
                        Size s = new Size(frame.PixelWidth, frame.PixelHeight);

                        zoomSlider.Maximum = ((frame.PixelWidth / canvasMain.ActualWidth) * scale);
                        zoomSlider.Minimum = ((canvasMain.ActualWidth / frame.PixelWidth) / scale);
                        zoomSlider.Value = ((((frame.PixelWidth / canvasMain.ActualWidth) * scale) - ((canvasMain.ActualWidth / frame.PixelWidth) / scale)) / (scale / 2));

                        test.Content = (zoomSlider.Maximum - zoomSlider.Minimum) / scale;

                    }
                    using (var file = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        canvasMain.Strokes.Clear();
                        canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
                        image.Source = (new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute)));
                        canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);                                            
                        canvasMain.Children.Add(image);
                        file.Close();                                          
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, Title);
                }
            }

            //canvasMain.zoomSlider = 10;
            //fit to page 
            
        }

        private void SaveFile(object sender, RoutedEventArgs args)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bitmap files (*.jpg; *.jpeg; *.gif; *.bmp; *.tiff)|*.jpg; *.jpeg; *.gif; *.bmp; *.tiff";

            if ((bool)dialog.ShowDialog(this))
            {
                try
                {
                    using (FileStream file = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        int marg = int.Parse(canvasMain.Margin.Left.ToString());

                        double widthCanvas = canvasMain.ActualWidth ;
                        double heightCanvas = canvasMain.ActualHeight;

                        if (image != null)
                        {
                            widthCanvas = image.ActualWidth ;
                            heightCanvas = image.ActualHeight ;
                        }

                        RenderTargetBitmap rtb = new RenderTargetBitmap((int)widthCanvas - marg,
                                        (int)heightCanvas - marg, 0, 0, PixelFormats.Default);
                        rtb.Render(canvasMain);
                        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(rtb));
                        encoder.Save(file);
                        file.Close();
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, Title);
                }
            }
        }

        private void NewCanvas(object sender, RoutedEventArgs args)
        {
            canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
            canvasMain.Strokes.Clear();
            //Tu można automatycznie otworzyć okno wyboru nowego pliku (OpenFile())
        }

        private void AboutMe(object sender, RoutedEventArgs e)
        {
            AboutMe aboutMe = new AboutMe();
            aboutMe.ShowDialog();
        }

        #endregion

        #region DrawMethod

        private void EventDrow(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void DrawASquare()
        {            
            canvasMain.Strokes.Add(drawMethod.DrawASquare(PositionX, PositionY, PositionXLast, PositionYLast));
            blockLine = !blockLine;
        }

        private void DrawATriangle()
        {
            canvasMain.Strokes.Add(drawMethod.DrawATriangle(PositionX, PositionY, PositionXLast, PositionYLast));
            blockLine = !blockLine;
        }

        private void DrawLines()
        {
            canvasMain.Strokes.Add(drawMethod.DrawLines(PositionX, PositionY, PositionXLast, PositionYLast));
            blockLine = !blockLine;
        }

        private void ShowParameters(object sender, RoutedEventArgs e)
        {
            canvasMain.Strokes.Clear();
            canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
            canvasMain.Children.Add(image);

            //odstepy oczek od krawedzi
            double margin = Convert.ToDouble(marginVal.Text);
            //srednica oczek
            double widthCutOut = Convert.ToDouble(cutOutWidthVal.Text);
            //odstep miedzy oczkami poziomo
            double spaceHorizontal;
            double spaceVertical;

            if (ratio.IsChecked ?? false)
            {
                spaceHorizontal = Convert.ToDouble(hSpacingVal.Text);
                spaceVertical = spaceHorizontal;
            } else
            {
                spaceHorizontal = Convert.ToDouble(hSpacingVal.Text);
                spaceVertical = Convert.ToDouble(vSpacingVal.Text);
            }

            

            double widthCanvas = canvasMain.ActualWidth - (margin * 2);
            double heightCanvas = canvasMain.ActualHeight - (margin * 2);

            if (image!=null)
            {
                widthCanvas = image.ActualWidth - (margin * 2);
                heightCanvas = image.ActualHeight - (margin * 2);
            }

            double columnNumbers = (widthCanvas / (spaceHorizontal + widthCutOut));
            double fixSpaceHorizontal = widthCanvas + spaceHorizontal - ((int)columnNumbers * (widthCutOut + spaceHorizontal));
            fixSpaceHorizontal = fixSpaceHorizontal / (((int)columnNumbers) - 1);

            double RowNumbers = (heightCanvas / (spaceVertical + widthCutOut));
            double fixSpaceVertical = heightCanvas + spaceVertical - ((int)RowNumbers * (widthCutOut + spaceVertical));
            fixSpaceVertical = fixSpaceVertical / (((int)RowNumbers) - 1);

            double spaceHorizontalColumn = margin + (widthCutOut / 2);
            double spaceVerticalColumn = margin + (widthCutOut / 2);

            for (int i = 0; i < (int)RowNumbers; i++)
            {
                spaceHorizontalColumn = margin + (widthCutOut / 2);
                for (int j = 0; j < (int)columnNumbers; j++)
                {
                    if ((int)RowNumbers == (i + 1))
                    {
                        droveCutOut(widthCutOut, spaceHorizontalColumn, spaceVerticalColumn);
                    }
                    else if (j == 0)
                    {
                        droveCutOut(widthCutOut, spaceHorizontalColumn, spaceVerticalColumn);
                    }
                    else if (i == 0)
                    {
                        droveCutOut(widthCutOut, spaceHorizontalColumn, spaceVerticalColumn);
                    }

                    else if ((int)columnNumbers == (j + 1))
                    {
                        droveCutOut(widthCutOut, spaceHorizontalColumn, spaceVerticalColumn);
                    }

                    spaceHorizontalColumn += (widthCutOut + spaceHorizontal + fixSpaceHorizontal);
                }

                spaceVerticalColumn += (widthCutOut + spaceVertical + fixSpaceVertical);
            }

            labelSpaceHorizontal.Content = "Odległość\nw poziomie\n" + (((spaceHorizontal + fixSpaceHorizontal)*2.54)/96);
            labelSpaceVertical.Content = "Odległość\nw pionie\n" + (((spaceVertical + fixSpaceVertical)*2.54)/ 96);
            labelSpaceHorizontal.Visibility = Visibility.Visible;
            labelSpaceVertical.Visibility = Visibility.Visible;
        }

        private void droveCutOut(double widthCutOut, double spaceHorizontalColumn, double spaceVerticalColumn)
        {
            if (DrawASquareRadioButton.IsChecked == true)
            {
                var offser = widthCutOut / 2;
                cutOutType = CutOut.square;
                //canvasMain.Strokes.Add(drawMethod.DrawASquare(spaceHorizontalColumn - offser, spaceVerticalColumn - offser, spaceHorizontalColumn + offser, spaceVerticalColumn + offser));
                canvasMain.Strokes.Add(setCircle(widthCutOut, spaceHorizontalColumn, spaceVerticalColumn, cutOutType));
            }

            else if (DrawACircleRadioButton.IsChecked == true)
            {
                cutOutType = CutOut.circle;
                canvasMain.Strokes.Add(setCircle(widthCutOut, spaceHorizontalColumn, spaceVerticalColumn, cutOutType));
            }

        }

        private Stroke setCircle(double widthCutOut, double PositionX, double PositionY, CutOut cutOut)
        {
            StylusPointCollection pts = new StylusPointCollection();
            Stroke st = null;
            pts.Add(new StylusPoint(widthCutOut / 2, widthCutOut / 2));
            pts.Add(new StylusPoint(PositionX, PositionY));

            switch (cutOut)
            {
                case CutOut.circle:
                    st = new customCircleStroke(pts);
                    break;
                case CutOut.square:
                    st = new customSquareStroke(pts);
                    break;
            }

            st.DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString("" + ColorInkSelect.Background);
            return st;
        }

        private void ratio_Checked(object sender, RoutedEventArgs e)
        {
            if (ratio.IsChecked == false)
            {
                //Label newLabel =  verticalSpacing;
                verticalSpacing.Visibility = Visibility.Visible;
                vSpacingVal.Visibility = Visibility.Visible;
                horizontalSpacing.Content = "Odległość między\noczkami (poziomo)";
            }
            else
            {
                horizontalSpacing.Content = "Odległość między\noczkami";
                verticalSpacing.Visibility = Visibility.Hidden;
                vSpacingVal.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Else Methods      

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void ClearSelect()
        {
            //InkSelect.IsChecked = true;
            //InkSelect.IsChecked = false;
        }

        #endregion
    }

    public class customCircleStroke : Stroke
    {
        public customCircleStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.StylusPoints = pts;
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }
            if (null == drawingAttributes)
            {
                throw new ArgumentNullException("drawingAttributes");
            }
            DrawingAttributes originalDa = drawingAttributes.Clone();
            SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
            brush2.Freeze();
            StylusPoint stp = this.StylusPoints[0];
            StylusPoint sp = this.StylusPoints[1];

            drawingContext.DrawEllipse(brush2, null, new Point((sp.X), (sp.Y)), stp.X, stp.Y);

        }
    }

    public class customSquareStroke : Stroke
    {
        public customSquareStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.StylusPoints = pts;
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }
            if (null == drawingAttributes)
            {
                throw new ArgumentNullException("drawingAttributes");
            }
            DrawingAttributes originalDa = drawingAttributes.Clone();
            SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
            brush2.Freeze();
            StylusPoint stp = this.StylusPoints[0];
            StylusPoint sp = this.StylusPoints[1];

            drawingContext.DrawRectangle(brush2, null, new Rect(sp.X - stp.X, sp.Y - stp.X, stp.X * 2, stp.X * 2));
        }
    }
}
