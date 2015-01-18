using System;
using System.Net.Sockets;
using OpenNETCF.IO.Ports;

namespace OpenNETCF.IO.Ports.Streams
{
	/// <summary>
	/// This class is a wrapper on top of th NetworkStream.
	/// Use it when serial port gets exposed as a network port
	/// </summary>
	internal class SerialStreamSocket : NetworkStream, ISerialStreamCtrl
	{
		internal SerialStreamSocket( Socket socket ) : base( socket )
		{
		}

		public int			BaudRate
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool			BreakState
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}


		public const int	FIONREAD   = 0x4004667F;
		public int			BytesToRead
		{
			get 
			{
				// 0x4004667F
				// IOVT T|vendor/addr fmly|  code
				// 0100 0|000  0000 0100  |  0110 0110  0111 1111
				//
				byte[] outValue = BitConverter.GetBytes(0);	// (int)0 -> byte[4]

				// Check how many bytes have been received.
				base.Socket.IOControl(FIONREAD, null, outValue);
    
				return (int)BitConverter.ToUInt32(outValue, 0);
			}    
		}

		public int			BytesToWrite
		{
			get { return 0; }	// always report as if everything has been transmitted
		}

		public bool			CDHolding
		{
			get { throw new NotImplementedException(); }
		}

		public bool			CtsHolding
		{
			get { throw new NotImplementedException(); }
		}

		public int			DataBits
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool			DiscardNull
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool			DsrHolding
		{
			get { throw new NotImplementedException(); }
		}

		public bool			DtrEnable
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public Handshake	Handshake
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool			IsOpen
		{
			get { throw new NotImplementedException(); }
		}

		public Parity		Parity
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public byte			ParityReplace
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public int			ReadBufferSize
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public int			ReadTimeout
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public int			ReceivedBytesThreshold
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool			RtsEnable
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public StopBits		StopBits
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public int			WriteBufferSize
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public int			WriteTimeout
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}


		public event SerialErrorEventHandler		ErrorEvent;
		public event SerialReceivedEventHandler		ReceivedEvent;
		public event SerialPinChangedEventHandler	PinChangedEvent;

		public void DiscardInBuffer()
		{
			throw new NotImplementedException();
		}

		public void DiscardOutBuffer()
		{
			throw new NotImplementedException();
		}
	}
}