using DFRobot.MicrowaveRadar;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Drivers.BasicGraphics;
using GHIElectronics.TinyCLR.Drivers.SolomonSystech.SSD1306;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace HumanRadar
{
    internal class Program
    {
        static UartController uart;
        static byte[] rxBuffer;
        static SSD1306Controller display;
        static BasicGraphics graphic;
        static void Main()
        {
            var settings = SSD1306Controller.GetConnectionSettings();
            var controller = I2cController.FromName(SC13048.I2cBus.I2c1);
            var device = controller.GetDevice(settings);
            display = new SSD1306Controller(device);
            graphic = new BasicGraphics(128, 64, ColorFormat.OneBpp);
           
            MicrowaveRadarModule sensor = new MicrowaveRadarModule(SC13048.UartPort.Uart1);
            sensor.HumanDetected += (a, b) => {
                Debug.WriteLine($"human detected: {b}");
                graphic.Clear();
                graphic.DrawString("--BMC Human Detector--", 1, 0, 0);
                graphic.DrawLine(1, 0, 10, 128, 10);
                graphic.DrawString($"Human Count: {b}", 1, 0, 20);
               

                display.DrawBufferNative(graphic.Buffer);
            };
            sensor.detRangeCfg(9);    //Set sensing distance, up to 9m 
            
            Thread.Sleep(-1);



        }
        
    }
}
