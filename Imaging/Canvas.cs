using System;
using System.Collections;
using Microsoft.SPOT;
using netduino.helpers.Hardware;
using netduino.helpers.Helpers;
using netduino.helpers.Fonts;
namespace netduino.helpers.Imaging {
    // Based on MicroBuilder's code: http://www.microbuilder.eu/Projects/LPC1343ReferenceDesign/TFTLCDAPI.aspx
    public class RGBColor24 {
        public byte Red;
        public byte Green;
        public byte Blue;
    }
    public class Canvas {
        public enum RoundedCornerStyle {
            None,
            All,
            Top,
            Bottom,
            Left,
            Right
        }
        public enum CornerPosition {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        public enum DrawingDirection {
            Left,
            Right,
            Up,
            Down
        }
        protected enum Command {
            DrawTestPattern,
            DrawPixel,
            DrawFill,
            DrawLine,
            DrawLineDotted,
            DrawCircle,
            DrawCircleFilled,
            DrawCornerFilled,
            DrawArrow,
            DrawRectangle,
            DrawRectangleFilled,
            DrawRectangleRounded,
            DrawTriangle,
            DrawTriangleFilled,
            DrawProgressBar,
            DrawButton,
            DrawIcon16,
            DrawString,
            DrawBitmapImage,
            DrawCirclePoints,
            SetOrientation
        }
        public Canvas() {
        }
        public Canvas(LCD display) {
            SetDisplay(display);
        }
        protected void SetDisplay(LCD display){
            _display = display;
        }
        // Draws a simple color test pattern
        virtual public void DrawTestPattern() {
            _display.Test();
        }
        // Draws a single pixel at the specified location
        virtual public void DrawPixel(int x, int y, BasicColor color) {
            if ((x >= _display.Width) || (y >= _display.Height)) {
                return;
            }
            _display.DrawPixel((ushort)x, (ushort)y, (ushort)color);
        }
        // Fills the screen with the specified color
        virtual public void DrawFill(BasicColor color) {
            _display.FillRGB((ushort)color);
        }
        // Draws a bresenham line
        // x0: Starting x co-ordinate
        // y0: Starting y co-ordinate
        // x1: Ending x co-ordinate
        // y1: Ending y co-ordinate
        // color: Color used when drawing
        virtual public void DrawLine(int x0, int y0, int x1, int y1, BasicColor color) {
            DrawLineDotted(x0, y0, x1, y1, 0, 1, color);
        }
        // Draws a bresenham line with a fixed pattern of empty and solid pixels
        // Based on: http://www.cs.unc.edu/~mcmillan/comp136/Lecture6/Lines.html
        // x0: Starting x co-ordinate
        // y0: Starting y co-ordinate
        // x1: Ending x co-ordinate
        // y1: Ending y co-ordinate
        // empty: The number of 'empty' pixels to render
        // solid: The number of 'solid' pixels to render
        // color: Color used when drawing
        virtual public void DrawLineDotted(int x0, int y0, int x1, int y1, int empty, int solid, BasicColor color) {
            if (solid == 0) {
                return;
            }
            // If a negative y int was passed in it will overflow to 65K something
            // Ugly, but drawCircleFilled() can pass in negative values so we need to check the values here
            y0 = (y0 > 65000) ?  0 : y0;
            y1 = (y1 > 65000) ?  0 : y1;
            // Check if we can use the optimised horizontal line method
            if ((y0 == y1) && (empty == 0)) {
                _display.DrawHLine((ushort)x0, (ushort)x1, (ushort)y0, (ushort)color);
                return;
            }
            // Check if we can use the optimised vertical line method.
            // This can make a huge difference in performance, but may
            // not work properly on every LCD controller:
            //if ((x0 == x1) && (empty == 0)) {
            //    // Warning: This may actually be slower than drawing individual pixels on 
            //    // short lines ... Set a minimum line size to use the 'optimised' method
            //    // (which changes the screen orientation) ?
            //    _display.DrawHLine((ushort)x0, (ushort)y0, (ushort)y1, (ushort)color);
            //    return;
            //}
            // Draw non-horizontal or dotted line
            var dy = y1 - y0;
            var dx = x1 - x0;
            var stepx = 0;
            var stepy = 0;
            var emptycount = 0;
            var solidcount = 0;
            if (dy < 0) { dy = -dy; stepy = -1; } else { stepy = 1; }
            if (dx < 0) { dx = -dx; stepx = -1; } else { stepx = 1; }
            dy <<= 1;                               // dy is now 2*dy
            dx <<= 1;                               // dx is now 2*dx
            emptycount = empty;
            solidcount = solid;
            DrawPixel(x0, y0, color);               // always start with solid pixels
            solidcount--;
            if (dx > dy) {
                int fraction = dy - (dx >> 1);        // same as 2*dy - dx
                while (x0 != x1) {
                    if (fraction >= 0) {
                        y0 +=  stepy;
                        fraction -= dx;                   // same as fraction -= 2*dx
                    }
                    x0 +=  stepx;
                    fraction += dy;                     // same as fraction -= 2*dy
                    if (empty == 0) {
                        // always draw a pixel ... no dotted line requested
                        DrawPixel(x0, y0, color);
                    } else if (solidcount != 0) {
                        // Draw solid pxiel and decrement counter
                        DrawPixel(x0, y0, color);
                        solidcount--;
                    } else if (emptycount != 0) {
                        // Empty pixel ... don't draw anything an decrement counter
                        emptycount--;
                    } else {
                        // Reset counters and draw solid pixel
                        emptycount = empty;
                        solidcount = solid;
                        DrawPixel(x0, y0, color);
                        solidcount--;
                    }
                }
            } else {
                int fraction = dx - (dy >> 1);
                while (y0 != y1) {
                    if (fraction >= 0) {
                        x0 += stepx;
                        fraction -= dy;
                    }
                    y0 += stepy;
                    fraction += dx;
                    if (empty == 0) {
                        // always draw a pixel ... no dotted line requested
                        DrawPixel(x0, y0, color);
                    }
                    if (solidcount != 0) {
                        // Draw solid pixel and decrement counter
                        DrawPixel(x0, y0, color);
                        solidcount--;
                    } else if (emptycount != 0) {
                        // Empty pixel ... don't draw anything an decrement counter
                        emptycount--;
                    } else {
                        // Reset counters and draw solid pixel
                        emptycount = empty;
                        solidcount = solid;
                        DrawPixel(x0, y0, color);
                        solidcount--;
                    }
                }
            }
        }
        // Draws a circle
        // Based on: http://www.cs.unc.edu/~mcmillan/comp136/Lecture7/circle.html
        // xCenter: The horizontal center of the circle
        // yCenter: The vertical center of the circle
        // radius: The circle's radius in pixels
        // color: Color used when drawing
        virtual public void DrawCircle(int xCenter, int yCenter, int radius, BasicColor color) {
            var x = 0;
            var y = radius;
            var p = (5 - radius * 4) / 4;
            DrawCirclePoints(xCenter, yCenter, x, y, color);
            while (x < y) {
                x++;
                if (p < 0) {
                    p += 2 * x + 1;
                } else {
                    y--;
                    p += 2 * (x - y) + 1;
                }
                DrawCirclePoints(xCenter, yCenter, x, y, color);
            }
        }
        // Draws a filled circle
        // xCenter: The horizontal center of the circle
        // yCenter: The vertical center of the circle
        // radius: The circle's radius in pixels
        // color: Color used when drawing
        virtual public void DrawCircleFilled(int xCenter, int yCenter, int radius, BasicColor color) {
            var f = 1 - radius;
            var ddF_x = 1;
            var ddF_y = -2 * radius;
            var x = 0;
            var y = radius;
            var xc_px = 0;
            var yc_my = 0;
            var xc_mx = 0;
            var xc_py = 0;
            var yc_mx = 0;
            var xc_my = 0;
            var lcdWidth = _display.Width;
            if (xCenter < lcdWidth) {
                DrawLine(
                    xCenter, 
                    yCenter - radius < 0 ? 0 : (yCenter - radius),
                    xCenter,
                    (yCenter - radius) + (2 * radius), color);
            }
            while (x < y) {
                if (f >= 0) {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;
                xc_px = xCenter + x;
                xc_mx = xCenter - x;
                xc_py = xCenter + y;
                xc_my = xCenter - y;
                yc_mx = yCenter - x;
                yc_my = yCenter - y;
                // Make sure X positions are not negative or too large or the pixels will overflow.  Y overflow is handled in drawLine().
                if ((xc_px < lcdWidth) && (xc_px >= 0)) DrawLine(xc_px, yc_my, xc_px, yc_my + 2 * y, color);
                if ((xc_mx < lcdWidth) && (xc_mx >= 0)) DrawLine(xc_mx, yc_my, xc_mx, yc_my + 2 * y, color);
                if ((xc_py < lcdWidth) && (xc_py >= 0)) DrawLine(xc_py, yc_mx, xc_py, yc_mx + 2 * x, color);
                if ((xc_my < lcdWidth) && (xc_my >= 0)) DrawLine(xc_my, yc_mx, xc_my, yc_mx + 2 * x, color);
            }
        }
        // Draws a filled rounded corner
        // xCenter: The horizontal center of the circle
        // yCenter: The vertical center of the circle
        // radius: The circle's radius in pixels
        // position: The position of the corner, which affects how it will be rendered
        // color: Color used when drawing
        virtual public void DrawCornerFilled(int xCenter, int yCenter, int radius, CornerPosition position, BasicColor color) {
            var f = 1 - radius;
            var ddF_x = 1;
            var ddF_y = -2 * radius;
            var x = 0;
            var y = radius;
            var xc_px = 0;
            var yc_my = 0;
            var xc_mx = 0;
            var xc_py = 0;
            var yc_mx = 0;
            var xc_my = 0;
            var lcdWidth = _display.Width;
            switch (position) {
                case CornerPosition.TopRight:
                case CornerPosition.TopLeft:
                    if (xCenter < lcdWidth) DrawLine(xCenter, yCenter - radius < 0 ? 0 : yCenter - radius, xCenter, yCenter, color);
                    break;
                case CornerPosition.BottomRight:
                case CornerPosition.BottomLeft:
                    if (xCenter < lcdWidth) DrawLine(xCenter, yCenter - radius < 0 ? 0 : yCenter, xCenter, (yCenter - radius) + (2 * radius), color);
                    break;
            }
            while (x < y) {
                if (f >= 0) {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;
                xc_px = xCenter + x;
                xc_mx = xCenter - x;
                xc_py = xCenter + y;
                xc_my = xCenter - y;
                yc_mx = yCenter - x;
                yc_my = yCenter - y;
                switch (position) {
                    case CornerPosition.TopRight:
                        if ((xc_px < lcdWidth) && (xc_px >= 0)) DrawLine(xc_px, yc_my, xc_px, yCenter, color);
                        if ((xc_py < lcdWidth) && (xc_py >= 0)) DrawLine(xc_py, yc_mx, xc_py, yCenter, color);
                        break;
                    case CornerPosition.BottomRight:
                        if ((xc_px < lcdWidth) && (xc_px >= 0)) DrawLine(xc_px, yCenter, xc_px, yc_my + 2 * y, color);
                        if ((xc_py < lcdWidth) && (xc_py >= 0)) DrawLine(xc_py, yCenter, xc_py, yc_mx + 2 * x, color);
                        break;
                    case CornerPosition.TopLeft:
                        if ((xc_mx < lcdWidth) && (xc_mx >= 0)) DrawLine(xc_mx, yc_my, xc_mx, yCenter, color);
                        if ((xc_my < lcdWidth) && (xc_my >= 0)) DrawLine(xc_my, yc_mx, xc_my, yCenter, color);
                        break;
                    case CornerPosition.BottomLeft:
                        if ((xc_mx < lcdWidth) && (xc_mx >= 0)) DrawLine(xc_mx, yCenter, xc_mx, yc_my + 2 * y, color);
                        if ((xc_my < lcdWidth) && (xc_my >= 0)) DrawLine(xc_my, yCenter, xc_my, yc_mx + 2 * x, color);
                        break;
                }
            }
        }
        // Draws a simple arrow of the specified width
        // x: X co-ordinate of the smallest point of the arrow
        // y: Y co-ordinate of the smallest point of the arrow
        // size: Total width/height of the arrow in pixels
        // direction: The direction that the arrow is pointing
        // color: Color used when drawing
        virtual public void DrawArrow(int x, int y, int size, DrawingDirection direction, BasicColor color) {
            DrawPixel(x, y, color);
            if (size == 1) {
                return;
            }
            var i = 0;
            switch (direction) {
                case DrawingDirection.Left:
                    for (i = 1; i < size; i++) {
                        DrawLine(x + i, y - i, x + i, y + i, color);
                    }
                    break;
                case DrawingDirection.Right:
                    for (i = 1; i < size; i++) {
                        DrawLine(x - i, y - i, x - i, y + i, color);
                    }
                    break;
                case DrawingDirection.Up:
                    for (i = 1; i < size; i++) {
                        DrawLine(x - i, y + i, x + i, y + i, color);
                    }
                    break;
                case DrawingDirection.Down:
                    for (i = 1; i < size; i++) {
                        DrawLine(x - i, y - i, x + i, y - i, color);
                    }
                    break;
                default:
                    break;
            }
        }
        // Draws a simple (empty) rectangle
        // x0: Starting x coordinate
        // y0: Starting y coordinate
        // x1: Ending x coordinate
        // y1: Ending y coordinate
        // color: Color used when drawing
        virtual public void DrawRectangle(int x0, int y0, int x1, int y1, BasicColor color) {
            var x = 0;
            var y = 0;
            if (y1 < y0) { // Switch y1 and y0
                y = y1;
                y1 = y0;
                y0 = y;
            }
            if (x1 < x0) { // Switch x1 and x0
                x = x1;
                x1 = x0;
                x0 = x;
            }
            DrawLine(x0, y0, x1, y0, color);
            DrawLine(x1, y0, x1, y1, color);
            DrawLine(x1, y1, x0, y1, color);
            DrawLine(x0, y1, x0, y0, color);
        }
        // Draws a filled rectangle
        // x0: Starting x co-ordinate
        // y0: Starting y co-ordinate
        // x1: Ending x co-ordinate
        // y1: Ending y co-ordinate
        // color: Color used when drawing
        virtual public void DrawRectangleFilled(int x0, int y0, int x1, int y1, BasicColor color) {
            var height = 0;
            var x = 0;
            var y = 0;
            if (y1 < y0) { // Switch y1 and y0
                y = y1;
                y1 = y0;
                y0 = y;
            }
            if (x1 < x0) { // Switch x1 and x0
                x = x1;
                x1 = x0;
                x0 = x;
            }
            height = y1 - y0;
            for (height = y0; y1 > height - 1; ++height) {
                DrawLine(x0, height, x1, height, color);
            }
        }
        // Draws a filled rectangle with rounded corners
        // x0: Starting x co-ordinate
        // y0: Starting y co-ordinate
        // x1: Ending x co-ordinate
        // y1: Ending y co-ordinate
        // color: Color used when drawing
        // radius: Corner radius in pixels
        // corners: Which corners to round
        virtual public void DrawRectangleRounded(int x0, int y0, int x1, int y1, BasicColor color, int radius, RoundedCornerStyle corners) {
            var height = 0;
            var y = 0;
            if (corners == RoundedCornerStyle.None) {
                DrawRectangleFilled(x0, y0, x1, y1, color);
                return;
            }
            // Calculate height
            if (y1 < y0) {
                y = y1;
                y1 = y0;
                y0 = y;
            }
            height = y1 - y0;
            // Check radius
            if (radius > height / 2) {
                radius = height / 2;
            }
            radius -= 1;

            // Draw body
            DrawRectangleFilled(x0 + radius, y0, x1 - radius, y1, color);
            switch (corners) {
                case RoundedCornerStyle.All:
                    DrawCircleFilled(x0 + radius, y0 + radius, radius, color);
                    DrawCircleFilled(x1 - radius, y0 + radius, radius, color);
                    DrawCircleFilled(x0 + radius, y1 - radius, radius, color);
                    DrawCircleFilled(x1 - radius, y1 - radius, radius, color);
                    if (radius * 2 + 1 < height) {
                        DrawRectangleFilled(x0, y0 + radius, x0 + radius, y1 - radius, color);
                        DrawRectangleFilled(x1 - radius, y0 + radius, x1, y1 - radius, color);
                    }
                    break;
                case RoundedCornerStyle.Top:
                    DrawCircleFilled(x0 + radius, y0 + radius, radius, color);
                    DrawCircleFilled(x1 - radius, y0 + radius, radius, color);
                    DrawRectangleFilled(x0, y0 + radius, x0 + radius, y1, color);
                    DrawRectangleFilled(x1 - radius, y0 + radius, x1, y1, color);
                    break;
                case RoundedCornerStyle.Bottom:
                    DrawCircleFilled(x0 + radius, y1 - radius, radius, color);
                    DrawCircleFilled(x1 - radius, y1 - radius, radius, color);
                    DrawRectangleFilled(x0, y0, x0 + radius, y1 - radius, color);
                    DrawRectangleFilled(x1 - radius, y0, x1, y1 - radius, color);
                    break;
                case RoundedCornerStyle.Left:
                    DrawCircleFilled(x0 + radius, y0 + radius, radius, color);
                    DrawCircleFilled(x0 + radius, y1 - radius, radius, color);
                    if (radius * 2 + 1 < height) {
                        DrawRectangleFilled(x0, y0 + radius, x0 + radius, y1 - radius, color);
                    }
                    DrawRectangleFilled(x1 - radius, y0, x1, y1, color);
                    break;
                case RoundedCornerStyle.Right:
                    DrawCircleFilled(x1 - radius, y0 + radius, radius, color);
                    DrawCircleFilled(x1 - radius, y1 - radius, radius, color);
                    if (radius * 2 + 1 < height) {
                        DrawRectangleFilled(x1 - radius, y0 + radius, x1, y1 - radius, color);
                    }
                    DrawRectangleFilled(x0, y0, x0 + radius, y1, color);
                    break;
                default:
                    break;
            }
        }
        // Draws a triangle
        // x0: x co-ordinate for point 0
        // y0: y co-ordinate for point 0
        // x1: x co-ordinate for point 1
        // y1: y co-ordinate for point 1
        // x2: x co-ordinate for point 2
        // y2: y co-ordinate for point 2
        // color: Color used when drawing
        virtual public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, BasicColor color) {
            DrawLine(x0, y0, x1, y1, color);
            DrawLine(x1, y1, x2, y2, color);
            DrawLine(x2, y2, x0, y0, color);
        }
        // Draws a filled triangle
        // x0: x co-ordinate for point 0
        // y0: y co-ordinate for point 0
        // x1: x co-ordinate for point 1
        // y1: y co-ordinate for point 1
        // x2: x co-ordinate for point 2
        // y2: y co-ordinate for point 2
        // color: Fill color
        // Example
        //      Draw a white triangle
        //          DrawTriangleFilled ( 100, 10, 20, 120, 230, 290, COLOR_WHITE);
        //      Draw black circles at each point of the triangle
        //          DrawCircleFilled(100, 10, 2, COLOR_BLACK);
        //          DrawCircleFilled(20, 120, 2, COLOR_BLACK);
        //          DrawCircleFilled(230, 290, 2, COLOR_BLACK);
        virtual public void DrawTriangleFilled(int x0, int y0, int x1, int y1, int x2, int y2, BasicColor color) {
            if (y0 > y1) { // Re-order vertices by ascending Y values (smallest first)
                Swap(ref y0, ref y1); 
                Swap(ref x0, ref x1);
            }
            if (y1 > y2) {
                Swap(ref y2, ref y1);
                Swap(ref x2, ref x1);
            }
            if (y0 > y1) {
                Swap(ref y0, ref y1);
                Swap(ref x0, ref x1);
            }
            // Interpolation deltas
            var dx1 = 0;
            var dx2 = 0;
            var dx3 = 0;
            // Scanline co-ordinates
            var sx1 = 0;
            var sx2 = 0;
            var sy = 0;
            sx1 = sx2 = x0 * 1000;  // Use fixed point math for x axis values
            sy = y0;
            // Calculate interpolation deltas
            if (y1 - y0 > 0) dx1 = ((x1 - x0) * 1000) / (y1 - y0);
            else dx1 = 0;
            if (y2 - y0 > 0) dx2 = ((x2 - x0) * 1000) / (y2 - y0);
            else dx2 = 0;
            if (y2 - y1 > 0) dx3 = ((x2 - x1) * 1000) / (y2 - y1);
            else dx3 = 0;
            // Render scanlines (horizontal lines are the fastest rendering method)
            if (dx1 > dx2) {
                for (; sy <= y1; sy++, sx1 += dx2, sx2 += dx1) {
                    DrawLine(sx1 / 1000, sy, sx2 / 1000, sy, color);
                }
                sx2 = x1 * 1000;
                sy = y1;
                for (; sy <= y2; sy++, sx1 += dx2, sx2 += dx3) {
                    DrawLine(sx1 / 1000, sy, sx2 / 1000, sy, color);
                }
            } else {
                for (; sy <= y1; sy++, sx1 += dx1, sx2 += dx2) {
                    DrawLine(sx1 / 1000, sy, sx2 / 1000, sy, color);
                }
                sx1 = x1 * 1000;
                sy = y1;
                for (; sy <= y2; sy++, sx1 += dx3, sx2 += dx2) {
                    DrawLine(sx1 / 1000, sy, sx2 / 1000, sy, color);
                }
            }
        }
        // Draws a progress bar with rounded corners
        // x: Starting x location
        // y: Starting y location
        // width: Total width of the progress bar in pixels
        // height: Total height of the progress bar in pixels
        // borderCorners: The type of rounded corners to render with the progress bar border
        // progressCorners: The type of rounded corners to render with the inner progress bar
        // borderColor: 16-bit color for the outer border
        // borderFillColor: 16-bit color for the interior of the outer border
        // progressBorderColor: 16-bit color for the progress bar's border
        // progressFillColor: 16-bit color for the inner bar's fill
        // progress: Progress percentage (between 0 and 100)
        // Draw a the progress bar (150x15 pixels large, starting at X:10, Y:195 with rounded corners on the top and showing 72% progress)
        //      DrawProgressBar(10, 195, 150, 15, RoundedCornerStyle.TOP, RoundedCornerStyle.TOP, GrayScaleValues.GRAY_128, GrayScaleValues.GRAY_225, ColorTheme.LIMEGREEN_LIGHTER, ColorTheme.LIMEGREEN_BASE, 72 );
        virtual public void DrawProgressBar(int x, int y, int width, int height, RoundedCornerStyle borderCorners, RoundedCornerStyle progressCorners, BasicColor borderColor, BasicColor borderFillColor, BasicColor progressBorderColor, BasicColor progressFillColor, int progress) {
            // Draw border with rounded corners
            DrawRectangleRounded(x, y, x + width, y + height, borderColor, 5, borderCorners);
            DrawRectangleRounded(x + 1, y + 1, x + width - 1, y + height - 1, borderFillColor, 5, borderCorners);
            // Progress bar
            if (progress > 0 && progress <= 100) {
                // Calculate bar size
                var bw = (width - 6);   // bar at 100%
                if (progress != 100) {
                    bw = (bw * progress) / 100;
                }
                DrawRectangleRounded(x + 3, y + 3, bw + x + 3, y + height - 3, progressBorderColor, 5, progressCorners);
                DrawRectangleRounded(x + 4, y + 4, bw + x + 3 - 1, y + height - 4, progressFillColor, 5, progressCorners);
            }
        }
        virtual public void DrawButton(int x, int y, int width, int height, FontInfo fontInfo, int fontHeight, BasicColor borderColor, BasicColor fillColor, BasicColor fontColor, string text, Canvas.RoundedCornerStyle cornerStyle = RoundedCornerStyle.All) {
            // Border
            DrawRectangleRounded(x, y, x + width, y + height, borderColor, 5, cornerStyle);
            // Fill
            DrawRectangleRounded(x + 2, y + 2, x + width - 2, y + height - 2, fillColor, 5, cornerStyle);
            // Render text
            if (text.Length != 0) {
                var textWidth = GetStringWidth(fontInfo, text);
                var xStart = x + (width / 2) - (textWidth / 2);
                var yStart = y + (height / 2) - (fontHeight / 2) + 1;
                DrawString(xStart, yStart, fontColor, fontInfo, text);
            }
        }
        // Renders a 16x16 monochrome icon using the supplied uint16_t array.
        // x: The horizontal location to start rendering from
        // y: The vertical location to start rendering from
        // color: The RGB565 color to use when rendering the icon
        // icon: The uint16_t array containing the 16x16 image data
        // Example
        //      Renders the info icon, which has two seperate parts ... the exterior
        //      and a seperate interior mask if you want to fill the contents with a different color
        //  DrawIcon16(132, 202, BasicColor.BLUE, icons16_info);
        //  DrawIcon16(132, 202, BasicColor.WHITE, icons16_info_interior);
        virtual public void DrawIcon16(int x, int y, BasicColor color, ushort[] icon) {
            int i;
            for (i = 0; i < 16; i++) {
                if ((icon[i] & 0X8000) != 0) DrawPixel(x, y + i, color);
                if ((icon[i] & 0X4000) != 0) DrawPixel(x + 1, y + i, color);
                if ((icon[i] & 0X2000) != 0) DrawPixel(x + 2, y + i, color);
                if ((icon[i] & 0X1000) != 0) DrawPixel(x + 3, y + i, color);
                if ((icon[i] & 0X0800) != 0) DrawPixel(x + 4, y + i, color);
                if ((icon[i] & 0X0400) != 0) DrawPixel(x + 5, y + i, color);
                if ((icon[i] & 0X0200) != 0) DrawPixel(x + 6, y + i, color);
                if ((icon[i] & 0X0100) != 0) DrawPixel(x + 7, y + i, color);
                if ((icon[i] & 0X0080) != 0) DrawPixel(x + 8, y + i, color);
                if ((icon[i] & 0x0040) != 0) DrawPixel(x + 9, y + i, color);
                if ((icon[i] & 0X0020) != 0) DrawPixel(x + 10, y + i, color);
                if ((icon[i] & 0X0010) != 0) DrawPixel(x + 11, y + i, color);
                if ((icon[i] & 0X0008) != 0) DrawPixel(x + 12, y + i, color);
                if ((icon[i] & 0X0004) != 0) DrawPixel(x + 13, y + i, color);
                if ((icon[i] & 0X0002) != 0) DrawPixel(x + 14, y + i, color);
                if ((icon[i] & 0X0001) != 0) DrawPixel(x + 15, y + i, color);
            }
        }
        // Converts a 24-bit RGB color to an equivalent 16-bit RGB565 value
        public ushort GetRGB24toRGB565(byte r, byte g, byte b) {
            var rgb = ((r / 8) << 11) | ((g / 4) << 5) | (b / 8);
            return (ushort) rgb;
        }
        // Converts a 16-bit RGB565 color to a standard 32-bit BGRA32 color (with alpha set to 0xFF)
        public uint GetRGB565toBGRA32(uint color) {
            var bits = color;
            var blue = bits & 0x001F;     // 5 bits blue
            var green = bits & 0x07E0;    // 6 bits green
            var red = bits & 0xF800;      // 5 bits red
            // Return shifted bits with alpha set to 0xFF
            var bgr = (red << 8) | (green << 5) | (blue << 3) | 0xFF000000;
            return (uint) bgr;
        }
        // Reverses a 16-bit color from BGR to RGB
        public uint GetBGRtoRGB(uint color) {
            byte r = 0;
            byte g = 0;
            byte b = 0;
            b = (byte)((color >> 0) & 0x1f);
            g = (byte)((color >> 5) & 0x3f);
            r = (byte)((color >> 11) & 0x1f);
            return (uint)((b << 11) + (g << 5) + (r << 0));
        }
        //  Draws a string using the supplied font
        // x: Starting x co-ordinate
        // y: Starting y co-ordinate
        // color: Color to use when rendering the font
        // fontInfo: FontInfo reference to use when drawing the string
        // str: The string to render
        // Example
        //  DrawString(0, 90,  BasicColor.BLACK, bitstreamVeraSansMono9ptFontInfo, "Vera Mono 9 (30 chars wide)");
        //  DrawString(0, 105, BasicColor.BLACK, bitstreamVeraSansMono9ptFontInfo, "123456789012345678901234567890");
        virtual public void DrawString(int x, int y, BasicColor color, FontInfo fontInfo, string text) {
            // set current x, y to that of requested
            var currentX = x;
            // Send individual characters
            foreach (var characterToOutput in text) {
                // We need to manually calculate width in pages since this is screwy with variable width fonts
                // var heightPages = charWidth % 8 ? charWidth / 8 : charWidth / 8 + 1;
                FontCharInfo fontCharInfo = fontInfo.GetFontCharInfo(characterToOutput);
                DrawCharBitmap(currentX, y, color, fontInfo.Data, fontCharInfo.Offset, fontCharInfo.WidthBits, fontInfo.Height);
                // next char X
                currentX += fontCharInfo.WidthBits + 1;
            }
        }
        public int GetStringWidth(FontInfo fontInfo, string text) {
            var width = 0;
            var startChar = fontInfo.StartChar;
            foreach (var characterToOutput in text) {
                FontCharInfo fontCharInfo = fontInfo.GetFontCharInfo(characterToOutput);
                width += fontCharInfo.WidthBits + 1;
            }
            return width;
        }
        virtual public void DrawBitmapImage(int x, int y, string filename) {
            throw new NotImplementedException("yet");
        }
        virtual public void SetOrientation(LCD.Orientation orientation) {
            _display.SetOrientation(orientation);
        }
        // Draws a single bitmap character
        protected void DrawCharBitmap(int xPixel, int yPixel, BasicColor color, byte[] glyph, int glyphDataOffset, int cols, int rows) {
            var currentY = 0;
            var currentX = 0;
            var indexIntoGlyph = 0;
            var _colPages = 0;

            // set initial current y
            currentY = yPixel;
            currentX = xPixel;

            // Figure out how many columns worth of data we have
            if (cols % 8 != 0) {
                _colPages = cols / 8 + 1;
            } else {
                _colPages = cols / 8;
            }
            for (var _row =0; _row < rows; _row++) {
                for (var _col = 0; _col < _colPages; _col++) {
                    if (_row == 0) {
                        indexIntoGlyph = _col;
                    } else {
                        indexIntoGlyph = (_row * _colPages) + _col;
                    }
                    currentY = yPixel + _row;
                    currentX = xPixel + (_col * 8);
                    // send the data byte
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X80)) != 0) DrawPixel(currentX, currentY, color);
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X40)) != 0) DrawPixel(currentX + 1, currentY, color);
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X20)) != 0) DrawPixel(currentX + 2, currentY, color);
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X10)) != 0) DrawPixel(currentX + 3, currentY, color);
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X08)) != 0) DrawPixel(currentX + 4, currentY, color);
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X04)) != 0) DrawPixel(currentX + 5, currentY, color);
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X02)) != 0) DrawPixel(currentX + 6, currentY, color);
                    if ((glyph[glyphDataOffset + indexIntoGlyph] & (0X01)) != 0) DrawPixel(currentX + 7, currentY, color);
                }
            }
        }
        //private void drawCharSmall(int x, int y, int color, byte c, Font font)
        // Helper method to accurately draw individual circle points
        virtual protected void DrawCirclePoints(int cx, int cy, int x, int y, BasicColor color) {
            if (x == 0) {
                DrawPixel(cx, cy + y, color);
                DrawPixel(cx, cy - y, color);
                DrawPixel(cx + y, cy, color);
                DrawPixel(cx - y, cy, color);
            } else if (x == y) {
                DrawPixel(cx + x, cy + y, color);
                DrawPixel(cx - x, cy + y, color);
                DrawPixel(cx + x, cy - y, color);
                DrawPixel(cx - x, cy - y, color);
            } else if (x < y) {
                DrawPixel(cx + x, cy + y, color);
                DrawPixel(cx - x, cy + y, color);
                DrawPixel(cx + x, cy - y, color);
                DrawPixel(cx - x, cy - y, color);
                DrawPixel(cx + y, cy + x, color);
                DrawPixel(cx - y, cy + x, color);
                DrawPixel(cx + y, cy - x, color);
                DrawPixel(cx - y, cy - x, color);
            }
        }
        // Swaps values a and b
        protected void Swap(ref int a, ref int b) {
            var t = a;
            a = b;
            b = t;
        }
        public void Replay(BasicTypeDeSerializerContext context) {
            if (context == null) throw new ArgumentNullException("context");
            byte command = 0;
            while (context.MoreData) {
                command = BasicTypeDeSerializer.Get(context);
                switch ((Command)command) {
                    case Command.DrawTestPattern:
                        DrawTestPattern();
                        break;
                    case Command.DrawPixel:
                        GetDrawPixel(context);
                        break;
                    case Command.DrawFill:
                        GetDrawFill(context);
                        break;
                    case Command.DrawLine:
                        GetDrawLine(context);
                        break;
                    case Command.DrawLineDotted:
                        GetDrawLineDotted(context);
                        break;
                    case Command.DrawCircle:
                        GetDrawCircle(context);
                        break;
                    case Command.DrawCircleFilled:
                        GetDrawCircleFilled(context);
                        break;
                    case Command.DrawCornerFilled:
                        GetDrawCornerFilled(context);
                        break;
                    case Command.DrawArrow:
                        GetDrawArrow(context);
                        break;
                    case Command.DrawRectangle:
                        GetDrawRectangle(context);
                        break;
                    case Command.DrawRectangleFilled:
                        GetDrawRectangleFilled(context);
                        break;
                    case Command.DrawRectangleRounded:
                        GetDrawRectangleRounded(context);
                        break;
                    case Command.DrawTriangle:
                        GetDrawTriangle(context);
                        break;
                    case Command.DrawTriangleFilled:
                        GetDrawTriangleFilled(context);
                        break;
                    case Command.DrawProgressBar:
                        GetDrawProgressBar(context);
                        break;
                    case Command.DrawButton:
                        GetDrawButton(context);
                        break;
                    case Command.DrawIcon16:
                        GetDrawIcon16(context);
                        break;
                    case Command.DrawString:
                        GetDrawString(context);
                        break;
                    case Command.DrawBitmapImage:
                        throw new NotImplementedException("command");
                    case Command.DrawCirclePoints:
                        GetDrawCirclePoints(context);
                        break;
                    case Command.SetOrientation:
                        GetSetOrientation(context);
                        break;
                    default:
                        throw new ApplicationException("command");
                }
            }
        }
        private void GetDrawPixel(BasicTypeDeSerializerContext context) {
            ushort x = 0;
            ushort y = 0;
            ushort color = 0;
            x = BasicTypeDeSerializer.Get(context, x);
            y = BasicTypeDeSerializer.Get(context, y);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawPixel(x, y, (BasicColor)color);
        }
        private void GetDrawFill(BasicTypeDeSerializerContext context) {
            ushort color = 0;
            color = BasicTypeDeSerializer.Get(context, color);
            DrawFill((BasicColor)color);
        }
        private void GetDrawLine(BasicTypeDeSerializerContext context) {
            ushort x0 = 0;
            ushort y0 = 0;
            ushort x1 = 0;
            ushort y1 = 0;
            ushort color = 0;
            x0 = BasicTypeDeSerializer.Get(context, x0);
            y0 = BasicTypeDeSerializer.Get(context, y0);
            x1 = BasicTypeDeSerializer.Get(context, x1);
            y1 = BasicTypeDeSerializer.Get(context, y1);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawLine(x0, y0, x1, y1, (BasicColor)color);
        }
        private void GetDrawLineDotted(BasicTypeDeSerializerContext context) {
            ushort x0 = 0;
            ushort y0 = 0;
            ushort x1 = 0;
            ushort y1 = 0;
            ushort empty = 0;
            ushort solid = 0;
            ushort color = 0;
            x0 = BasicTypeDeSerializer.Get(context, x0);
            y0 = BasicTypeDeSerializer.Get(context, y0);
            x1 = BasicTypeDeSerializer.Get(context, x1);
            y1 = BasicTypeDeSerializer.Get(context, y1);
            color = BasicTypeDeSerializer.Get(context, color);
            empty = BasicTypeDeSerializer.Get(context, empty);
            solid = BasicTypeDeSerializer.Get(context, solid);
            DrawLineDotted(x0, y0, x1, y1, empty, solid, (BasicColor)color);
        }
        private void GetDrawCircle(BasicTypeDeSerializerContext context) {
            ushort xCenter = 0;
            ushort yCenter = 0;
            ushort radius = 0;
            ushort color = 0;
            xCenter = BasicTypeDeSerializer.Get(context, xCenter);
            yCenter = BasicTypeDeSerializer.Get(context, yCenter);
            radius = BasicTypeDeSerializer.Get(context, radius);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawCircle(xCenter, yCenter, radius, (BasicColor)color);
        }
        private void GetDrawCircleFilled(BasicTypeDeSerializerContext context) {
            ushort xCenter = 0;
            ushort yCenter = 0;
            ushort radius = 0;
            ushort color = 0;
            xCenter = BasicTypeDeSerializer.Get(context, xCenter);
            yCenter = BasicTypeDeSerializer.Get(context, yCenter);
            radius = BasicTypeDeSerializer.Get(context, radius);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawCircleFilled(xCenter, yCenter, radius, (BasicColor)color);
        }
        private void GetDrawCornerFilled(BasicTypeDeSerializerContext context) {
            ushort xCenter = 0;
            ushort yCenter = 0;
            ushort radius = 0;
            ushort position = 0;
            ushort color = 0;
            xCenter = BasicTypeDeSerializer.Get(context, xCenter);
            yCenter = BasicTypeDeSerializer.Get(context, yCenter);
            radius = BasicTypeDeSerializer.Get(context, radius);
            position = BasicTypeDeSerializer.Get(context, position);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawCornerFilled(xCenter, yCenter, radius, (CornerPosition)position, (BasicColor)color);
        }
        private void GetDrawArrow(BasicTypeDeSerializerContext context) {
            ushort x = 0;
            ushort y = 0;
            ushort size = 0;
            ushort direction = 0;
            ushort color = 0;
            x = BasicTypeDeSerializer.Get(context, x);
            y = BasicTypeDeSerializer.Get(context, y);
            size = BasicTypeDeSerializer.Get(context, size);
            direction = BasicTypeDeSerializer.Get(context, direction);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawArrow(x, y, size, (DrawingDirection)direction, (BasicColor)color);
        }
        private void GetDrawRectangle(BasicTypeDeSerializerContext context) {
            ushort x0 = 0;
            ushort y0 = 0;
            ushort x1 = 0;
            ushort y1 = 0;
            ushort color = 0;
            x0 = BasicTypeDeSerializer.Get(context, x0);
            y0 = BasicTypeDeSerializer.Get(context, y0);
            x1 = BasicTypeDeSerializer.Get(context, x1);
            y1 = BasicTypeDeSerializer.Get(context, y1);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawRectangle(x0, y0, x1, y1, (BasicColor)color);
        }
        private void GetDrawRectangleFilled(BasicTypeDeSerializerContext context) {
            ushort x0 = 0;
            ushort y0 = 0;
            ushort x1 = 0;
            ushort y1 = 0;
            ushort color = 0;
            x0 = BasicTypeDeSerializer.Get(context, x0);
            y0 = BasicTypeDeSerializer.Get(context, y0);
            x1 = BasicTypeDeSerializer.Get(context, x1);
            y1 = BasicTypeDeSerializer.Get(context, y1);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawRectangleFilled(x0, y0, x1, y1, (BasicColor)color);
        }
        private void GetDrawRectangleRounded(BasicTypeDeSerializerContext context) {
            ushort x0 = 0;
            ushort y0 = 0;
            ushort x1 = 0;
            ushort y1 = 0;
            ushort color = 0;
            ushort radius = 0;
            ushort corners = 0;
            x0 = BasicTypeDeSerializer.Get(context, x0);
            y0 = BasicTypeDeSerializer.Get(context, y0);
            x1 = BasicTypeDeSerializer.Get(context, x1);
            y1 = BasicTypeDeSerializer.Get(context, y1);
            color = BasicTypeDeSerializer.Get(context, color);
            radius = BasicTypeDeSerializer.Get(context, radius);
            corners = BasicTypeDeSerializer.Get(context, corners);
            DrawRectangleRounded(x0, y0, x1, y1, (BasicColor)color, radius, (RoundedCornerStyle)corners);
        }
        private void GetDrawTriangle(BasicTypeDeSerializerContext context) {
            ushort x0 = 0;
            ushort y0 = 0;
            ushort x1 = 0;
            ushort y1 = 0;
            ushort x2 = 0;
            ushort y2 = 0; 
            ushort color = 0;
            x0 = BasicTypeDeSerializer.Get(context, x0);
            y0 = BasicTypeDeSerializer.Get(context, y0);
            x1 = BasicTypeDeSerializer.Get(context, x1);
            y1 = BasicTypeDeSerializer.Get(context, y1);
            x2 = BasicTypeDeSerializer.Get(context, x2);
            y2 = BasicTypeDeSerializer.Get(context, y2);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawTriangle(x0, y0, x1, y1, x2, y2, (BasicColor)color);
        }
        private void GetDrawTriangleFilled(BasicTypeDeSerializerContext context) {
            ushort x0 = 0;
            ushort y0 = 0;
            ushort x1 = 0;
            ushort y1 = 0;
            ushort x2 = 0;
            ushort y2 = 0;
            ushort color = 0;
            x0 = BasicTypeDeSerializer.Get(context, x0);
            y0 = BasicTypeDeSerializer.Get(context, y0);
            x1 = BasicTypeDeSerializer.Get(context, x1);
            y1 = BasicTypeDeSerializer.Get(context, y1);
            x2 = BasicTypeDeSerializer.Get(context, x2);
            y2 = BasicTypeDeSerializer.Get(context, y2);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawTriangleFilled(x0, y0, x1, y1, x2, y2, (BasicColor)color);
        }
        private void GetDrawProgressBar(BasicTypeDeSerializerContext context) {
            ushort x = 0;
            ushort y = 0;
            ushort width = 0;
            ushort height = 0;
            ushort borderCorners = 0;
            ushort progressCorners = 0;
            ushort borderColor = 0;
            ushort borderFillColor = 0;
            ushort progressBorderColor = 0;
            ushort progressFillColor = 0;
            ushort progress = 0;
            x = BasicTypeDeSerializer.Get(context, x);
            y = BasicTypeDeSerializer.Get(context, y);
            width = BasicTypeDeSerializer.Get(context, width);
            height = BasicTypeDeSerializer.Get(context, height);
            borderCorners = BasicTypeDeSerializer.Get(context, borderCorners);
            progressCorners = BasicTypeDeSerializer.Get(context, progressCorners);
            borderColor = BasicTypeDeSerializer.Get(context, borderColor);
            borderFillColor = BasicTypeDeSerializer.Get(context, borderFillColor);
            progressBorderColor = BasicTypeDeSerializer.Get(context, progressBorderColor);
            progressFillColor = BasicTypeDeSerializer.Get(context, progressFillColor);
            progress = BasicTypeDeSerializer.Get(context, progress);
            DrawProgressBar(
                x, y,
                width, height,
                (RoundedCornerStyle)borderCorners,(RoundedCornerStyle)progressCorners,
                (BasicColor)borderColor, (BasicColor)borderFillColor,
                (BasicColor)progressBorderColor, (BasicColor)progressFillColor,
                progress);
        }
        private void GetDrawButton(BasicTypeDeSerializerContext context) {
            ushort x=0;
            ushort y=0;
            ushort width=0;
            ushort height=0;
            ushort fontInfoID=0;
            ushort fontHeight=0;
            ushort borderColor=0;
            ushort fillColor=0;
            ushort fontColor=0;
            string text;
            ushort cornerStyle = 0;
            x = BasicTypeDeSerializer.Get(context, x);
            y = BasicTypeDeSerializer.Get(context, y);
            width = BasicTypeDeSerializer.Get(context, width);
            height = BasicTypeDeSerializer.Get(context, height);
            fontInfoID = BasicTypeDeSerializer.Get(context, fontInfoID);
            fontHeight = BasicTypeDeSerializer.Get(context, fontHeight);
            borderColor = BasicTypeDeSerializer.Get(context, borderColor);
            fillColor = BasicTypeDeSerializer.Get(context, fillColor);
            fontColor = BasicTypeDeSerializer.Get(context, fontColor);
            text = BasicTypeDeSerializer.Get(context, "");
            cornerStyle = BasicTypeDeSerializer.Get(context, cornerStyle);
            DrawButton(
                x, y,
                width, height,
                FontInfoLookUp(fontInfoID), fontHeight,
                (BasicColor)borderColor, (BasicColor)fillColor, (BasicColor)fontColor,
                text,
                (RoundedCornerStyle)cornerStyle);
        }       
        private FontInfo FontInfoLookUp(ushort fontInfoID) {
            if (_fontInfoTable.Contains(fontInfoID) == true) {
                return ((FontDefinition)_fontInfoTable[fontInfoID]).GetFontInfo();
            } else if (fontInfoID == DejaVuSans9.ID) {
                _fontInfoTable[fontInfoID] = new DejaVuSans9();
            } else if (fontInfoID == DejaVuSansBold9.ID) {
                _fontInfoTable[fontInfoID] = new DejaVuSansBold9();
            } else if (fontInfoID == DejaVuSansCondensed9.ID) {
                _fontInfoTable[fontInfoID] = new DejaVuSansCondensed9();
            } else if (fontInfoID == DejaVuSansMono8.ID) {
                _fontInfoTable[fontInfoID] = new DejaVuSansMono8();
            } else if (fontInfoID == DejaVuSansMonoBold8.ID) {
                _fontInfoTable[fontInfoID] = new DejaVuSansMonoBold8();
            } else if (fontInfoID == Verdana14.ID) {
                _fontInfoTable[fontInfoID] = new Verdana14();
            } else if (fontInfoID == Verdana9.ID) {
                _fontInfoTable[fontInfoID] = new Verdana9();
            } else if (fontInfoID == VerdanaBold14.ID) {
                _fontInfoTable[fontInfoID] = new VerdanaBold14();
            } else {
                throw new ArgumentOutOfRangeException("fontInfoID");
            }
            return ((FontDefinition)_fontInfoTable[fontInfoID]).GetFontInfo();
        }
        private void GetDrawIcon16(BasicTypeDeSerializerContext context) {
            ushort x = 0;
            ushort y = 0;
            ushort color = 0;
            ushort[] icon = null;
            x = BasicTypeDeSerializer.Get(context, x);
            y = BasicTypeDeSerializer.Get(context, y);
            color = BasicTypeDeSerializer.Get(context, color);
            icon = BasicTypeDeSerializer.Get(context, icon);
            DrawIcon16(x, y, (BasicColor)color, icon);
        }
        private void GetDrawString(BasicTypeDeSerializerContext context) {
            ushort x = 0;
            ushort y = 0;
            ushort color = 0;
            ushort fontInfoID = 0;
            string text;
            x = BasicTypeDeSerializer.Get(context, x);
            y = BasicTypeDeSerializer.Get(context, y);
            color = BasicTypeDeSerializer.Get(context, color);
            fontInfoID = BasicTypeDeSerializer.Get(context, fontInfoID);
            text = BasicTypeDeSerializer.Get(context, "");
            DrawString(x, y, (BasicColor)color, FontInfoLookUp(fontInfoID), text);
        }
        private void GetDrawCirclePoints(BasicTypeDeSerializerContext context) {
            ushort cx = 0;
            ushort cy = 0;
            ushort x = 0;
            ushort y = 0;
            ushort color = 0;
            cx = BasicTypeDeSerializer.Get(context, cx);
            cy = BasicTypeDeSerializer.Get(context, cy);
            x = BasicTypeDeSerializer.Get(context, x);
            y = BasicTypeDeSerializer.Get(context, y);
            color = BasicTypeDeSerializer.Get(context, color);
            DrawCirclePoints(cx, cy, x, y, (BasicColor)color);
        }
        private void GetSetOrientation(BasicTypeDeSerializerContext context) {
            ushort orientation = 0;
            orientation = BasicTypeDeSerializer.Get(context, orientation);
            SetOrientation((LCD.Orientation)orientation);
        }
        private LCD _display; 
        private Hashtable _fontInfoTable = new Hashtable();
    }
}
