using FunnyExperience.Core.Systems.TreeSystem;
using System.Linq;
using Terraria.ModLoader.IO;

namespace FunnyExperience.Core.Systems
{
	internal class ExpSystem : ModPlayer
	{
		public int level = 0;
		public int exp = 0;

		public int NextLevel => level * 250 + (int)(80 * Math.Pow(2, 1 + level * 0.2f));

		public override void PreUpdate()
		{
			if (exp > NextLevel)
			{
				exp -= NextLevel;
				level++;

				Main.NewText($"You've reached level {level}!", new Color(145, 255, 160));
				Main.NewText($"You have gained 1 skill point", new Color(255, 255, 160));

				Main.NewText($"Maximum life has increased by 5", new Color(255, 160, 160));
				Main.NewText($"Maximum mana has increased by 2", new Color(160, 160, 255));
				Main.NewText($"Power has increased by 1", new Color(255, 220, 160));

				Player.GetModPlayer<TreePlayer>().Points++;
			}
		}

		public override void UpdateEquips()
		{
			Player.statLifeMax2 += level * 5;
			Player.statManaMax2 += level * 2;
			Player.GetDamage(DamageClass.Generic) += level * 0.01f;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["level"] = level;
			tag["exp"] = exp;
		}

		public override void LoadData(TagCompound tag)
		{
			level = tag.GetInt("level");
			exp = tag.GetInt("exp");
		}
	}

	internal class KillExp : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			int amount = (int)Math.Max(1, npc.lifeMax * 0.25f);

			foreach (Player player in Main.player.Where(n => n.active && Vector2.DistanceSquared(n.Center, npc.Center) < Math.Pow(2000, 2)))
			{
				player.GetModPlayer<ExpSystem>().exp += amount;
				CombatText.NewText(player.Hitbox, new Color(145, 255, 160), $"+{amount}");
			}
		}
	}
}
