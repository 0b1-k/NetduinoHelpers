using System;
namespace Nwazet.Go.Display.TouchScreen {
    public class ScreenArea {
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ScreenArea(int x, int y, int width, int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public ScreenArea(int x, int y, int radius) {
            X = x;
            Y = y;
            Radius = radius;
        }
        public bool IsWithinArea(int x, int y) {
            if (Radius != 0) {
                // Check if the coordinate is within the circle
                return ((X - x) * (X - x) + (Y - y) * (Y - y) < Radius * Radius);
            } else {
                // Check if the coordinate is within the rectangle
                if (x >= X && x < X + Width && y >= Y && y < Y + Height) {
                    return true;
                }
            }
            return false;
        }
    }
}
