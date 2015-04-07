using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Nwazet.Go.Helpers;
using Nwazet.Go.Fonts;
using Nwazet.Go.Imaging;
using Nwazet.Go.Display.TouchScreen;

// Only needed when using this on a regular Netduino
//using Nwazet.Go.SD;
// Requires adding a reference to the SecretLabs.NETMF.IO assembly
//using SecretLabs.NETMF.IO;

namespace NetduinoPlusTouchDisplay {
    public class Program {
        // Color scheme
        public static readonly ushort ColorBackground = (ushort)BasicColor.White;
        public static readonly ushort ColorText = (ushort)BasicColor.White;
        public static readonly ushort ColorButton = (ushort)GrayScaleValues.Gray_50;
        public static readonly ushort ColorButtonBorder = (ushort)GrayScaleValues.Gray_50;
        public static readonly ushort ColorButtonText = (ushort)BasicColor.White;
        public static readonly ushort ColorButtonActive = (ushort)DefaultColorTheme.Base;
        public static readonly ushort ColorButtonActiveBorder = (ushort)DefaultColorTheme.Darker;
        public static readonly ushort ColorButtonActiveText = (ushort)BasicColor.Black;
        public static readonly ushort ColorMenu = (ushort)GrayScaleValues.Gray_30;
        public static readonly ushort ColorMenuLighter = (ushort)GrayScaleValues.Gray_50;
        public static readonly ushort ColorMenuText = (ushort)BasicColor.White;
        public static readonly ushort ColorMenuActive = (ushort)DefaultColorTheme.Darker;
        public static readonly ushort ColorMenuActiveLighter = (ushort)DefaultColorTheme.Base;
        public static readonly ushort ColorMenuActiveText = (ushort)BasicColor.Black;

        public static readonly RoundedCornerStyle CornerStyle = RoundedCornerStyle.All;

        // Pin connected to the transistor controlling the display's power supply
        public static OutputPort PowerTransistor = new OutputPort(Pins.GPIO_PIN_D7, true);

