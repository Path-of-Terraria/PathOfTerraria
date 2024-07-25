using PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;
using PathOfTerraria.Core;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Loaders.UILoading;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.GrimoireSelection;
using Terraria.ID;
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
		Item.maxStack = 1;

		ItemType = ItemType.Weapon;
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

		if (storage.Count(x => x.type == Type) >= 5)
		{
			spawnText = Language.GetText("Mods.PathOfTerraria.Misc.GrimoireBoon").WithFormatArgs(Item.Name).Value;
			textColor = Color.Silver;

			Item.SetDefaults(ItemID.SilverCoin);
			Item.stack = 20;
		}

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

		if (Item.type != ItemID.SilverCoin && player.whoAmI == Main.myPlayer && UILoader.GetUIState<GrimoireSelectionUIState>().IsVisible)
		{
			GrimoireSelectionUIState.RefreshStorage();
		}

		return Item.type == ItemID.SilverCoin;
	}

	public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		if (Affixes.Count > 0)
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