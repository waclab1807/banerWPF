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
        private string fName;

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
            filefilter = "Bitmap files (*.jpg; *.jpeg; *.gif; *.bmp; *.tif; *.tiff; *.pdf)|*.jpg; *.jpeg; *.gif; *.bmp; *.tif; *.tiff; *.pdf";
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
            var result = number;
            if (mmRadio.IsChecked == true)
            {
                // convert px to mm (assume that dpi is always equal 60)
                //result = ((number * 25.4) / 60);
                result = ((number * 60) / 25.4);
            }
            Console.WriteLine(result);
            return result;
        }

        private double setUnitMM(double number)
        {
            var result = number;
            if (mmRadio.IsChecked == true)
            {
                // convert px to mm (assume that dpi is always equal 60)
                result = ((number * 25.4) / 60);
                //result = ((number * 60) / 25.4);
            }
            Console.WriteLine(result);
            return result;
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

        private static BitmapSource ConvertBitmapTo60DPI(BitmapImage bitmapImage)
        {
            double dpi = 60;
            int width = bitmapImage.PixelWidth;
            int height = bitmapImage.PixelHeight;

            int stride = width * bitmapImage.Format.BitsPerPixel;
            byte[] pixelData = new byte[stride * height];
            bitmapImage.CopyPixels(pixelData, stride, 0);

            return BitmapSource.Create(width, height, dpi, dpi, bitmapImage.Format, null, pixelData, stride);
        }

        private void OpenFile(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = dialog.Filter = filefilter;

            if ((bool)dialog.ShowDialog(this))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        BitmapFrame frame = BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);

                        imgDpiInfo.Content = "DPI: " + frame.DpiX + "dpi";
                        imgWidthInfo.Content = "Szerokość: " + frame.PixelWidth + "px";
                        imgHeightInfo.Content = "Wysokość: " + frame.PixelHeight + "px";
                    }
                    using (var file = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                    {

                        canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
                        canvasMain.Strokes.Clear();
                        fName = dialog.FileName;
                        image.Source = ConvertBitmapTo60DPI(new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute)));
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
                return setUnit(Double.Parse(Regex.Replace(upTunnelVal.Text, @"\s+", " ")));
            return 0.0;
        }

        private double getHeightBottomTunnel()
        {
            if (IsTextAllowed(downTunnelVal.Text) && bottomTunnel.IsChecked == true)
            {
                if (Double.Parse(downTunnelVal.Text) < (30))
                    downTunnelVal.Text = String.Format("{0}", setUnit(30));
                return setUnit(Double.Parse(Regex.Replace(downTunnelVal.Text, @"\s+", " ")));
            }
            return 0.0;
        }

        private double getHeightLeftTunnel()
        {
            if (IsTextAllowed(leftTunnelVal.Text) && leftTunnel.IsChecked == true)
                return setUnit(Double.Parse(Regex.Replace(leftTunnelVal.Text, @"\s+", " ")));
            return 0.0;
        }

        private double getTrimTop()
        {
            if (IsTextAllowed(trimTop.Text) && trimTopChkbx.IsChecked == true && trimTop.Text != "")
                return setUnit(Double.Parse(Regex.Replace(trimTop.Text, @"\s+", " ")));
            return 0.0;
        }

        private double getTrimBottom()
        {
            if (IsTextAllowed(trimBottom.Text) && trimBottomChkbx.IsChecked == true && trimBottom.Text != "")
                return setUnit(Double.Parse(Regex.Replace(trimBottom.Text, @"\s+", " ")));
            return 0.0;
        }

        private double getTrimRight()
        {
            if (IsTextAllowed(trimRight.Text) && trimRightChkbx.IsChecked == true && trimRight.Text != "")
                return setUnit(Double.Parse(Regex.Replace(trimRight.Text, @"\s+", " ")));
            return 0.0;
        }

        private double getTrimLeft()
        {
            if (IsTextAllowed(trimLeft.Text) && trimLeftChkbx.IsChecked == true && trimLeft.Text != "")
                return setUnit(Double.Parse(Regex.Replace(trimLeft.Text, @"\s+", " ")));
            return 0.0;
        }

        private double getWeldidth()
        {
            if (IsTextAllowed(sealVal.Text))
                return setUnit(Double.Parse(Regex.Replace(sealVal.Text, @"\s+", " ")));
            return 0.0;
        }

        private double getHeightRightTunnel()
        {
            if (IsTextAllowed(rightTunnelVal.Text) && rightTunnel.IsChecked == true)
                return setUnit(Double.Parse(Regex.Replace(rightTunnelVal.Text, @"\s+", " ")));
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

                        double widthCanvas = setUnit(canvasMain.ActualWidth);
                        double heightCanvas = setUnit(canvasMain.ActualHeight);

                        if (image != null)
                        {
                            widthCanvas = (image.ActualWidth) + (getHeightLeftTunnel() + getHeightRightTunnel()) - getTrimTop() - getTrimBottom();
                            heightCanvas = (image.ActualHeight) + (getHeightTopTunnel() + getHeightBottomTunnel() + (getWeldidth() * 2)) - getTrimLeft() - getTrimRight();
                        }

                        if (customSize.IsChecked == true)
                        {
                            widthCanvas = setUnit(Convert.ToDouble(customWidth.Text));
                            heightCanvas = setUnit(Convert.ToDouble(customHeight.Text));
                        }

                        RenderTargetBitmap rtb = new RenderTargetBitmap((int)CorectScale(widthCanvas) - marg,
                                    (int)CorectScale(heightCanvas) - marg, 60, 60, PixelFormats.Default);
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

        private double CorectScale(double source)
        {
            return (source * (62.5 / 100));
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
            canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
            DefaultValues();
            InkCanvas.SetTop(image, 0);
            InkCanvas.SetLeft(image, 0);
            canvasMain.Children.Add(image);
        }

        private void AboutMe(object sender, RoutedEventArgs e)
        {
            AboutMe aboutMe = new AboutMe();
            aboutMe.ShowDialog();
        }

        #endregion

        #region DrawMethod

        private void ResizeImage(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(customWidth.Text) || string.IsNullOrWhiteSpace(customHeight.Text))
            {
                MessageBox.Show("Podaj obie wartości!");
                return;
            }

            if (image.IsVisible == false)
            {
                MessageBox.Show("Wczytaj najpierw plik!");
                return;
            }

            //here is method for resizing image

            var buffer = System.IO.File.ReadAllBytes(fName);
            MemoryStream ms = new MemoryStream(buffer);

            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.StreamSource = ms;
            src.DecodePixelHeight = int.Parse(customWidth.Text);
            src.DecodePixelWidth = int.Parse(customHeight.Text);
            src.EndInit();

            image.Source = src;
        }

        private void TrimImage(object sender, RoutedEventArgs e)
        {
            if (image.IsVisible == false)
            {
                MessageBox.Show("Wczytaj najpierw plik!");
                return;
            }

            canvasMain.Strokes.Clear();
            canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);

            InkCanvas.SetTop(image, getHeightTopTunnel() - getTrimTop());
            InkCanvas.SetLeft(image, getHeightLeftTunnel() - getTrimLeft());
            canvasMain.Children.Add(image);

            var height = (image.ActualHeight) - getTrimTop() - getTrimBottom();
            var Width = (image.ActualWidth) - getTrimRight() - getTrimLeft();

            canvasMain.Strokes.Add(setCircle(Width + getHeightLeftTunnel(), 0, getHeightRightTunnel() + getTrimRight(), height + (getWeldidth() * 2) + getHeightTopTunnel() + getHeightBottomTunnel(), CutOut.quadrangle, Colors.White));
            canvasMain.Strokes.Add(setCircle(0, (image.ActualHeight) + getHeightTopTunnel() - getTrimBottom() - getTrimTop(), (image.ActualWidth) + getHeightLeftTunnel() + getHeightRightTunnel(), getTrimBottom(), CutOut.quadrangle, Colors.White));

        }

        private void GenerateView(object sender, RoutedEventArgs e)
        {
            if (image.IsVisible == false)
            {
                MessageBox.Show("Wczytaj najpierw plik!");
                return;
            }

            //var test = getTrimRight();

            /*image.Clip.SetValue(Canvas.WidthProperty, image.ActualWidth - getTrimRight());
            image.Clip.SetValue(Canvas.HeightProperty, image.ActualHeight - getTrimBottom());
            image.Clip.SetValue(Canvas.TopProperty, getTrimTop());
            image.Clip.SetValue(Canvas.LeftProperty, getTrimLeft());*/
            var height = (image.ActualHeight) - getTrimTop() - getTrimBottom();
            var Width = (image.ActualWidth) - getTrimRight() - getTrimLeft();

            canvasMain.Strokes.Clear();

            canvasMain.Children.RemoveRange(0, canvasMain.Children.Count);
            //Zgrzewy

            var color = (Color)ColorConverter.ConvertFromString("" + ColorSpecial.Background);
            canvasMain.Strokes.Add(setCircle(0, 0, Width + getHeightLeftTunnel() + getHeightRightTunnel(), getHeightTopTunnel() + getWeldidth(), CutOut.quadrangle, Colors.White));
            canvasMain.Strokes.Add(setCircle(0, (image.ActualHeight) + getWeldidth() + getHeightTopTunnel() - getTrimBottom() - getTrimTop(), (image.ActualWidth) + getHeightLeftTunnel() + getHeightRightTunnel() - getTrimLeft(), getWeldidth(), CutOut.quadrangle, Colors.White));
            canvasMain.Strokes.Add(setCircle(Width + getHeightLeftTunnel(), 0, getHeightRightTunnel() + getTrimRight(), height + (getWeldidth() * 2) + getHeightTopTunnel() + getHeightBottomTunnel(), CutOut.quadrangle, Colors.White));
            canvasMain.Strokes.Add(setCircle(0, 0, getHeightLeftTunnel(), height + getHeightTopTunnel() + getHeightBottomTunnel() + (getWeldidth() * 2), CutOut.quadrangle, color));
            canvasMain.Strokes.Add(setCircle(Width + getHeightLeftTunnel() -1, 0, getHeightRightTunnel()+1, height + (getWeldidth() * 2) + getHeightTopTunnel() + getHeightBottomTunnel(), CutOut.quadrangle, color));
            canvasMain.Strokes.Add(setCircle(0, 0, Width + getHeightLeftTunnel() + getHeightRightTunnel(), getHeightTopTunnel(), CutOut.quadrangle, color));

            InkCanvas.SetTop(image, getHeightTopTunnel() + getWeldidth() - getTrimTop());
            InkCanvas.SetLeft(image, getHeightLeftTunnel() - getTrimLeft());
            canvasMain.Children.Add(image);

            if (bottomTunnel.IsChecked == true)
            {
                Label titles = new Label();
                titles.Content = string.Format("Uwagi {0}, Liczba zamówień  {1}", extrasVal.Text, amountVal.Text);
                titles.Background = ColorSpecial.Background;
                titles.Width = Width + getHeightLeftTunnel() + getHeightRightTunnel() - getHeightLeftTunnel();
                titles.Height = getHeightBottomTunnel();
                titles.Margin = new Thickness(-1, 0, 0, 0);
                canvasMain.Children.Add(titles);
                InkCanvas.SetTop(titles, height + getWeldidth() + getWeldidth() + getHeightTopTunnel());
                InkCanvas.SetLeft(titles, getHeightLeftTunnel());
            }

            canvasMain.Strokes.Add(setCircle(0, (image.ActualHeight) + getWeldidth() + getWeldidth() + getHeightTopTunnel() + getHeightBottomTunnel() - getTrimBottom() - getTrimTop(), (image.ActualWidth) + getHeightLeftTunnel() + getHeightRightTunnel(), getTrimBottom(), CutOut.quadrangle, Colors.White));

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



            double widthCanvas = Width - (margin * 2);
            double heightCanvas = height - (margin * 2);


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

            //labelSpaceHorizontal.Content = "Odległość w poziomie\n" + Math.Round((((spaceHorizontal + fixSpaceHorizontal) * 25.4) / 60), 2);
            //labelSpaceVertical.Content = "Odległość w pionie\n" + Math.Round((((spaceVertical + fixSpaceVertical) * 25.4) / 60), 2);

            labelSpaceHorizontal.Content = "Odległość w poziomie\n" + setUnitMM(spaceHorizontal + fixSpaceHorizontal);
            labelSpaceVertical.Content = "Odległość w pionie\n" + setUnitMM(spaceVertical + fixSpaceVertical);

            labelSpaceHorizontal.Visibility = Visibility.Visible;
            labelSpaceVertical.Visibility = Visibility.Visible;

            if ((spaceHorizontal + fixSpaceHorizontal) < 0 || Double.IsInfinity(spaceHorizontal + fixSpaceHorizontal) || Double.IsNaN(spaceHorizontal + fixSpaceHorizontal))
            {
                MessageBox.Show("Przekroczyłeś zakres w poziomie! Oczka nie mieszczą się na obrazku.");
                return;
            }

            if ((spaceVertical + fixSpaceVertical) < 0 || Double.IsInfinity(spaceVertical + fixSpaceVertical) || Double.IsNaN(spaceVertical + fixSpaceVertical))
            {
                MessageBox.Show("Przekroczyłeś zakres w pionie! Oczka nie mieszczą się na obrazku.");
                return;
            }
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

        private void turnOnSeal(object sender, RoutedEventArgs e)
        {
            if (tunnelsChkbx.IsChecked ?? true)
            {
                sealVal.Text = "10";
            }
            else
            {
                sealVal.Text = "0";
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
            Pen pen = new Pen();
            SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
            pen.Brush = brush2;
            brush2.Freeze();

            StylusPoint stp = this.StylusPoints[0];
            StylusPoint sp = this.StylusPoints[1];

            drawingContext.DrawRectangle(brush2, pen, new Rect(sp.X, sp.Y, stp.X, stp.Y));
        }
    }
}
