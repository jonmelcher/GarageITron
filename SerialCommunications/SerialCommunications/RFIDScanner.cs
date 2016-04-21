﻿// ********************************************************************
//  filename    :   RFIDTransmission.cs
//  purpose     :   encapsulates process of producing an RFID ID string
//                  from a stream of bytes
//
//  written by Jonathan Melcher and Brennan MacGregor on 2016-03-31
// ********************************************************************


using System;
using System.Linq;
using GenericContainers;


namespace SerialCommunications
{
    internal sealed class RFIDScanner
    {
        private const byte START_TRANSMISSION = 0x0A;
        private const byte STOP_TRANSMISSION = 0x0D;
        private const byte TRANSMISSION_LENGTH = 10;

        private ThreadSafeQueue<byte> Incoming { get; set; } = new ThreadSafeQueue<byte>();
        private RFIDTransmissionState State { get; set; } = RFIDTransmissionState.Waiting;

        internal event Action<string> OnIDScan;

        public void Read(byte read)
        {
            if (State == RFIDTransmissionState.Collecting)
                ProcessRead(read);
            else if (read == START_TRANSMISSION)
                State = RFIDTransmissionState.Collecting;
        }

        private void ProcessRead(byte read)
        {
            switch (read)
            {
                case START_TRANSMISSION:
                    Incoming.Clear();
                    break;
                case STOP_TRANSMISSION:
                    ProcessTransmission();
                    break;
                default:
                    Incoming.Enqueue(read);
                    break;
            }
        }

        private void ProcessTransmission()
        {
            var transmission = Incoming.EmptyIntoArray();
            if (IsValidTransmission(transmission))
                OnIDScan(GetTransmissionString(transmission));
            State = RFIDTransmissionState.Waiting;
        }

        private static string GetTransmissionString(byte[] id)
        {
            return new string(id.Select(b => (char)b).ToArray());
        }

        private static bool IsValidTransmission(byte[] id)
        {
            if (id == null || id.Length != TRANSMISSION_LENGTH)
                return false;
            return id.All(b => char.IsLetterOrDigit((char)b));
        }

        private enum RFIDTransmissionState { Waiting, Collecting }
    }
}