using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Business;
using log4net;

namespace TMSAccountPurgeWS
{
	public partial class TMSAccountPurgeWS : ServiceBase
	{
		private ILog _log;
		private readonly TMSAccountService _tmsAccountService = new TMSAccountService(new TmsAccountPurgeSettings());

		public TMSAccountPurgeWS()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				DefaultLog();
				_log.InfoFormat("In OnStart");
				new Thread(() => { _tmsAccountService.ProcessTmsAccounts(); }).Start();
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

				_log.ErrorFormat(message.ToString());
			}
		}

		private void DefaultLog()
		{
			log4net.Config.XmlConfigurator.Configure();
			_log = LogManager.GetLogger(this.GetType());

		}
		protected override void OnStop()
		{
			_log.InfoFormat("In OnStop");
		}
	}
}