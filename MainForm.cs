using Serilog;
using Microsoft.Extensions.Logging;
using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace WoWSimsApp
{
	public partial class MainForm : Form
	{
		private ProcessManager processManager;
		private BinaryHandler binaryHandler;
		private TrayIconManager trayIconManager;
		private WowApplicationHelper wowApplicationHelper;
		private ToolStripMenuItem updateMenuItem;

		public MainForm()
		{
			// Configure Serilog
			Log.Logger = new LoggerConfiguration()
				.WriteTo.File( "logs/app.log", rollingInterval: RollingInterval.Day )
				.CreateLogger();

			// Configure logging
			var loggerFactory = LoggerFactory.Create( builder =>
			{
				builder.AddSerilog();
			} );
			var logger = loggerFactory.CreateLogger<MainForm>();

			// Attach global exception handlers
			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				logger.LogError( args.ExceptionObject as Exception, "Unhandled exception occurred" );
			};

			Application.ThreadException += ( sender, args ) =>
			{
				logger.LogError( args.Exception, "Thread exception occurred" );
			};

			// Check for internet connection
			if (!IsInternetAvailable())
			{
				MessageBox.Show("This application requires an internet connection to function. Please check your connection and restart the application.", 
					"Internet Connection Required", 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error);
				Application.Exit();
				return;
			}

			InitializeComponent();

			processManager = new ProcessManager();
			binaryHandler = new BinaryHandler( processManager );
			trayIconManager = new TrayIconManager();
			wowApplicationHelper = new WowApplicationHelper();

			this.ShowInTaskbar = false;
			this.WindowState = FormWindowState.Minimized;
			this.Visible = false;
			this.FormBorderStyle = FormBorderStyle.None;

			InitializeTrayIcons();

			 // Subscribe to update availability changes
			binaryHandler.UpdateAvailabilityChanged += OnUpdateAvailabilityChanged;

			// Run update check
			binaryHandler.CheckForUpdates( true, false );
		}

		private static bool IsInternetAvailable()
		{
			try
			{
				using (var ping = new Ping())
				{
					var reply = ping.Send("www.google.com", 3000); // Ping Google with a timeout of 3 seconds
					return reply != null && reply.Status == IPStatus.Success;
				}
			}
			catch
			{
				return false;
			}
		}

		private void OnUpdateAvailabilityChanged( bool isUpdateAvailable )
		{
			if (updateMenuItem != null)
			{
				updateMenuItem.Text = isUpdateAvailable ? "Update Sim" : "Check for Updates";
			}
		}

		private void InitializeTrayIcons()
		{
			trayIconManager.SetDoubleClick( () => processManager.LaunchSim() );

			var launchMenu = trayIconManager.AddMenuItem( "Open", 0, null, () => { } );
			foreach (var classEntry in SimData.Classes)
			{
				var classMenu = new ToolStripMenuItem( classEntry.Name, classEntry.Icon );
				for (int i = 0; i < classEntry.Specs.Count; i++)
				{
					string spec = classEntry.Specs[i];
					classMenu.DropDownItems.Add( spec, classEntry.SpecIcons[i], ( s, e ) =>
					{
						string url = $"{Constants.LocalHostUrl}{classEntry.Name.ToLowerInvariant()}/{spec.ToLowerInvariant()}".Replace( ' ', '_' );
						processManager.OpenUrl( url );
					} );
				}
				launchMenu.DropDownItems.Add( classMenu );
			}

			var characterMenu = trayIconManager.AddMenuItem( "Characters", 96, null, () => { } );

			updateMenuItem = trayIconManager.AddMenuItem( 
				binaryHandler.IsUpdateAvailable ? "Update Sim" : "Check for Updates", 
				99, 
				null, 
				() => binaryHandler.CheckForUpdates( false, true ) );

			trayIconManager.AddMenuItem( "Exit", 100, null, () =>
			{
				processManager.KillProcess();
				Application.Exit();
			} );

			trayIconManager.SetOnOpen( () => refreshCharaters() );

			void refreshCharaters()
			{
				characterMenu.DropDownItems.Clear();
				var characters = wowApplicationHelper.GetCharacters();
				foreach (var character in characters)
				{
					var localCharacter = character;
					characterMenu.DropDownItems.Add( $"{character.Name} ({character.Realm})", SimData.GetSpecIcon( character.Class, character.Spec ), ( s, e ) =>
					{
						MessageBox.Show( "Character data has been copied to clipboard.\n\nImport -> Addon\n\nand paste to start siming your character.", "Sim Character", MessageBoxButtons.OK, MessageBoxIcon.Information );
						string url = $"{Constants.LocalHostUrl}{SimData.CharacterToUrlCombo( character )}".Replace( ' ', '_' );
						Clipboard.SetText( character.Json );
						processManager.OpenUrl( url );
					} );
				}
			}
		}
	}
}
