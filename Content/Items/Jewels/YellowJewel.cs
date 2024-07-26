
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.TreeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Jewels;
internal class YellowJewel : Jewel
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.GoldCoin}";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}
}
