using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMinionDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
	}
}

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
			modifiers.FinalDamage += level * 0.1f;
		}
	}
}

internal class IncreasedWhipDamage : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.MeleeNoSpeed) += Level * Value * 0.01f;
	}
}

internal class IncreasedWhipSpeed : Passive
{
	private class WhipSpeedItem : GlobalItem
	{
		public override float UseSpeedMultiplier(Item item, Player player)
		{
			return 1 + player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(IncreasedWhipSpeed)) * 0.05f;
		}
	}
}