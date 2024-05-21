using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.MobSystem;
using PathOfTerraria.Core.Systems.TreeSystem;
using System.Linq;
using Terraria.Audio;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems
{
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class ExpSystem : ModPlayer
	{
		public int Level;
#pragma warning disable IDE1006 // Naming Styles
		public int exp;
#pragma warning restore IDE1006 // Naming Styles

		public int NextLevel => Level == 100 ? 1 : Level * 250 + (int)(80 * Math.Pow(2, 1 + Level * 0.2f));

		public override void PreUpdate()
		{
			if (exp <= NextLevel || Level >= 100)
				return;
			
			SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5"));

			exp -= NextLevel;
			Level++;

			Main.NewText($"You've reached level {Level}!", new Color(145, 255, 160));
			Main.NewText($"You have gained 1 skill point. Click the experience bar to open the skill tree.", new Color(255, 255, 160));

			Player.GetModPlayer<TreePlayer>().Points++;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["level"] = Level;
			tag["exp"] = exp;
		}

		public override void LoadData(TagCompound tag)
		{
			Level = tag.GetInt("level");
			exp = tag.GetInt("exp");
		}
	}

	internal class KillExp : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			int amount = (int)Math.Max(1, npc.lifeMax * 0.25f);
			
			MobRaritySpawnSystem npcSystem = npc.GetGlobalNPC<MobRaritySpawnSystem>();
			switch (npcSystem.Rarity) //We will need to evaluate this as magic/rare natively get more HP. So we do even want this? Was just POC, maybe just change amount evaluation?
			{
				case MobRarity.Rare:
					amount = Convert.ToInt32(amount * 1.1); //Rare mobs give 10% increase xp
					break;
				case MobRarity.Magic:
					amount = Convert.ToInt32(amount * 1.05); //Magic  mobs give 5% increase xp
					break;
			}

			foreach (Player player in Main.player.Where(n => n.active && Vector2.DistanceSquared(n.Center, npc.Center) < Math.Pow(2000, 2)))
			{
				player.GetModPlayer<ExpSystem>().exp += amount;
				CombatText.NewText(player.Hitbox, new Color(145, 255, 160), $"+{amount}");
			}
		}
	}
}
