using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class WardensBow : WoodenBow
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
		staticData.AltUseDescription = this.GetLocalization("AltUse");

		BowAnimationProjectile.OverridenShootActionsByItemId[Type] = proj =>
		{
			Player owner = Main.player[proj.owner];

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, proj.Center);
			owner.PickAmmo(owner.HeldItem, out int type, out float speed, out int damage, out float kB, out int ammoUsed);
			owner.GetModPlayer<AltUsePlayer>().SetAltCooldown((int)(proj.ai[1] * 60f));

			damage = (int)(damage * 3f);
			Vector2 vel = proj.DirectionTo(Main.MouseWorld) * speed * 1.5f;
			IEntitySource src = owner.GetSource_ItemUse_WithPotentialAmmo(owner.HeldItem, ammoUsed);
			int newProj = Projectile.NewProjectile(src, proj.Center, vel, type, damage, kB, owner.whoAmI);

			Main.projectile[newProj].GetGlobalProjectile<WardensBowProjectile>().WardenProj = true;
		};
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 40;
		Item.Size = new Vector2(24, 48);
		Item.value = Item.buyPrice(0, 2, 0, 0);
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var lifeAffix = (ItemAffix)Affix.CreateAffix<FlatLifeAffix>(25, 35);
		var dexAffix = (ItemAffix)Affix.CreateAffix<DexterityItemAffix>(15f, 25f);
		var fetidCarapace = (ItemAffix)Affix.CreateAffix<FetidCarapaceAffix>(1);
		var armorShredAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyRootedGearAffix>(0.2f, 0.3f);

		return [lifeAffix, dexAffix, fetidCarapace, armorShredAffix];
	}

	public class WardensBowShard : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(10);
			Projectile.timeLeft = 180;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.frame = Main.rand.Next(3);
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void AI()
		{
			Projectile.velocity *= 0.9f;
			Projectile.Opacity = MathHelper.Clamp(Projectile.velocity.Length(), 0, 3f) / 4f;
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.Opacity <= 0.2f)
			{
				Projectile.Kill();
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 3; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
			}
		}
	}

	public class WardensBowProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool WardenProj = false;

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (WardenProj && Main.rand.NextFloat() < 20.1f)
			{
				target.AddBuff(ModContent.BuffType<RootedDebuff>(), 5 * 60);
			}
		}

		public override void OnKill(Projectile projectile, int timeLeft)
		{
			int shardId = ModContent.ProjectileType<WardensBowShard>();

			if (WardenProj && projectile.type != shardId)
			{
				for (int i = 0; i < 3; ++i)
				{
					Vector2 vel = projectile.velocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.8f, 1f);
					int damage = (int)(projectile.damage * 0.4f);
					int proj = Projectile.NewProjectile(projectile.GetSource_Death(), projectile.Center, vel, shardId, damage, 2, projectile.owner);

					Main.projectile[proj].GetGlobalProjectile<WardensBowProjectile>().WardenProj = true;
				}
			}
		}
	}
}