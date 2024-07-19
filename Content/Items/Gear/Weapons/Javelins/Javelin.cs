using PathOfTerraria.Core.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal abstract class Javelin : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Javelins/{GetType().Name}";
	public override string AltUseDescription => Language.GetTextValue("Mods.PathOfTerraria.Gear.Javelin.AltUse");
	public override float DropChance => 1f;

	/// <summary>
	/// Used to define the size of the item and associated projectile in load time.
	/// </summary>
	public abstract Vector2 ItemSize { get; }

	/// <summary>
	/// Used to define the ID of the dust used for the associated projectile's death fx.
	/// </summary>
	public abstract int DeathDustType { get; }

	protected override string GearLocalizationCategory => "Javelin";

	public override void Load()
	{
		Mod.AddContent(new JavelinThrown(GetType().Name + "Thrown", ItemSize, DeathDustType));
	}

	public override void Defaults()
	{
		Item.DefaultToThrownWeapon(Mod.Find<ModProjectile>(GetType().Name + "Thrown").Type, 50, 7, true);
		Item.consumable = false;
		Item.SetWeaponValues(3, 1);
		Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 0, 1, 0));
		Item.noUseGraphic = true;
		Item.Size = new(12);
		Item.useAmmo = AmmoID.None;
		Item.UseSound = SoundID.Item1;
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
			player.GetModPlayer<JavelinDashPlayer>().StoredVelocity = player.DirectionTo(Main.MouseWorld) * 15;
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

		public override void PostUpdateRunSpeeds()
		{
			AltUsePlayer altUsePlayer = Player.GetModPlayer<AltUsePlayer>();

			if (!altUsePlayer.AltFunctionActive)
			{
				return;
			}

			Player.maxFallSpeed = StoredVelocity.Y;
		}

		public override void PreUpdateMovement()
		{
			AltUsePlayer altUsePlayer = Player.GetModPlayer<AltUsePlayer>();

			if (!altUsePlayer.AltFunctionActive)
			{
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
				}
			}
		}
	}
}
