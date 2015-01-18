using System;
using System.IO;
using OpenNETCF.IO.Ports;
using System.Runtime.InteropServices;

namespace OpenNETCF.IO.Ports.Streams
{
	public class WinStream : Stream, ISerialStreamCtrl
	{
		internal OpenNETCF.IO.Serial.Port _port;

		#region Construction and Disposing
		internal WinStream(
			int			baudRate,
			int			dataBits,
			bool		discardNull,
			bool		dtrEnable,
			Handshake	handshake,
			Parity		parity,
			byte		parityReplace,
			string		portName,
			int			readBufferSize,
			int			readTimeout,
			int			receivedBytesThreshold,
			bool		rtsEnable,
			StopBits	stopBits,
			int			writeBufferSize,
			int			writeTimeout )
		{
			_port = new OpenNETCF.IO.Serial.Port( portName, readBufferSize, writeBufferSize );
			
			BaudRate				= baudRate;
			DataBits				= dataBits;
			DiscardNull				= discardNull;
			DtrEnable				= dtrEnable;
			Handshake				= handshake;
			Parity					= parity;
			ParityReplace			= parityReplace;
// TODO:	ReadTimeout				= readTimeout;
			ReceivedBytesThreshold	= receivedBytesThreshold;
			RtsEnable				= rtsEnable;
			StopBits				= stopBits;
// TODO:	WriteTimeout			= writeTimeout;

			_port.DataReceived		+= new OpenNETCF.IO.Serial.Port.CommEvent(_port_DataReceived);
			_port.OnError			+= new OpenNETCF.IO.Serial.Port.CommErrorEvent(_port_OnError);
			_port.RingChange		+= new OpenNETCF.IO.Serial.Port.CommChangeEvent(_port_RingChange);
			_port.RLSDChange		+= new OpenNETCF.IO.Serial.Port.CommChangeEvent(_port_RLSDChange);

			if( !_port.Open() )
				throw new UnauthorizedAccessException();
		}

		protected void AssertOpenPort()
		{
			if( !_port.IsOpen )
				throw new InvalidOperationException("Serial Port is not open");
		}

		protected bool GetCommStatusFlag( OpenNETCF.IO.Serial.CommModemStatusFlags statusFlag )
		{
			AssertOpenPort();

			uint modemStatus = 0;
			_port.m_CommAPI.GetCommModemStatus( _port.hPort, ref modemStatus );
			
			return ( (uint)statusFlag & modemStatus ) != 0;
		}

		public void Dispose()
		{
			_port.Dispose();
		}

		#endregion

		#region Stream Implementation

		public override bool CanRead	{ get { return _port.IsOpen; }}
		public override bool CanWrite	{ get { return _port.IsOpen; }}
		public override bool CanSeek	{ get { return false; }}
		public override long Length		{ get { throw new NotSupportedException(); }}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}


		public override void Close()
		{
			base.Close ();
			Dispose();
		}

		public override void Flush()
		{
			AssertOpenPort();

			if( _port.OutBufferCount > 0 )
			{
				int oldSThreshold = _port.SThreshold;
				_port.SThreshold = -1; // force even empty buffer to be written out
				_port.Output = new byte[0];
				_port.SThreshold = oldSThreshold;
			}

			// Flush OS buffer
			if( !_port.m_CommAPI.FlushFileBuffers(_port.hPort) )
				throw new InvalidOperationException( string.Format( "Error flushing buffer: {0}", Marshal.GetLastWin32Error() ));
		}

		public override int  Read( byte[] buffer, int offset, int count )
		{
			AssertOpenPort();

			if( buffer == null )
				throw new ArgumentException( "null", "buffer" );

			if( offset < 0 )
				throw new ArgumentException( "<0", "offset" );

			if( count < 0 )
				throw new ArgumentException( "<0", "count" );
			else if( count == 0 )
				return 0;

			//
			// modified from original Port.Input
			//
			// lock the rx FIFO while reading
			_port.rxBufferBusy.WaitOne();

			// how much data are we *actually* going to return from the call?
			int dequeueLength = (count < _port.rxFIFO.Count) ? count : _port.rxFIFO.Count;

			// dequeue the data
			for(int p = offset ; p < offset + dequeueLength ; p++)
				buffer[p] = (byte)_port.rxFIFO.Dequeue();

			// release the mutex so the Rx thread can continue
			_port.rxBufferBusy.ReleaseMutex();

			return dequeueLength;
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			throw new NotSupportedException();
		}

