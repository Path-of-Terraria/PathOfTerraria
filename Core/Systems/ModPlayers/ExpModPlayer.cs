using PathOfTerraria.Core.Systems.TreeSystem;
using Terraria.Audio;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.ModPlayers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ExpModPlayer : ModPlayer
{
	public int Level;
	public int Exp;

	public int NextLevel => Level == 100 ? 1 : Level * 250 + (int)(80 * Math.Pow(2, 1 + Level * 0.2f));

	public override void PreUpdate()
	{
		if (Exp <= NextLevel || Level >= 100)
		{
			return;
		}

		SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5"));

		Exp -= NextLevel;
		Level++;

		Main.NewText($"You've reached level {Level}!", new Color(145, 255, 160));
		Main.NewText($"You have gained 1 skill point. Click the experience bar to open the skill tree.",
			new Color(255, 255, 160));

		Player.GetModPlayer<TreePlayer>().Points++;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["level"] = Level;
		tag["exp"] = Exp;
	}

	public override void LoadData(TagCompound tag)
	{
		Level = tag.GetInt("level");
		Exp = tag.GetInt("exp");
	}
}