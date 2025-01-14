﻿using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Amulets.AddedLife;

public class RootedAmulet : VigorAmulet
{
	public override List<ItemAffix> GenerateAffixes()
	{
		var strengthAffix = (ItemAffix)Affix.CreateAffix<StrengthItemAffix>(15f, 25f);
		var moveAffix = (ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(3, 5);
		var rootedAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyRootedGearAffix>(3, 5);

		return [strengthAffix, moveAffix, rootedAffix];
	}
}