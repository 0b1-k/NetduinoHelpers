using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using netduino.helpers.Imaging;

namespace netduino.helpers.Hardware {
    // Based on MicroBuilder's code: http://www.microbuilder.eu/Projects/LPC1343ReferenceDesign/TFTLCDAPI.aspx
    public class AdaFruitILI932x : LCD
    {
        public enum Register {
            DRIVERCODEREAD                  = 0x00,
            DRIVEROUTPUTCONTROL1		    = 0x01,
            LCDDRIVINGCONTROL		        = 0x02,
            ENTRYMODE			            = 0x03,
            RESIZECONTROL			        = 0x04,
            DISPLAYCONTROL1			        = 0x07,
            DISPLAYCONTROL2			        = 0x08,
            DISPLAYCONTROL3			        = 0x09,
            DISPLAYCONTROL4			        = 0x0A,
            RGBDISPLAYINTERFACECONTROL1	    = 0x0C,
            FRAMEMARKERPOSITION		        = 0x0D,
            RGBDISPLAYINTERFACECONTROL2	    = 0x0F,
            POWERCONTROL1			        = 0x10,
            POWERCONTROL2			        = 0x11,
            POWERCONTROL3			        = 0x12,
            POWERCONTROL4			        = 0x13,
            HORIZONTALGRAMADDRESSSET		= 0x20,
            VERTICALGRAMADDRESSSET			= 0x21,
            WRITEDATATOGRAM				    = 0x22,
            POWERCONTROL7			        = 0x29,
            FRAMERATEANDCOLORCONTROL	    = 0x2B,
            GAMMACONTROL1			        = 0x30,
            GAMMACONTROL2			        = 0x31,
            GAMMACONTROL3			        = 0x32,
            GAMMACONTROL4			        = 0x35,
            GAMMACONTROL5			        = 0x36,
            GAMMACONTROL6			        = 0x37,
            GAMMACONTROL7			        = 0x38,
            GAMMACONTROL8			        = 0x39,
            GAMMACONTROL9			        = 0x3C,
            GAMMACONTROL10		            = 0x3D,
            HORIZONTALADDRESSSTARTPOSITION	= 0x50,
            HORIZONTALADDRESSENDPOSITION	= 0x51,
            VERTICALADDRESSSTARTPOSITION	= 0x52,
            VERTICALADDRESSENDPOSITION		= 0x53,
            DRIVEROUTPUTCONTROL2		    = 0x60,
            BASEIMAGEDISPLAYCONTROL		    = 0x61,
            VERTICALSCROLLCONTROL		    = 0x6A,
            PARTIALIMAGE1DISPLAYPOSITION	= 0x80,
            PARTIALIMAGE1AREASTARTLINE	    = 0x81,
            PARTIALIMAGE1AREAENDLINE	    = 0x82,
            PARTIALIMAGE2DISPLAYPOSITION	= 0x83,
            PARTIALIMAGE2AREASTARTLINE	    = 0x84,
            PARTIALIMAGE2AREAENDLINE	    = 0x85,
            PANELINTERFACECONTROL1		    = 0x90,
            PANELINTERFACECONTROL2		    = 0x92,
            PANELINTERFACECONTROL3		    = 0x93,
            PANELINTERFACECONTROL4		    = 0x95,
            PANELINTERFACECONTROL5		    = 0x97,
            PANELINTERFACECONTROL6		    = 0x98,
            OTPVCMPROGRAMMINGCONTROL        = 0x00A1,
            OTPVCMSTATUSANDENABLE           = 0x00A2,
            OTPPROGRAMMINGIDKEY             = 0x00A5
        }

        private OutputPort _chipSelect;
        private OutputPort _commandData;
        private OutputPort _write;
        //private OutputPort _read;
        private OutputPort _reset;
        private ShiftRegister74HC595 _parallelDataOut;
        private LcdProperties _lcdProperties;
        private PWM _backLight;

        public Orientation Orientation { get; set; }

