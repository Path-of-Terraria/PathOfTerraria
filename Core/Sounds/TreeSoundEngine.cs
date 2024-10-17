using Terraria.Audio;

namespace PathOfTerraria.Core.Sounds;

public class TreeSoundEngine
{
	public static void PlaySoundForTreeAllocation(int maxLevel, int level)
	{
		switch (maxLevel)
		{
			case 1:
				switch (level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
				}

				return;
			case 2:
				switch (level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier2")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
				}

				return;
			case 3:
				switch (level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier3")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
				}

				return;
			case 5:
				switch (level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
				}

				return;
			case 6:
				switch (level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
					case 6: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier1")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
				}

				return;
			case 7:
				switch (level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
					case 6: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier2")); break;
					case 7: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier1")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
				}

				return;
			default:
				switch (level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5")); break;
				}

				break;
		}
	}
}