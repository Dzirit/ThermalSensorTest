﻿using System;
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

        public MySerialReader(string portName, int baudRate, bool writeData, Parity parity, int DataBits, int stopBits, int handshake)
        {
            serialPort = new SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.Parity = Parity.None;
            serialPort.ReadTimeout = Timeout.Infinite;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.Two;
            serialPort.Handshake = Handshake.RequestToSendXOnXOff;
            serialPort.Open();
            serialPort.DataReceived += serialPort_DataReceived;
            if (writeData)
            {
                serialPort.WriteLine("Hello!");
            }
            
            Console.ReadLine();

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
    }
}