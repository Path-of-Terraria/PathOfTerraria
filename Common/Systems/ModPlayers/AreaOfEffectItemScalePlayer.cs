using PathOfTerraria.Common.Items;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class AreaOfEffectItemScalePlayer : ModPlayer
{
	public override void ModifyItemScale(Item item, ref float scale)
	{
		if (!CustomItemSets.AreaOfEffectWeapons[item.type])
		{
			return;
		}

		float areaScale = Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.AreaOfEffect.ApplyTo(1f);
		scale *= areaScale;
	}
}
