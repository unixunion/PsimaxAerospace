using System;

namespace KSPSerial
{
	public class Helper
	{
		int open_serial (char *devfile)
		{
			int fd;
			fd = open (devfile, O_RDWR | O_NOCTTY | O_NONBLOCK);

			return fd;
		}
	}
}

