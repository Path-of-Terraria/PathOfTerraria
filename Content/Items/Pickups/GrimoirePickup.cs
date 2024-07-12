using PathOfTerraria.Content.GUI.GrimoireSelection;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.ModPlayers;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Pickups;

internal abstract class GrimoirePickup : PoTItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Pickups/GrimoirePickups/{GetType().Name}";

	/// <summary>
	/// These materials shouldn't drop through the typical <see cref="PoTItem"/> system. These will manually add their drops to their respective NPC(s).
	/// </summary>
	public sealed override float DropChance => 0;

	public abstract Point Size { get; }

	public override void Defaults()
	{
		Item.width = Size.X;
		Item.height = Size.Y;
	}

	public override bool ItemSpace(Player player)
	{
		return true;
	}

	public override bool OnPickup(Player player)
	{
		player.GetModPlayer<GrimoireStoragePlayer>().Storage.Add(Item);

		var request = new AdvancedPopupRequest
		{
			Text = Language.GetText("Mods.PathOfTerraria.Misc.GrimoireConsume").WithFormatArgs(Item.Name).Value,
			DurationInFrames = 60,
			Velocity = Vector2.UnitY * -10,
			Color = Color.IndianRed
		};

		PopupText.NewText(request, player.Center);

		if (player.whoAmI == Main.myPlayer && UILoader.GetUIState<GrimoireSelectionUIState>().IsVisible)
		{
			GrimoireSelectionUIState.RefreshStorage();
		}

		return false;
	}

	public abstract void AddDrops(NPC npc, ref NPCLoot loot);
}

internal class GrimoirePickupLoader : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		for (int i = 0; i < ItemLoader.ItemCount; ++i)
		{
			var item = new Item(i);

			if (item.ModItem is GrimoirePickup grim)
			{
				grim.AddDrops(npc, ref npcLoot);
			}
		}
	}
}