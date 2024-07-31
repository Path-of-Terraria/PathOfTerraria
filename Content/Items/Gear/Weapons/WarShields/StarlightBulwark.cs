using PathOfTerraria.Core.Systems.ModPlayers;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class StarlightBulwark : LeadBattleBulwark
{
	public override bool IsUnique => true;
	public override ShieldData Data => new(15, 160, 14f, DustID.YellowStarDust);

	private int _projTimer = 0;
	private int _lastProj = -1;
	private int _lastAltClick = 0;

	public override void Defaults()
	{
		base.Defaults();

		Item.Size = new(34);
		Item.knockBack = 10;
	}

	public override void HoldItem(Player player)
	{
		if (player.GetModPlayer<WarShieldPlayer>().Bashing)
		{
			if (_projTimer++ % 3 == 0)
			{
				Vector2 vel = (player.velocity * Main.rand.NextFloat(0.4f, 0.8f) * 0.8f).RotatedByRandom(1f);
				int type = ModContent.ProjectileType<StarlightBulwarkStar>();
				_lastProj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, vel, type, 20, 0, player.whoAmI, _lastProj);
			}
		}
		else if (player.whoAmI != Main.myPlayer || !Main.mouseRight)
		{
			_lastProj = -1;
		}

		_lastAltClick--;

		if (player.whoAmI == Main.myPlayer && Main.mouseRight && Main.mouseRightRelease)
		{
			if (_lastAltClick > 0)
			{

			}

			_lastAltClick = 15;
		}
	}

	public override bool ParryProjectile(Player player, Projectile projectile)
	{
		Vector2 vel = -(projectile.velocity * Main.rand.NextFloat(0.4f, 0.7f)).RotatedByRandom(0.6f);
		int type = ModContent.ProjectileType<StarlightBulwarkComet>();
		_lastProj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, vel, type, projectile.damage, 0, player.whoAmI, _lastProj);

		base.ParryProjectile(player, projectile);
		return true;
	}

	public class StarlightBulwarkStar : ModProjectile
	{
		public static Asset<Texture2D> ConnectionTexture;

		public Projectile LastStarProj => Main.projectile[(int)LastStar];

		public ref float LastStar => ref Projectile.ai[0];

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
