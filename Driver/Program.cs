using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Business;
using log4net;

namespace Driver
{
	class Program
	{
		private static ILog _log;
		static void Main(string[] args)
		{
			var tmsAccountService = new TMSAccountService(new TmsAccountPurgeSettings());
			try
			{
				DefaultLog();

				tmsAccountService.ProcessTmsAccounts();
			}
			catch (Exception exception)
			{
				var message = new StringBuilder();
				message = message.AppendLine("[Exception.Message]: " + exception.Message);
				if (exception.InnerException != null)
				{
					message.AppendLine("[InnerException.Message]: " + exception.InnerException.Message);
				}
				message.AppendLine("[StackTrace]: " + exception.StackTrace);

				_log.Error(message.ToString());
			}
		}
		static void DefaultLog()
		{
			log4net.Config.XmlConfigurator.Configure();
			_log = LogManager.GetLogger("TMSAccountService");
		}
	}
}