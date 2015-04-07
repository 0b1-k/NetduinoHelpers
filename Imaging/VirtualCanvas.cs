using System;
using System.Collections;
using netduino.helpers.Helpers;
using netduino.helpers.Hardware;
using netduino.helpers.Fonts;
namespace netduino.helpers.Imaging {
    public class VirtualCanvas : Canvas {
        protected BasicTypeSerializerContext Context;
        public VirtualCanvas() {
        }
        public VirtualCanvas(BasicTypeSerializerContext context) {
            Context = context;
        }
        override public void DrawTestPattern() {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawTestPattern);
        }
        override public void DrawPixel(int x, int y, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawPixel);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawFill(BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawFill);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawLine(int x0, int y0, int x1, int y1, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawLine);
            BasicTypeSerializer.Put(Context,(ushort)x0);
            BasicTypeSerializer.Put(Context,(ushort)y0);
            BasicTypeSerializer.Put(Context,(ushort)x1);
            BasicTypeSerializer.Put(Context,(ushort)y1);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawLineDotted(int x0, int y0, int x1, int y1, int empty, int solid, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawLineDotted);
            BasicTypeSerializer.Put(Context,(ushort)x0);
            BasicTypeSerializer.Put(Context,(ushort)y0);
            BasicTypeSerializer.Put(Context,(ushort)x1);
            BasicTypeSerializer.Put(Context,(ushort)y1);
            BasicTypeSerializer.Put(Context,(ushort)empty);
            BasicTypeSerializer.Put(Context,(ushort)solid);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawCircle(int xCenter, int yCenter, int radius, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawCircle);
            BasicTypeSerializer.Put(Context,(ushort)xCenter);
            BasicTypeSerializer.Put(Context,(ushort)yCenter);
            BasicTypeSerializer.Put(Context,(ushort)radius);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawCircleFilled(int xCenter, int yCenter, int radius, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawCircleFilled);
            BasicTypeSerializer.Put(Context,(ushort)xCenter);
            BasicTypeSerializer.Put(Context,(ushort)yCenter);
            BasicTypeSerializer.Put(Context,(ushort)radius);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawCornerFilled(int xCenter, int yCenter, int radius, CornerPosition position, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawCornerFilled);
            BasicTypeSerializer.Put(Context,(ushort)xCenter);
            BasicTypeSerializer.Put(Context,(ushort)yCenter);
            BasicTypeSerializer.Put(Context,(ushort)radius);
            BasicTypeSerializer.Put(Context,(ushort)position);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawArrow(int x, int y, int size, DrawingDirection direction, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawArrow);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,(ushort)size);
            BasicTypeSerializer.Put(Context,(ushort)direction);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawRectangle(int x0, int y0, int x1, int y1, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawRectangle);
            BasicTypeSerializer.Put(Context,(ushort)x0);
            BasicTypeSerializer.Put(Context,(ushort)y0);
            BasicTypeSerializer.Put(Context,(ushort)x1);
            BasicTypeSerializer.Put(Context,(ushort)y1);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawRectangleFilled(int x0, int y0, int x1, int y1, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawRectangleFilled);
            BasicTypeSerializer.Put(Context,(ushort)x0);
            BasicTypeSerializer.Put(Context,(ushort)y0);
            BasicTypeSerializer.Put(Context,(ushort)x1);
            BasicTypeSerializer.Put(Context,(ushort)y1);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawRectangleRounded(int x0, int y0, int x1, int y1, BasicColor color, int radius, RoundedCornerStyle corners) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawRectangleRounded);
            BasicTypeSerializer.Put(Context,(ushort)x0);
            BasicTypeSerializer.Put(Context,(ushort)y0);
            BasicTypeSerializer.Put(Context,(ushort)x1);
            BasicTypeSerializer.Put(Context,(ushort)y1);
            BasicTypeSerializer.Put(Context,(ushort)color);
            BasicTypeSerializer.Put(Context,(ushort)radius);
            BasicTypeSerializer.Put(Context,(ushort)corners);
        }
        override public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawTriangle);
            BasicTypeSerializer.Put(Context,(ushort)x0);
            BasicTypeSerializer.Put(Context,(ushort)y0);
            BasicTypeSerializer.Put(Context,(ushort)x1);
            BasicTypeSerializer.Put(Context,(ushort)y1);
            BasicTypeSerializer.Put(Context,(ushort)x2);
            BasicTypeSerializer.Put(Context,(ushort)y2);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawTriangleFilled(int x0, int y0, int x1, int y1, int x2, int y2, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawTriangleFilled);
            BasicTypeSerializer.Put(Context,(ushort)x0);
            BasicTypeSerializer.Put(Context,(ushort)y0);
            BasicTypeSerializer.Put(Context,(ushort)x1);
            BasicTypeSerializer.Put(Context,(ushort)y1);
            BasicTypeSerializer.Put(Context,(ushort)x2);
            BasicTypeSerializer.Put(Context,(ushort)y2);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void DrawProgressBar(
            int x, int y,
            int width, int height,
            RoundedCornerStyle borderCorners,
            RoundedCornerStyle progressCorners,
            BasicColor borderColor, BasicColor borderFillColor,
            BasicColor progressBorderColor, BasicColor progressFillColor,
            int progress) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawProgressBar);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,(ushort)width);
            BasicTypeSerializer.Put(Context,(ushort)height);
            BasicTypeSerializer.Put(Context,(ushort)borderCorners);
            BasicTypeSerializer.Put(Context,(ushort)progressCorners);
            BasicTypeSerializer.Put(Context,(ushort)borderColor);
            BasicTypeSerializer.Put(Context,(ushort)borderFillColor);
            BasicTypeSerializer.Put(Context,(ushort)progressBorderColor);
            BasicTypeSerializer.Put(Context,(ushort)progressFillColor);
            BasicTypeSerializer.Put(Context,(ushort)progress);
        }
        override public void DrawButton(
            int x, int y,
            int width, int height,
            FontInfo fontInfo,
            int fontHeight,
            BasicColor borderColor,
            BasicColor fillColor,
            BasicColor fontColor,
            string text,
            Canvas.RoundedCornerStyle cornerStyle = RoundedCornerStyle.All) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawButton);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,(ushort)width);
            BasicTypeSerializer.Put(Context,(ushort)height);
            BasicTypeSerializer.Put(Context,fontInfo.ID);
            BasicTypeSerializer.Put(Context,(ushort)fontHeight);
            BasicTypeSerializer.Put(Context,(ushort)borderColor);
            BasicTypeSerializer.Put(Context,(ushort)fillColor);
            BasicTypeSerializer.Put(Context,(ushort)fontColor);
            BasicTypeSerializer.Put(Context,text, true);
            BasicTypeSerializer.Put(Context,(ushort)cornerStyle);
        }
        override public void DrawIcon16(int x, int y, BasicColor color, ushort[] icon) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawIcon16);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,(ushort)color);
            BasicTypeSerializer.Put(Context,icon);
        }
        override public void DrawString(int x, int y, BasicColor color, FontInfo fontInfo, string text) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawString);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,(ushort)color);
            BasicTypeSerializer.Put(Context,fontInfo.ID);
            BasicTypeSerializer.Put(Context,text, true);
        }
        override public void DrawBitmapImage(int x, int y, string filename) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawBitmapImage);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,filename);
        }
        override protected void DrawCirclePoints(int cx, int cy, int x, int y, BasicColor color) {
            BasicTypeSerializer.Put(Context,(byte)Command.DrawCirclePoints);
            BasicTypeSerializer.Put(Context,(ushort)cx);
            BasicTypeSerializer.Put(Context,(ushort)cy);
            BasicTypeSerializer.Put(Context,(ushort)x);
            BasicTypeSerializer.Put(Context,(ushort)y);
            BasicTypeSerializer.Put(Context,(ushort)color);
        }
        override public void SetOrientation(LCD.Orientation orientation) {
            BasicTypeSerializer.Put(Context,(byte)Command.SetOrientation);
            BasicTypeSerializer.Put(Context,(ushort)orientation);
        }
    }
}
