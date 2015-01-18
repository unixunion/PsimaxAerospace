using System;
using KSPPluginFramework;
using KSP;
using UnityEngine;
using System.Threading;


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

		internal override void Awake()
		{
			LogFormatted("Child is awake");
//			_serialPort = new SerialPort("/dev/tty.usbmodem621", 115200, Parity.None, 8, StopBits.One);

		}
		internal override void RepeatingWorker()
		{
			LogFormatted ("CHILD status");
			Thread.Sleep(1000);
		}

	}

}
