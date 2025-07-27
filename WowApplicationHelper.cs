using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WoWSimsApp.Characters;

namespace WoWSimsApp
{
	internal class WowApplicationHelper
	{
		private const string WowDisplayName = "Cataclysm Classic";
		public WowApplicationHelper()
		{
		}

		public List<SavedCharacter> GetCharacters()
		{
			List<SavedCharacter> characters = new List<SavedCharacter>();

			string installLocation = GetInstallLocation();
			if (string.IsNullOrEmpty( installLocation ))
			{
				return characters;
			}

			string accountsPath = Path.Combine( installLocation, "WTF", "Account" );
			if (!Directory.Exists( accountsPath ))
			{
				return characters;
			}

			foreach (var accountDir in Directory.GetDirectories( accountsPath ))
			{
				string charactersPath = Path.Combine( accountDir, "SavedVariables", "WowSimsExporter.lua" );
				if (!File.Exists( charactersPath ))
				{
					continue;
				}
				try
				{
					var savedCharacters = SavedCharactersParser.ParseFromLuaFile( charactersPath );
					characters.AddRange( savedCharacters );
				}
				catch (Exception ex)
				{
					Console.WriteLine( $"Error parsing characters from {charactersPath}: {ex.Message}" );
				}
			}

			return characters.OrderBy( x => x.Realm ).ThenBy( x => x.Name ).ToList();
		}

		public string GetInstallLocation()
		{
			string installLocation = "";
			using RegistryKey key = Registry.LocalMachine.OpenSubKey( @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall" );
			if (key == null)
			{
				return installLocation;
			}

			foreach (var subkeyName in key.GetSubKeyNames())
			{
				using RegistryKey subkey = key.OpenSubKey( subkeyName );
				var name = subkey?.GetValue( "DisplayName" ) as string;
				if (string.Equals( name, WowDisplayName, StringComparison.OrdinalIgnoreCase ))
				{
					installLocation = subkey.GetValue( "InstallLocation" ) as string;
					break;
				}
			}

			if (!string.IsNullOrEmpty( installLocation ))
			{
				installLocation = $"{installLocation}\\_classic_";
			}

			return installLocation;
		}
	}
}
