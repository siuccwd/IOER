using System;
using System.Net;


namespace ILPathways.Controllers
{
	public class EventFailureEventArgs : EventArgs
    {
        public string Message;

        public EventFailureEventArgs( string message )
        {
            this.Message = message;
        }

	}
}
