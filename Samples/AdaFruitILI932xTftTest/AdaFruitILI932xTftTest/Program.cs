//#define DRAWTOFILE

using System;
using System.IO;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using SecretLabs.NETMF.IO;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

using netduino.helpers.Hardware;
using netduino.helpers.Fonts;
using netduino.helpers.Imaging;
using netduino.helpers.Helpers;

namespace AdaFruitILI932xTftTest {
    public class Program {
        public static ShiftRegister74HC595 shiftRegister = new ShiftRegister74HC595(Pins.GPIO_PIN_D9);
        public static AdaFruitILI932x tft = new AdaFruitILI932x(
                                    shiftRegister,
                                    tftChipSelect: Pins.GPIO_PIN_D8,
                                    tftCommandData: Pins.GPIO_PIN_D7,
                                    tftWrite: Pins.GPIO_PIN_D6,
                                    tftRead: Pins.GPIO_NONE,
                                    tftReset: Pins.GPIO_PIN_D5);

        // Color scheme
        public static BasicColor ColorBackground = BasicColor.White;
        public static BasicColor ColorText = (BasicColor)BasicColor.White;
        public static BasicColor ColorButton = (BasicColor)GrayScaleValues.Gray_50;
        public static BasicColor ColorButtonBorder = (BasicColor)GrayScaleValues.Gray_50;
        public static BasicColor ColorButtonText = (BasicColor)BasicColor.White;
        public static BasicColor ColorButtonActive = (BasicColor)DefaultColorTheme.Base;
        public static BasicColor ColorButtonActiveBorder = (BasicColor)DefaultColorTheme.Darker;
        public static BasicColor ColorButtonActiveText = (BasicColor)BasicColor.Black;
        public static BasicColor ColorMenu = (BasicColor)GrayScaleValues.Gray_30;
        public static BasicColor ColorMenuLighter = (BasicColor)GrayScaleValues.Gray_50;
        public static BasicColor ColorMenuText = (BasicColor)BasicColor.White;
        public static BasicColor ColorMenuActive = (BasicColor)DefaultColorTheme.Darker;
        public static BasicColor ColorMenuActiveLighter = (BasicColor)DefaultColorTheme.Base;
        public static BasicColor ColorMenuActiveText = (BasicColor)BasicColor.Black;

