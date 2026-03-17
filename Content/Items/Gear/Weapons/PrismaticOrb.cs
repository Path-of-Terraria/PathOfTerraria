using System.Collections.Generic;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Core.Items;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons;

internal class PrismaticOrb : Gear
{
	private const int buffTime = 60 * 5;
	private const int buffCooldown = 60 * 15;
    public override void SetStaticDefaults()
    {
	    base.SetStaticDefaults();

	    PoTStaticItemData staticData = this.GetStaticData();
	    staticData.DropChance = 1f;
	    staticData.IsUnique = true;
	    staticData.Description = this.GetLocalization("Description");
	    staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	    staticData.MinDropItemLevel = 18;
    
	    ElementalWeaponSets.WeaponElementProportionsById[Type] = new Dictionary<ElementType, float>
	    {
		    { ElementType.Fire, 0.33f },
		    { ElementType.Cold, 0.33f },
		    { ElementType.Lightning, 0.33f },
		    { ElementType.Chaos, 0.0f }
	    };

    }

    public override void SetDefaults()
    {
        Item.damage = 25;
        Item.DamageType = DamageClass.Magic;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Thrust;
        Item.knockBack = 0.2f;
        Item.value = Item.buyPrice(gold: 1);
        Item.UseSound = SoundID.Item8;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<PrismaticOrbProjectile>();
        Item.shootSpeed = 7f;
        Item.mana = 15;
        Item.noMelee = true;
    }

    public override bool AltFunctionUse(Player player)
    {
	    return true;
    }
    
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
	    if (player.altFunctionUse == 2)
	    {
		    var altUsePlayer = player.GetModPlayer<AltUsePlayer>();
		    if (altUsePlayer.AltFunctionAvailable)
		    {
			    player.AddBuff(ModContent.BuffType<Buffs.PrismaticOrbBuff>(), buffTime);
			    altUsePlayer.SetAltCooldown(buffCooldown); 
		    }
		    return false;
	    }
	    return true;
    }
}

 public class PrismaticOrbProjectile : ModProjectile
{
	private const int MaxBounces = 5;

	private int bounces = 0;
	private Vector2 originalVelocity;
	private float baseHorizontalSpeed;
	private bool hasInitialized = false;


    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = MaxBounces + 1;
        Projectile.timeLeft = 600;
        Projectile.light = 0.5f;
        Projectile.tileCollide = true;
    }

    public override void AI()
    {
	    if (!hasInitialized)
	    {
		    originalVelocity = Projectile.velocity;
		    baseHorizontalSpeed = Math.Abs(Projectile.velocity.X);
		    hasInitialized = true;
	    }

	    Projectile.velocity.Y += 0.4f; 
    
	    Projectile.rotation += Projectile.velocity.X * 0.02f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (bounces >= MaxBounces)
        {
            CreateExplosion();
            return true;
        }
        
        float energyRetention = MathHelper.Lerp(0.85f, 0.7f, bounces / (float)MaxBounces);
        
        float newHorizontalSpeed = baseHorizontalSpeed * energyRetention;
        int horizontalDirection = Math.Sign(originalVelocity.X);
        Projectile.velocity.X = newHorizontalSpeed * horizontalDirection;
        
        float bounceHeight = MathHelper.Lerp(8f, 6f, bounces / (float)MaxBounces);
        
        float impactAngleFactor = Math.Abs(oldVelocity.Y) / oldVelocity.Length();
        bounceHeight *= (0.7f + impactAngleFactor * 0.5f); 
        
        Projectile.velocity.Y = -bounceHeight;

        baseHorizontalSpeed *= energyRetention;

        if (Math.Abs(Projectile.velocity.X) < 2f)
        {
            Projectile.velocity.X = 2f * horizontalDirection;
        }

        bounces++;
        CreateExplosion();
        
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        
        return false;
    }

    private void CreateExplosion()
    {
	    float explosionRadius = 120f;
    
	    // Debug visual
	    if (Main.netMode != NetmodeID.Server)
	    {
		    for (int i = 0; i < 32; i++)
		    {
			    Vector2 circlePos = Projectile.Center + Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 32f) * explosionRadius;
			    var debugDust = Dust.NewDustPerfect(circlePos, DustID.Electric, Vector2.Zero);
			    debugDust.noGravity = true;
			    debugDust.scale = 0.5f;
			    debugDust.alpha = 200;
		    }
	    }
    
	    ElementType randomElement = (ElementType)Main.rand.Next(1, 4); 
    
	    for (int i = 0; i < 25; i++)
	    {
		    var dust = Dust.NewDustDirect(Projectile.Center, 0, 0, GetDustTypeForElement(randomElement));
		    dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
		    dust.noGravity = true;
		    dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
	    }

	    if (Main.myPlayer == Projectile.owner)
	    {
		    Player owner = Main.player[Projectile.owner];
		    int explosionCount = owner.HasBuff(ModContent.BuffType<Buffs.PrismaticOrbBuff>()) ? 2 : 1;
        
		    for (int i = 0; i < explosionCount; i++)
		    {
			    CreateExplosionProjectile();
		    }
	    }
    }
    
    private void CreateExplosionProjectile()
    {
	    Projectile explosion = Projectile.NewProjectileDirect(
		    Projectile.GetSource_FromThis(),
		    Projectile.Center,
		    Vector2.Zero,
		    ModContent.ProjectileType<PrismaticOrbExplosion>(),
		    Projectile.damage,
		    Projectile.knockBack,
		    Projectile.owner
	    );
    
	    explosion.GetGlobalProjectile<ElementalProjectile>().Container = Projectile.GetGlobalProjectile<ElementalProjectile>().Container;
	    
	    SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
    }

    private int GetDustTypeForElement(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => DustID.Torch,
            ElementType.Cold => DustID.Ice,
            ElementType.Lightning => DustID.Electric,
            _ => DustID.RainbowMk2
        };
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
}

 public class PrismaticOrbExplosion : ModProjectile
 {
	 public override void SetDefaults()
	 {
		 Projectile.width = 240;
		 Projectile.height = 240;
		 Projectile.friendly = true;
		 Projectile.hostile = false;
		 Projectile.timeLeft = 3; 
		 Projectile.tileCollide = false;
		 Projectile.penetrate = -1;
		 Projectile.usesLocalNPCImmunity = true; 
		 Projectile.localNPCHitCooldown = -1; 
		 Projectile.ignoreWater = true;
		 Projectile.DamageType = DamageClass.Magic;
		 Projectile.alpha = 255;
	 }

	 public override bool? CanHitNPC(NPC target)
	 {
		 return !target.friendly && target.CanBeChasedBy();
	 }

	 public override void AI()
	 {
		 Projectile.velocity = Vector2.Zero;
	 }
 }
