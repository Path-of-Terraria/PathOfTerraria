using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;


internal class IncreasedMinionDamagePassive : Passive
{
	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCWithProjEvent += BuffMinions;
	}

	private void BuffMinions(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		int level = player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(Name);
		
		if (proj.minion)
		{
			modifiers.FinalDamage += (Value/100.0f) * level;
		}
	}
}
