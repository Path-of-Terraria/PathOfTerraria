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
        
        ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
        ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;

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

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
	    if (player.altFunctionUse != 2)
	    {
		    position = Main.MouseWorld;
		    player.LimitPointToPlayerReachableArea(ref position);
	    }
    }
    
    public override void HoldItem(Player player)
    {
	    if (Main.myPlayer != player.whoAmI)
	    {
		    return;
	    }
    
	    if (Main.mouseRight && Main.mouseRightRelease)
	    {
		    var altUsePlayer = player.GetModPlayer<AltUsePlayer>();
		    if (altUsePlayer.AltFunctionAvailable)
		    {
			    player.AddBuff(ModContent.BuffType<FleaBellEnhancedBuff>(), 5 * 60);
			    altUsePlayer.SetAltCooldown(20 * 60);
			    Main.mouseRightRelease = false;
		    }
	    }
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
	    player.AddBuff(Item.buffType, 2);
	    return true;
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
    private const float IdleThreshold = 20f;
    private const float HoverFrequency = 0.05f; 
    private const float VelocityResetFactor = 0.1f;
    private const float DetectionRange = 400f;
    private const float ChargeSpeed = 12f;
    
    //Bouncing
    private const float BounceSpeed = 16f;
    private const int BounceDuration = 45;
    private const float BounceGravity = 0.3f;
    private const float InitialBounceUpVelocity = -8f; 
    private const float BounceHorizontalSpeed = 4f;
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
        Player owner = Main.player[Projectile.owner];

        if (!CheckActive(owner))
        {
            return;
        }

        GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
        SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
        Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
        Visuals();
    }

    private bool CheckActive(Player owner)
    {
        if (owner.dead || !owner.active)
        {
            owner.ClearBuff(ModContent.BuffType<FleaBellBuff>());
            return false;
        }

        if (owner.HasBuff(ModContent.BuffType<FleaBellBuff>()))
        {
            Projectile.timeLeft = 2;
        }

        return true;
    }
    
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
	    if (IsEnhanced)
	    {
		    modifiers.FinalDamage *= EnhancedDamageMultiplier;
		    Projectile.localNPCHitCooldown = EnhancedHitCooldown;
	    }
	    else
	    {
		    Projectile.localNPCHitCooldown = HitCooldown;
	    }
    }

    private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
    {
        Vector2 idlePosition = owner.Center;
        idlePosition.Y += IdleOffsetY;
        
        CalculatePersonalOffset(owner);
        idlePosition += personalOffset;

        vectorToIdlePosition = idlePosition - Projectile.Center;
        distanceToIdlePosition = vectorToIdlePosition.Length();

        if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > TeleportDistance)
        {
            Projectile.position = idlePosition;
            Projectile.velocity *= VelocityResetFactor;
            Projectile.netUpdate = true;
        }

        float overlapVelocity = 0.04f;
        foreach (var other in Main.ActiveProjectiles)
        {
            if (other.whoAmI != Projectile.whoAmI && other.owner == Projectile.owner && 
                Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
            {
                if (Projectile.position.X < other.position.X)
                    Projectile.velocity.X -= overlapVelocity;
                else
                    Projectile.velocity.X += overlapVelocity;

                if (Projectile.position.Y < other.position.Y)
                    Projectile.velocity.Y -= overlapVelocity;
                else
                    Projectile.velocity.Y += overlapVelocity;
            }
        }
    }

   

    private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
    {
	    distanceFromTarget = DetectionRange;
	    targetCenter = Projectile.position;
	    foundTarget = false;

	    if (owner.HasMinionAttackTargetNPC)
	    {
		    NPC npc = Main.npc[owner.MinionAttackTargetNPC];
		    if (npc.active && npc.life > 0)
		    {
			    float between = Vector2.Distance(npc.Center, Projectile.Center);

			    if (between < 2000f)
			    {
				    distanceFromTarget = between;
				    targetCenter = npc.Center;
				    foundTarget = true;
			    }
		    }
	    }

	    if (!foundTarget)
	    {
		    foreach (var npc in Main.ActiveNPCs)
		    {
			    if (npc.CanBeChasedBy())
			    {
				    float between = Vector2.Distance(npc.Center, Projectile.Center);
				    bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
				    bool inRange = between < distanceFromTarget;
				    bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, 
					    npc.position, npc.width, npc.height);
				    bool closeThroughWall = between < 100f;

				    if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
				    {
					    distanceFromTarget = between;
					    targetCenter = npc.Center;
					    foundTarget = true;
				    }
			    }
		    }
	    }
    }

