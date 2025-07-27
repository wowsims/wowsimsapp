using System.IO;
using System.Windows.Forms;

namespace WoWSimsApp
{
	internal static class Constants
	{
		public const string ProcessName = "wowsimmop-windows";

		public const string LocalHostUrl = "http://localhost:3333/mop/";

		public static string LatestReleaseIdFile = Path.Combine( Application.LocalUserAppDataPath, "WoWSims", ".last_release_id" );
		public static string PendingUpdateFile = Path.Combine( Application.LocalUserAppDataPath, "WoWSims", ".pending_update" );
		public static string ExtractReleaseFile = Path.Combine( Application.LocalUserAppDataPath, "WoWSims", "binary" );
	}
}
