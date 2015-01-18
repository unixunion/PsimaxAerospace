using System;
using KSPPluginFramework;
using KSP;
using UnityEngine;
using System.Threading;
using KSPSerial.IO.Ports;


/*
 * This is the PACS plugin for KSP, this will transmit KSP game state information to 
 * PACSDaemon over network socket, and if possible also read data from PACSDaemon and
 * pass commands on to the KSP game
 */
namespace PACS
{
    
    // Commands
    enum Command
    {
        SetLed, // Command to request led to be set in specific state
    };
		
	[KSPAddon(KSPAddon.Startup.MainMenu,false)]
	public class PACS : MonoBehaviourExtended
    {
	

					
		internal override void Awake()
		{
			LogFormatted("PACS is awake");
			PACSChild Child = gameObject.AddComponent<PACSChild>();	
			StartRepeatingWorker(1);
		}


		internal override void RepeatingWorker()
		{
			LogFormatted ("Parent Repeater");
			Thread.Sleep(1000);        
		}




    }


	public class PACSChild : MonoBehaviourExtended
	{

		private SerialPort port;
		private bool _sasState;
		private bool _rcsState;
		private bool _gearState;
		private bool _lightState;

		internal override void Awake()
		{
			LogFormatted("Connecting to Arduino");
			port = new SerialPort ("/dev/tty.usbmodem621", 115200);
			port.Open ();

			StartRepeatingWorker(1);
//			_serialPort = new SerialPort("/dev/tty.usbmodem621", 115200, Parity.None, 8, StopBits.One);

		}
		internal override void RepeatingWorker()
		{

			_sasState = (FlightGlobals.ActiveVessel.ActionGroups [KSPActionGroup.SAS]);
			_rcsState = (FlightGlobals.ActiveVessel.ActionGroups [KSPActionGroup.RCS]);
			_gearState = (FlightGlobals.ActiveVessel.ActionGroups [KSPActionGroup.Gear]);
			_lightState = (FlightGlobals.ActiveVessel.ActionGroups [KSPActionGroup.Light]);


			LogFormatted ("CHILD status");

			// send the states one at a time
			port.Write (_sasState ? "S" : "s");
			port.Write (_rcsState ? "R" : "r");

			Thread.Sleep(1000);
		}

		private void port_DataReceived()
		{
			LogFormatted ("Data received");
		}

	}

}
