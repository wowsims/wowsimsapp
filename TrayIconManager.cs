using System;
using System.Drawing;
using System.Windows.Forms;

namespace WoWSimsApp
{
	internal class TrayIconManager
	{
		private NotifyIcon trayIcon;

		private ContextMenuStrip contextMenu;

		public TrayIconManager()
		{
			trayIcon = new NotifyIcon();
			trayIcon.Icon = Properties.Resources.icon;
			trayIcon.Visible = true;
			trayIcon.Text = "WoWSims";

			contextMenu = new ContextMenuStrip();
			trayIcon.ContextMenuStrip = contextMenu;
		}

		public void SetDoubleClick( Action onAction )
		{
			trayIcon.DoubleClick += ( s, e ) =>
			{
				onAction();
			};
		}

		public void SetOnOpen( Action onAction )
		{
			contextMenu.Opening += ( s, e ) =>
			{
				onAction();
			};
		}

		public ToolStripMenuItem AddMenuItem( string menuItem, int index, Bitmap icon, Action onClick )
		{
			var item = new ToolStripMenuItem( menuItem, icon, ( s, e ) => { onClick(); } );
			item.MergeIndex = index;
			contextMenu.Items.Add( item );
			return item;
		}
	}
}
