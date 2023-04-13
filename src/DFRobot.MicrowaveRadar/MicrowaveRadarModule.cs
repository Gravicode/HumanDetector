using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DFRobot.MicrowaveRadar
{

    public class MicrowaveRadarModule
    {

        public delegate void HumanDetectedHandler(object sender, int HumanCount);
        public event HumanDetectedHandler HumanDetected;

        private UartController _ser;

        public bool HitOnlyWhenHumanIsPresent { get; set; } = false;
        public MicrowaveRadarModule(string Uart = SC13048.UartPort.Uart1)
        {
            var uart = UartController.FromName(Uart);

            var uartSetting = new UartSetting()
            {
                BaudRate = 115200,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None,
            };
            uart.SetActiveSettings(uartSetting);
            uart.Enable();

            rxBuffer = new byte[150];
            uart.DataReceived += uart_DataReceived;
            _ser = uart;
        }
        byte[] rxBuffer;
        string tempstr=string.Empty;
        void uart_DataReceived(UartController sender, DataReceivedEventArgs e)
        {

            var bytesReceived = _ser.Read(rxBuffer, 0, e.Count);
            var datastr = Encoding.UTF8.GetString(rxBuffer, 0, bytesReceived);
            
            
            if (datastr.IndexOf(Environment.NewLine) > -1)
            {
                var split = Regex.Split(Environment.NewLine, datastr, RegexOptions.IgnoreCase);
                ProcessMessage(tempstr + split[0]);
                if (split.Length > 1)
                    tempstr = split[1];
                else
                    tempstr = "";
                
                
            }
            else
            {
                tempstr += datastr;
            }
            
        }

        void ProcessMessage(string Data)
        {
            Debug.WriteLine(Data);
            var idx = Data.IndexOf("$JYBSS,");
            if (idx == 0)
            {
                Data = Strings.Replace(Data, "$JYBSS,", string.Empty);
                var split = Data.Split(',');
                
                int Count = int.Parse(Strings.Replace( split[0],Environment.NewLine,string.Empty));
                if(HitOnlyWhenHumanIsPresent && Count > 0)
                {
                    HumanDetected.Invoke(this, Count);
                }
                else
                {
                    HumanDetected.Invoke(this, Count);
                }
            }
            idx = Data.IndexOf("save cfg complete");
            if(idx>-1)
            {
                Debug.WriteLine("Config saved.");
            }
        }
        public bool begin()
        {
            if (_ser == null)
            {
                return false;
            }
            return true;
        }
        static long millis()
        {
            return DateTime.Now.Ticks;
        }

        byte[] temp = new byte[1];
        public int readN(ref byte[] buf, int len)
        {
            int offset = 0, left = len;
            int Tineout = 1500;
            byte[] buffer = buf;
            long curr = millis();
            while (left > 0)
            {
                if (_ser.BytesToRead > 0)
                {
                    //buffer[offset] = (byte)_ser.Read();
                    //offset++;
                    //left--;
                    var count = _ser.Read(temp);
                    buffer[offset] = temp[0];//(byte)_ser.Read();
                    offset++;
                    left--;
                }
                if (millis() - curr > Tineout)
                {
                    break;
                }
            }
            return offset;
        }

        public bool sendCommand(string COM)
        {
            long curr = millis();
            long curr1 = curr;
            var bytes = Encoding.UTF8.GetBytes(COM+Environment.NewLine);
            _ser.Write(bytes);
            while (true)
            {
                
                if (millis() - curr > 1000)
                {
                    Debug.WriteLine(COM);
                    Debug.WriteLine("Error");
                    return false;
                }
                if (millis() - curr1 > 300)
                {
                    _ser.Write(bytes);
                    curr1 = millis();
                }
                System.Threading.Thread.Sleep(100);
                if (readDone())
                {
                    Debug.WriteLine(COM);
                    Debug.WriteLine("Done");
                    return true;
                }
            }
        }

        public bool readDone()
        {
            int len = _ser.BytesToRead;
            if (len > 0)
            {
                byte[] Data = new byte[len];
                _ser.Read(Data, 0, len);
                //Debug.WriteLine(Data);
                if (Encoding.UTF8.GetString(Data).IndexOf("Done")>-1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
       
        public void detRangeCfg(int distance)
        {
            var comDetRangeCfgStr =  $"setRange 0 {distance}";
           

            sendCommand("sensorStop");                                  //Stop detection
            sendCommand(comDetRangeCfgStr);                                //Configuration distance
            sendCommand("saveConfig");                                  //Save configuration
            sendCommand("sensorStart");                                 //Start the module to start running.
        }

        public void setSensitivity(int sensitivity)
        {
           
            var comSetSensitivity = $"setSensitivity {sensitivity}";
            sendCommand(comSetSensitivity);
        }
        /*
        public bool readWaveData(int[] data, int len)
        {
            bool ret = false;
            int i = 0;
            char[] buf = new char[16];
            while (i < len)
            {
                if (recdData(buf))
                {
                    DBG(buf);
                    if (buf[7] == 'W')
                    {
                        data[i++] = int.Parse(buf + 8);
                    }
                }
            }
            return true;
        }
        */
        void DBG(char[] data)
        {
            Debug.WriteLine(data.ToString());
        }
        /*
        public bool readWaveData(int[] data, int len, int timeout)
        {
            bool ret = false;
            int i = 0;
            char[] buf = new char[16];
            long curr = millis();
            while (i < len)
            {
                if (recdData(buf))
                {
                    DBG(buf);
                    if (buf[7] == 'W')
                    {
                        data[i++] = atoi(buf + 8);
                    }
                }
                if (millis() - curr > timeout)
                {
                    break;
                }
            }
            return true;
        }
        */
    }
}
