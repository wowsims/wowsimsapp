using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WoWSimsApp
{
	internal class ProcessManager
	{
		private Process activeProcess;

		public ProcessManager()
		{
			var processes = Process.GetProcesses();
			activeProcess = processes.FirstOrDefault( p => p.ProcessName.Contains( Constants.ProcessName, StringComparison.OrdinalIgnoreCase ) );
		}

		public void LaunchSim()
		{
			if (activeProcess == null)
			{
				LaunchExecutable( true );
			}
			else
			{
				OpenUrl( Constants.LocalHostUrl );
			}
		}

		public void OpenUrl( string url )
		{
			Process.Start( new ProcessStartInfo()
			{
				FileName = url,
				UseShellExecute = true
			} );
		}

		public void LaunchExecutable( bool doRestart )
		{
			if (activeProcess != null && doRestart == false)
			{
				return;
			}

			string exePath = Path.Combine( Constants.ExtractReleaseFile, $"{Constants.ProcessName}.exe" );
			if (File.Exists( exePath ))
			{
				KillProcess();

				ProcessStartInfo psi = new ProcessStartInfo()
				{
					CreateNoWindow = true,
					FileName = exePath,
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WindowStyle = ProcessWindowStyle.Hidden,
				};

				activeProcess = Process.Start( psi );
			}
		}

		public void KillProcess()
		{
			if (activeProcess != null)
			{
				activeProcess.Kill();
				activeProcess.WaitForExit();
				activeProcess.Dispose();
			}

			activeProcess = null;
		}
	}
}