		public override void SetLength( long value )
		{
			throw new NotSupportedException();
		}

		public override void Write( byte[] buffer, int offset, int count )
		{
			AssertOpenPort();

			if( buffer == null )
				throw new ArgumentException( "null", "buffer" );

			if( offset < 0 )
				throw new ArgumentException( "<0", "offset" );

			if( count < 0 )
				throw new ArgumentException( "<0", "count" );
			else if( count == 0 )
				return;

			byte[] buf = new byte[ count ];
			Buffer.BlockCopy( buffer, offset, buf, 0, count );

			_port.Output = buf;
		}
		#endregion

		#region	  ISerialStreamCtrl Members
		public int			BaudRate
		{
			get { return (int) _port.Settings.BaudRate; }
			set { _port.Settings.BaudRate = (OpenNETCF.IO.Serial.BaudRates) value; }
		}

		public bool			BreakState
		{
			get { return _port.Break; }
			set { _port.Break = value; }
		}

		public int			BytesToRead
		{
			get { return _port.InBufferCount; }
		}

		public int			BytesToWrite
		{
			get { return _port.OutBufferCount; }
		}

		public bool			CDHolding
		{
			get { return GetCommStatusFlag( OpenNETCF.IO.Serial.CommModemStatusFlags.MS_RLSD_ON ); }
		}

		public bool			CtsHolding
		{
			get { return GetCommStatusFlag( OpenNETCF.IO.Serial.CommModemStatusFlags.MS_CTS_ON ); }
		}

		public int			DataBits
		{
			get { return _port.Settings.ByteSize; }
			set { _port.Settings.ByteSize = (byte)value; }
		}

		public bool			DiscardNull
		{
			get { return _port.DetailedSettings.DiscardNulls; }
			set { _port.DetailedSettings.DiscardNulls = value; }
		}

		public bool			DsrHolding
		{
			get { return GetCommStatusFlag( OpenNETCF.IO.Serial.CommModemStatusFlags.MS_DSR_ON ); }
		}

		public bool			DtrEnable
		{
			get { return _port.DTREnable; }
			set { _port.DTREnable = value; }
		}

		public Handshake	Handshake
		{
			get
			{
				if( _port.DetailedSettings is OpenNETCF.IO.Serial.HandshakeNone )
					return Handshake.None;
				else if( _port.DetailedSettings is OpenNETCF.IO.Serial.HandshakeCtsRts )
					return Handshake.RequestToSend;
				else if( _port.DetailedSettings is OpenNETCF.IO.Serial.HandshakeXonXoff )
					return Handshake.XOnXOff;
				else if( _port.DetailedSettings is OpenNETCF.IO.Serial.HandshakeDsrDtr )
					return Handshake.RequestToSendXOnXOff;
				else
					throw new NotImplementedException();
			}
			set
			{
				// Creating a new Handshaking object resets BasicSettings parameters.
				OpenNETCF.IO.Serial.BasicPortSettings tempPortSettings = _port.DetailedSettings.BasicSettings;

				switch( value )
				{
					case OpenNETCF.IO.Ports.Handshake.None:
						_port.DetailedSettings = new OpenNETCF.IO.Serial.HandshakeNone();
						break;
					case OpenNETCF.IO.Ports.Handshake.RequestToSend:
						_port.DetailedSettings = new OpenNETCF.IO.Serial.HandshakeCtsRts();
						break;
					case OpenNETCF.IO.Ports.Handshake.XOnXOff:
						_port.DetailedSettings = new OpenNETCF.IO.Serial.HandshakeXonXoff();
						break;
					case OpenNETCF.IO.Ports.Handshake.RequestToSendXOnXOff:
						_port.DetailedSettings = new OpenNETCF.IO.Serial.HandshakeDsrDtr();
						break;
					default:
						throw new NotImplementedException();
				}

				_port.DetailedSettings.BasicSettings = tempPortSettings;
			}
		}

