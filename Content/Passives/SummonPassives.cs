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
}

internal class IncreasedWhipSpeed : Passive
{
}

internal class WhipModsItem : GlobalItem
{
	public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
	{
		if (item.shoot > ProjectileID.None && ProjectileID.Sets.IsAWhip[item.shoot])
		{
			damage += player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(IncreasedWhipDamage)) * 0.05f;
		}
	}

	public override float UseSpeedMultiplier(Item item, Player player)
	{
		if (item.shoot > ProjectileID.None && ProjectileID.Sets.IsAWhip[item.shoot])
		{
			return 1 + player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(IncreasedWhipSpeed)) * 0.05f;
		}

		return 1;
	}
}