public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    SoundEngine.PlaySound(FleaBell.BellHitSound, Projectile.Center);

    State = AIState.Bouncing;
    StateTimer = 0f;
    hasBounceDestination = false;

    Vector2 direction = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitX);
    float bounceSpeed = IsEnhanced ? BounceSpeed * EnhancedSpeedMultiplier : BounceSpeed;
    Projectile.velocity = direction * bounceSpeed;
}



private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, 
    float distanceToIdlePosition, Vector2 vectorToIdlePosition)
{
    float speedMultiplier = IsEnhanced ? EnhancedSpeedMultiplier : 1f;

    if (foundTarget && State == AIState.Following)
    {
        State = AIState.Charging;
        StateTimer = 0f;
        hoverTimer = 0f;
    }

    if (State == AIState.Charging)
    {
        StateTimer++;
        
        if (!foundTarget || distanceFromTarget > DetectionRange * 2f)
        {
            TargetWhoAmI = -1;
            State = AIState.Following;
            StateTimer = 0f;
            Projectile.friendly = false;
            
            Player owner = Main.player[Projectile.owner];
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC targetNPC = Main.npc[owner.MinionAttackTargetNPC];
                if (!targetNPC.active || targetNPC.life <= 0 || Vector2.Distance(targetNPC.Center, Projectile.Center) > DetectionRange * 2f)
                {
                    owner.MinionAttackTargetNPC = -1;
                }
            }
            return;
        }
        
        Vector2 direction = targetCenter - Projectile.Center;
        direction.Normalize();
        Projectile.velocity = direction * ChargeSpeed * speedMultiplier;
        Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;
        
        Projectile.friendly = true;
    }
    else if (State == AIState.Following)
    {
        TargetWhoAmI = -1;
        Projectile.friendly = false;
        
        Vector2 idlePosition = Main.player[Projectile.owner].Center;
        idlePosition.Y += IdleOffsetY;
        CalculatePersonalOffset(Main.player[Projectile.owner]);
        idlePosition += personalOffset;
        
        float hoverOffset = (float)Math.Sin(hoverTimer) * 8f; 
        idlePosition.Y += hoverOffset;
        
        Vector2 vectorToIdle = idlePosition - Projectile.Center;
        float distanceToIdle = vectorToIdle.Length();

        if (distanceToIdle > IdleThreshold)
        {
            float moveSpeed = 8f * speedMultiplier; 
            if (distanceToIdle > 600f)
            {
                moveSpeed = 12f * speedMultiplier;
            }
            
            vectorToIdle.Normalize();
            vectorToIdle *= moveSpeed;
            
            Projectile.velocity = (Projectile.velocity * 39f + vectorToIdle) / 40f;
        }
        else
        {
            Vector2 randomMovement = Main.rand.NextVector2Circular(0.8f, 0.8f);
            Projectile.velocity += randomMovement * 0.1f;
            Projectile.velocity *= 0.96f; 
        }
        
        hoverTimer += HoverFrequency;
        
        if (Math.Abs(Projectile.velocity.X) > 0.1f)
        {
            Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
        }
    }
    else if (State == AIState.Bouncing)
    {
        TargetWhoAmI = -1;
        Projectile.friendly = false;
        
        StateTimer++;
        
        Player owner = Main.player[Projectile.owner];
        
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
}
    private void Visuals()
    {
        if (State != AIState.Charging)
        {
            Projectile.rotation = Projectile.velocity.X * 0.05f;
        }

        HandleAnimation();

        if (IsEnhanced && Main.rand.NextBool(3))
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.scale = 0.8f;
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

    public override bool? CanCutTiles()
    {
	    return false;
    }

    public override bool MinionContactDamage()
    {
	    return true;
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