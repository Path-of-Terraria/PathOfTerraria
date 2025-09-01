using PathOfTerraria.Common.Systems;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged.Javelin;

public class JavelinThrown(string name, Vector2 itemSize, int dustType) : ModProjectile
{
	protected override bool CloneNewInstances => true;
	
	public override string Name => InstanceName;
	
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Javelins/{Name.Replace("Thrown", "")}";

	public bool UsingAlt
	{
		get => Projectile.ai[2] == 1;
		set => Projectile.ai[2] = value ? 1 : 0;
	}

	protected string InstanceName = name;
	protected Vector2 ItemSize = itemSize;
	protected int DustType = dustType;

	public override ModProjectile Clone(Projectile newEntity)
	{
		var proj = base.Clone(newEntity) as JavelinThrown;
		proj.InstanceName = InstanceName;
		proj.ItemSize = ItemSize;
		proj.DustType = DustType;
		return proj;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.CloneDefaults(ProjectileID.JavelinFriendly);
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 10;
		Projectile.extraUpdates = 1;
	}

	public override bool? CanDamage()
	{
		return !UsingAlt;
	}

	public override void AI()
	{
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 - 0.05f;
		Projectile.velocity.Y -= 0.05f;

		if (UsingAlt)
		{
			Player player = Main.player[Projectile.owner];
			Items.Gear.Weapons.Javelins.Javelin.JavelinDashPlayer javelinDashPlayer = player.GetModPlayer<Items.Gear.Weapons.Javelins.Javelin.JavelinDashPlayer>();
			Vector2 storedVel = javelinDashPlayer.StoredVelocity;
			Projectile.rotation = storedVel.ToRotation() + MathHelper.PiOver4;
			Projectile.Center = player.Center + storedVel;

			if (!player.GetModPlayer<AltUsePlayer>().AltFunctionActive)
			{
				Projectile.active = false;
			}
		}
	}

	public override void OnKill(int timeLeft)
	{
		Vector2 location = Projectile.Center;
		Vector2 tip = ItemSize.RotatedBy(Projectile.rotation + MathHelper.PiOver2);

		for (int i = 0; i < 16; ++i)
		{
			Dust.NewDust(location + tip * Main.rand.NextFloat(), 1, 1, DustType, Scale: Main.rand.NextFloat(1, 1.5f) * Projectile.scale);
		}

		SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
	}

	public override void ModifyDamageHitbox(ref Rectangle hitbox)
	{
		hitbox.Width = (int)(hitbox.Width * Projectile.scale);
		hitbox.Height = (int)(hitbox.Height * Projectile.scale);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Color color = lightColor * Projectile.Opacity;
		Vector2 position = Projectile.Center - Main.screenPosition;
		Main.EntitySpriteDraw(tex, position, null, color, Projectile.rotation, tex.Size() * new Vector2(0.75f, 0.25f), Projectile.scale, SpriteEffects.None, 0);

		return false;
	}
}