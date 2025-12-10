using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MeleeSizePassive : Passive
{
	private class MeleeSizePlayer : ModPlayer
	{
		public override void ModifyItemScale(Item item, ref float scale)
		{
			scale += Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<MeleeSizePassive>() / 100f;
		}
	}
}
