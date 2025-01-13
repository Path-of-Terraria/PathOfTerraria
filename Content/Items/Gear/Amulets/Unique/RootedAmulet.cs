using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Amulets.Unique;

public class RootedAmulet : Amulet
{
	public override List<ItemAffix> GenerateAffixes()
	{
		var lifeAffix = (ItemAffix)Affix.CreateAffix<FlatLifeAffix>(25, 35);
		var strengthAffix = (ItemAffix)Affix.CreateAffix<StrengthItemAffix>(15f, 25f);
		var moveAffix = (ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(3, 5);

		return [lifeAffix, strengthAffix, moveAffix];
	}
}