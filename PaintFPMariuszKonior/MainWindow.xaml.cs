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
using System.Text.RegularExpressions;


namespace PaintFPMariuszKonior
{
    public partial class MainWindow : Window
    {
        Color currColor = Colors.Black;
        DrawMethod drawMethod = new DrawMethod();
        TranslateMethod translateMethod = new TranslateMethod();
        ColorsAndTools colorsAndTools = new ColorsAndTools();
        Image image = new Image();
        CutOut cutOutType = CutOut.circle;
        InkCanvas[] backUp = new InkCanvas[3];
        string filefilter = string.Empty;
        string saveTiffFormat = string.Empty;
        string incorectValue = string.Empty;
        CanvasDimensions canvasDimensions = new CanvasDimensions();

        public MainWindow()
        {
            InitializeComponent();
            uiScaleSlider.MouseDoubleClick += new MouseButtonEventHandler(RestoreScalingFactor);
            ApplicationCommands.Close.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            DefaultValues();

        }

        public void DefaultValues()
        {
            labelSpaceHorizontal.Visibility = labelSpaceVertical.Visibility = Visibility.Hidden;
            hSpacingVal.Text = "10";
            vSpacingVal.Text = "10";
            cutOutWidthVal.Text = "10";
            marginVal.Text = "10";
            amountVal.Text = "1";
            upTunnelVal.Text = "30";
            downTunnelVal.Text = "30";
            leftTunnelVal.Text = "30";
            rightTunnelVal.Text = "30";
            sealVal.Text = "10";
            filefilter = "Bitmap files (*.jpg; *.jpeg; *.gif; *.bmp; *.tif; *.tiff)|*.jpg; *.jpeg; *.gif; *.bmp; *.tif; *.tiff; *.pdf";
            saveTiffFormat = "Tagged Image File Format (*.tiff)|*.tiff";
            incorectValue = "Nie poprawna wartość";
            canvasMain.EditingMode = InkCanvasEditingMode.None;
        }

        #region Zoom

        void RestoreScalingFactor(object sender, MouseButtonEventArgs args)
        {
            ((Slider)sender).Value = 1.0;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)

