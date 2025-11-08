using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class SharpenedSpellsMastery : Passive
{
	internal class SharpenedSpellsPlayer : ModPlayer, ElementalPlayerHooks.IElementalOnHitPlayer
	{
		public void ElementalOnHitNPC(bool post, NPC target, ElementInstance ele, ElementalContainer con, ElementalContainer other, int damage, NPC.HitInfo hitInfo, Item item = null)
		{
			if (!post && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SharpenedSpellsMastery>(out float value) && target.HasBuff<FreezeDebuff>())
			{
				target.AddBuff(ModContent.BuffType<BrittleDebuff>(), target.buffTime[target.FindBuffIndex(ModContent.BuffType<FreezeDebuff>())]);
			}
		}
	}
}
