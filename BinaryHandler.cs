using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoWSimsApp
{
	internal class BinaryHandler
	{
		private const string repo = "wowsims/mop";

		private ProcessManager processManager;
		private Timer updateTimer;

		private bool isUpdateAvailable = false;

		public event Action<bool> UpdateAvailabilityChanged;

		public bool IsUpdateAvailable => isUpdateAvailable;

		public BinaryHandler( ProcessManager processManager )
		{
			this.processManager = processManager;

			// Start background timer for updates every X hours
			updateTimer = new Timer();
			updateTimer.Interval = 1000 * 60 * 60 * 1; // 1 hour
			updateTimer.Tick += UpdateTimer_Tick;
			updateTimer.Start();

			SystemEvents.PowerModeChanged += PowerModeChanged;

			LoadPendingUpdateState();
		}

		private void PowerModeChanged( object sender, PowerModeChangedEventArgs eventArgs )
		{
			switch (eventArgs.Mode)
			{
				case PowerModes.Suspend:
					updateTimer.Stop();
					break;
				case PowerModes.Resume:
					updateTimer.Start();
					CheckForUpdates( false, false );
					break;
			}
		}

		private void UpdateTimer_Tick( object sender, EventArgs e )
		{
			CheckForUpdates( false, false );
		}

		private void LoadPendingUpdateState()
		{
			if (File.Exists( Constants.PendingUpdateFile ))
			{
				updateTimer.Stop();
				isUpdateAvailable = true;
				UpdateAvailabilityChanged?.Invoke( true );
			}
		}

		public async void CheckForUpdates( bool startup, bool showMessage )
		{
			// If we have a pending update, proceed with installation
			if (isUpdateAvailable && File.Exists( Constants.PendingUpdateFile ))
			{
				await ProcessPendingUpdate( startup, showMessage );
				return;
			}

			// Fetch latest release info (use HttpClient)
			var releaseData = await GetLatestReleaseAsync();

			// Process release info...
			if (releaseData != null)
			{
				string releaseId = releaseData.id.ToString();
				string tagName = releaseData.tag_name;

				string lastReleaseId = null;
				if (File.Exists( Constants.LatestReleaseIdFile ))
				{
					lastReleaseId = File.ReadAllText( Constants.LatestReleaseIdFile );
				}

				if (releaseId != lastReleaseId)
				{
					DialogResult result = DialogResult.Yes;

					// always download if no version found
					if ( !startup && lastReleaseId != null)
					{
						// Ask for confirmation before updating
						result = MessageBox.Show(
							$"A new version ({tagName}) is available. Do you want to update now?",
							"Update Available",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question );
					}

					if (result == DialogResult.Yes)
					{
						await PerformUpdate( releaseData, tagName );
					}
					else
					{
						// Mark update as available without caching release data
						updateTimer.Stop();
						isUpdateAvailable = true;
						File.WriteAllText( Constants.PendingUpdateFile, "true" );
						UpdateAvailabilityChanged?.Invoke( true );

						if (showMessage)
						{
							MessageBox.Show( "Update has been postponed. You can install it later from the tray menu.", "Update Postponed", MessageBoxButtons.OK, MessageBoxIcon.Information );
						}
					}
				}
				else if (showMessage)
				{
					MessageBox.Show( $"You are already on the latest version: {tagName}", "No Updates Available", MessageBoxButtons.OK, MessageBoxIcon.Information );
				}
			}

			processManager.LaunchExecutable( false );
		}

		private async Task ProcessPendingUpdate( bool startup, bool showMessage )
		{
			if (File.Exists( Constants.PendingUpdateFile ))
			{
				// Fetch the latest release data instead of using cached data
				var releaseData = await GetLatestReleaseAsync();
				
				if (releaseData == null)
				{
					if (showMessage)
					{
						MessageBox.Show( "Unable to fetch update information. Please try again later.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning );
					}
					processManager.LaunchExecutable( false );
					return;
				}

				string tagName = releaseData.tag_name;
				string releaseId = releaseData.id.ToString();

				// Check if we're already on the latest version
				string lastReleaseId = null;
				if (File.Exists( Constants.LatestReleaseIdFile ))
				{
					lastReleaseId = File.ReadAllText( Constants.LatestReleaseIdFile );
				}

				if (releaseId == lastReleaseId)
				{
					// No update needed anymore, clear the pending state
					File.Delete( Constants.PendingUpdateFile );

					updateTimer.Start();
					isUpdateAvailable = false;
					UpdateAvailabilityChanged?.Invoke( false );

					if (showMessage)
					{
						MessageBox.Show( $"You are already on the latest version: {tagName}", "No Update Needed", MessageBoxButtons.OK, MessageBoxIcon.Information );
					}
					processManager.LaunchExecutable( false );
					return;
				}

				if ( !startup && showMessage)
				{
					var result = MessageBox.Show( 
						$"Update to version {tagName} is ready. Install now?",
						"Install Update", 
						MessageBoxButtons.YesNo, 
						MessageBoxIcon.Question );

					if (result == DialogResult.No)
					{
						processManager.LaunchExecutable( false );
						return;
					}
				}

				await PerformUpdate( releaseData, tagName );
			}
		}

		private async Task PerformUpdate( dynamic releaseData, string tagName )
		{
			processManager.KillProcess();

			// Download new release
			await DownloadAndExtractAsync( releaseData );
			File.WriteAllText( Constants.LatestReleaseIdFile, releaseData.id.ToString() );

			// Clear pending update
			if (File.Exists( Constants.PendingUpdateFile ))
			{
				File.Delete( Constants.PendingUpdateFile );
			}

			updateTimer.Start();
			isUpdateAvailable = false;
			UpdateAvailabilityChanged?.Invoke( false );

			MessageBox.Show( $"Succesfully updated to version: {tagName}", "Updates Complete", MessageBoxButtons.OK, MessageBoxIcon.Information );

			processManager.LaunchExecutable( true );
		}

		private async Task<dynamic> GetLatestReleaseAsync()
		{
			var client = new HttpClient();
			var url = $"https://api.github.com/repos/{repo}/releases/latest";

			client.DefaultRequestHeaders.UserAgent.TryParseAdd( "Mozilla/5.0" );
			var response = await client.GetAsync( url );
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				return Newtonsoft.Json.JsonConvert.DeserializeObject( json );
			}
			return null;
		}

		private async Task DownloadAndExtractAsync( dynamic releaseData )
		{
			var assets = releaseData.assets;
			string downloadUrl = null;

			foreach (var asset in assets)
			{
				if (asset.name.ToString().Contains( "wowsimmop-windows.exe.zip" ))
				{
					downloadUrl = asset.browser_download_url.ToString();
					break;
				}
			}

			if (downloadUrl == null)
			{
				return;
			}

			string zipPath = Path.Combine( Path.GetTempPath(), "wowsimmop-windows.exe.zip" );

			// Create and show the progress dialog
			using (var progressDialog = new ProgressDialog())
			{
				progressDialog.Show();

				using (var client = new HttpClient())
				{
					// Use HttpClient with progress reporting
					var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
					var totalBytes = response.Content.Headers.ContentLength ?? -1L;
					var receivedBytes = 0L;

					using var stream = await response.Content.ReadAsStreamAsync();
					using var fs = new FileStream( zipPath, FileMode.Create, FileAccess.Write );
					var buffer = new byte[8192];
					int bytesRead;
					while ((bytesRead = await stream.ReadAsync( buffer )) > 0)
					{
						await fs.WriteAsync( buffer.AsMemory( 0, bytesRead ) );
						receivedBytes += bytesRead;

						// Update progress bar
						if (totalBytes > 0)
						{
							progressDialog.UpdateProgress( (int)(receivedBytes * 100 / totalBytes) );
						}
					}
				}

				progressDialog.Close();
			}

			string extractPath = Constants.ExtractReleaseFile;
			if (Directory.Exists( extractPath ))
			{
				Directory.Delete( extractPath, true );
			}

			System.IO.Compression.ZipFile.ExtractToDirectory( zipPath, extractPath );
			File.Delete( zipPath );
		}
	}
}
