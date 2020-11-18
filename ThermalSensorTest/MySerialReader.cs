using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace ThermalSensorTest
{
    public class MySerialReader : IDisposable
    {
        private SerialPort serialPort;
        private Queue<byte> recievedData = new Queue<byte>();

        public MySerialReader(string portName, int baudRate, bool writeData, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            try
            {
                serialPort = new SerialPort();
                serialPort.PortName = SetPortName(portName);
                serialPort.BaudRate = SetPortBaudRate(baudRate);
                serialPort.Parity = parity;
                serialPort.ReadTimeout = Timeout.Infinite;
                serialPort.DataBits = dataBits;
                serialPort.StopBits = stopBits;
                serialPort.Handshake = handshake;
                serialPort.Open();
                Console.WriteLine($"Port {serialPort.PortName} opened");
                Console.WriteLine($"Port settings: \r\n" +
                    $"BaudRate:{serialPort.BaudRate} \r\n" +
                    $"Parity:{serialPort.Parity} \r\n" +
                    $"DataBits: {serialPort.DataBits} \r\n" +
                    $"StopBits: {serialPort.StopBits} \r\n" +
                    $"Handshake: {serialPort.Handshake} \r\n" +
                    $"Waiting data...");
                serialPort.DataReceived += serialPort_DataReceived;
                if (writeData)
                {
                    serialPort.WriteLine("Hello!");
                }

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                serialPort.Close();
                serialPort.Dispose();
            }


            //var dis = Observable
            //    .FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventHandler>(x => serialPort.DataReceived += x, x => serialPort.DataReceived -= x)
            //    .Subscribe(x=> {
            //        byte[] data = new byte[serialPort.BytesToRead];
            //        serialPort.Read(data, 0, data.Length);
            //        var msg = ByteArrayToString(data);
            //        Console.WriteLine(msg);
            //        data.ToList().ForEach(b => recievedData.Enqueue(b));
            //        processData();
            //    });
        }

        void serialPort_DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);
            var str = System.Text.Encoding.Default.GetString(data);
            var msg = ByteArrayToString(data);
            Console.WriteLine(str);
            Console.WriteLine(msg);
            data.ToList().ForEach(b => recievedData.Enqueue(b));
            processData();

        }

        void processData()
        {
            // Determine if we have a "packet" in the queue
            if (recievedData.Count > 50)
            {
                var packet = Enumerable.Range(0, 50).Select(i => recievedData.Dequeue());
            }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public void Dispose()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
        }
        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }
    }
}
