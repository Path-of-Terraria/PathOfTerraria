using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.Experience;
using PathOfTerraria.Core.Systems.MobSystem;
using PathOfTerraria.Core.Systems.TreeSystem;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
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
			return;

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

internal class KillExp : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		int amount = (int)Math.Max(1, npc.lifeMax * 0.25f);

		MobRaritySpawnSystem npcSystem = npc.GetGlobalNPC<MobRaritySpawnSystem>();
		amount =
			npcSystem.Rarity
				switch //We will need to evaluate this as magic/rare natively get more HP. So we do even want this? Was just POC, maybe just change amount evaluation?
				{
					MobRarity.Rare => Convert.ToInt32(amount * 1.1) //Rare mobs give 10% increase xp
					,
					MobRarity.Magic => Convert.ToInt32(amount * 1.05) //Magic mobs give 5% increase xp
					,
					_ => amount
				};

		foreach (Player player in Main.player.Where(n =>
			         n.active && Vector2.DistanceSquared(n.Center, npc.Center) < Math.Pow(2000, 2)))
		{
			if(Main.netMode == NetmodeID.SinglePlayer)
				ExperienceTracker.SpawnExperience(amount, npc.Center, 6f, player.whoAmI);
			else
				Networking.Networking.SendSpawnExperienceOrbs(-1, player.whoAmI, amount, npc.Center, 6f);
		}
	}
}