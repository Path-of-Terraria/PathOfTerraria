using System.Collections.Generic;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Core.Items;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons;

public class Ouroboros : Gear
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

		ElementalWeaponSets.WeaponElementProportionsById[Type] = new Dictionary<ElementType, float>
		{
			{ ElementType.Fire, 0.0f },
			{ ElementType.Cold, 0.0f },
			{ ElementType.Lightning, 0.0f },
			{ ElementType.Chaos, 1.0f }
		};
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 24;
		Item.DamageType = DamageClass.MeleeNoSpeed;
		Item.width = 42;
		Item.height = 38;
		Item.useTime = 25;
		Item.useAnimation = 25;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.knockBack = 2.5f;
		Item.value = Item.buyPrice(gold: 3);
		Item.UseSound = SoundID.Item1;
		Item.shoot = ModContent.ProjectileType<OuroborosProjectile>();
		Item.shootSpeed = 16f;
		Item.channel = true;
		Item.noMelee = true;
		Item.noUseGraphic = true;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override void HoldItem(Player player)
	{
		if (Main.mouseRight && Main.mouseRightRelease && player.channel)
		{
			var altUsePlayer = player.GetModPlayer<AltUsePlayer>();
			if (altUsePlayer.AltFunctionAvailable)
			{
				foreach (Projectile proj in Main.ActiveProjectiles)
				{
					if (proj.owner == player.whoAmI && proj.ModProjectile is OuroborosProjectile yoyo)
					{
						yoyo.ActivateDevour();
						altUsePlayer.SetAltCooldown(8 * 60);
						Main.mouseRightRelease = false;
						break;
					}
				}
			}
		}
	}
}

public class OuroborosProjectile : ModProjectile
{
    private const int PoisonGasInterval = 8;
    private const int DevourDuration = 4 * 60; 
    private const float DevourRange = 200f;
    private const float DevourExecuteThreshold = 0.15f;
    private const float DevourPullStrength = 8f;
    private const float DevourRangeSquared = DevourRange * DevourRange;

