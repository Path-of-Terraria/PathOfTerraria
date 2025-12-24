using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons;

public class FleaBell : Gear
{
    public static readonly SoundStyle BellSound = new("PathOfTerraria/Assets/Sounds/FleaBell")
    {
        Volume = 0.7f,
        PitchVariance = 0.1f,
        MaxInstances = 3
    };
    
    public static readonly SoundStyle BellHitSound = new("PathOfTerraria/Assets/Sounds/FleaBellHit")
    {
	    Volume = 1f,
	    PitchVariance = 0.2f,
	    MaxInstances = 3,
	    Pitch = -0.1f
    };

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        PoTStaticItemData staticData = this.GetStaticData();
        staticData.DropChance = 1f;
        staticData.MinDropItemLevel = 1;
        staticData.IsUnique = true;
        staticData.Description = this.GetLocalization("Description");
        staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.damage = 25;
        Item.DamageType = DamageClass.Summon;
        Item.width = 26;
        Item.height = 34;
        Item.useTime = 36;
        Item.useAnimation = 36;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.UseSound = BellSound;
        Item.value = Item.buyPrice(0, 0, 10, 0);
        Item.autoReuse = true;
        Item.buffType = ModContent.BuffType<FleaBellBuff>();
        Item.shoot = ModContent.ProjectileType<FleaBellMinion>();
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool? UseItem(Player player)
    {
	    if (player.altFunctionUse == 2)
	    {
		    var altUsePlayer = player.GetModPlayer<AltUsePlayer>();
		    if (altUsePlayer.AltFunctionAvailable)
		    {
			    player.AddBuff(ModContent.BuffType<FleaBellEnhancedBuff>(), 5 * 60);
			    altUsePlayer.SetAltCooldown(20 * 60);
		    }
		    return true;
	    }

	    player.AddBuff(Item.buffType, 2);
	    var projectile = Projectile.NewProjectileDirect(
		    player.GetSource_ItemUse(Item), 
		    player.Center, 
		    Vector2.Zero, 
		    Item.shoot, 
		    Item.damage, 
		    Item.knockBack, 
		    player.whoAmI
	    );
	    projectile.originalDamage = Item.damage;
    
	    return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
	    return false; 
    }
}

public class FleaBellBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<FleaBellMinion>()] > 0)
        {
            player.buffTime[buffIndex] = 18000;
        }
        else
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}

public class FleaBellEnhancedBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = false;
    }
}

public class FleaBellMinion : ModProjectile
{
    private const float IdleOffsetY = -48f;
    private const float TeleportDistance = 700f;
    private const float MovementSpeed = 8f;
    private const float VelocitySmoothing = 40f;
    private const float VelocityDamping = 0.96f;
    private const float IdleThreshold = 20f;
    private const float HoverAmplitude = 8f;
    private const float HoverFrequency = 0.05f; 
    private const float VelocityResetFactor = 0.1f;
    private const float DetectionRange = 400f;
    private const float ChargeSpeed = 12f;
    
    //Bouncing
    private const float BounceSpeed = 16f;
    private const float BounceDamping = 0.92f;
    private const int BounceDuration = 45;
    private const float BounceGravity = 0.3f;
    private const float InitialBounceUpVelocity = -8f; 
    private const float BounceHorizontalSpeed = 4f;
    private Vector2 bounceDestination = Vector2.Zero;
    private bool hasBounceDestination = false;


    private const int MinionDuration = 18000;
    private const int HitCooldown = 30;
    
    private float hoverTimer = 0f;
    
    private const float EnhancedDamageMultiplier = 1.5f;
    private const float EnhancedSpeedMultiplier = 1.4f;
    private const int EnhancedHitCooldown = 20;
    
    private const float SpreadRadius = 80f;
    private Vector2 personalOffset = Vector2.Zero;
    private bool hasCalculatedOffset = false;
    
    // Animation stuff
    private const int AnimationFrameCount = 5;
    private const int AnimationSpeed = 8;
    private int animationFrame = 0;
    private int animationTimer = 0;


    public override void SetStaticDefaults()
    {
	    Main.projFrames[Projectile.type] = AnimationFrameCount; 
        Main.projPet[Projectile.type] = true;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 33;
        Projectile.height = 38;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.minionSlots = 1f;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinionDuration;
        Projectile.aiStyle = -1;
        
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = HitCooldown;
    }

