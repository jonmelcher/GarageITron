﻿// ***********************************************************************************
//  filename    :   MediatorProcessingState.cs
//  purpose     :   the third state for GarageMediator
//                  herein the GarageMediator is waiting to process a vehicle. all
//                  functionality should be available, where Process starts the comms
//                  between the Mediator and the Microcontroller to process a vehicle.
//                  an event is available to allow the GarageMediator to know when the
//                  vehicle has finished processing.  this allows the GarageMediator
//                  to process another vehicle or move back ito the listening state
//
//  written by Jonathan Melcher and Brennan MacGregor on 2016-04-07
// ***********************************************************************************


using System;
using System.Threading.Tasks;
using System.Threading;
using GarageModel;


namespace GarageMediator
{
    internal sealed class MediatorProcessingState : MediatorState
    {
        // communication protocol to the microcontroller
        private const byte START_INSTRUCTION = 0xF0;
        private const byte CONTINUE_INSTRUCTION = 0x01;
        private const byte STOP_INSTRUCTION = 0xF1;
        private const byte INSTRUCTIONS_COMPLETED = 0x03;
        private const byte COMMUNICATION_DELAY_MS = 250;
        private const byte OUTGOING = 0x20;
        private const byte INCOMING = 0x21;

        private Task _instructor;

        // requires GarageMediator context to be constructed so is initialized in constructor
        private Action<GarageAssignment> _Process;

        // event to propogate upwards when a vehicle has finished being processed by the microcontroller

        public static event Action VehicleInstructionsStarted;
        public static event Action VehicleProcessingStarted;
        public static event Action VehicleProcessed;

        public MediatorProcessingState(GarageMediator context)
        {
            _Process = GetProcess(context);
        }
        
        private Action<GarageAssignment> GetProcess(GarageMediator context)
        {
            return (assignment) =>
            {
                context.MicroCommunication.Write(START_INSTRUCTION);
                VehicleInstructionsStarted();

                byte cont = 0;
                while (cont != CONTINUE_INSTRUCTION)
                {
                    cont = context.MicroCommunication.Read();
                    System.Diagnostics.Debug.WriteLine(cont);
                    Thread.Sleep(0);
                }

                //while (context.MicroCommunication.Read() != CONTINUE_INSTRUCTION)
                //    Thread.Sleep(0);
                context.MicroCommunication.Write(assignment.Cell);
                while (context.MicroCommunication.Read() != CONTINUE_INSTRUCTION)
                    Thread.Sleep(0);
                context.MicroCommunication.Write(assignment.Stored ? OUTGOING : INCOMING);
                while (context.MicroCommunication.Read() != CONTINUE_INSTRUCTION)
                    Thread.Sleep(0);
                context.MicroCommunication.Write(STOP_INSTRUCTION);
                VehicleProcessingStarted();
                while (context.MicroCommunication.Read() != INSTRUCTIONS_COMPLETED)
                    Thread.Sleep(0);
                context.DatabaseCommunication.MoveVehicle(assignment.ID, !assignment.Stored);
                VehicleProcessed();
            };
        }

        public override void ProcessVehicle(GarageAssignment assignment)
        {
            _instructor = Task.Run(() => _Process(assignment));
        }

        public override void Kill(GarageMediator context)
        {
            base.Kill(context);
        }

        public override void Change(GarageMediator context)
        {
            context.State = new MediatorListeningState(context);
        }
    }
}