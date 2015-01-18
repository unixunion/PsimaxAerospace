/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

using System;

	public class SerialReceivedEventArgs : EventArgs
	{
		internal SerialReceivedEventArgs (SerialData eventType)
		{
			this.eventType = eventType;
		}

		// properties

		public SerialData EventType {
			get {
				return eventType;
			}
		}

		SerialData eventType;
	}

	public enum SerialData 
	{
		Chars = 1,
		Eof
	} 
	




