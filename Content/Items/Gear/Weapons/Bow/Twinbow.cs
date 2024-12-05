using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class Twinbow : Bow
{
	public static Asset<Texture2D> ShineTex = null;

	protected override int AnimationSpeed => 8;
	protected override float CooldownTimeInSeconds => 2;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ShineTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Bow/TwinbowArrowShine");

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.Description = this.GetLocalization("Description");
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");

		BowAnimationProjectile.OverridenShootActionsByItemId[Type] = static proj =>
		{
			Player owner = Main.player[proj.owner];

			if (owner.whoAmI != Main.myPlayer || owner.HeldItem.ModItem is not Bow bow)
			{
				return;
			}

			SoundEngine.PlaySound(SoundID.Item5, proj.Center);
			owner.PickAmmo(owner.HeldItem, out int type, out float speed, out int damage, out float kB, out int ammoUsed);
			owner.GetModPlayer<AltUsePlayer>().SetAltCooldown((int)(proj.ai[1] * 60f));

			Vector2 vel = proj.DirectionTo(Main.MouseWorld) * speed * 1.5f;
			IEntitySource src = owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, ammoUsed);

			for (int i = 0; i < 6; ++i)
			{
				Vector2 velocity = vel.RotatedByRandom(0.15f) * Main.rand.NextFloat(1f, 1.1f);
				int projIndex = Projectile.NewProjectile(src, proj.Center, velocity, ProjectileID.WoodenArrowFriendly, damage, kB, owner.whoAmI);
				Main.projectile[projIndex].GetGlobalProjectile<TwinbowArrow>().PiercingArrow = true;
			}
		};
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.useTime = 24;
		Item.useAnimation = 24;
		Item.width = 18;
		Item.height = 30;
		Item.damage = 20;
		Item.autoReuse = true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2)
		{
			SoundEngine.PlaySound(SoundID.Item5, player.Center); // Play sound here to not make it play twice when alt firing

			for (int i = 0; i < 2; ++i)
			{
				Vector2 modVel = velocity.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.9f, 1.2f);
				int proj = Projectile.NewProjectile(source, position, modVel, type, damage, knockback, player.whoAmI);
				Main.projectile[proj].GetGlobalProjectile<TwinbowArrow>().PiercingArrow = true;
			}
		}

		return false;
	}

	public class TwinbowArrow : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool PiercingArrow = false;

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (PiercingArrow)
			{
				modifiers.ScalingArmorPenetration += 0.4f;
			}
		}

		public override void PostDraw(Projectile projectile, Color lightColor)
		{
			if (PiercingArrow)
			{
				Texture2D tex = ShineTex.Value;
				Vector2 pos = projectile.Center - Main.screenPosition;
				Color color = Color.Lerp(lightColor, Color.White, 0.5f) * 0.5f;

				Main.spriteBatch.Draw(tex, pos, null, color, Main.GameUpdateCount * 0.2f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
			}
		}
	}
}