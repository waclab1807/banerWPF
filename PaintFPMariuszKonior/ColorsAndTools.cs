using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Ink;

namespace PaintFPMariuszKonior
{  
    public class ColorsAndTools
    {
        public void SetPenSize(InkCanvas canvasMain, ComboBox PenSize)
        {
            DrawingAttributes inkDA = new DrawingAttributes();
            inkDA.Width = inkDA.Height = Convert.ToDouble(((ComboBoxItem)PenSize.SelectedItem).Content.ToString());
            inkDA.Color = canvasMain.DefaultDrawingAttributes.Color;
            inkDA.IsHighlighter = canvasMain.DefaultDrawingAttributes.IsHighlighter;
            canvasMain.DefaultDrawingAttributes = inkDA;
        }

        public void ChangeColor(InkCanvas canvasMain)
        {
            StrokeCollection strokes = canvasMain.GetSelectedStrokes();
            switch (strokes.Count)
            {
                case 0:
                    MessageBox.Show("to change the color, the first you must select the object");
                    break;
                default:
                    StyleSettings dialog = new StyleSettings();

                    if (strokes.Count > 0)
                        dialog.DrawingAttributes = strokes[0].DrawingAttributes;
                    else
                        dialog.DrawingAttributes = canvasMain.DefaultDrawingAttributes;

                    if (dialog.ShowDialog().GetValueOrDefault())
                    {
                        foreach (Stroke strk in strokes)
                            strk.DrawingAttributes = dialog.DrawingAttributes;
                    }
                    break;
            }
        }

        public void ColorInk(InkCanvas canvasMain, Grid ColorInkSelect)
        {
            StyleSettings dialog = new StyleSettings();
            dialog.DrawingAttributes = canvasMain.DefaultDrawingAttributes;
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                canvasMain.DefaultDrawingAttributes = dialog.DrawingAttributes;
                ColorInkSelect.Background = new SolidColorBrush(dialog.DrawingAttributes.Color);
            }
        }
        public void getColor(Grid ColorInkSelect)
        {
            StyleSettings dialog = new StyleSettings();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                ColorInkSelect.Background = new SolidColorBrush(dialog.DrawingAttributes.Color);
            }
        }
    }
}
