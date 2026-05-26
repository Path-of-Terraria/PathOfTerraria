using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExpModPlayer : ModPlayer
{
	public int Level;
	public int QuestLevel;
	public int EffectiveLevel => Level + QuestLevel;

	private int _exp;

	public int Exp
	{
		get => _exp;
		set
		{
			int gained = value - _exp;
			_exp = value;

			if (gained > 0 && ShouldDisplayExperienceGain() && SmartUiLoader.TryGetUiState(out ExpBar expBar))
			{
				expBar.AddExperienceGain(gained);
			}
		}
	}

	public int NextLevel
	{
		get
		{
			if (Level == 100)
			{
				return 1;
			}

			int baseXp = Level * 250 + (int)(80 * Math.Pow(2, 1 + Level * 0.2f));

			if (Level <= 10)
			{
				float progress = (Level - 1) / 9f;
				float multiplier = MathHelper.Lerp(0.6f, 0.95f, progress);
				return (int)(baseXp * multiplier);
			}

			return baseXp;
		}
	}

	public static void GrantExperience(Player player, int amount)
	{
		if (amount <= 0 || !player.active)
		{
			return;
		}

		player.GetModPlayer<ExpModPlayer>().Exp += amount;

		if (Main.netMode == NetmodeID.Server)
		{
			ExperienceHandler.Send((byte)player.whoAmI, amount);
		}
	}

	public override void PreUpdate()
	{
		if (_exp <= NextLevel || Level >= 100)
		{
			return;
		}

		_exp -= NextLevel;
		Level++;

		RemoteInfoPlayer.SendRemoteInfoHandler.Send();

		if (Main.myPlayer == Player.whoAmI && !Main.dedServ) //Only use level up text and sounds on the local client, despite progress being otherwise synced
		{
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/Tier5"));

			Main.NewText(Language.GetText("Mods.PathOfTerraria.Misc.Experience.LevelUp").WithFormatArgs(Level).Value, new Color(145, 255, 160));
			Main.NewText(Language.GetText("Mods.PathOfTerraria.Misc.Experience.SkillUp"), new Color(255, 255, 160));
		}

		Player.GetModPlayer<PassiveTreePlayer>().Points++;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["level"] = Level;
		tag["questLevel"] = QuestLevel;
		tag["exp"] = _exp;
	}

	public override void LoadData(TagCompound tag)
	{
		Level = tag.GetInt("level");
		QuestLevel = tag.GetInt("questLevel");
		_exp = tag.GetInt("exp");
	}

	private bool ShouldDisplayExperienceGain()
	{
		return !Main.dedServ && Main.myPlayer == Player.whoAmI && Player.active;
	}
}