		public bool			IsOpen
		{
			get { return _port.IsOpen; }
		}

		public Parity		Parity
		{
			get { return (Parity) Convert.ToInt32(_port.Settings.Parity); }
			set { _port.Settings.Parity = (OpenNETCF.IO.Serial.Parity) Convert.ToInt32(value); }
		}

		public byte			ParityReplace
		{
			get
			{
				return _port.DetailedSettings.ReplaceErrorChar ? (byte)_port.DetailedSettings.ErrorChar : (byte)0;
			}
			set
			{
				if( value == 0 )
					_port.DetailedSettings.ReplaceErrorChar = false;
				else
				{
					_port.DetailedSettings.ErrorChar		= (char) value;
					_port.DetailedSettings.ReplaceErrorChar	= true;
				}
			}
		}

		public int			ReadBufferSize
		{
			get { return _port.rxBufferSize; }
			set { throw new InvalidOperationException("Only available during port initialization"); }
		}

		public int			ReadTimeout
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public int			ReceivedBytesThreshold
		{
			get { return _port.RThreshold; }
			set { _port.RThreshold = value; }
		}

		public bool			RtsEnable
		{
			get { return _port.RTSEnable; }
			set { _port.RTSEnable = value; }
		}

		public StopBits		StopBits
		{
			get
			{
				switch( _port.Settings.StopBits )
				{
					default:
						throw new InvalidOperationException();
					case OpenNETCF.IO.Serial.StopBits.one:
						return StopBits.One;
					case OpenNETCF.IO.Serial.StopBits.onePointFive:
						return StopBits.OnePointFive;
					case OpenNETCF.IO.Serial.StopBits.two:
						return StopBits.Two;
				}
			}
			set
			{
				switch( value )
				{
					case StopBits.One:
						_port.Settings.StopBits = OpenNETCF.IO.Serial.StopBits.one;
						break;
					case StopBits.OnePointFive:
						_port.Settings.StopBits = OpenNETCF.IO.Serial.StopBits.onePointFive;
						break;
					case StopBits.Two:
						_port.Settings.StopBits = OpenNETCF.IO.Serial.StopBits.two;
						break;
				}
			}
		}

		public int			WriteBufferSize
		{
			get { return _port.txBufferSize; }
			set { throw new InvalidOperationException("Only available during port initialization"); }
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
			// lock the rx FIFO while reading
			_port.rxBufferBusy.WaitOne();

			// Empty buffer
			_port.rxFIFO.Clear();

			// release the mutex so the Rx thread can continue
			_port.rxBufferBusy.ReleaseMutex();
		}

		public void DiscardOutBuffer()
		{
			_port.ptxBuffer = 0;
		}

		#endregion

		#region Internal Events Forwarding
		private void _port_DataReceived()
		{
			if( ReceivedEvent != null )
				ReceivedEvent(this, new SerialReceivedEventArgs(SerialReceived.ReceivedChars));
		}

		private void _port_OnError(string Description)
		{
			if( ErrorEvent != null )
			{
				SerialErrors err = (SerialErrors) 0;

				if( Description.IndexOf("Framing") >= 0 )
					err |= SerialErrors.Frame;
				if( Description.IndexOf("Overrun") >= 0 )
					err |= SerialErrors.Overrun;
				if( Description.IndexOf("Receive Overflow") >= 0 )
					err |= SerialErrors.RxOver;
				if( Description.IndexOf("Parity") >= 0 )
					err |= SerialErrors.RxParity;
				if( Description.IndexOf("Transmit Overflow") >= 0 )
					err |= SerialErrors.TxFull;

				ErrorEvent( this, new SerialErrorEventArgs(err) );
			}
		}

		private void _port_RingChange(bool NewState)
		{
			if( PinChangedEvent != null )
				PinChangedEvent(this, new SerialPinChangedEventArgs(SerialPinChanges.Ring));
		}

		private void _port_RLSDChange(bool NewState)
		{
			if( PinChangedEvent != null )
				PinChangedEvent(this, new SerialPinChangedEventArgs(SerialPinChanges.CDChanged));
		}

		#endregion
	}
}
