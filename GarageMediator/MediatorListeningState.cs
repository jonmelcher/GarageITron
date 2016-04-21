// **********************************************************************************
//  filename    :   MediatorListeningState.cs
//  purpose     :   the second state for GarageMediator
//                  the servers/repos have been started but need to have listeners
//                  added.  only Change/Kill functionality should be available.
//                  an event is available to allow the GarageMediator to know when a
//                  valid RFID tag has been scanned so it can request to change state
//                  or to continue listening (by clearing the CurrentID)
//
//  written by Jonathan Melcher and Brennan MacGregor on 2016-04-07
// **********************************************************************************


using System;


namespace GarageMediator
{
    internal sealed class MediatorListeningState : MediatorState
    {
        // event to propogate upwards when a valid RFID tag has been scanned
        public static event Action<object, string> IDScanned;

        // constructor - sets up _worker task using context
        public MediatorListeningState(GarageMediator context)
        {
            context.RFIDCommunication.OnIDScan += RFIDCommunication_OnIDScan;
        }

        public override void Change(GarageMediator context)
        {
            context.RFIDCommunication.OnIDScan -= RFIDCommunication_OnIDScan;
            context.State = new MediatorProcessingState(context);
        }

        private void RFIDCommunication_OnIDScan(string id)
        {
            IDScanned(this, id);
        }
    }
}