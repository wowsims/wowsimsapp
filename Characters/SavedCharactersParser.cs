using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Linq;

namespace WoWSimsApp.Characters
{
	public struct SavedCharacter
	{
		public string Json;
		public string Name;
		public string Class;
		public string Spec;
		public string Realm;
	}

	internal static class SavedCharactersParser
	{
		public static List<SavedCharacter> ParseFromLuaFile( string filePath )
		{
			var result = new List<SavedCharacter>();
			string fileContent = File.ReadAllText( filePath );

			// Regex to match ["data"] = "...."
			var regex = new Regex( @"\[\s*""data""\s*\]\s*=\s*""((?:[^""\\]|\\.)*)""", RegexOptions.Compiled );

			foreach (Match match in regex.Matches( fileContent ))
			{
				// Unescape the JSON string (Lua escapes quotes as \")
				string json = match.Groups[1].Value.Replace( "\\\"", "\"" );

				string charName = "";
				string className = "";
				string specName = "";
				string realmName = "";

				try
				{
					using var doc = JsonDocument.Parse( json );
					var root = doc.RootElement;
					if (root.TryGetProperty( "name", out var nameProp ))
					{
						charName = nameProp.GetString() ?? "";
					}

					if (root.TryGetProperty( "class", out var classProp ))
					{
						className = classProp.GetString() ?? "";
					}

					if (root.TryGetProperty( "spec", out var specProp ))
					{
						specName = specProp.GetString() ?? "";
					}

					if (root.TryGetProperty( "realm", out var realmProp ))
					{
						realmName = realmProp.GetString() ?? "";
					}
				}
				catch
				{
					// Ignore parse errors, leave fields empty
				}

				result.Add( new SavedCharacter 
				{ 
					Json = json,
					Name = charName,
					Class = className,
					Spec = specName,
					Realm = realmName,
				} );
			}

			return result;
		}
	}
}