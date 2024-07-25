using PathOfTerraria.Common.Systems.TreeSystem;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExpModPlayer : ModPlayer
{
	public int Level;
	public int QuestLevel;
	public int EffectiveLevel => Level + QuestLevel;

	public int Exp;

	public int NextLevel => Level == 100 ? 1 : Level * 250 + (int)(80 * Math.Pow(2, 1 + Level * 0.2f));

	public override void PreUpdate()
	{
		if (Exp <= NextLevel || Level >= 100)
		{
			return;
		}

		SoundEngine.PlaySound(new SoundStyle($"{nameof(PathOfTerraria)}/Sounds/Tier5"));

		Exp -= NextLevel;
		Level++;

		Main.NewText(Language.GetText("Mods.PathOfTerraria.Misc.Experience.LevelUp").WithFormatArgs(Level).Value, new Color(145, 255, 160));
		Main.NewText(Language.GetText("Mods.PathOfTerraria.Misc.Experience.SkillUp"), new Color(255, 255, 160));

		Player.GetModPlayer<TreePlayer>().Points++;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["level"] = Level;
		tag["questLevel"] = QuestLevel;
		tag["exp"] = Exp;
	}

	public override void LoadData(TagCompound tag)
	{
		Level = tag.GetInt("level");
		QuestLevel = tag.GetInt("questLevel");
		Exp = tag.GetInt("exp");
	}
}