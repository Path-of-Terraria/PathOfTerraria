using System.Collections.Generic;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class DwarvenGreatsword : Sword, GenerateName.IItem
{
	public int ItemLevel
	{
		get => 1;
		set => this.GetInstanceData().RealLevel = value; // Technically preserves previous behavior.
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
		staticData.Description = this.GetLocalization("Description");
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.rare = ItemRarityID.Red;
		Item.damage = 36;
		Item.width = 66;
		Item.height = 66;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.None;
		Item.useTime = Item.useAnimation = 34;
	}

	string GenerateName.IItem.GenerateName(string defaultName)
	{
		return Language.GetTextValue("Mods.PathOfTerraria.Items.DwarvenGreatsword.DisplayName");
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		var sharpAffix = (ItemAffix)Affix.CreateAffix<AddedDamageAffix>(0, 33, 33); // Add 33% damage
		var lifeAffix = (ItemAffix)Affix.CreateAffix<AddedLifeAffix>(0, 50, 50); // Add 50% life
		var kbAffix = (ItemAffix)Affix.CreateAffix<AddedKnockbackItemAffix>(0, 10, 10); // Add 10% kb
		var shredAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyArmorShredGearAffix>(0, 1, 1); // Add shred affix

		return [sharpAffix, lifeAffix, kbAffix, shredAffix];
	}

	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		// If cooldown is still active, do not allow alt usage.
		if (!modPlayer.AltFunctionAvailable)
		{
			return false;
		}

		// Otherwise, set the cooldown and allow alt usage.
		modPlayer.SetAltCooldown(1500);
		int projId = ModContent.ProjectileType<DwarvenGreatswordAltHammer>();
		Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, projId, 50, 10f, player.whoAmI);
		return true;
	}

	public override bool CanUseItem(Player player)
	{
		int projId = ModContent.ProjectileType<DwarvenGreatswordAltHammer>();
		return player.ownedProjectileCounts[projId] <= 0 && player.altFunctionUse == 0;
	}

	public class DwarvenGreatswordAltHammer : ModProjectile
	{
		Player Owner => Main.player[Projectile.owner];

		ref float Timer => ref Projectile.ai[0];

		private Vector2 off = Vector2.Zero;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 5;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.Size = new Vector2(42);
			Projectile.timeLeft = 600;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
		}

		public override bool? CanDamage()
		{
			return Main.mouseLeft;
		}

		public override void AI()
		{
			Timer += 1.25f;

			Owner.SetDummyItemTime(2);
			Owner.itemAnimation = 0;

			if (Projectile.timeLeft < 60f)
			{
				Projectile.Opacity = Projectile.timeLeft / 60f;
			}

			if (Main.myPlayer == Projectile.owner)
			{
				Projectile.hide = !Main.mouseLeft;

				off = Vector2.Lerp(off, Projectile.DirectionFrom(Main.MouseWorld) * 6, 0.2f);
				Projectile.Center = Owner.Center + off.RotatedBy(MathHelper.Pi);
				Projectile.rotation = off.ToRotation() - MathHelper.Pi + MathF.Sin(Timer * 0.2f) * MathHelper.PiOver2;
			}

			if (!Projectile.hide)
			{
				Owner.direction = Projectile.Center.X > Owner.Center.X ? 1 : -1;
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float throwaway = 0f;
			Vector2 end = Projectile.Center + new Vector2(70, 0).RotatedBy(Projectile.rotation);
			return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, end, 8, ref throwaway);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			var origin = new Vector2(0, tex.Height);

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; ++i)
			{
				Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
				Color col = lightColor * (1 - i / (ProjectileID.Sets.TrailCacheLength[Type] + 1f)) * Projectile.Opacity;
				Main.EntitySpriteDraw(tex, drawPos, null, col, Projectile.oldRot[i] + MathHelper.PiOver4, origin, 1f, SpriteEffects.None, 0);
			}

			Main.EntitySpriteDraw(tex, pos, null, lightColor * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver4, origin, 1f, SpriteEffects.None, 0);

			return false;
		}
	}
}