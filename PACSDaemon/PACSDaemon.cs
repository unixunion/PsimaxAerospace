using System;
using FTSerial.IO.Ports;

namespace PACSDaemon
{

	public struct Command
	{
		public Boolean sas;
		public Boolean rcs;
		public Boolean gear;
		public Boolean light;

	}

	public class Program
	{
		static void Main()
		{



			// mimics Arduino calling structure
			SerialPort port = new SerialPort ("/dev/tty.usbmodem621", 115200);
			port.Open ();

			Command cmd = new Command();
			cmd.sas = true;

			port.Write(cmd.ToString());

		}
	}
}