    private enum AIState
    {
        Following,
        Charging,
        Bouncing
    }

    private AIState State
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    private ref float StateTimer => ref Projectile.ai[1];
    private ref float TargetWhoAmI => ref Projectile.ai[2];

    private bool IsEnhanced => Main.player[Projectile.owner].HasBuff(ModContent.BuffType<FleaBellEnhancedBuff>());

    private void CalculatePersonalOffset(Player owner)
    {
        if (hasCalculatedOffset) return;

        int fleaIndex = 0;
        int totalFleas = 0;
        
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.type == Projectile.type && proj.owner == owner.whoAmI)
            {
                totalFleas++;
                if (proj.whoAmI == Projectile.whoAmI)
                {
                    fleaIndex = totalFleas - 1;
                }
            }
        }

        if (totalFleas > 1)
        {
            float angle = (MathHelper.TwoPi / totalFleas) * fleaIndex;
            float radius = SpreadRadius * Math.Min(totalFleas / 3f, 1f);
            personalOffset = new Vector2(
                (float)Math.Cos(angle) * radius,
                (float)Math.Sin(angle) * radius * 0.5f 
            );
        }
        else
        {
            personalOffset = Vector2.Zero;
        }

        hasCalculatedOffset = true;
    }

    public override void AI()
    {
	    HandleAnimation();

        Player owner = Main.player[Projectile.owner];

        if (!owner.active)
        {
            Projectile.active = false;
            return;
        }

        if (!owner.HasBuff(ModContent.BuffType<FleaBellBuff>()))
        {
            Projectile.active = false;
            return;
        }

        Projectile.localNPCHitCooldown = IsEnhanced ? EnhancedHitCooldown : HitCooldown;

        //For spacing multiple minions
        CalculatePersonalOffset(owner);

        float speedMultiplier = IsEnhanced ? EnhancedSpeedMultiplier : 1f;

        // flea brew fx
        if (IsEnhanced && Main.rand.NextBool(3))
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.scale = 0.8f;
        }
        
        if (State == AIState.Following)
        {
	        hoverTimer += HoverFrequency;
        }

        if (State == AIState.Following)
        {
            Vector2 idlePosition = owner.Center;
            idlePosition.Y += IdleOffsetY;
            idlePosition += personalOffset;
            
            // subtle hover to make them feel more alive
            float hoverOffset = (float)Math.Sin(hoverTimer) * HoverAmplitude;
            idlePosition.Y += hoverOffset;

            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();

            if (distanceToIdlePosition > TeleportDistance && State == AIState.Following)
            {
                Projectile.position = idlePosition;
                Projectile.velocity *= VelocityResetFactor;
            }
            else
            {
                if (distanceToIdlePosition > IdleThreshold)
                {
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= MovementSpeed * speedMultiplier;
                    Projectile.velocity = (Projectile.velocity * VelocitySmoothing + vectorToIdlePosition) / (VelocitySmoothing + 1f);
                }
                else
                {
	                Vector2 randomMovement = Main.rand.NextVector2Circular(0.8f, 0.8f);
	                Projectile.velocity += randomMovement * 0.1f;

                    Projectile.velocity *= VelocityDamping;
                }
            }
            
            Vector2 directionToPlayer = owner.Center - Projectile.Center;
            if (Math.Abs(directionToPlayer.X) > 5f) 
            {
                Projectile.spriteDirection = Math.Sign(directionToPlayer.X);
            }

            NPC closestNPC = FindClosestNPC(DetectionRange);
            if (closestNPC != null)
            {
                State = AIState.Charging;
                TargetWhoAmI = closestNPC.whoAmI;
                StateTimer = 0f;
                hoverTimer = 0f;
            }
        }
        else if (State == AIState.Charging)
        {
            StateTimer++;
            
            NPC target = Main.npc[(int)TargetWhoAmI];
            if (!target.active || target.life <= 0)
            {
                State = AIState.Following;
                return;
            }

            Vector2 direction = target.Center - Projectile.Center;
            direction.Normalize();
            
            Projectile.velocity = direction * ChargeSpeed * speedMultiplier;
            
            Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;
        }
        else if (State == AIState.Bouncing)
        {
	        StateTimer++;
    
	        if (!hasBounceDestination)
	        {
		        Vector2 directionToPlayer = (owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
        
		        float horizontalSpeed = IsEnhanced ? BounceHorizontalSpeed * EnhancedSpeedMultiplier : BounceHorizontalSpeed;
		        Projectile.velocity.X = directionToPlayer.X * horizontalSpeed;
        
		        float upVelocity = IsEnhanced ? InitialBounceUpVelocity * 1.3f : InitialBounceUpVelocity;
		        Projectile.velocity.Y = upVelocity;
        
		        hasBounceDestination = true;
	        }
    
	        Projectile.velocity.Y += BounceGravity;
    
	        Projectile.rotation *= 0.95f;
    
	        int bounceDuration = IsEnhanced ? (int)(BounceDuration * 0.7f) : BounceDuration;
    
	        if (StateTimer > bounceDuration)
	        {
		        State = AIState.Following;
		        StateTimer = 0f;
		        Projectile.rotation = 0f;
		        hasBounceDestination = false; 
	        }
        }


        if (State == AIState.Charging)
        {
	        CheckForEnemyCollisions();
        }
        else
        {
	        Projectile.rotation *= 0.9f;
            
	        if (Math.Abs(Projectile.velocity.X) > 0.1f)
	        {
		        Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
	        }
        }
    }
    
    private void HandleAnimation()
    {
	    animationTimer++;
        
	    int currentAnimationSpeed = State switch
	    {
		    AIState.Following => AnimationSpeed,
		    AIState.Charging => AnimationSpeed / 2,
		    AIState.Bouncing => AnimationSpeed * 2, 
		    _ => AnimationSpeed
	    };

	    if (animationTimer >= currentAnimationSpeed)
	    {
		    animationTimer = 0;
		    animationFrame++;
            
		    if (animationFrame >= AnimationFrameCount)
		    {
			    animationFrame = 0;
		    }
	    }

	    Projectile.frame = animationFrame;
    }

    
    private void CheckForEnemyCollisions()
    {
        if (Main.myPlayer != Projectile.owner) return;
    
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.life <= 0 || !npc.CanBeChasedBy()) continue;
        
            if (Projectile.Hitbox.Intersects(npc.Hitbox))
            {
                int finalDamage = IsEnhanced ? (int)(Projectile.damage * EnhancedDamageMultiplier) : Projectile.damage;
                
                var hitInfo = new NPC.HitInfo()
                {
                    Damage = finalDamage,
                    Knockback = Projectile.knockBack,
                    HitDirection = Math.Sign(Projectile.velocity.X),
                    DamageType = Projectile.DamageType
                };
            
                npc.StrikeNPC(hitInfo);
                HitNPC(npc, hitInfo, finalDamage);
                break;
            }
        }
    }

    public void HitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
	    //this is a meme, remove later probably
	    SoundEngine.PlaySound(FleaBell.BellHitSound, Projectile.Center);

	    State = AIState.Bouncing;
	    StateTimer = 0f;
	    hasBounceDestination = false;
	    
	    Vector2 direction = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitX);
	    float bounceSpeed = IsEnhanced ? BounceSpeed * EnhancedSpeedMultiplier : BounceSpeed;
	    Projectile.velocity = direction * bounceSpeed;
        
	    TargetWhoAmI = -1;
    }

    private NPC FindClosestNPC(float maxDetectDistance)
    {
        NPC closestNPC = null;
        float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
        float sqrClosestDistance = sqrMaxDetectDistance;

        for (int k = 0; k < Main.maxNPCs; k++)
        {
            NPC target = Main.npc[k];
            if (target.CanBeChasedBy())
            {
                float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                if (sqrDistanceToTarget < sqrClosestDistance)
                {
                    sqrClosestDistance = sqrDistanceToTarget;
                    closestNPC = target;
                }
            }
        }

        return closestNPC;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        if (IsEnhanced)
        {
            return Color.Lerp(lightColor, Color.Yellow, 0.6f);
        }
        return base.GetAlpha(lightColor);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false; 
    }
}