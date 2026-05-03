using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class SniperMastery : Passive
{
	internal class SniperPlayer : ModPlayer
	{
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SniperMastery>(out float value))
				return;
			if (target.DistanceSQ(Player.Center) > PoTMod.NearbyDistanceSq && modifiers.DamageType.CountsAsClass(DamageClass.Ranged))
			{
				modifiers.SourceDamage *= Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<SniperMastery>() / 100f;
			}
		}
	}
}