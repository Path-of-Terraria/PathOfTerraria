using Terraria.Localization;

namespace PathOfTerraria.Common.ModCompatibility;

internal class MusicDisplayCompat : ModSystem
{
	public override void PostSetupContent()
	{
		if (!ModLoader.TryGetMod("MusicDisplay", out Mod display))
		{
			return;
		}

		LocalizedText modName = Language.GetText("Mods.PathOfTerraria.MusicDisplay.ModName");

		void AddMusic(string name)
		{
			LocalizedText author = Language.GetText("Mods.PathOfTerraria.MusicDisplay." + name + ".Author");
			LocalizedText displayName = Language.GetText("Mods.PathOfTerraria.MusicDisplay." + name + ".DisplayName");
			short music = (short)MusicLoader.GetMusicSlot(Mod, "Assets/Music/" + name);
			display.Call("AddMusic", music, displayName, author, modName);
		}

		AddMusic("SunDevourer");
	}
}
