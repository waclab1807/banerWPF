using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace PaintFPMariuszKonior
{
    public class DrawMethod
    {
        public Stroke DrawASquare(double PositionX, double PositionY, double PositionXLast, double PositionYLast)
        {
            StylusPoint stylusPoint1 = new StylusPoint(PositionX, PositionY);
            StylusPoint stylusPoint2 = new StylusPoint(PositionX, PositionYLast);
            StylusPoint stylusPoint3 = new StylusPoint(PositionXLast, PositionYLast);
            StylusPoint stylusPoint4 = new StylusPoint(PositionXLast, PositionY);
            StylusPoint stylusPoint5 = new StylusPoint(PositionX, PositionY);

            StylusPointCollection collecStylusPoint = new StylusPointCollection(new StylusPoint[] { stylusPoint1, stylusPoint2, stylusPoint3, stylusPoint4, stylusPoint5 });

            return new Stroke(collecStylusPoint);
        }

        public Stroke DrawATriangle(double PositionX, double PositionY, double PositionXLast, double PositionYLast)
        {
            StylusPoint stylusPoint1 = new StylusPoint(PositionX, PositionY);
            StylusPoint stylusPoint2 = new StylusPoint(PositionX - ((PositionY + PositionYLast) / 2), PositionYLast);
            StylusPoint stylusPoint3 = new StylusPoint(PositionX + ((PositionY + PositionYLast) / 2), PositionYLast); ;
            StylusPoint stylusPoint4 = new StylusPoint(PositionX, PositionY);

            StylusPointCollection collecStylusPoint = new StylusPointCollection(new StylusPoint[] { stylusPoint1, stylusPoint2, stylusPoint3, stylusPoint4 });

            return new Stroke(collecStylusPoint);
        }

        public Stroke DrawLines(double PositionX, double PositionY, double PositionXLast, double PositionYLast)
        {
            StylusPoint stylusPoint1 = new StylusPoint(PositionX, PositionY);
            StylusPoint stylusPoint2 = new StylusPoint(PositionXLast, PositionYLast);

            StylusPointCollection collecStylusPoint = new StylusPointCollection(new StylusPoint[] { stylusPoint1, stylusPoint2 });

            Stroke newStroke = new Stroke(collecStylusPoint);

            return new Stroke(collecStylusPoint);
        }

    }
}
