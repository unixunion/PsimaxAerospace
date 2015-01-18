// This 1st example will make the PC toggle the integrated led on the arduino board. 
// It demonstrates how to:
// - Define commands
// - Set up a serial connection
// - Send a command with a parameter to the Arduino


using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandMessenger;
using CommandMessenger.Serialport;
using CommandMessenger.TransportLayer;
using KSPPluginFramework;
using KSP;
using UnityEngine;

namespace PsimaxAerospace
{
    
    // This is the list of recognized commands. These can be commands that can either be sent or received. 
    // In order to receive, attach a callback function to these events
    // 
    // Default commands
    // Note that commands work both directions:
    // - All commands can be sent
    // - Commands that have callbacks attached can be received
    // 
    // This means that both sides should have an identical command list:
    // one side can either send it or receive it (sometimes both)
 
    // Commands
    enum Command
    {
        SetLed, // Command to request led to be set in specific state
    };
		
	[KSPAddon(KSPAddon.Startup.MainMenu,false)]
	public class PsimaxAerospace : MonoBehaviourExtended
    {

		public static SerialTransport _serialTransport;
		public static CmdMessenger _cmdMessenger;
        private bool _ledState;
		private bool _sasState;
					
		internal override void Awake()
		{
			LogFormatted("PACS is awake");
			MBExtendedChild Child = gameObject.AddComponent<MBExtendedChild>();

			LogFormatted ("Setting up serial port");
			// Create Serial Port object
			SerialTransport _serialTransport = new SerialTransport();
			_serialTransport.CurrentSerialSettings.PortName = "/dev/tty.usbmodem621";    // Set com port
			_serialTransport.CurrentSerialSettings.BaudRate = 115200;     // Set baud rate
			_serialTransport.CurrentSerialSettings.DtrEnable = false;     // For some boards (e.g. Sparkfun Pro Micro) DtrEnable may need to be true.

			// Initialize the command messenger with the Serial Port transport layer
			LogFormatted ("Setting up messenger");
			CmdMessenger _cmdMessenger = new CmdMessenger(_serialTransport);

			// Tell CmdMessenger if it is communicating with a 16 or 32 bit Arduino board
			_cmdMessenger.BoardType = BoardType.Bit16;

			// Attach the callbacks to the Command Messenger
//			AttachCommandCallBacks();
			_ledState = false;

			// Start listening
			LogFormatted ("Connecting to Arduino");
			_cmdMessenger.Connect();  
			//Start the repeating worker to fire once each second
			StartRepeatingWorker(1);
		}


		internal override void RepeatingWorker()
		{
			LogFormatted ("Reading SAS status");
			_sasState = (FlightGlobals.ActiveVessel.ActionGroups [KSPActionGroup.SAS]);

			// Create command
			LogFormatted ("Creating a command");
			var command = new SendCommand((int)Command.SetLed,_ledState);               

			// Send command
			LogFormatted ("Sending Command");
			_cmdMessenger.SendCommand(command);

			LogFormatted("sending SAS status to Arduino ");
			LogFormatted(_sasState?"on":"off");

			// Wait for 1 second and repeat
			Thread.Sleep(1000);
			_ledState = !_ledState;   // Toggle led state          
		}




    }


	public class MBExtendedChild : MonoBehaviourExtended
	{

		public static SerialTransport _serialTransport;
		public static CmdMessenger _cmdMessenger;
		private bool _ledState;
		private bool _sasState;


		internal override void Awake()
		{
			LogFormatted("Child is awake");
			SerialTransport _serialTransport = new SerialTransport();
		}
		internal override void RepeatingWorker()
		{
			LogFormatted ("CHILD status");
			Thread.Sleep(1000);
		}

	}

}
