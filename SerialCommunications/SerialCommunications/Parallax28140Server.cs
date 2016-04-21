// ***************************************************************************************
//  filename    :   Parallax28140Server.cs
//  purpose     :   provide a concrete SerialPortServer for the Parallax28140 RFID Reader
//                  which runs through the USB serial (COM3) at 2400 baudrate.  Server
//                  will scan incoming bytes and process them into 10 byte chunks
//                  representing scanned RFID IDs using the START and STOP bytes as guides
//
//  written by Jonathan Melcher and Brennan MacGregor on 2016-03-30
// ***************************************************************************************


using System;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;


namespace SerialCommunications
{
    public sealed class Parallax28140Server : SerialPortServer
    {
        private const int DEFAULT_DELAY_MS = 10;
        private RFIDScanner Scanner { get; set; }

        public string CurrentID { get; private set; }

        internal Parallax28140Server(SerialPort port) : base(port) { }

        public event Action<string> OnIDScan;

        public override void StartServer()
        {
            StopServer();
            Scanner = new RFIDScanner();
            Scanner.OnIDScan += Scanner_OnIDScan;
            Port.Open();
            _isRunning = true;
            Reader = new Task(ReaderProcess);
        }

        public override void StopServer()
        {
            _isRunning = false;
            Reader?.Wait();
            Port.Close();
        }

        public void ClearScan()
        {
            CurrentID = string.Empty;
        }

        private void Scanner_OnIDScan(string obj)
        {
            if (string.IsNullOrEmpty(CurrentID))
                return;
            CurrentID = obj;
            OnIDScan(CurrentID);
        }

        private void ReaderProcess()
        {
            while (_isRunning)
            {
                try
                {
                    byte read = (byte)Port.ReadChar();
                    Scanner.Read(read);
                }
                catch (TimeoutException) { }

                Thread.Sleep(DEFAULT_DELAY_MS);
            }
        }
    }
}