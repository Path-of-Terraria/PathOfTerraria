using System.Collections.Generic;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

public class Simulacra : Gear
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        PoTStaticItemData staticData = this.GetStaticData();
        staticData.DropChance = 1f;
        staticData.MinDropItemLevel = 15;
        staticData.IsUnique = true;
        staticData.Description = this.GetLocalization("Description");
        staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.damage = 35;
        Item.autoReuse = true;
        Item.DamageType = DamageClass.Ranged; 
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.shootSpeed = 18f;
        Item.knockBack = 4f;
        Item.value = Item.buyPrice(gold: 2);
        Item.UseSound = SoundID.Item5;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.useAmmo = AmmoID.Arrow;
        Item.noMelee = true;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
    
    public override bool CanConsumeAmmo(Item ammo, Player player)
    {
	    return player.altFunctionUse != 2;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2)
        {
            var altUsePlayer = player.GetModPlayer<AltUsePlayer>();
            if (altUsePlayer.AltFunctionAvailable)
            {
                Vector2 cursorPosition = Main.MouseWorld;
                
                Projectile.NewProjectileDirect(
                    source,
                    cursorPosition,
                    Vector2.Zero,
                    ModContent.ProjectileType<SimulacraMirror>(),
                    0,
                    0,
                    player.whoAmI
                );

                altUsePlayer.SetAltCooldown(5 * 60); 
            }
            return false;
        }
        else
        {
            Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            var echoProj = Projectile.NewProjectileDirect( 
	            source,
	            position,
	            Vector2.Zero,
	            ModContent.ProjectileType<SimulacraEcho>(),
	            damage,
	            knockback,
	            player.whoAmI,
	            ai0: position.X, 
	            ai1: position.Y,
	            ai2: velocity.X
            );
        
            echoProj.localAI[0] = velocity.Y;
            echoProj.localAI[1] = type;
            echoProj.localAI[2] = 0; 
            
            return false;
        }
    }
}

public class SimulacraEcho : ModProjectile
{
    private const int EchoDelay = 14; 
    
    /// <summary>
    /// Gets the original position stored in the projectile's AI data
    /// </summary>
    private Vector2 OriginalPosition => new Vector2(Projectile.ai[0], Projectile.ai[1]);

    /// <summary>
    /// Gets the original velocity stored in the projectile's AI data
    /// </summary>
    private Vector2 OriginalVelocity => new Vector2(Projectile.ai[2], Projectile.localAI[0]);

    /// <summary>
    /// Gets the projectile type stored in the projectile's local AI data
    /// </summary>
    private int ProjectileType => (int)Projectile.localAI[1];

    /// <summary>
    /// Determines if this echo projectile is the original one (not created by a mirror)
    /// Original echoes trigger mirror shots, while mirror echoes only fire from their own position
    /// </summary>
    private bool IsOriginalEcho => Projectile.localAI[2] == 0;
    
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true; 
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = EchoDelay + 1;
        Projectile.alpha = 255;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Projectile.timeLeft--;
        
        if (Projectile.timeLeft <= 1) 
        {
            if (Main.myPlayer == Projectile.owner)
            {
	            FireEchoProjectile(OriginalPosition, OriginalVelocity);
                
	            if (IsOriginalEcho)
	            {
		            FireFromAllMirrors();
	            }
            }
            
            Projectile.Kill();
        }
    }

    /// <summary>
    /// Fires a projectile with the specified parameters, used for both original and mirror echo shots
    /// </summary>
    /// <param name="position">The spawn position of the projectile</param>
    /// <param name="velocity">The velocity vector of the projectile</param>
    private void FireEchoProjectile(Vector2 position, Vector2 velocity)
    {
        Projectile.NewProjectileDirect(
            Projectile.GetSource_FromThis(),
            position,
            velocity,
            ProjectileType,
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner
        );
    }
    
    /// <summary>
    /// Finds all active mirrors and fires projectiles from each one, including creating delayed echo projectiles
    /// </summary>
    private void FireFromAllMirrors()
    {
        var mirrorClones = GetAllActiveMirrorClones();
        Vector2 originalVelocity = OriginalVelocity;
        
        foreach (var mirror in mirrorClones)
        {
            Vector2 mirrorVelocity = CalculateMirrorVelocity(mirror.Center, originalVelocity);
            FireEchoProjectile(mirror.Center, mirrorVelocity);
            
            CreateMirrorEcho(mirror.Center, mirrorVelocity);
        }
    }

    /// <summary>
    /// Creates a delayed echo projectile from a mirror position that will fire after the echo delay
    /// </summary>
    /// <param name="position">The mirror position to fire from</param>
    /// <param name="velocity">The velocity vector for the delayed shot</param>
    private void CreateMirrorEcho(Vector2 position, Vector2 velocity)
    {
        var mirrorEcho = Projectile.NewProjectileDirect(
            Projectile.GetSource_FromThis(),
            position,
            Vector2.Zero,
            ModContent.ProjectileType<SimulacraEcho>(),
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner,
            ai0: position.X,
            ai1: position.Y,
            ai2: velocity.X
        );
        
        mirrorEcho.localAI[0] = velocity.Y;
        mirrorEcho.localAI[1] = ProjectileType;
        mirrorEcho.localAI[2] = 1;
    }

    /// <summary>
    /// Searches through all active projectiles to find SimulacraMirror instances owned by the same player
    /// </summary>
    /// <returns>A list of all active mirror clone projectiles</returns>
    private List<Projectile> GetAllActiveMirrorClones()
    {
        var mirrors = new List<Projectile>();
        
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.type == ModContent.ProjectileType<SimulacraMirror>() && proj.owner == Projectile.owner)
            {
                mirrors.Add(proj);
            }
        }
        
        return mirrors;
    }

    private Vector2 CalculateMirrorVelocity(Vector2 mirrorPosition, Vector2 originalVelocity)
    {
        Vector2 mousePos = Main.MouseWorld;
        Vector2 directionToCursor = (mousePos - mirrorPosition).SafeNormalize(Vector2.UnitX);
        return directionToCursor * originalVelocity.Length();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false; 
    }
}

public class SimulacraMirror : ModProjectile
{
	private const int MirrorDuration = 8 * 60; 
    
	public override void SetDefaults()
	{
		Projectile.width = 20;
		Projectile.height = 42;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = MirrorDuration;
		Projectile.tileCollide = false;
		Projectile.alpha = 100;
		Projectile.ignoreWater = true;
	}

	public override void AI()
	{
		if (Main.myPlayer == Projectile.owner)
		{
			Vector2 directionToCursor = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
			Projectile.rotation = directionToCursor.ToRotation();
			Projectile.netUpdate = true;
		}

		if (Projectile.timeLeft > MirrorDuration - 60)
		{
			Projectile.alpha = (int)MathHelper.Lerp(255, 100, (MirrorDuration - Projectile.timeLeft) / 60f);
		}
		else if (Projectile.timeLeft < 60)
		{
			Projectile.alpha = (int)MathHelper.Lerp(100, 255, (60 - Projectile.timeLeft) / 60f);
		}

		if (Main.rand.NextBool(3))
		{
			var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror);
			dust.velocity *= 0.3f;
			dust.noGravity = true;
			dust.alpha = 150;
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.Cyan * ((255 - Projectile.alpha) / 255f);
	}
}