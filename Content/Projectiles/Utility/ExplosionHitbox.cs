using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

public readonly record struct ExplosionSpawnInfo(bool Friendly = true, float Knockback = 8f, Vector2? Velocity = null, int BuffType = 0, int BuffLength = 0, bool CanSpawnProjectile = true)
{
	/// <summary>
	/// Spawns a hostile projectile, only on the singleplayer or server.
	/// </summary>
	public static readonly ExplosionSpawnInfo HostileSpawn = new(false, CanSpawnProjectile: Main.netMode != NetmodeID.MultiplayerClient);

	internal static readonly ExplosionSpawnInfo FriendlySpawn = new(true);

	/// <summary>
	/// Spawns a friendly projectile, only on the owner.
	/// </summary>
	public static ExplosionSpawnInfo PlayerOwned(int owner)
	{
		return FriendlySpawn with { CanSpawnProjectile = owner == Main.myPlayer };
	}
}

/// <summary>
/// Defines a generic "explosion" class with variable <see cref="Width"/> and <see cref="Height"/> - ai[0] and ai[1], 
/// alongside <see cref="BuffId"/> + <see cref="BuffLength"/> (localAI[0] and [1]) for flexibility.
/// </summary>
internal class ExplosionHitbox : ModProjectile
{
	/// <summary>
	/// Contains information related to a standard explosion's visual and audio effects.
	/// </summary>
	/// <param name="GoreCount">How many smoke gores to spawn. Defaults to 4 repeat.</param>
	/// <param name="SmokeDustCount">How many smoke dusts to spawn. Defaults to 20.</param>
	/// <param name="TorchDustCount">
	/// How many torch dusts to spawn. Defaults to 10. 
	/// Note that this will loop <paramref name="TorchDustCount"/> / 2 times, since the loop has two dust spawns in it.
	/// </param>
	/// <param name="Sfx">Whether the sfx should play.</param>
	/// <param name="GoreRange">Range of Gore ids to use.</param>
	/// <param name="ForceDustVelocity">Forces the dust velocity after it's spawned. This may be more consistent for certain dusts which behave oddly.</param>
	public readonly record struct VFXPackage(int GoreCount = 4, int SmokeDustCount = 20, int TorchDustCount = 10, bool Sfx = true, float Volume = 1f, Range? GoreRange = null,
		int SmokeDustType = DustID.Smoke, int TorchDustType = DustID.Torch, float DustVelocityModifier = 1f, bool ForceDustVelocity = false)
	{
		public static readonly VFXPackage None = new(0, 0, 0, false, 0, null, 0, 0, 0);
	}

	public override string Texture => UseBaseTexture ? (GetType().Namespace + "." + Name).Replace('.', '/') : "Terraria/Images/NPC_0";

	protected virtual bool UseBaseTexture => false;

	private ref float Width => ref Projectile.ai[0];
	private ref float Height => ref Projectile.ai[1];
	private int BuffId => (int)Projectile.localAI[0];
	private int BuffLength => (int)Projectile.localAI[1];

	public override void SetDefaults()
	{
		Projectile.friendly = true;
		Projectile.hostile = true;
		Projectile.timeLeft = 3;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(100, 100);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
		Projectile.penetrate = -1;
		Projectile.aiStyle = -1;
	}

	public override void AI()
	{
		if (Projectile.width != (int)Width || Projectile.height != (int)Height)
		{
			Projectile.Resize((int)Width, (int)Height);
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (BuffId != 0)
		{
			target.AddBuff(BuffId, BuffLength);
		}
	}

	/// <summary>
	/// Copies vanilla's bomb/grenade explosion VFX with some modifyability.
	/// </summary>
	/// <param name="entity">Enity that's spawning the VFX.</param>
	public static void VFX(Entity entity, in VFXPackage? package = null)
	{
		VFXPackage value = package ?? new VFXPackage(4);

		if (value.Sfx && !Main.dedServ)
		{
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = value.Volume }, entity.position);
		}

		for (int i = 0; i < value.SmokeDustCount; i++)
		{
			int dust = Dust.NewDust(entity.position, entity.width, entity.height, value.SmokeDustType, 0f, 0f, 100, default, 1.5f);
			Dust newDust = Main.dust[dust];
			newDust.velocity *= 1.4f * value.DustVelocityModifier;

			CheckForcedSpeed(value, newDust, 1.4f);
		}
		
		for (int i = 0; i < value.TorchDustCount / 2; i++)
		{
			int dust = Dust.NewDust(entity.position, entity.width, entity.height, value.TorchDustType, 0f, 0f, 100, default, 2.5f);
			Dust newDust = Main.dust[dust];
			newDust.noGravity = true;
			newDust.velocity *= 5f * value.DustVelocityModifier;

			CheckForcedSpeed(value, newDust, 5);

			dust = Dust.NewDust(entity.position, entity.width, entity.height, value.TorchDustType, 0f, 0f, 100, default, 1.5f);
			newDust = Main.dust[dust];
			newDust.velocity *= 3f * value.DustVelocityModifier;

			CheckForcedSpeed(value, newDust, 3);
		}

		if (Main.dedServ)
		{
			return;
		}

		IEntitySource src = entity.GetSource_Death();
		Range goreIdRange = value.GoreRange ?? GoreID.Smoke1..GoreID.Smoke3;

		for (int i = 0; i < value.GoreCount; ++i)
		{
			int slot = Gore.NewGore(src, entity.position, new Vector2(1, 0).RotatedBy(MathHelper.PiOver2 * (i % 4)), RandomSmoke(goreIdRange));
			Gore gore = Main.gore[slot];
			gore.velocity *= 0.4f;
			gore.velocity.X += 1f;
			gore.velocity.Y += 1f;
		}

		return;

		static int RandomSmoke(Range range)
		{
			return Main.rand.Next(range.Start.Value, range.End.Value + 1);
		}
	}

	private static void CheckForcedSpeed(VFXPackage value, Dust newDust, float baseModifier)
	{
		if (value.ForceDustVelocity)
		{
			newDust.velocity = Main.rand.NextVector2Circular(1, 1) * value.DustVelocityModifier * baseModifier;
		}
	}

	/// <summary>
	/// Quickly and easily spawns a <see cref="ExplosionHitbox"/> and also calls <see cref="VFXPackage"/> for visuals.<br/>
	/// Use <paramref name="info"/>'s <see cref="ExplosionSpawnInfo.CanSpawnProjectile"/> to spawn the projectile only on the owner - 
	/// ideally, either using <see cref="ExplosionSpawnInfo.HostileSpawn"/> or <see cref="ExplosionSpawnInfo.PlayerOwned(int)"/>.
	/// </summary>
	public static int QuickSpawn(IEntitySource source, Entity sourceEntity, int damage, int owner, Vector2 size, in ExplosionSpawnInfo info, in VFXPackage? package = null)
	{
		int proj = -1;

		if (info.CanSpawnProjectile)
		{
			int type = info.Friendly ? ModContent.ProjectileType<ExplosionHitboxFriendly>() : ModContent.ProjectileType<ExplosionHitbox>();
			proj = Projectile.NewProjectile(source, sourceEntity.Center, info.Velocity ?? Vector2.Zero, type, damage, info.Knockback, owner, size.X, size.Y);
			Projectile projectile = Main.projectile[proj];
			projectile.localAI[0] = info.BuffType;
			projectile.localAI[1] = info.BuffLength;
		}

		VFX(sourceEntity, package);
		return proj;
	}
}

internal class ExplosionHitboxFriendly : ExplosionHitbox
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.hostile = false;
	}
}