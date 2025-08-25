using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Consumables.Maps.ExplorableMaps;

public abstract class ExplorableMap : Map
{
	public override void OpenMap()
	{
		List<MapAffix> collection = [.. this.GetInstanceData().Affixes.Where(x => x is MapAffix).Select(x => (MapAffix)x)];

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			MappingWorld.AreaLevel = WorldTier;
			MappingWorld.MapTier = Tier;
			MappingWorld.Affixes = [];
			MappingWorld.Affixes.AddRange(collection);
		}
		else
		{
			ModContent.GetInstance<SendMappingDomainInfoHandler>().Send((short)WorldTier, (short)Tier, collection);
		}

		OpenMapInternal();
	}
}