        public static void Main() {
            Debug.EnableGCMessages(true);
            Debug.Print("Available RAM: " + Debug.GC(true).ToString() + " bytes.");
            Debug.EnableGCMessages(false); 
            
            PowerUpDisplay();

            var canvas = new VirtualCanvas(TouchEventHandler, WidgetClickedHandler);
            canvas.Initialize(
                displaySpi: SPI.SPI_module.SPI1,
                displayChipSelect: Pins.GPIO_PIN_D9,
                displayGPIO: Pins.GPIO_PIN_D8,
                speedKHz: 5000);

            CalibrateTouchscreen(canvas);
            BmpImageTest(canvas);
            BasicUITest(canvas);
            MultiWidgetTest(canvas);
            BasicTouchEventTest(canvas);
            NonBlockingTouchEventTest(canvas);
            TouchscreenAlphanumericDialogTest(canvas);

            canvas.Reboot();
            Thread.Sleep(1000);

            canvas.Dispose();

        }
        public static void PowerUpDisplay() {
            // Ensure that the GPIO pin in low to prevent the display module to start in bootloader mode
            var goBusGPIO = new OutputPort(Pins.GPIO_PIN_D8, false);
            // Power up the display module
            PowerTransistor.Write(false);
            // Always wait 250ms after power-up to ensure that the display module is fully initialized before sending commands
            Thread.Sleep(250);
            goBusGPIO.Dispose();
        }
        public static void CalibrateTouchscreen(VirtualCanvas canvas) {
            try {
                //var sd = new SDCardReader();
                //sd.Initialize(SPI.SPI_module.SPI1,Pins.GPIO_PIN_D10);
                var calibrationDataFilename = @"SD\TouchscreenCalibration.bin";
                // If the touchscreen calibration data was previously retrieved from the display module and was stored to an SD card,
                // the calibration data can be sent to the display module instead of calling TouchscreenCalibration() before using
                // the touchscreen for the first time.
                if (File.Exists(calibrationDataFilename)) {
                    using (var calibrationDataFile = new FileStream(calibrationDataFilename, FileMode.Open)) {
                        var context = new BasicTypeDeSerializerContext(calibrationDataFile);
                        var matrix = new CalibrationMatrix();
                        matrix.Get(context);
                        canvas.SetTouchscreenCalibrationMatrix(matrix);
                    }
                } else {
                    // No pre-existing calibration data, create it...
                    using (var calibrationDataFile = new FileStream(calibrationDataFilename, FileMode.Create)) {
                        var matrix = canvas.GetTouchscreenCalibrationMatrix();
                        var context = new BasicTypeSerializerContext(calibrationDataFile);
                        matrix.Put(context);
                    }
                }
                //sd.Dispose();
            } catch (Exception) {
                Debug.Print("SD Card or file I/O error: manual calibration required.");
                canvas.TouchscreenCalibration();
            }
        }
        public static void BmpImageTest(VirtualCanvas canvas) {
            try {
                //var sd = new SDCardReader();
                //sd.Initialize(SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
                DisplayBmpPicture(canvas, @"Nwazet\03.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\05.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\09.bmp");
                canvas.SetOrientation(Orientation.Landscape);
                DisplayBmpPicture(canvas, @"Nwazet\00.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\01.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\02.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\04.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\06.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\07.bmp");
                DisplayBmpPicture(canvas, @"Nwazet\08.bmp");
                //sd.Dispose();
            } catch (Exception e) {
                Debug.Print(e.Message);
                Debug.Print("You need an SD card loaded with the demo photos to run this part of the demo.");
            }
        }
        private static void DisplayBmpPicture(VirtualCanvas canvas, string pictureName) {
            canvas.DrawBitmapImage(0, 0, @"SD\" + pictureName);
            canvas.TouchscreenWaitForEvent();
        }

        public static int lastTouchX;
        public static int lastTouchY;
        public static int lastTouchIsValid;

        public static void TouchEventHandler(VirtualCanvas canvas, TouchEvent touchEvent) {
            Debug.Print("------------TouchEventHandler------------");
            Debug.Print("X: " + touchEvent.X);
            Debug.Print("Y: " + touchEvent.Y);
            Debug.Print("Pressure: " + touchEvent.Pressure);

            lastTouchX = touchEvent.X;
            lastTouchY = touchEvent.Y;
            lastTouchIsValid = touchEvent.IsValid;
        }
        public static void WidgetClickedHandler(VirtualCanvas canvas, Widget widget, TouchEvent touchEvent) {
        }
        public static void NonBlockingTouchEventTest(VirtualCanvas canvas) {
            var message = "Touch To Continue";
            var fontInfo = new DejaVuSansBold9().GetFontInfo();
            var stringLength = fontInfo.GetStringWidth(message);
            canvas.DrawFill(ColorBackground);
            canvas.DrawString(
                (canvas.Width - stringLength) / 2, 150,
                (ushort)BasicColor.Black, fontInfo.ID, message);

            var random = new Random(lastTouchX * lastTouchY);
            lastTouchIsValid = 0;
            while (lastTouchIsValid == 0) {
                canvas.DrawCircleFilled(random.Next(canvas.Width), random.Next(canvas.Height), 4, (ushort)BasicColor.Red);
                canvas.Execute();
                canvas.TouchscreenWaitForEvent(TouchScreenEventMode.NonBlocking);
            }
        }
        public static void BasicTouchEventTest(VirtualCanvas canvas) {
            var message = "Touch Event Test";
            var fontInfo = new DejaVuSansBold9().GetFontInfo();
            var stringLength = fontInfo.GetStringWidth(message);
            canvas.SetOrientation(Orientation.Portrait);
            canvas.DrawFill(ColorBackground);
            canvas.DrawString(
                (canvas.Width - stringLength) / 2, 150,
                (ushort)BasicColor.Black, fontInfo.ID, message);

            canvas.TouchscreenWaitForEvent();

            canvas.DrawCircleFilled(lastTouchX, lastTouchY, 4, (ushort)BasicColor.Red);
            canvas.Execute();

            Thread.Sleep(1000);
        }
        public static void TouchscreenAlphanumericDialogTest(VirtualCanvas canvas) {
            canvas.SetOrientation(Orientation.Landscape);
            var response = canvas.TouchscreenShowDialog(DialogType.Alphanumeric);
            Debug.Print("User Input: " + response);
            canvas.SetOrientation(Orientation.Portrait);
            response = canvas.TouchscreenShowDialog(DialogType.Alphanumeric);
            Debug.Print("User Input: " + response);
        }
        public static void BasicUITest(VirtualCanvas canvas) {
            canvas.SetOrientation(Orientation.Portrait);
            canvas.DrawFill(ColorBackground);
            canvas.DrawString(5, 10, (ushort)BasicColor.Black, DejaVuSansBold9.ID, "DejaVu Sans 9 Bold");
            canvas.DrawString(5, 30, (ushort)BasicColor.Black, DejaVuSans9.ID, "DejaVu Sans 9");
            canvas.DrawString(5, 50, (ushort)BasicColor.Black, DejaVuSansMono8.ID, "DejaVu Sans Mono 8");
            canvas.SetOrientation(Orientation.Landscape);
            canvas.DrawString(5, 10, (ushort)BasicColor.Black, DejaVuSans9.ID, "DejaVu Sans 9 (Rotated)");
            canvas.SetOrientation(Orientation.Portrait);
            RenderPrimitiveShapes(canvas);

            var fontInfo = new DejaVuSans9().GetFontInfo();

            RenderCompoundShapes(canvas, fontInfo);
            RenderIcons(canvas);

            var button = new ButtonWidget(20, 285, 200, 25, fontInfo, "Continue Demo");
            canvas.RegisterWidget(button);
            canvas.RenderWidgets();
            while (!button.Clicked) {
                canvas.TouchscreenWaitForEvent();
            }
            button.Dirty = true;
            canvas.RenderWidgets();
            canvas.Execute();
            canvas.UnRegisterWidget(button);
        }
        public static void MultiWidgetTest(VirtualCanvas canvas) {
            canvas.SetOrientation(Orientation.Landscape);
            canvas.DrawFill(ColorHelpers.GetRGB24toRGB565(255, 255, 255));

            var fontInfo = new DejaVuSans9().GetFontInfo();

            var redButton = new ButtonWidget(10, 204, 44, 22, fontInfo, "Red");
            redButton.FillColor = ColorHelpers.GetRGB24toRGB565(255, 0, 0);
            redButton.FillColorClicked = ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            redButton.FontColorClicked = ColorHelpers.GetRGB24toRGB565(255, 0, 0);

            var greenButton = new ButtonWidget(60, 204, 44, 22, fontInfo, "Green");
            greenButton.FillColor = ColorHelpers.GetRGB24toRGB565(0, 255, 0);
            greenButton.FillColorClicked = ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            greenButton.FontColorClicked = ColorHelpers.GetRGB24toRGB565(0, 255, 0);

            var blueButton = new ButtonWidget(110, 204, 44, 22, fontInfo, "Blue");
            blueButton.FillColor = ColorHelpers.GetRGB24toRGB565(0, 0, 255);
            blueButton.FillColorClicked = ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            blueButton.FontColorClicked = ColorHelpers.GetRGB24toRGB565(0, 0, 255);

            var continueButton = new ButtonWidget(247, 204, 64, 22, fontInfo, "Continue");
            continueButton.FillColor = ColorHelpers.GetRGB24toRGB565(255, 255, 255);
            continueButton.FontColorClicked = ColorHelpers.GetRGB24toRGB565(0, 0, 0);

            canvas.RegisterWidget(redButton);
            canvas.RegisterWidget(greenButton);
            canvas.RegisterWidget(blueButton);
            canvas.RegisterWidget(continueButton);

            canvas.WidgetClicked += ColorButtonsClickedHandler;

            canvas.RenderWidgets();

            while (!continueButton.Clicked) {
                canvas.ActivateWidgets(true);
                canvas.RenderWidgets();
                canvas.Execute();

                canvas.TouchscreenWaitForEvent();

                canvas.RenderWidgets(Render.All);
                canvas.Execute();
            }

            canvas.WidgetClicked -= ColorButtonsClickedHandler;

            continueButton.Dirty = true;
            continueButton.Draw(canvas);
            canvas.Execute();

            canvas.UnRegisterAllWidgets();
        }
        public static void ColorButtonsClickedHandler(VirtualCanvas canvas, Widget widget, TouchEvent touchEvent) {
            widget.Dirty = true;
            canvas.DrawFill(((ButtonWidget)widget).FillColor);
        }
        public static void RenderCompoundShapes(VirtualCanvas canvas, FontInfo fontInfo) {
            canvas.DrawProgressBar(
                70, 140,
                75, 12,
                CornerStyle,
                CornerStyle,
                (ushort)BasicColor.Black,
                (ushort)GrayScaleValues.Gray_128,
                (ushort)GrayScaleValues.Gray_30,
                (ushort)BasicColor.Green,
                78);
            canvas.DrawString(5, 144, (ushort)BasicColor.Black, fontInfo.ID, "Progress");
            canvas.DrawString(155, 144, (ushort)BasicColor.Black, fontInfo.ID, "78%");
            canvas.DrawRectangleFilled(0, 275, 239, 319, (ushort)GrayScaleValues.Gray_80);
        }
        public static void RenderPrimitiveShapes(VirtualCanvas canvas) {
            canvas.DrawLine(5, 65, 200, 65, (ushort)BasicColor.Red);
            canvas.DrawLine(5, 67, 200, 67, (ushort)BasicColor.Green);
            canvas.DrawLine(5, 69, 200, 69, (ushort)BasicColor.Blue);
            canvas.DrawCircleFilled(30, 105, 23, (ushort)ColorHelpers.GetRGB24toRGB565(0x33, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 19, (ushort)ColorHelpers.GetRGB24toRGB565(0x66, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 15, (ushort)ColorHelpers.GetRGB24toRGB565(0x99, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 11, (ushort)ColorHelpers.GetRGB24toRGB565(0xCC, 0x00, 0x00));
            canvas.DrawCircleFilled(30, 105, 7, (ushort)ColorHelpers.GetRGB24toRGB565(0xFF, 0x00, 0x00));
            canvas.DrawRectangleFilled(80, 80, 180, 125, (ushort)GrayScaleValues.Gray_15);
            canvas.DrawRectangleFilled(85, 85, 175, 120, (ushort)GrayScaleValues.Gray_30);
            canvas.DrawRectangleFilled(90, 90, 170, 115, (ushort)GrayScaleValues.Gray_50);
            canvas.DrawRectangleFilled(95, 95, 165, 110, (ushort)GrayScaleValues.Gray_80);
            canvas.DrawRectangleFilled(100, 100, 160, 105, (ushort)GrayScaleValues.Gray_128);
        }
        public static void RenderIcons(VirtualCanvas canvas) {
            // Cross/Failed
            canvas.DrawRectangleRounded(10, 190, 30, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(12, 192, (ushort)BasicColor.Red, Icons16.Failed);
            canvas.DrawRectangleRounded(10, 220, 30, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(12, 222, (ushort)BasicColor.Red, Icons16.Failed);
            canvas.DrawIcon16(12, 222, (ushort)BasicColor.White, Icons16.FailedInterior);
            canvas.DrawRectangleRounded(10, 250, 30, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(12, 252, (ushort)BasicColor.White, Icons16.FailedInterior);

            // Alert
            canvas.DrawRectangleRounded(40, 190, 60, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(42, 192, (ushort)BasicColor.Yellow, Icons16.Alert);
            canvas.DrawRectangleRounded(40, 220, 60, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(42, 222, (ushort)BasicColor.Yellow, Icons16.Alert);
            canvas.DrawIcon16(42, 222, (ushort)BasicColor.White, Icons16.AlertInterior);
            canvas.DrawRectangleRounded(40, 250, 60, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(42, 252, (ushort)BasicColor.White, Icons16.AlertInterior);

            // Checkmark/Passed
            canvas.DrawRectangleRounded(70, 190, 90, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(72, 192, (ushort)BasicColor.Green, Icons16.Passed);
            canvas.DrawRectangleRounded(70, 220, 90, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(72, 222, (ushort)BasicColor.Green, Icons16.Passed);
            canvas.DrawIcon16(72, 222, (ushort)BasicColor.White, Icons16.PassedInterior);
            canvas.DrawRectangleRounded(70, 250, 90, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(72, 252, (ushort)BasicColor.White, Icons16.PassedInterior);

            // Info
            canvas.DrawRectangleRounded(100, 190, 120, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(102, 192, (ushort)BasicColor.Blue, Icons16.Info);
            canvas.DrawRectangleRounded(100, 220, 120, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(102, 222, (ushort)BasicColor.Blue, Icons16.Info);
            canvas.DrawIcon16(102, 222, (ushort)BasicColor.White, Icons16.InfoInterior);
            canvas.DrawRectangleRounded(100, 250, 120, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(102, 252, (ushort)BasicColor.White, Icons16.InfoInterior);

            // Tools/Config
            canvas.DrawRectangleRounded(130, 190, 150, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(132, 192, (ushort)BasicColor.Green, Icons16.Tools);

            // Pointer
            canvas.DrawRectangleRounded(160, 190, 180, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(162, 192, (ushort)BasicColor.Magenta, Icons16.Pointer);
            canvas.DrawRectangleRounded(160, 220, 180, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(162, 222, (ushort)BasicColor.Magenta, Icons16.Pointer);
            canvas.DrawIcon16(162, 222, (ushort)BasicColor.White, Icons16.PointerDot);
            canvas.DrawRectangleRounded(160, 250, 180, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(162, 252, (ushort)BasicColor.White, Icons16.PointerDot);

            // Tag
            canvas.DrawRectangleRounded(190, 190, 210, 210, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(192, 192, (ushort)BasicColor.Cyan, Icons16.Tag);
            canvas.DrawRectangleRounded(190, 220, 210, 240, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(192, 222, (ushort)BasicColor.Cyan, Icons16.Tag);
            canvas.DrawIcon16(192, 222, (ushort)BasicColor.White, Icons16.TagDot);
            canvas.DrawRectangleRounded(190, 250, 210, 270, ColorButton, 5, CornerStyle);
            canvas.DrawIcon16(192, 252, (ushort)BasicColor.White, Icons16.TagDot);
        }
    }
}
