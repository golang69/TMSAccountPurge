using System;

namespace Model
{
	public class IncomingMsgExtract
	{
		public Guid IncomingMessageId { get; set; }
		public DateTime CreatedOn { get; set; }
		public String XmlMessage { get; set; }
	}
}