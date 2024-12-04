using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Experience;

namespace PathOfTerraria.Common.Systems.MobSystem;

public class MobExperienceGlobalNPC : GlobalNPC
{
	/// <summary>
	/// Handles the experience gained from killing a mob.
	/// </summary>
	/// <param name="npc"></param>
	public override void OnKill(NPC npc)
	{
		if (npc.friendly || npc.CountsAsACritter)
		{
			return;
		}
		
		MobAprgSystem npcSystem = npc.GetGlobalNPC<MobAprgSystem>();

		if (npcSystem is null)
		{
			return;
		}

		if (npcSystem.Experience == null)
		{
			Main.NewText($"No experience entry for {npc.TypeName} - {npc.netID}");
		}
		
		int amount = npcSystem.Experience ?? (int)Math.Max(1, npc.lifeMax * 0.25f);
		amount = npcSystem.Rarity
			switch //We will need to evaluate this as magic/rare natively get more HP. So we do even want this? Was just POC, maybe just change amount evaluation?
			{
				ItemRarity.Rare => Convert.ToInt32(amount * 1.1), //Rare mobs give 10% increase xp
				ItemRarity.Magic => Convert.ToInt32(amount * 1.05), //Magic mobs give 5% increase xp
				_ => amount
			};
		
		foreach (Player player in Main.ActivePlayers)
		{
			if (Vector2.DistanceSquared(player.Center, npc.Center) > 2000 * 2000)
			{
				continue;
			}

			ExperienceTracker.SpawnExperience(amount, npc.Center, Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 6f, player.whoAmI);
		}
	}
}