namespace TMSAccountPurgeWS
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tmsAccountPurgeProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.tmsAccountPurgeInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// tmsAccountPurgeProcessInstaller
			// 
			this.tmsAccountPurgeProcessInstaller.Password = null;
			this.tmsAccountPurgeProcessInstaller.Username = null;
			// 
			// tmsAccountPurgeInstaller
			// 
			this.tmsAccountPurgeInstaller.Description = "Scans Incoming Messages DB table for non-relevant items";
			this.tmsAccountPurgeInstaller.DisplayName = "TFM Account Purge Service";
			this.tmsAccountPurgeInstaller.ServiceName = "TMS Account Purge Service";
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.tmsAccountPurgeProcessInstaller,
            this.tmsAccountPurgeInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller tmsAccountPurgeProcessInstaller;
		private System.ServiceProcess.ServiceInstaller tmsAccountPurgeInstaller;
	}
}