using ReLogic.Content;
using System.Collections.Generic;

using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;

using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class StarlightBulwark : LeadBattleBulwark
{
	public static Asset<Texture2D> GlowTexture;
	
	public override ShieldData Data => new(15, 160, 14f, DustID.YellowStarDust);

	private int _projTimer = 0;
	private int _lastProj = -1;
	private int _lastAltClick = 0;
	private bool _specialAlt = false;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		
		GlowTexture = ModContent.Request<Texture2D>(Texture + "Shine");
		
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.IsUnique = true;
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
		staticData.Description = this.GetLocalization("Description");
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(-1, 15, 25);
		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<ManaAffix>(-1, 15, 20);
		var noFallDamageAffix = (ItemAffix)Affix.CreateAffix<NoFallDamageAffix>(-1, 1f, 4f);
		return [addedDamageAffix, attackSpeedAffix, noFallDamageAffix];
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new(34);
		Item.knockBack = 10;
	}

	public override bool? UseItem(Player player)
	{
		if (player.altFunctionUse != 2)
		{
			player.GetModPlayer<WarShieldPlayer>().StartBash(Data.DashTime, _specialAlt ? Data.DashTime : Data.Cooldown, Data.DashMagnitude);
		}

		return true;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);

		if (player.GetModPlayer<WarShieldPlayer>().Bashing)
		{
			if (_projTimer++ % (_specialAlt ? 6 : 3) == 0)
			{
				Vector2 vel = (player.velocity * Main.rand.NextFloat(0.4f, 0.8f) * 0.8f).RotatedByRandom(1f);
				int type = ModContent.ProjectileType<StarlightBulwarkStar>();
				IEntitySource source = player.GetSource_ItemUse(Item);
				int damage = (int)(Item.damage * 0.6f);
				_lastProj = Projectile.NewProjectile(source, player.Center, vel, type, damage, 0, player.whoAmI, _lastProj, _specialAlt ? 1 : 0);
			}
		}
		else if (player.whoAmI != Main.myPlayer || !Main.mouseRight)
		{
			_lastProj = -1;
		}

		_lastAltClick--;

		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();

		if (player.whoAmI == Main.myPlayer && Main.mouseRight && Main.mouseRightRelease && (altUsePlayer.AltFunctionAvailable || altUsePlayer.AltFunctionActive))
		{
			if (_lastAltClick > 0)
			{
				altUsePlayer.SetAltCooldown(4 * 60, 120);
				_specialAlt = true;

				for (int i = 0; i < 16; ++i)
				{
					Vector2 vel = new Vector2(Main.rand.NextFloat(4, 8), 0).RotatedByRandom(MathHelper.TwoPi);
					Dust.NewDust(player.position, 1, 1, DustID.YellowStarDust, vel.X, vel.Y);
				}
			}

			_lastAltClick = 15;
		}

		if (!altUsePlayer.AltFunctionAvailable && !altUsePlayer.AltFunctionActive)
		{
			_specialAlt = false;
		}
	}

	public override bool CanRaiseShield(Player player)
	{
		return base.CanRaiseShield(player) || _specialAlt;
	}

	public override void OnRaiseShield(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();

		if (!altUsePlayer.AltFunctionActive && !_specialAlt)
		{
			altUsePlayer.SetAltCooldown(2 * 60, 60);
		}
	}

	public override bool ParryProjectile(Player player, Projectile projectile)
	{
		Vector2 vel = -(projectile.velocity * Main.rand.NextFloat(0.4f, 0.7f)).RotatedByRandom(0.6f);
		int type = _specialAlt ? ModContent.ProjectileType<StarlightBulwarkStar>() : ModContent.ProjectileType<StarlightBulwarkComet>();
		_lastProj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, vel, type, projectile.damage, 0, player.whoAmI, _lastProj);

		base.ParryProjectile(player, projectile);
		return true;
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		if (_specialAlt)
		{
			spriteBatch.Draw(GlowTexture.Value, position, null, drawColor with { A = 0 } * 0.7f, 0f, origin * (40 / 34f), scale, SpriteEffects.None, 0);
		}

		return true;
	}

	public class StarlightBulwarkStar : ModProjectile
	{
		public static Asset<Texture2D> ConnectionTexture;

		public Projectile LastStarProj => Main.projectile[(int)LastStar];

		public ref float LastStar => ref Projectile.ai[0];

		public bool Home => Projectile.ai[1] == 1;

		public override void SetStaticDefaults()
		{
			ConnectionTexture = ModContent.Request<Texture2D>(Texture + "Connection");
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.aiStyle = -1;
			Projectile.Opacity = 1f;
			Projectile.Size = new(18);
		}

		public override void AI()
		{
			Projectile.velocity *= 0.95f;
			Projectile.rotation += 0.06f * Projectile.velocity.Length() + 0.006f;

			if (Main.rand.NextBool(45))
			{
				Color newColor = GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())) ?? Color.White;
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, newColor: newColor);
			}

			if (Projectile.timeLeft < 60)
			{
				Projectile.Opacity = Projectile.timeLeft / 60f;
			}

			if (LastStar != -1)
			{
				if (!LastStarProj.active || LastStarProj.type != Type)
				{
					LastStar = -1;
				}
			}

			if (Home)
			{
				NPC nearestNPC = null;

				foreach (NPC npc in Main.ActiveNPCs)
				{
					float dist = npc.DistanceSQ(Projectile.Center);

					if (npc.CanBeChasedBy() && dist < 600 * 600 && (nearestNPC is null || dist < nearestNPC.DistanceSQ(Projectile.Center)))
					{
						nearestNPC = npc;
					}
				}

				if (nearestNPC != null)
				{
					Projectile.velocity += Projectile.DirectionTo(nearestNPC.Center) * 0.05f;
				}

				if (Projectile.velocity.LengthSquared() > 9 * 9)
				{
					Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 9;
				}
			}
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return lightColor with { A = 0 } * Projectile.Opacity;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (LastStar == -1)
			{
				return true;
			}

			float opacity = (Projectile.Opacity + LastStarProj.Opacity) / 2f;
			var position = Vector2.Lerp(Projectile.Center, LastStarProj.Center, 0.5f);
			float rotation = Projectile.AngleTo(LastStarProj.Center);
			Texture2D tex = ConnectionTexture.Value;
			var scale = new Vector2(Projectile.Distance(LastStarProj.Center) / tex.Width * 0.9f, 0.5f);
			Main.spriteBatch.Draw(tex, position - Main.screenPosition, null, Color.Pink * opacity * 0.85f, rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0);
			return true;
		}
	}

	public class StarlightBulwarkComet : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.aiStyle = -1;
			Projectile.Opacity = 1f;
			Projectile.Size = new(18);
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.velocity.Y += 0.1f;

			if (Main.rand.NextBool(15))
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleCrystalShard);
				Main.dust[dust].noGravity = true;
			}

			if (Projectile.timeLeft < 60)
			{
				Projectile.Opacity = Projectile.timeLeft / 60f;
			}
		}
	}
}
