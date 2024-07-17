using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal abstract class Javelin : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Javelins/{GetType().Name}";
	public override float DropChance => 1f;

	/// <summary>
	/// Used to define the size of the item and associated projectile in load time.
	/// </summary>
	public abstract Vector2 ItemSize { get; }

	protected override string GearLocalizationCategory => "Javelin";

	public override void Load()
	{
		Mod.AddContent(new JavelinThrown(GetType().Name + "Thrown", ItemSize));
	}

	public override void Defaults()
	{
		Item.DefaultToThrownWeapon(Mod.Find<ModProjectile>(GetType().Name + "Thrown").Type, 50, 10, true);
		Item.consumable = false;
		Item.SetWeaponValues(3, 1);
		Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 0, 1, 0));
		Item.noUseGraphic = true;
	}

	private class JavelinThrown(string name, Vector2 itemSize) : ModProjectile
	{
		protected override bool CloneNewInstances => true;
		public override string Name => name;
		public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Javelins/{name.Replace("Thrown", "")}";

		private string name = name;
		private Vector2 itemSize = itemSize;

		public override ModProjectile Clone(Projectile newEntity)
		{
			var proj = base.Clone(newEntity) as JavelinThrown;
			proj.name = name;
			proj.itemSize = itemSize;
			return proj;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.JavelinFriendly);
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			Vector2 tip = itemSize.RotatedBy(Projectile.velocity.ToRotation()) / 2f;
			hitbox.Location = new Point(hitbox.Location.X + (int)tip.X, hitbox.Location.Y + (int)tip.Y);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Color color = lightColor * Projectile.Opacity;
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);

			return false;
		}
	}
}