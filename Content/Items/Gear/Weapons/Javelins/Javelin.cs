using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Content.Projectiles.Ranged.Javelin;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal abstract class Javelin : Gear
{
	/// <summary>
	/// Used to define the size of the item and associated projectile in load time.
	/// </summary>
	public abstract Vector2 ItemSize { get; }

	/// <summary>
	/// Used to define the ID of the dust used for the associated projectile's death fx.
	/// </summary>
	public abstract int DeathDustType { get; }

	public virtual bool AutoloadProjectile => true;
	public virtual bool UseChargeAlt => true;

	protected override string GearLocalizationCategory => "Javelin";

	public override void Load()
	{
		if (AutoloadProjectile)
		{
			Mod.AddContent(new JavelinThrown(GetType().Name + "Thrown", ItemSize, DeathDustType));
		}
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetText("Mods.PathOfTerraria.Gear.Javelin.AltUse");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		// Default to ThrowingKnife to placehold until the manually loaded projectile is added
		int shotId = AutoloadProjectile ? Mod.Find<ModProjectile>(GetType().Name + "Thrown").Type : ProjectileID.ThrowingKnife;
		Item.DefaultToThrownWeapon(shotId, 50, 7, true);

		Item.maxStack = 1;
		Item.consumable = false;
		Item.SetWeaponValues(3, 1);
		Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 0, 1, 0));
		Item.noUseGraphic = true;
		Item.Size = new(12);
		Item.useAmmo = AmmoID.None;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Javelin;
	}

	public override bool CanUseItem(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();

		if (player.altFunctionUse == 2 && altUsePlayer.OnCooldown)
		{
			return false;
		}

		if (player.altFunctionUse == 2)
		{
			altUsePlayer.SetAltCooldown(4 * 60, 15);

			if (UseChargeAlt)
			{
				player.GetModPlayer<JavelinDashPlayer>().StoredVelocity = player.DirectionTo(Main.MouseWorld) * 15;
				player.GetModPlayer<JavelinDashPlayer>().JavelinAltUsed = true;
			}
		}

		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse == 2)
		{
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, 0, 1);
			return false;
		}

		return true;
	}

	public class JavelinDashPlayer : ModPlayer
	{
		public Vector2 StoredVelocity = Vector2.Zero;
		public bool JavelinAltUsed = false;

		public override void PostUpdateRunSpeeds()
		{
			if (!JavelinAltUsed)
			{
				return;
			}

			AltUsePlayer altUsePlayer = Player.GetModPlayer<AltUsePlayer>();

			if (!altUsePlayer.AltFunctionActive)
			{
				return;
			}

			Player.maxFallSpeed = StoredVelocity.Y;
		}

		public override void PreUpdateMovement()
		{
			if (!JavelinAltUsed)
			{
				return;
			}

			AltUsePlayer altUsePlayer = Player.GetModPlayer<AltUsePlayer>();

			if (!altUsePlayer.AltFunctionActive || Player.HeldItem.ModItem is not Javelin javelin || !javelin.UseChargeAlt)
			{
				JavelinAltUsed = false;
				return;
			}

			Player.SetDummyItemTime(2);
			Player.SetImmuneTimeForAllTypes(2);
			Player.maxFallSpeed = StoredVelocity.Y;
			Player.velocity = StoredVelocity;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.Hitbox.Intersects(Player.Hitbox))
				{
					npc.SimpleStrikeNPC((int)(Player.HeldItem.damage * 1.5f), Math.Sign(StoredVelocity.X), true);

					altUsePlayer.SetAltCooldown(altUsePlayer.AltFunctionCooldown, 0);
					Player.velocity = -StoredVelocity;
					JavelinAltUsed = true;
				}
			}
		}
	}
}
