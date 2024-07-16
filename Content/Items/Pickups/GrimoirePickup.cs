using PathOfTerraria.Content.GUI.GrimoireSelection;
using PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.ModPlayers;
using ReLogic.Content;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Pickups;

internal abstract class GrimoirePickup : PoTItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Pickups/GrimoirePickups/{GetType().Name}";

	/// <summary>
	/// These materials shouldn't drop through the typical <see cref="PoTItem"/> system. These will manually add their drops to their respective NPC(s).
	/// </summary>
	public sealed override float DropChance => 20;

	public abstract Point Size { get; }

	public override void Defaults()
	{
		Item.width = Size.X;
		Item.height = Size.Y;

		ItemType = ItemType.Weapon;
	}

	public override bool ItemSpace(Player player)
	{
		return true;
	}

	public override bool OnPickup(Player player)
	{
		player.GetModPlayer<GrimoireStoragePlayer>().Storage.Add(Item);
		int projType = ModContent.ProjectileType<GrimoireVisageEffect>();
		
		if (player.ownedProjectileCounts[projType] <= 0)
		{
			Projectile.NewProjectile(player.GetSource_FromAI(), player.Top - Vector2.UnitY * 40, Vector2.Zero, projType, 0, 0, player.whoAmI);
		}

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