    private int poisonGasTimer = 0;
    private bool devourActive = false;
    private int devourTimer = 0;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 8f;
        ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 280f;
        ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 13f;
    }

    public override void SetDefaults()
    {
        Projectile.extraUpdates = 0;
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = 99;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.scale = 1f;
    }

    /// <summary>
    /// Activates the devour ability, pulling enemies in and executing low health targets
    /// </summary>
    public void ActivateDevour()
    {
        devourActive = true;
        devourTimer = DevourDuration;
        SoundEngine.PlaySound(SoundID.Roar, Projectile.Center);
    }

    public override void AI()
    {
        HandlePoisonGasTrail();

        if (devourActive)
        {
            HandleDevourMode();
        }
    }

    /// <summary>
    /// Creates poison gas clouds along the yoyo's trail
    /// </summary>
    private void HandlePoisonGasTrail()
    {
	    poisonGasTimer++;
    
	    if (poisonGasTimer >= PoisonGasInterval)
	    {
		    poisonGasTimer = 0;
        
		    if (Main.myPlayer == Projectile.owner)
		    {
			    Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
            
			    // Check if there's already a gas cloud in the vicinity
			    if (!IsGasCloudNearby(spawnPosition))
			    {
				    Projectile.NewProjectileDirect(
					    Projectile.GetSource_FromThis(),
					    spawnPosition,
					    Vector2.Zero,
					    ModContent.ProjectileType<OuroborosPoisonGas>(),
					    Projectile.damage / 10,
					    0,
					    Projectile.owner
				    );
			    }
		    }
	    }
    }
    
    /// <summary>
    /// Checks if there's already a gas cloud within rough overlapping distance of the spawn position
    /// </summary>
    /// <param name="spawnPosition">Position where we want to spawn a new gas cloud</param>
    /// <returns>True if there's a gas cloud nearby that would overlap</returns>
    private bool IsGasCloudNearby(Vector2 spawnPosition)
    {
	    const float overlapCheckRadius = 50f; 
    
	    for (int i = 0; i < Main.maxProjectiles; i++)
	    {
		    Projectile proj = Main.projectile[i];
		    if (proj.active && 
		        proj.type == ModContent.ProjectileType<OuroborosPoisonGas>() && 
		        proj.owner == Projectile.owner)
		    {
			    float distance = Vector2.Distance(proj.Center, spawnPosition);
			    if (distance < overlapCheckRadius)
			    {
				    return true;
			    }
		    }
	    }
    
	    return false;
    }
    
    /// <summary>
    /// Handles the devour mode mechanics - pulling enemies and executing low health targets
    /// </summary>
    private void HandleDevourMode()
    {
	    devourTimer--;
    
	    if (devourTimer <= 0)
	    {
		    devourActive = false;
		    return;
	    }

	    //placeholder visuals
	    if (Main.rand.NextBool(2))
	    {
		    var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.VenomStaff);
		    dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
		    dust.noGravity = true;
		    dust.scale = 1.2f;
	    }
	    
	    // Pull enemies towards the yoyo - we can probably make this stronger later
	    for (int i = 0; i < Main.maxNPCs; i++)
	    {
		    NPC npc = Main.npc[i];
		    if (!npc.active || npc.life <= 0 || !npc.CanBeChasedBy()) continue;

		    float distanceSquared = npc.DistanceSQ(Projectile.Center);
		    if (distanceSquared <= DevourRangeSquared)
		    {
			    if ((float)npc.life / npc.lifeMax <= DevourExecuteThreshold)
			    {
				    ExecuteEnemy(npc);
			    }
			    else
			    {
				    Vector2 pullDirection = (Projectile.Center - npc.Center).SafeNormalize(Vector2.UnitX);
				    float distance = (float)Math.Sqrt(distanceSquared);
				    float pullForce = DevourPullStrength * (1f - (distance / DevourRange));
				    npc.velocity += pullDirection * pullForce * 0.1f;
			    }
		    }
	    }
    }

    /// <summary>
    /// Instantly kills an enemy and creates dramatic effects
    /// </summary>
    /// <param name="npc">The enemy to execute</param>
    private void ExecuteEnemy(NPC npc)
    {
        if (Main.myPlayer != Projectile.owner)
        {
	        return;
        }

        var hitInfo = new NPC.HitInfo()
        {
	        InstantKill = true,
	        Knockback = 0f,
	        HitDirection = 0,
	        DamageType = DamageClass.MeleeNoSpeed
        };

        npc.StrikeNPC(hitInfo);

        SoundEngine.PlaySound(SoundID.NPCDeath1, npc.Center);
        
        for (int i = 0; i < 15; i++)
        {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood);
            dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
            dust.scale = 1.5f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
		PoisonedDebuff.Apply(target, 5 * 60, Main.player[Projectile.owner]);
    }

    public override Color? GetAlpha(Color lightColor)
    {
        if (devourActive)
        {
            return Color.Lerp(lightColor, Color.DarkGreen, 0.7f * (devourTimer / (float)DevourDuration));
        }
        return base.GetAlpha(lightColor);
    }

    public bool RightClick(Player player, bool mouseDirectlyOver)
    {
        if (mouseDirectlyOver && !devourActive)
        {
            var altUsePlayer = player.GetModPlayer<AltUsePlayer>();
            if (altUsePlayer.AltFunctionAvailable)
            {
                ActivateDevour();
                altUsePlayer.SetAltCooldown(8 * 60);
                return true;
            }
        }
        return false;
    }
    
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
	    behindNPCs.Add(index);
    }
    
    public override bool PreDrawExtras()
    {
	    Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
	    Vector2 center = Projectile.Center;
	    Vector2 distToProj = playerCenter - Projectile.Center;
	    float projRotation = distToProj.ToRotation() - MathHelper.PiOver2;
	    float distance = distToProj.Length();
    
	    var stringTexture = ModContent.Request<Texture2D>("PathOfTerraria/Content/Items/Gear/Weapons/OuroborosString");
    
	    while (distance > 12f && !float.IsNaN(distance))
	    {
		    distToProj.Normalize();
		    distToProj *= 12f;
		    center += distToProj;
		    distance = (playerCenter - center).Length();
        
		    Color drawColor = Lighting.GetColor((int)center.X / 16, (int)center.Y / 16);

        
		    Main.spriteBatch.Draw(
			    stringTexture.Value,
			    center - Main.screenPosition,
			    null,
			    drawColor,
			    projRotation,
			    stringTexture.Value.Size() * 0.5f,
			    1f,
			    SpriteEffects.None,
			    0f
		    );
	    }
    
	    return false; 
    }
}

public class OuroborosPoisonGas : ModProjectile
{
    private const int GasDuration = 3 * 60;
    private const float GasRadius = 64f;
    private const int PoisonTickInterval = 30; 

    private int poisonTickTimer = 0;

    public override void SetDefaults()
    {
	    Projectile.Size = new(GasRadius * 2); 
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.timeLeft = GasDuration;
        Projectile.alpha = 100;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = PoisonTickInterval;
    }

    public override void AI()
    {
	    Projectile.alpha = (int)MathHelper.Lerp(100, 255, 1f - (Projectile.timeLeft / (float)GasDuration));

	    // another placeholder for visual fx
	    if (Main.rand.NextBool(3))
	    {
		    var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.JungleSpore);
		    dust.velocity *= 0.1f;
		    dust.noGravity = true;
		    dust.alpha = 150;
		    dust.scale = 0.8f;
	    }
    }
    
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
	    PoisonedDebuff.Apply(target, 5 * 60, Main.player[Projectile.owner]);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}