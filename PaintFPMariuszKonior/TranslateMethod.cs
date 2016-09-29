using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace PaintFPMariuszKonior
{
    public class TranslateMethod
    {
        public void Rotation(InkCanvas canvasMain)
        {
            StrokeCollection collecStrokes = canvasMain.GetSelectedStrokes();
            Rect selectionBounds = canvasMain.GetSelectionBounds();
            switch (collecStrokes.Count)
            {
                case 0:
                    MessageBox.Show("to change the rotation, the first you must select the object");
                    break;
                default:
                    Matrix rotationalMatrix = Matrix.Identity;
                    rotationalMatrix.RotateAt(90, selectionBounds.Left + selectionBounds.Width / 2, selectionBounds.Top + selectionBounds.Height / 2);
                    canvasMain.GetSelectedStrokes().Transform(rotationalMatrix, true);
                    break;
            }
        }

        public void RotationSelect(InkCanvas canvasMain, ComboBox RotationAngle)
        {
            StrokeCollection collecStrokes = canvasMain.GetSelectedStrokes();
            Rect selectionBounds = canvasMain.GetSelectionBounds();
            switch (collecStrokes.Count)
            {
                case 0:
                    MessageBox.Show("to change the rotation, the first you must select the object");
                    break;
                default:
                    int angle = 0;
                    switch (((ComboBoxItem)RotationAngle.SelectedItem).Content.ToString())
                    {
                        case "90°":
                            angle = 90;
                            break;
                        case "180°":
                            angle = 180;
                            break;
                        case "270°":
                            angle = 270;
                            break;
                    }
                    Matrix rotationalMatrix = Matrix.Identity;
                    rotationalMatrix.RotateAt(angle, selectionBounds.Left + selectionBounds.Width / 2, selectionBounds.Top + selectionBounds.Height / 2);
                    canvasMain.GetSelectedStrokes().Transform(rotationalMatrix, true);
                    break;
            }
        }

        public void FlipSelect(InkCanvas canvasMain, ComboBox Flip)
        {
            StrokeCollection collecStrokes = canvasMain.GetSelectedStrokes();
            Rect selectionBounds = canvasMain.GetSelectionBounds();
            switch (collecStrokes.Count)
            {
                case 0:
                    MessageBox.Show("to flip the object, the first you must select the object");
                    break;
                default:
                    int scaleX = 0;
                    int scaleY = 0;

                    switch (((ComboBoxItem)Flip.SelectedItem).Content.ToString())
                    {
                        case "horizontal":
                            scaleX = -1;
                            scaleY = 1;
                            break;
                        case "vertical":
                            scaleX = 1;
                            scaleY = -1;
                            break;
                    }
                    Matrix rotationalMatrix = Matrix.Identity;
                    rotationalMatrix.ScaleAt(scaleX, scaleY, selectionBounds.Left + selectionBounds.Width / 2, selectionBounds.Top + selectionBounds.Height / 2);
                    canvasMain.GetSelectedStrokes().Transform(rotationalMatrix, true);
                    break;
            }
        }
    }
}
