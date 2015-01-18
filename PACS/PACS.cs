using System;
using KSPPluginFramework;
using KSP;
using UnityEngine;
using System.Threading;
using OpenNETCF.IO.Serial;


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

//		public static SerialPort _serialPort;
//		private bool _ledState;
		private static Port port;
		private DetailedPortSettings portSettings;

		internal override void Awake()
		{
			LogFormatted("Child is awake");
			portSettings = new HandshakeNone ();
			portSettings.BasicSettings.BaudRate = BaudRates.CBR_115200;

			port = new Port ("/dev/tty.usbmodem621", portSettings);
			port.DataReceived += new Port.CommEvent (port_DataReceived);
			StartRepeatingWorker(1);
//			_serialPort = new SerialPort("/dev/tty.usbmodem621", 115200, Parity.None, 8, StopBits.One);

		}
		internal override void RepeatingWorker()
		{
			LogFormatted ("CHILD status");
			byte[] outputData = new byte[1];
			outputData [0] = Convert.ToByte ('0');
			port.Output = outputData;
			Thread.Sleep(1000);
		}

		private void port_DataReceived()
		{
			LogFormatted ("Data received");
		}

	}

}
