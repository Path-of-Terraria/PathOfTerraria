using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives;


internal class IncreasedSentryDamagePassive : Passive
{
	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCWithProjEvent += BuffSentries;
	}

	private void BuffSentries(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		int level = player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(Name);

		if (proj.sentry || ProjectileID.Sets.SentryShot[proj.type])
		{
			modifiers.FinalDamage += level * (Value/100.0f);
		}
	}
}