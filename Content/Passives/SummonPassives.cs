using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.TreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class MinionPassive : Passive
{
	public override string InternalIdentifier => "IncreasedMinionDamage";
	public override string Name => "Empowered Horde";
	public override string Tooltip => "Increases your minions' damage by 10% per level";
}

internal class SentryPassive : Passive
{
	public override string InternalIdentifier => "IncreasedSentryDamage";
	public override string Name => "Steadfast Sentries";
	public override string Tooltip => "Increases your sentries' damage by 10% per level";

	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCWithProjEvent += BuffSentries;
	}

	private void BuffSentries(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		int level = player.GetModPlayer<TreePlayer>().GetCumulativeLevel(InternalIdentifier);

		if (proj.sentry || ProjectileID.Sets.SentryShot[proj.type])
		{
			modifiers.FinalDamage += level * 0.1f;
		}
	}
}