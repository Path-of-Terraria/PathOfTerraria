using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class KingSlimeMap : Map
{
	public override int MaxUses
	{
		get
		{
			int def = 6 / BossDomainPlayer.GetLivesPerPlayer();

			if (Main.CurrentFrameFlags.ActivePlayersCount > 6)
			{
				def = Main.CurrentFrameFlags.ActivePlayersCount;
			}

			return 2;// def;
		}
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(44, 36);
	}

	public override void OpenMap()
	{
		SubworldSystem.Enter<KingSlimeDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}