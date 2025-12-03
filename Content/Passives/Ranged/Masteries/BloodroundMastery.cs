using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class BloodroundMastery : Passive
{
	internal class BloodroundPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life <= 0 && (target.HasBuff<BleedDebuff>() || target.HasBuff<IgnitedDebuff>() || target.HasBuff(BuffID.OnFire) || target.HasBuff(BuffID.OnFire3)) 
				&& Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BloodroundMastery>(out float value))
			{
				Player.AddBuff(ModContent.BuffType<BloodroundBuff>(), (int)(value * 60));
			}
		}
	}
}