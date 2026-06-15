using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class BiggerExplosivesPassive : Passive
{
	private class MeleeAreaSizePlayer : ModPlayer
	{
		public override void ModifyItemScale(Item item, ref float scale)
		{
			if (!item.DamageType.CountsAsClass(DamageClass.Melee))
			{
				return;
			}

			float passiveValue = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<BiggerExplosivesPassive>();
			scale += passiveValue / 100f;
		}
	}

	public override void BuffPlayer(Player player)
	{
		float areaBonus = Value / 100f;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ExplosionSize += areaBonus;
		player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.AreaOfEffect += areaBonus;
	}
}