        public override ushort Width { 
            get{
                switch (Orientation) {
                    case Orientation.Portrait:
                        return _lcdProperties.Width;
                    case Orientation.Landscape:
                    default:
                        return _lcdProperties.Height;
                }
            }
            set{
                _lcdProperties.Width = value;
            }
        }
        public override ushort Height {
            get {
                switch (Orientation) {
                    case Orientation.Portrait:
                        return _lcdProperties.Height;
                    case Orientation.Landscape:
                    default:
                        return _lcdProperties.Width;
                }
            }
            set {
                _lcdProperties.Height = value;
            }
        }
        public AdaFruitILI932x(
            ShiftRegister74HC595 shiftRegister,
            Cpu.Pin tftChipSelect,
            Cpu.Pin tftCommandData,
            Cpu.Pin tftWrite,
            Cpu.Pin tftRead,
            Cpu.Pin tftReset,
            Cpu.Pin tftBackLight = Cpu.Pin.GPIO_NONE) {
                _parallelDataOut = shiftRegister;
                _chipSelect = new OutputPort(tftChipSelect, true);
                _commandData = new OutputPort(tftCommandData, true);
                //_read = new OutputPort(tftRead, false);
                _write = new OutputPort(tftWrite, true);
                _reset = new OutputPort(tftReset, true);
                Orientation = LCD.Orientation.Portrait;
                _lcdProperties = new LcdProperties(
                    width: 240, height: 320,
                    supportTouch: true, supportHardwareScrolling: true, supportOrientation: true);
                if (tftBackLight != Cpu.Pin.GPIO_NONE) {
                    _backLight = new PWM(tftBackLight);
                }
        }
        public override void Initialize() {
            Reset();
            ushort _delay = 0xFF;
            ushort[] _initializationSequence = new ushort[] {
            (ushort) Register.DRIVERCODEREAD, 0x0001,     // start oscillator
            _delay, 50,                           // this will make a delay of 50 milliseconds
            (ushort) Register.DRIVEROUTPUTCONTROL1, 0x0100, 
            (ushort) Register.LCDDRIVINGCONTROL, 0x0700,
            (ushort) Register.ENTRYMODE, 0x1030,
            (ushort) Register.RESIZECONTROL, 0x0000,
            (ushort) Register.DISPLAYCONTROL2, 0x0202,
            (ushort) Register.DISPLAYCONTROL3, 0x0000,
            (ushort) Register.DISPLAYCONTROL4, 0x0000,
            (ushort) Register.RGBDISPLAYINTERFACECONTROL1, 0x0,
            (ushort) Register.FRAMEMARKERPOSITION, 0x0,
            (ushort) Register.RGBDISPLAYINTERFACECONTROL2, 0x0,
            (ushort) Register.POWERCONTROL1, 0x0000,
            (ushort) Register.POWERCONTROL2, 0x0007,
            (ushort) Register.POWERCONTROL3, 0x0000,
            (ushort) Register.POWERCONTROL4, 0x0000,
            _delay, 200,
            (ushort) Register.POWERCONTROL1, 0x1690,
            (ushort) Register.POWERCONTROL2, 0x0227,
            _delay, 50,
            (ushort) Register.POWERCONTROL3, 0x001A,
            _delay, 50,
            (ushort) Register.POWERCONTROL4, 0x1800,
            (ushort) Register.POWERCONTROL7, 0x002A,
            _delay,50,
            (ushort) Register.GAMMACONTROL1, 0x0000,    
            (ushort) Register.GAMMACONTROL2, 0x0000, 
            (ushort) Register.GAMMACONTROL3, 0x0000,
            (ushort) Register.GAMMACONTROL4, 0x0206,   
            (ushort) Register.GAMMACONTROL5, 0x0808,  
            (ushort) Register.GAMMACONTROL6, 0x0007,  
            (ushort) Register.GAMMACONTROL7, 0x0201,
            (ushort) Register.GAMMACONTROL8, 0x0000,  
            (ushort) Register.GAMMACONTROL9, 0x0000,  
            (ushort) Register.GAMMACONTROL10, 0x0000,  
            (ushort) Register.HORIZONTALGRAMADDRESSSET, 0x0000,  
            (ushort) Register.VERTICALGRAMADDRESSSET, 0x0000,  
            (ushort) Register.HORIZONTALADDRESSSTARTPOSITION, 0x0000,
            (ushort) Register.HORIZONTALADDRESSENDPOSITION, 0x00EF,
            (ushort) Register.VERTICALADDRESSSTARTPOSITION, 0X0000,
            (ushort) Register.VERTICALADDRESSENDPOSITION, 0x013F,
            (ushort) Register.DRIVEROUTPUTCONTROL2, 0xA700,     // Driver Output Control (R60h)
            (ushort) Register.BASEIMAGEDISPLAYCONTROL, 0x0003,     // Driver Output Control (R61h)
            (ushort) Register.VERTICALSCROLLCONTROL, 0x0000,     // Driver Output Control (R62h)
            (ushort) Register.PANELINTERFACECONTROL1, 0X0010,     // Panel Interface Control 1 (R90h)
            (ushort) Register.PANELINTERFACECONTROL2, 0X0000,
            (ushort) Register.PANELINTERFACECONTROL3, 0X0003,
            (ushort) Register.PANELINTERFACECONTROL4, 0X1100,
            (ushort) Register.PANELINTERFACECONTROL5, 0X0000,
            (ushort) Register.PANELINTERFACECONTROL6, 0X0000,
            (ushort) Register.DISPLAYCONTROL1, 0x0133     // Display Control (R07h) - Display ON
            };
            for (var i = 0; i < _initializationSequence.Length; i+=2) {
                var register = (ushort) _initializationSequence[i];
                var data = (ushort) _initializationSequence[i + 1];
                if (register == _delay) {
                    Thread.Sleep(data);
                    //Debug.Print("Delay: " + data);
                } else {
                    WriteRegister((Register)register, data);
                    //Debug.Print("Register: " + register + "(" + data + ")");
                }
            }
        }
        public override void Reset() {
          _reset.Write(false);
          Thread.Sleep(2);
          _reset.Write(true);
          // resync
          WriteData(0);
          WriteData(0);
          WriteData(0);
          WriteData(0);
        }
        public override void Test() {
            Home();
            for (var i = 0; i < 320; i++) {
                for (var j = 0; j < 240; j++) {
                    if (i > 279) WriteData((ushort)BasicColor.White);
                    else if (i > 239) WriteData((ushort)BasicColor.Blue);
                    else if (i > 199) WriteData((ushort)BasicColor.Green);
                    else if (i > 159) WriteData((ushort)BasicColor.Cyan);
                    else if (i > 119) WriteData((ushort)BasicColor.Red);
                    else if (i > 79) WriteData((ushort)BasicColor.Magenta);
                    else if (i > 39) WriteData((ushort)BasicColor.Yellow);
                    else WriteData((ushort)BasicColor.Black);
                }
            }
        }
        public override void GetPixel(ushort x, ushort y) {
            throw new NotSupportedException();
        }
        public override void FillRGB(ushort data) {
            Home();
            var pixels = Width * Height;
            for (var i=0; i < pixels; i++ ) {
                WriteData(data);
            }
        }
        public override void DrawPixel(ushort x, ushort y, ushort color) {
            SetCursor(x, y);
            WriteCommand(Register.WRITEDATATOGRAM);
            WriteData(color);
        }
        public override void DrawPixels(ushort x, ushort y, ushort[] data, int length = 0) {
            SetCursor(x, y);
            WriteCommand(Register.WRITEDATATOGRAM);
            if (length == 0) {
                length = data.Length;
            }
            for (var i = 0; i < length; i++) {
                WriteData(data[i]);
            }
        }
        public override void DrawHLine(ushort x0, ushort x1, ushort y, ushort color) {
            // Allows for slightly better performance than setting individual pixels
            ushort x, pixels;
            if (x1 < x0) { // Switch x1 and x0
                x = x1;
                x1 = x0;
                x0 = x;
            }
            // Check limits
            if (x1 >= Width) {
                x1 = (ushort) (Width - 1);
            }
            if (x0 >= Width) {
                x0 = (ushort) (Width - 1);
            }
            SetCursor(x0, y);
            WriteCommand(Register.WRITEDATATOGRAM);
            for (pixels = 0; pixels < x1 - x0 + 1; pixels++) {
                WriteData(color);
            }
        }
        public override void DrawVLine(ushort x, ushort y0, ushort y1, ushort color) {
            throw new NotSupportedException("DrawVLine");
            //Orientation oldOrientation = Orientation;
            //if (oldOrientation == Orientation.Portrait) {
            //    SetOrientation(Orientation.Landscape);
            //    DrawHLine(y0, y1, (ushort) (Height - (x + 1)), color);
            //} else {
            //    SetOrientation(Orientation.Portrait);
            //    DrawHLine((ushort) (Width - (y0 + 1)), (ushort) (Width - (y1 + 1)), x, color);
            //}
            //// Switch orientation back
            //SetOrientation(oldOrientation);
        }

