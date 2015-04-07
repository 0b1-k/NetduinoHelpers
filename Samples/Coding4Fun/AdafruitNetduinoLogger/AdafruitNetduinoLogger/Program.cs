using System;
using System.Collections;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using SecretLabs.NETMF.IO;
using Maxim.Clock;
using Maxim.Temperature;

namespace Adafruit.Netduino.Logger {
    // Netduino data logger application using the Adafruit data logging shield for Arduino: http://adafruit.com/products/243
    // Author: Fabien Royer | [Nwazet, LLC. | http://nwazet.com | See license.txt for terms of use.
    public class Program {
        public static readonly string SdMountPoint = "SD";
        public static readonly int TemperatureLoggerPeriod = 10 * 1000; // milliseconds
        public static OutputPort LedRed = new OutputPort(Pins.GPIO_PIN_D0, false);
        public static OutputPort LedGreen = new OutputPort(Pins.GPIO_PIN_D1, false);
        public static InputPort CardDetect = new InputPort(Pins.GPIO_PIN_D3, true, Port.ResistorMode.PullUp);
        public static ManualResetEvent ResetPeripherals = new ManualResetEvent(false);
        public static readonly Cpu.Pin ThermoCoupleChipSelect = Pins.GPIO_PIN_D2;
        public static Timer TemperatureSampler;
        public static DS1307 Clock;
        public static Max6675 ThermoCouple;
        public static ArrayList Buffer = new ArrayList();

        public static void Main() {
            while (true) {
                InitializePeripherals();
                ResetPeripherals.WaitOne();
                ResetPeripherals.Reset();
                DeInitializePeripherals();
            }
        }
        public static void DeInitializePeripherals() {
            LedGreen.Write(true);
            TemperatureSampler.Dispose();
            InitializeStorage(false);
            Clock.Dispose();
            ThermoCouple.Dispose();
            LedGreen.Write(false);
        }
        public static void InitializePeripherals() {
            LedGreen.Write(true);
            Clock = new DS1307();
            ThermoCouple = new Max6675();
            InitializeStorage(true);
            InitializeClock(new DateTime(2012, 06, 14, 17, 00, 00));
            ThermoCouple.Initialize(ThermoCoupleChipSelect);
            TemperatureSampler = new Timer(new TimerCallback(LogTemperature), null, 250, TemperatureLoggerPeriod);
            LedGreen.Write(false);
        }
        public static void LogTemperature(object obj) {
            LedRed.Write(true);
            var tickStart = Utility.GetMachineTime().Ticks;
            var now = Clock.Get();
            ThermoCouple.Read();
            var elapsedMs = (int)((Utility.GetMachineTime().Ticks - tickStart) / TimeSpan.TicksPerMillisecond);
            var date = AddZeroPrefix(now.Year) + "/" + AddZeroPrefix(now.Month) + "/" + AddZeroPrefix(now.Day);
            var time = AddZeroPrefix(now.Hour) + ":" + AddZeroPrefix(now.Minute) + ":" + AddZeroPrefix(now.Second) + ":" + AddZeroPrefix(elapsedMs);
            var celsius = Shorten(ThermoCouple.Celsius.ToString());
            var farenheit = Shorten(ThermoCouple.Farenheit.ToString());
            var latestRecord = date + "," + time + "," + celsius + "," + farenheit;
            Debug.Print(latestRecord);
            try {
                if (CardDetect.Read() == false) {
                    var filename = SdMountPoint + BuildTemperatureLogFilename(now);
                    if (File.Exists(filename) == false) {
                        using (var tempLogFile = new StreamWriter(filename, true)) {
                            tempLogFile.WriteLine("date,time,celsius,fahrenheit");
                        }
                    }
                    using (var tempLogFile = new StreamWriter(filename, true)) {
                        if (Buffer.Count != 0) {
                            foreach (var bufferedLine in Buffer) {
                                tempLogFile.WriteLine(bufferedLine);
                            }
                            Buffer.Clear();
                        }
                        tempLogFile.WriteLine(latestRecord);
                        tempLogFile.Flush();
                    }
                } else {
                    LogLine("No card in reader. Buffering record.");
                    Buffer.Add(latestRecord);
                }
                LedRed.Write(false);
            } catch (OutOfMemoryException e) {
                LogLine("Memory full. Clearing buffer.");
                Buffer.Clear();
            } catch (IOException e) {
                LogLine("IO error. Resetting peripherals.");
                Buffer.Add(latestRecord);
                ResetPeripherals.Set();
            }
        }
        public static string AddZeroPrefix(int value) {
            if (value < 10) {
                return "0" + value;
            }
            return value.ToString();
        }
        public static string Shorten(string str) {
            if (str.Length > 5) {
                return str.Substring(0, 5);
            }
            return str;
        }
        public static string BuildTemperatureLogFilename(DateTime dateTime) {
            var todayLog = @"\" + dateTime.Year.ToString() + "-" + dateTime.Month.ToString() + "-" + dateTime.Day.ToString() + ".csv";
            return todayLog;
        }
        public static void InitializeStorage(bool mount) {
            try {
                if (mount == true) {
                    StorageDevice.MountSD(SdMountPoint, SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
                } else {
                    StorageDevice.Unmount(SdMountPoint);
                }
            } catch (Exception e) {
                LogLine("InitializeStorage: " + e.Message);
                SignalCriticalError();
            }
        }
        public static void InitializeClock(DateTime dateTime) {
            var clockSetIndicator = SdMountPoint + @"\clockSet.txt";
            try {
                if (File.Exists(clockSetIndicator) == false) {
                    Clock.Set(dateTime);
                    Clock.Halt(false);
                    File.Create(clockSetIndicator);
                }
            } catch (Exception e) {
                LogLine("InitializeClock: " + e.Message);
                SignalCriticalError();
            }
        }
        public static void SignalCriticalError() {
            while (true) {
                LedRed.Write(true);
                LedGreen.Write(true);
                Thread.Sleep(100);
                LedRed.Write(false);
                LedGreen.Write(false);
                Thread.Sleep(100);
            }
        }
        public static void LogLine(string line) {
            Debug.Print(line);
        }
    }
}
