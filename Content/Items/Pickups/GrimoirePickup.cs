using PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.GrimoireSelection;
using Terraria.ID;
using Terraria.Localization;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Items.Pickups;

internal abstract class GrimoirePickup : ModItem, IPoTGlobalItem
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Pickups/GrimoirePickups/{GetType().Name}";

	public abstract Point Size { get; }

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		// These materials shouldn't drop through the typical item system. These will manually add their drops to their respective NPC(s).
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = Size.X;
		Item.height = Size.Y;
		Item.maxStack = 1;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Grimoire;
	}

	public override void OnSpawn(IEntitySource source)
	{
		base.OnSpawn(source);

		Item.GetInstanceData().Rarity = Main.rand.NextFloat() switch
		{
			> 0.3f => ItemRarity.Normal,
			> 0.05f => ItemRarity.Magic,
			_ => ItemRarity.Rare
		};

		// TODO: Make sure this works in Multiplayer
		PoTItemHelper.Roll(Item, PoTItemHelper.PickItemLevel());
	}

	public override bool ItemSpace(Player player)
	{
		return true;
	}

	public override bool OnPickup(Player player)
	{
		List<Item> storage = player.GetModPlayer<GrimoireStoragePlayer>().Storage;
		string spawnText = Language.GetText("Mods.PathOfTerraria.Misc.GrimoireConsume").WithFormatArgs(Item.Name).Value;
		Color textColor = Color.IndianRed;
		storage.Add(Item);
		int projType = ModContent.ProjectileType<GrimoireVisageEffect>();
		
		if (player.ownedProjectileCounts[projType] <= 0)
		{
			Projectile.NewProjectile(player.GetSource_FromAI(), player.Top - Vector2.UnitY * 40, Vector2.Zero, projType, 0, 0, player.whoAmI);
		}

		var request = new AdvancedPopupRequest
		{
			Text = spawnText,
			DurationInFrames = 60,
			Velocity = Vector2.UnitY * -10,
			Color = textColor
		};

		PopupText.NewText(request, player.Center);

		if (Item.type != ItemID.SilverCoin && player.whoAmI == Main.myPlayer && SmartUiLoader.GetUiState<GrimoireSelectionUIState>().IsVisible)
		{
			GrimoireSelectionUIState.RefreshStorage();
		}

		return Item.type == ItemID.SilverCoin;
	}

	public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		if (this.GetInstanceData().Affixes.Count > 0)
		{
			spriteBatch.Draw(GrimoirePickupLoader.AffixIconTex.Value, position - origin * 0.9f, Color.White);
		}
	}

	public abstract void AddDrops(NPC npc, ref NPCLoot loot);
}

internal class GrimoirePickupLoader : GlobalNPC
{
	public static Asset<Texture2D> AffixIconTex = null;

	public override void Load()
	{
		AffixIconTex = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Projectiles/Summoner/GrimoireSummons/AffixIcon");
	}

	public override void Unload()
	{
		AffixIconTex = null;
	}

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