        {

            base.OnPreviewMouseWheel(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
                uiScaleSlider.Value += (args.Delta > 0) ? 0.1 : -0.1;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs args)

        {

            base.OnPreviewMouseDown(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
            {

                if (args.MiddleButton == MouseButtonState.Pressed)
                    RestoreScalingFactor(uiScaleSlider, args);
            }

        }
        #endregion

        #region Tools

        private double setUnit(double number)
        {
            if (mmRadio.IsChecked == true)
            {
                number = number / 10; //switch mmm to cm
                number = number * 37.795279; //convert cm to px
            } 
            else
            {
                return number;
            }
            return number;
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

        private void ColorInkSpecial(object sender, RoutedEventArgs e)
        {
            colorsAndTools.getColor(ColorSpecial);
        }

        #endregion

        #region Save $ Open

        private void OpenFile(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = dialog.Filter = filefilter;

            if ((bool)dialog.ShowDialog(this))
            {
                try
                {
                    using (var file = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                    {

                        canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
                        canvasMain.Strokes.Clear();
                        image.Source = (new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute)));
                        canvasMain.Children.Add(image);
                        file.Close();
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, Title);
                }
            }

        }

        private double getHeightTopTunnel()
        {
            if (IsTextAllowed(upTunnelVal.Text) && topTunnel.IsChecked == true)
                return Double.Parse(Regex.Replace(upTunnelVal.Text, @"\s+", " "));
            return 0.0;
        }

        private double getHeightBottomTunnel()
        {
            if (IsTextAllowed(downTunnelVal.Text) && bottomTunnel.IsChecked == true)
            {
                if (Double.Parse(downTunnelVal.Text) < 30)
                    downTunnelVal.Text = "30";
                return Double.Parse(Regex.Replace(downTunnelVal.Text, @"\s+", " "));
            }
            return 0.0;
        }

        private double getHeightLeftTunnel()
        {
            if (IsTextAllowed(leftTunnelVal.Text) && leftTunnel.IsChecked == true)
                return Double.Parse(Regex.Replace(leftTunnelVal.Text, @"\s+", " "));
            return 0.0;
        }

        private double getWeldidth()
        {
            if (IsTextAllowed(sealVal.Text))
                return Double.Parse(Regex.Replace(sealVal.Text, @"\s+", " "));
            return 0.0;
        }

        private double getHeightRightTunnel()
        {
            if (IsTextAllowed(rightTunnelVal.Text) && rightTunnel.IsChecked == true)
                return Double.Parse(Regex.Replace(rightTunnelVal.Text, @"\s+", " "));
            return 0.0;
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void IsTextAllowedEvent(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveFile(object sender, RoutedEventArgs args)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = saveTiffFormat;

            if ((bool)dialog.ShowDialog(this))
            {
                try
                {
                    using (FileStream file = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        int marg = int.Parse(canvasMain.Margin.Left.ToString());

                        double widthCanvas = canvasMain.ActualWidth;
                        double heightCanvas = canvasMain.ActualHeight;

                        if (image != null)
                        {
                            widthCanvas = image.ActualWidth + (getHeightLeftTunnel() + getHeightRightTunnel());
                            heightCanvas = image.ActualHeight + (getHeightTopTunnel() + getHeightBottomTunnel() + (getWeldidth() * 2));
                        }

                         if (customSize.IsChecked == true)
                         {
                            widthCanvas = Convert.ToDouble(customWidth.Text);
                            heightCanvas = Convert.ToDouble(customHeight.Text);
                        }

                        RenderTargetBitmap rtb = new RenderTargetBitmap((int)widthCanvas - marg,
                                    (int)heightCanvas - marg, 0, 0, PixelFormats.Default);
                        rtb.Render(canvasMain);
                        TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                        //BmpBitmapEncoder encoder = new BmpBitmapEncoder();
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

        private void Reset(object sender, RoutedEventArgs args)
        {
            canvasMain.Strokes.Clear();
            DefaultValues();
        }

        private void AboutMe(object sender, RoutedEventArgs e)
        {
            AboutMe aboutMe = new AboutMe();
            aboutMe.ShowDialog();
        }

        #endregion

        #region DrawMethod


        private void GenerateView(object sender, RoutedEventArgs e)
        {
            if (image.IsVisible == false)
            {
                MessageBox.Show("Wczytaj najpierw plik!");
                return;
            }

            canvasMain.Strokes.Clear();

            canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
            //Zgrzewy

            var color = (Color)ColorConverter.ConvertFromString("" + ColorSpecial.Background);
            canvasMain.Strokes.Add(setCircle(0, 0, getHeightLeftTunnel(), image.ActualHeight + getHeightTopTunnel() + getHeightBottomTunnel() + (getWeldidth() * 2), CutOut.quadrangle, color));
            canvasMain.Strokes.Add(setCircle(image.ActualWidth + getHeightLeftTunnel(), 0, getHeightRightTunnel(), image.ActualHeight + (getWeldidth() * 2) + getHeightTopTunnel() + getHeightBottomTunnel(), CutOut.quadrangle, color));
            canvasMain.Strokes.Add(setCircle(0, 0, image.ActualWidth + getHeightLeftTunnel() + getHeightRightTunnel(), getHeightTopTunnel(), CutOut.quadrangle, color));
            //canvasMain.Strokes.Add(setCircle(0, image.ActualHeight + getWeldidth() + getWeldidth() + getHeightTopTunnel(), image.ActualWidth + getHeightLeftTunnel() + getHeightRightTunnel(), getHeightBottomTunnel(), CutOut.quadrangle, color));

            if (bottomTunnel.IsChecked == true)
            {
                Label titles = new Label();
                titles.Content = string.Format("Uwagi {0}, Liczba zamówień  {1}", extrasVal.Text, amountVal.Text);
                titles.Background = ColorSpecial.Background;
                titles.Width = image.ActualWidth + getHeightLeftTunnel() + getHeightRightTunnel() - getHeightLeftTunnel();
                titles.Height = getHeightBottomTunnel();
                titles.Margin = new Thickness(0, 0, 0, 0);
                canvasMain.Children.Add(titles);
                InkCanvas.SetTop(titles, image.ActualHeight + getWeldidth() + getWeldidth() + getHeightTopTunnel());
                InkCanvas.SetLeft(titles, getHeightLeftTunnel());
                //SET Z-INDEX FOR LABEL ???
            }

            InkCanvas.SetTop(image, getHeightTopTunnel() + getWeldidth());
            InkCanvas.SetLeft(image, getHeightLeftTunnel());
            canvasMain.Children.Add(image);
      
            //odstepy oczek od krawedzi
            double margin = setUnit(Convert.ToDouble(marginVal.Text));
            //srednica oczek
            double widthCutOut = setUnit(Convert.ToDouble(cutOutWidthVal.Text));
            //odstep miedzy oczkami poziomo
            double spaceHorizontal;
            double spaceVertical;

            if (ratio.IsChecked ?? false)
            {
                spaceHorizontal = setUnit(Convert.ToDouble(hSpacingVal.Text));
                spaceVertical = spaceHorizontal;
            }
            else
            {
                spaceHorizontal = setUnit(Convert.ToDouble(hSpacingVal.Text));
                spaceVertical = setUnit(Convert.ToDouble(vSpacingVal.Text));
            }



            double widthCanvas =image.ActualWidth - (margin * 2);
            double heightCanvas = image.ActualHeight - (margin * 2);


            double columnNumbers = (widthCanvas / (spaceHorizontal + widthCutOut));
            double fixSpaceHorizontal = widthCanvas + spaceHorizontal - ((int)columnNumbers * (widthCutOut + spaceHorizontal));
            fixSpaceHorizontal = fixSpaceHorizontal / (((int)columnNumbers) - 1);

            double RowNumbers = (heightCanvas / (spaceVertical + widthCutOut));
            double fixSpaceVertical = heightCanvas + spaceVertical - ((int)RowNumbers * (widthCutOut + spaceVertical));
            fixSpaceVertical = fixSpaceVertical / (((int)RowNumbers) - 1);

            double spaceHorizontalColumn = margin + (widthCutOut / 2) + getHeightLeftTunnel();
            double spaceVerticalColumn = margin + (widthCutOut / 2) + getHeightTopTunnel() + getWeldidth();

            for (int i = 0; i < (int)RowNumbers; i++)
            {
                spaceHorizontalColumn = margin + (widthCutOut / 2) + getHeightLeftTunnel();
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

            labelSpaceHorizontal.Content = "Odległość\nw poziomie\n" + Math.Round((((spaceHorizontal + fixSpaceHorizontal) * 2.54) / 96), 2);
            labelSpaceVertical.Content = "Odległość\nw pionie\n" + Math.Round((((spaceVertical + fixSpaceVertical) * 2.54) / 96), 2);
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
                canvasMain.Strokes.Add(setCircle(0, widthCutOut, spaceHorizontalColumn, spaceVerticalColumn, cutOutType, (Color)ColorConverter.ConvertFromString("" + ColorInkSelect.Background)));
            }

            else if (DrawACircleRadioButton.IsChecked == true)
            {
                cutOutType = CutOut.circle;
                canvasMain.Strokes.Add(setCircle(0, widthCutOut, spaceHorizontalColumn, spaceVerticalColumn, cutOutType, (Color)ColorConverter.ConvertFromString("" + ColorInkSelect.Background)));
            }

        }

        private Stroke setCircle(double special, double widthCutOut, double PositionX, double PositionY, CutOut cutOut, Color colors)
        {
            StylusPointCollection pts = new StylusPointCollection();
            Stroke st = null;

            switch (cutOut)
            {
                case CutOut.circle:
                    pts.Add(new StylusPoint(widthCutOut / 2, widthCutOut / 2));
                    pts.Add(new StylusPoint(PositionX, PositionY));
                    if (turnOnFill.IsChecked == true)
                        pts.Add(new StylusPoint(1, 1));
                    else
                        pts.Add(new StylusPoint(0, 0));
                    st = new customCircleStroke(pts);
                    break;
                case CutOut.square:
                    pts.Add(new StylusPoint(widthCutOut / 2, widthCutOut / 2));
                    pts.Add(new StylusPoint(PositionX, PositionY));
                    if (turnOnFill.IsChecked == true)
                        pts.Add(new StylusPoint(1, 1));
                    else
                        pts.Add(new StylusPoint(0, 0));
                    st = new customSquareStroke(pts);
                    break;
                case CutOut.quadrangle:
                    pts.Add(new StylusPoint(PositionX, PositionY));
                    pts.Add(new StylusPoint(special, widthCutOut));
                    st = new customQuadrangleStroke(pts);
                    break;
            }

            st.DrawingAttributes.Color = colors;
            return st;
        }

        private void ratio_Checked(object sender, RoutedEventArgs e)
        {
            if (ratio.IsChecked == false)
            {
                vSpacingVal.IsEnabled = true;
                //Label newLabel =  verticalSpacing;
                /*verticalSpacing.Visibility = Visibility.Visible;
                vSpacingVal.Visibility = Visibility.Visible;
                horizontalSpacing.Content = "Odległość między\noczkami (poziomo)";*/
            }
            else
            {
                vSpacingVal.IsEnabled = false;
                /*horizontalSpacing.Content = "Odległość między\noczkami";
                verticalSpacing.Visibility = Visibility.Hidden;
                vSpacingVal.Visibility = Visibility.Hidden;*/
            }
        }

        private void customSize_Checked(object sender, RoutedEventArgs e)
        {
            if (customSize.IsChecked == true)
            {
                customWidth.IsEnabled = true;
                customHeight.IsEnabled = true;
            }
            else
            {
                customWidth.IsEnabled = false;
                customHeight.IsEnabled = false;
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
            Pen pen = new Pen();
            SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
            pen.Brush = brush2;
            brush2.Freeze();
            StylusPoint stp = this.StylusPoints[0];
            StylusPoint sp = this.StylusPoints[1];
            StylusPoint fill = this.StylusPoints[2];

            if (fill.X == 1)
                drawingContext.DrawEllipse(brush2, null, new Point((sp.X), (sp.Y)), stp.X, stp.Y);
            else
                drawingContext.DrawEllipse(null, pen, new Point((sp.X), (sp.Y)), stp.X, stp.Y);

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
            Pen pen = new Pen();
            SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
            pen.Brush = brush2;
            brush2.Freeze();
            StylusPoint stp = this.StylusPoints[0];
            StylusPoint sp = this.StylusPoints[1];
            StylusPoint fill = this.StylusPoints[2];

            if (fill.X == 1)
                drawingContext.DrawRectangle(brush2, null, new Rect(sp.X - stp.X, sp.Y - stp.X, stp.X * 2, stp.X * 2));
            else
                drawingContext.DrawRectangle(null, pen, new Rect(sp.X - stp.X, sp.Y - stp.X, stp.X * 2, stp.X * 2));


        }
    }

    public class customQuadrangleStroke : Stroke
    {
        public customQuadrangleStroke(StylusPointCollection pts)
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

            drawingContext.DrawRectangle(brush2, null, new Rect(sp.X, sp.Y, stp.X, stp.Y));
        }
    }
}
