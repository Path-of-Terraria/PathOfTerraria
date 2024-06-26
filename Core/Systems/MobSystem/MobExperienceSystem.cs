using PathOfTerraria.Core.Systems.Experience;
using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.MobSystem;

public class MobExperienceSystem : GlobalNPC
{
	/// <summary>
	/// Handles the experience gained from killing a mob
	/// </summary>
	/// <param name="npc"></param>
	public override void OnKill(NPC npc)
	{
		MobAprgSystem npcSystem = npc.GetGlobalNPC<MobAprgSystem>();
		if (npcSystem.Experience == null)
		{
			Main.NewText($"No experience entry for {npc.FullName} - {npc.netID}");
		}
		
		int amount = npcSystem.Experience ?? (int)Math.Max(1, npc.lifeMax * 0.25f);
		amount =
			npcSystem.Rarity
				switch //We will need to evaluate this as magic/rare natively get more HP. So we do even want this? Was just POC, maybe just change amount evaluation?
				{
					Rarity.Rare => Convert.ToInt32(amount * 1.1) //Rare mobs give 10% increase xp
					,
					Rarity.Magic => Convert.ToInt32(amount * 1.05) //Magic mobs give 5% increase xp
					,
					_ => amount
				};
		
		foreach (Player player in Main.player.Where(n =>
			         n.active && Vector2.DistanceSquared(n.Center, npc.Center) < Math.Pow(2000, 2)))
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				ExperienceTracker.SpawnExperience(amount, npc.Center, 6f, player.whoAmI);
			}
			else
			{
				Networking.Networking.SendSpawnExperienceOrbs(-1, player.whoAmI, amount, npc.Center, 6f);
			}
		}
	}
}