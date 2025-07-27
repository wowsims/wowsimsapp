using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WoWSimsApp.Characters;

namespace WoWSimsApp
{
	internal static class SimData
	{
		public struct ClassData
		{
			public readonly string Name;
			public readonly Bitmap Icon;
			public readonly List<string> Specs;
			public readonly List<Bitmap> SpecIcons;

			public ClassData( string name, Bitmap icon, List<string> specs, List<Bitmap> specIcons )
			{
				Name = name;
				Icon = icon;
				Specs = specs;
				SpecIcons = specIcons;
			}
		}

		public static Bitmap GetSpecIcon( string className, string specName )
		{
			var classData = Classes.Find( c => RemoveWhiteSpace( c.Name ).Equals( className, System.StringComparison.CurrentCultureIgnoreCase ) );
			var foundSpec = classData.Specs.FindIndex( s => RemoveWhiteSpace( s ).Equals( specName, System.StringComparison.CurrentCultureIgnoreCase ) );
			if (foundSpec > -1)
			{
				return classData.SpecIcons[foundSpec];
			}
			return null; // or a default icon if needed
		}

		public static string RemoveWhiteSpace( string input )
		{
			return new string( input.Where( c => !char.IsWhiteSpace( c ) ).ToArray() );
		}

		public static string CharacterToUrlCombo( SavedCharacter character )
		{
			var result = string.Empty;

			result += character.Class switch
			{
				"deathknight" => "death_knight",
				_ => character.Class,
			};

			result += "/";

			result += character.Spec switch
			{
				"beastmastery" => "beast_mastery",
				_ => character.Spec,
			};

			return result.ToLowerInvariant();
		}

		public static readonly List<ClassData> Classes = new List<ClassData>
		{
			new ClassData(
				"Death Knight",
				Properties.Resources.DeathKnight, // class icon
				new List<string> { "Blood", "Frost", "Unholy" },
				new List<Bitmap> { Properties.Resources.DeathKnightBlood, Properties.Resources.DeathKnightFrost, Properties.Resources.DeathKnightUnholy }
			),
			new ClassData(
				"Druid",
				Properties.Resources.Druid,
				new List<string> { "Balance", "Feral", "Guardian", "Restoration" },
				new List<Bitmap> { Properties.Resources.DruidBalance, Properties.Resources.DruidFeral, Properties.Resources.DruidGuardian, Properties.Resources.DruidRestoration }
			),
			new ClassData(
				"Hunter",
				Properties.Resources.Hunter,
				new List<string> { "Beast Mastery", "Marksmanship", "Survival" },
				new List<Bitmap> { Properties.Resources.HunterBeastMastery, Properties.Resources.HunterMarksmanship, Properties.Resources.HunterSurvival }
			),
			new ClassData(
				"Mage",
				Properties.Resources.Mage,
				new List<string> { "Arcane", "Fire", "Frost" },
				new List<Bitmap> { Properties.Resources.MageArcane, Properties.Resources.MageFire, Properties.Resources.MageFrost }
			),
			new ClassData(
				"Monk",
				Properties.Resources.Monk,
				new List<string> { "Brewmaster", "Mistweaver", "Windwalker" },
				new List<Bitmap> { Properties.Resources.MonkBrewmaster, Properties.Resources.MonkMistweaver, Properties.Resources.MonkWindwalker }
			),
			new ClassData(
				"Paladin",
				Properties.Resources.Paladin,
				new List<string> { "Holy", "Protection", "Retribution" },
				new List<Bitmap> { Properties.Resources.PaladinHoly, Properties.Resources.PaladinProtection, Properties.Resources.PaladinRetribution }
			),
			new ClassData(
				"Priest",
				Properties.Resources.Priest,
				new List<string> { "Discipline", "Holy", "Shadow" },
				new List<Bitmap> { Properties.Resources.PriestDiscipline, Properties.Resources.PriestHoly, Properties.Resources.PriestShadow }
			),
			new ClassData(
				"Rogue",
				Properties.Resources.Rogue,
				new List<string> { "Assassination", "Combat", "Subtlety" },
				new List<Bitmap> { Properties.Resources.RogueAssassination, Properties.Resources.RogueCombat, Properties.Resources.RogueSubtlety }
			),
			new ClassData(
				"Shaman",
				Properties.Resources.Shaman,
				new List<string> { "Elemental", "Enhancement", "Restoration" },
				new List<Bitmap> { Properties.Resources.ShamanElemental, Properties.Resources.ShamanEnhancement, Properties.Resources.ShamanRestoration }
			),
			new ClassData(
				"Warlock",
				Properties.Resources.Warlock,
				new List<string> { "Affliction", "Demonology", "Destruction" },
				new List<Bitmap> { Properties.Resources.WarlockAffliction, Properties.Resources.WarlockDemonology, Properties.Resources.WarlockDestruction }
			),
			new ClassData(
				"Warrior",
				Properties.Resources.Warrior,
				new List<string> { "Arms", "Fury", "Protection" },
				new List<Bitmap> { Properties.Resources.WarriorArms, Properties.Resources.WarriorFury, Properties.Resources.WarriorProtection }
			)
		};
	}
}