        public const uint BackLightOnDutyCycle = 255;
        public const uint BackLightOffDutyCycle = 0;

        public override void BackLight(uint dutyCycle) {
            if (_backLight != null) {
                _backLight.SetDutyCycle(dutyCycle & 0xFF);
            }
        }
        public override void Scroll(ushort pixels, ushort fillColor) {
            var y = pixels;
            while (y < 0)
                y += 320;
            while (y >= 320)
                y -= 320;
            WriteRegister(Register.VERTICALSCROLLCONTROL, y);
        }
        public override void SetOrientation(Orientation orientation) {
            ushort entryMode = 0x1030;
            ushort outputControl = 0x0100;
            switch (orientation) {
                case Orientation.Portrait:
                    entryMode = 0x1030;
                    outputControl = 0x0100;
                    break;
                case Orientation.Landscape:
                    entryMode = 0x1028;
                    outputControl = 0x0000;
                    break;
            }
            WriteRegister(Register.ENTRYMODE, entryMode);
            WriteRegister(Register.DRIVEROUTPUTCONTROL1, outputControl);
            Orientation = orientation;
            SetCursor(0, 0);
        }
        public override ushort GetControllerID() {
            throw new NotSupportedException();
        }
        public override LcdProperties GetProperties() {
            return _lcdProperties;
        }
        // Protected methods specific to the LCD display
        protected override void Home() {
            SetCursor(0, 0);
            WriteCommand(Register.WRITEDATATOGRAM);
        }
        protected override void SetWindow(ushort x0, ushort y0, ushort x1, ushort y1) {
            WriteRegister(Register.HORIZONTALADDRESSSTARTPOSITION, x0);
            WriteRegister(Register.HORIZONTALADDRESSENDPOSITION, x1);
            WriteRegister(Register.VERTICALADDRESSSTARTPOSITION, y0);
            WriteRegister(Register.VERTICALADDRESSENDPOSITION, y1);
            SetCursor(x0, y0);
        }
        protected override void SetCursor(ushort x, ushort y) {
            if (Orientation == Orientation.Landscape) {
                WriteRegister(Register.HORIZONTALGRAMADDRESSSET, y);
                WriteRegister(Register.VERTICALGRAMADDRESSSET, x);
            } else {
                WriteRegister(Register.HORIZONTALGRAMADDRESSSET, x);
                WriteRegister(Register.VERTICALGRAMADDRESSSET, y);
            }
        }
        // Low-level display control commands
        protected void WriteRegister(Register address, ushort data) {
            WriteCommand((Register) address);
            WriteData(data);
        }
        protected void WriteCommand(Register command) {
            _chipSelect.Write(false);   
            _commandData.Write(false);  
            //_read.Write(true);          
            _write.Write(true);         
            _parallelDataOut.Write((byte)((uint)command >> 8));
            _write.Write(false); 
            _write.Write(true); 
            _parallelDataOut.Write((byte)(command));
            _write.Write(false); 
            _write.Write(true); 
            _chipSelect.Write(true);   
        }
        protected void WriteData(ushort data) {
            _chipSelect.Write(false);   
            _commandData.Write(true);  
            //_read.Write(true);          
            _write.Write(true);         
            _parallelDataOut.Write((byte)(data >> 8));
            _write.Write(false); 
            _write.Write(true); 
            _parallelDataOut.Write((byte)(data));
            _write.Write(false); 
            _write.Write(true); 
            _chipSelect.Write(true);   
        }
        protected void WriteDataUnsafe(ushort data) {
            _parallelDataOut.Write((byte)(data >> 8));
            _write.Write(false); 
            _write.Write(true); 
            _parallelDataOut.Write((byte)(data));
            _write.Write(false); 
            _write.Write(true); 
        }
    }
}