        public static void Main() {
            tft.Initialize();

#if DRAWTOFILE
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            var file = new FileStream(@"SD\VirtualCanvas.bin", FileMode.Create);
            var context = new BasicTypeSerializerContext(file);
#else
            var context = new BasicTypeSerializerContext();
#endif
            var virtualCanvas = new VirtualCanvas(context);
            var fontDejaVuSansBold9 = new DejaVuSansBold9();
            var fontDejaVuSans9 = new DejaVuSans9();
            var fontDejaVuSansMono8 = new DejaVuSansMono8();

            virtualCanvas.DrawFill(ColorBackground);

            virtualCanvas.DrawString(5, 10, BasicColor.Black, fontDejaVuSansBold9.GetFontInfo(), "DejaVu Sans 9 Bold");
            virtualCanvas.DrawString(5, 30, BasicColor.Black, fontDejaVuSans9.GetFontInfo(), "DejaVu Sans 9");
            virtualCanvas.DrawString(5, 50, BasicColor.Black, fontDejaVuSansMono8.GetFontInfo(), "DejaVu Sans Mono 8");

            // Check if the screen orientation can be changed
            if (tft.GetProperties().Orientation == true) {
                // Change the orientation
                virtualCanvas.SetOrientation(LCD.Orientation.Landscape);
                // Render some text in the new orientation
                virtualCanvas.DrawString(5, 10, BasicColor.Black, new DejaVuSans9().GetFontInfo(), "DejaVu Sans 9 (Rotated)");
                // Change the orientation back
                virtualCanvas.SetOrientation(LCD.Orientation.Portrait);
            }

            RenderPrimitiveShapes(virtualCanvas);
            RenderCompoundShapes(virtualCanvas, fontDejaVuSans9.GetFontInfo());
            RenderIcons(virtualCanvas);

            var localCanvas = new Canvas(tft);

#if DRAWTOFILE
            file.Flush();
            file.Close();
            localCanvas.Replay(new BasicTypeDeSerializerContext(new FileStream(@"SD\VirtualCanvas.bin", FileMode.Open)));
            StorageDevice.Unmount("SD");
#else
            //localCanvas.Replay(new BasicTypeDeSerializerContext(context.GetBuffer()));

            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            var file = new FileStream(@"SD\VirtualCanvas.bin", FileMode.Create);
            
            int contentSize = 0;
            byte[] buffer = context.GetBuffer(ref contentSize);

            file.Write(buffer, 0, contentSize);
            file.Flush();
            file.Close();
            StorageDevice.Unmount("SD");
#endif
        }
        public static void RenderCompoundShapes(Canvas canvas, FontInfo fontInfo) {
            canvas.DrawProgressBar(
                70, 140, 
                75, 12,
                Canvas.RoundedCornerStyle.None,
                Canvas.RoundedCornerStyle.None, 
                BasicColor.Black,
                (BasicColor)GrayScaleValues.Gray_128,
                (BasicColor)GrayScaleValues.Gray_30,
                BasicColor.Green,
                78);
            canvas.DrawString(5, 144, BasicColor.Black, fontInfo, "Progress");
            canvas.DrawString(155, 144, BasicColor.Black, fontInfo, "78%");
            canvas.DrawRectangleFilled(0, 275, 239, 319, (BasicColor)GrayScaleValues.Gray_80);
            canvas.DrawButton(
                20, 285, 
                200, 25, 
                fontInfo, 
                7,
                BasicColor.Black,
                BasicColor.Green,
                BasicColor.Black,
                "Click For Text Entry"
                );
        }
        public static void RenderPrimitiveShapes(Canvas canvas) {
            canvas.DrawLine(5, 65, 200, 65, BasicColor.Red);
            canvas.DrawLine(5, 67, 200, 67, BasicColor.Green);
            canvas.DrawLine(5, 69, 200, 69, BasicColor.Blue);
            canvas.DrawCircleFilled(30, 105, 23, (BasicColor)canvas.GetRGB24toRGB565(0x33, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 19, (BasicColor)canvas.GetRGB24toRGB565(0x66, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 15, (BasicColor)canvas.GetRGB24toRGB565(0x99, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 11, (BasicColor)canvas.GetRGB24toRGB565(0xCC, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 7, (BasicColor)canvas.GetRGB24toRGB565(0xFF, 0x00, 0x00));
            canvas.DrawRectangleFilled(80, 80, 180, 125, (BasicColor)GrayScaleValues.Gray_15);
            canvas.DrawRectangleFilled(85, 85, 175, 120, (BasicColor)GrayScaleValues.Gray_30);
            canvas.DrawRectangleFilled(90, 90, 170, 115, (BasicColor)GrayScaleValues.Gray_50);
            canvas.DrawRectangleFilled(95, 95, 165, 110, (BasicColor)GrayScaleValues.Gray_80);
            canvas.DrawRectangleFilled(100, 100, 160, 105, (BasicColor)GrayScaleValues.Gray_128);
        }
        public static void RenderIcons(Canvas canvas) {
            var icons = new Icons16();

            // Cross/Failed
            canvas.DrawRectangleRounded(10, 190, 30, 210, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(12, 192, BasicColor.Red, icons.Failed);
            canvas.DrawRectangleRounded(10, 220, 30, 240, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(12, 222, BasicColor.Red, icons.Failed);
            canvas.DrawIcon16(12, 222, BasicColor.White, icons.FailedInterior);
            canvas.DrawRectangleRounded(10, 250, 30, 270, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(12, 252, BasicColor.White, icons.FailedInterior);

            // Alert
            canvas.DrawRectangleRounded(40, 190, 60, 210, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(42, 192, BasicColor.Yellow, icons.Alert);
            canvas.DrawRectangleRounded(40, 220, 60, 240, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(42, 222, BasicColor.Yellow, icons.Alert);
            canvas.DrawIcon16(42, 222, BasicColor.White, icons.AlertInterior);
            canvas.DrawRectangleRounded(40, 250, 60, 270, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(42, 252, BasicColor.White, icons.AlertInterior);
 
            // Checkmark/Passed
            canvas.DrawRectangleRounded(70, 190, 90, 210, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(72, 192, BasicColor.Green, icons.Passed);
            canvas.DrawRectangleRounded(70, 220, 90, 240, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(72, 222, BasicColor.Green, icons.Passed);
            canvas.DrawIcon16(72, 222, BasicColor.White, icons.PassedInterior);
            canvas.DrawRectangleRounded(70, 250, 90, 270, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(72, 252, BasicColor.White, icons.PassedInterior);
 
            // Info
            canvas.DrawRectangleRounded(100, 190, 120, 210, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(102, 192, BasicColor.Blue, icons.Info);
            canvas.DrawRectangleRounded(100, 220, 120, 240, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(102, 222, BasicColor.Blue, icons.Info);
            canvas.DrawIcon16(102, 222, BasicColor.White, icons.InfoInterior);
            canvas.DrawRectangleRounded(100, 250, 120, 270, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(102, 252, BasicColor.White, icons.InfoInterior);
 
            // Tools/Config
            canvas.DrawRectangleRounded(130, 190, 150, 210, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(132, 192, BasicColor.Green, icons.Tools);

            // Pointer
            canvas.DrawRectangleRounded(160, 190, 180, 210, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(162, 192, BasicColor.Magenta, icons.Pointer);
            canvas.DrawRectangleRounded(160, 220, 180, 240, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(162, 222, BasicColor.Magenta, icons.Pointer);
            canvas.DrawIcon16(162, 222, BasicColor.White, icons.PointerDot);
            canvas.DrawRectangleRounded(160, 250, 180, 270, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(162, 252, BasicColor.White, icons.PointerDot);
 
            // Tag
            canvas.DrawRectangleRounded(190, 190, 210, 210, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(192, 192, BasicColor.Cyan, icons.Tag);
            canvas.DrawRectangleRounded(190, 220, 210, 240, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(192, 222, BasicColor.Cyan, icons.Tag);
            canvas.DrawIcon16(192, 222, BasicColor.White, icons.TagDot);
            canvas.DrawRectangleRounded(190, 250, 210, 270, ColorButton, 5, Canvas.RoundedCornerStyle.None);
            canvas.DrawIcon16(192, 252, BasicColor.White, icons.TagDot);
        }
    }
}
