using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace PathOfTerraria.Content.NPCs.BossDomain.SunDevourerDomain;

public sealed class SunDevourerNPC : ModNPC
{
	#region Durations

	/// <summary>
	///		Represents the duration of the idle state of the NPC, in ticks.
	/// </summary>
	public const int IDLE_DURATION = 5 * 60;

	/// <summary>
	///		Represents the duration of the charge focus of the NPC, in ticks.
	/// </summary>
	public const int FOCUS_DURATION_CHARGE = 3 * 60;

	/// <summary>
	///		Represents the duration of the eruption focus of the NPC, in ticks.
	/// </summary>
	public const int FOCUS_DURATION_ERUPTION = 90;

	/// <summary>
	///		Represents the duration of the sandstorm focus of the NPC, in ticks.
	/// </summary>
	public const int FOCUS_DURATION_SANDSTORM = 3 * 60;

	#endregion

	#region Counts
	
	/// <summary>
	///		Represents the amount of dashes the NPC performs during the charge state.
	/// </summary>
	public const int COUNT_CHARGE = 3;

	/// <summary>
	///		Represents the amount of projectiles the NPC shoots during the eruption state.
	/// </summary>
	public const int COUNT_ERUPTION = 50;
	
	#endregion
	
	#region Cooldowns

	/// <summary>
	///		Represents the cooldown between each dash of the NPC during the charge state, in ticks.
	/// </summary>
	public const int COOLDOWN_CHARGE = 90;

	#endregion
		
	#region Modes
	
	/// <summary>
	///		Represents the identity of the day time mode of the NPC.
	/// </summary>
	public const float MODE_DAY_TIME = 0f;

	/// <summary>
	///		Represents the identity of the night time mode of the NPC.
	/// </summary>
	public const float MODE_NIGHT_TIME = 1f;
	
	#endregion
	
	#region States

	/// <summary>
	///		Represents the identity of the idle state of the NPC.
	/// </summary>
	public const float STATE_IDLE = 0f;

	/// <summary>
	///		Represents the identity of the charge state of the NPC.
	/// </summary>
	public const float STATE_CHARGE = 1f;

	/// <summary>
	///		Represents the identity of the eruption state of the NPC.
	/// </summary>
	public const float STATE_ERUPTION = 2f;
	
	/// <summary>
	///		Represents the identity of the sandstorm state of the NPC.
	/// </summary>
	public const float STATE_SANDSTORM = 3f;
	
	#endregion
	
	#region Slots

	/// <summary>
	///		Gets or sets the previous state of the NPC. Shorthand for <c>NPC.localAI[0]</c>.
	/// </summary>
	public ref float Previous => ref NPC.localAI[0];
	
	/// <summary>
	///		Gets or sets the mode of the NPC. Shorthand for <c>NPC.ai[0]</c>.
	/// </summary>
	public ref float Mode => ref NPC.ai[0];
	
	/// <summary>
	///		Gets or sets the state of the NPC. Shorthand for <c>NPC.ai[1]</c>.
	/// </summary>
	public ref float State => ref NPC.ai[1];
	
	/// <summary>
	///		Gets or sets the timer of the NPC. Shorthand for <c>NPC.ai[2]</c>.
	/// </summary>
	public ref float Timer => ref NPC.ai[2];

	/// <summary>
	///		Gets or sets the counter of the NPC. Shorthand for <c>NPC.ai[3]</c>.
	/// </summary>
	public ref float Counter => ref NPC.ai[3];
	
	#endregion

	/// <summary>
	///		Gets whether the NPC is in day mode.
	/// </summary>
	public bool Day => Mode == MODE_DAY_TIME;
	
	/// <summary>
	///		Gets whether the NPC is in night mode.
	/// </summary>
	public bool Night => Mode == MODE_NIGHT_TIME;

	/// <summary>
	///		Gets whether the NPC is in eclipse mode.
	/// </summary>
	public bool Eclipse => NPC.life <= NPC.lifeMax / 6f;

	/// <summary>
	///		Gets the movement speed of the NPC.
	/// </summary>
	public float Speed => Eclipse ? 8f : 6f;

	/// <summary>
	///		Gets the rate at which the NPC shoots projectiles.
	/// </summary>
	public float Cooldown => Eclipse ? 5f : 10f;
	
	/// <summary>
	///		Gets the <see cref="Player"/> instance that the NPC is targeting. Shorthand for <c>Main.player[NPC.target]</c>.
	/// </summary>
	public Player Player => Main.player[NPC.target];

	private float Opacity
	{
		get => opacity;
		set => opacity = MathHelper.Clamp(value, 0f, 1f);
	}
	
	private float opacity;
	
	private Vector2 direction;

	#region Defaults
	
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		NPCID.Sets.TrailingMode[Type] = 3;
		NPCID.Sets.TrailCacheLength[Type] = 10;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.noTileCollide = true;
		NPC.lavaImmune = true;
		NPC.noGravity = true;
		NPC.boss = true;
		
		NPC.width = 20;
		NPC.height = 20;

		NPC.lifeMax = 10000;
		NPC.defense = 20;

		NPC.aiStyle = -1;

		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
	}
	
	#endregion

	#region Events
	
	public override void OnSpawn(IEntitySource source)
	{
		base.OnSpawn(source);
		
		Mode = Main.dayTime ? MODE_DAY_TIME : MODE_NIGHT_TIME;

		if (Night)
		{
			return;
		}
		
		Main.Moondialing();
	}

	public override void OnKill()
	{
		base.OnKill();

		if (Night)
		{
			return;
		}
		
		Main.Sundialing();
	}
	
	#endregion

	#region Network

	public override void SendExtraAI(BinaryWriter writer)
	{
		base.SendExtraAI(writer);
		
		writer.Write(Previous);
		
		writer.WriteVector2(direction);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		base.ReceiveExtraAI(reader);

		Previous = reader.ReadSingle();
		
		direction = reader.ReadVector2();
	}

	#endregion

	#region Behavior
	
	public override void AI()
	{
		base.AI();

		NPC.TargetClosest();

		switch (State)
		{
			case STATE_IDLE:
				UpdateIdle();
				break;
			case STATE_CHARGE:
				UpdateCharge();
				break;
			case STATE_ERUPTION:
				UpdateEruption();
				break;
			case STATE_SANDSTORM:
				UpdateSandstorm();
				break;
		}
		
		NPC.rotation = NPC.velocity.X * 0.1f;
	}
	
	private void UpdateIdle()
	{
		if (Day)
		{
			UpdateIdle_Day();
		}
		else
		{
			UpdateIdle_Night();
		}
	}

	private void UpdateIdle_Day()
	{
		Timer++;

		var radius = 16f * 16f;
		var angle = Timer * 0.025f;
    
		var position = Player.Center + new Vector2(radius, 0f).RotatedBy(angle) + Player.velocity;
		
		var direction = NPC.DirectionTo(position);
		var velocity = direction * Speed;

		NPC.velocity = Vector2.SmoothStep(NPC.velocity, velocity, 0.25f);

		if (Timer < IDLE_DURATION)
		{
			return;
		}

		switch (Previous)
		{
			case STATE_IDLE:
				UpdateState(STATE_CHARGE);
				break;
			case STATE_CHARGE:
				ApplyFocus(FOCUS_DURATION_ERUPTION);

				UpdateState(STATE_ERUPTION);
				break;
			case STATE_ERUPTION:
				ApplyFocus(FOCUS_DURATION_CHARGE);

				UpdateState(STATE_SANDSTORM);
				break;
			case STATE_SANDSTORM:
				ApplyFocus(FOCUS_DURATION_SANDSTORM);
				
				UpdateState(STATE_IDLE);
				break;
		}
	}

	private void UpdateIdle_Night()
	{
		// TODO: Behavior.
	}

	private void UpdateCharge()
	{
		if (Day)
		{
			UpdateCharge_Day();
		}
		else
		{
			UpdateCharge_Night();
		}
	}

	private void UpdateCharge_Day()
	{
		Timer++;
		
		if (Timer < COOLDOWN_CHARGE)
		{
			NPC.velocity *= 0.95f;
		}
		else
		{
			if (Timer == COOLDOWN_CHARGE)
			{
				direction = NPC.DirectionTo(Player.Center + Player.velocity * 2f);
				
				Counter++;
				
				NPC.netUpdate = true;
			}
			
			if (Timer > COOLDOWN_CHARGE)
			{
				Timer = 0f;
				
				NPC.netUpdate = true;
			}
			else
			{
				var velocity = direction * Speed * 2f;

				ApplyVelocity(velocity);
			}
		}

		if (Counter <= COUNT_ERUPTION)
		{
			return;
		}

		UpdateState(STATE_IDLE);
	}

	private void UpdateCharge_Night()
	{
		// TODO: Behavior.
	}

	private void UpdateEruption()
	{
		var position = Player.Center - new Vector2(0f, 24f * 16f) + new Vector2(0f, 64f).RotatedBy(Main.GameUpdateCount * 0.1f);

		ApplyMovement(position, Speed * 2f);
		
		Timer++;

		if (Timer < Cooldown)
		{
			return;
		}
		
		var origin = new Vector2(0f, 1f).RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(16f, 24f);
		var offset = new Vector2(Main.rand.Next(-Main.LogicCheckScreenWidth / 2, Main.LogicCheckScreenWidth / 2), -Main.LogicCheckScreenHeight / 2f);
		
		Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, origin, ModContent.ProjectileType<SunDevourerEruptionProjectile>(), 20, 1f, -1, 0f, Player.whoAmI);
		
		Timer = 0f;
		Counter++;

		NPC.netUpdate = true;
		
		if (Counter <= COUNT_ERUPTION)
		{
			return;
		}

		UpdateState(STATE_IDLE);
	}
	
	private void UpdateSandstorm()
	{
		NPC.velocity *= 0.95f;
	
		Timer++;

		Player.velocity += Player.DirectionTo(NPC.Center) * 0.1f;

		if (Timer < 10 * 60)
		{
			return;
		}

		UpdateState(STATE_IDLE);
	}

	private void UpdateState(float state)
	{
		Previous = State;
		State = state;

		Timer = 0f;
		Counter = 0f;

		NPC.netUpdate = true;
	}

	private void ApplyMovement(Vector2 position, float speed)
	{
		var direction = NPC.DirectionTo(position);
		var distance = Vector2.DistanceSquared(NPC.Center, position);

		if (distance < 16f * 16f)
		{
			NPC.velocity *= 0.95f; 
		}

		var velocity = direction * speed;

		ApplyVelocity(velocity);
	}

	private void ApplyVelocity(Vector2 velocity)
	{
		NPC.velocity = Vector2.SmoothStep(NPC.velocity, velocity, 0.25f);
	}
	
	#endregion
	
	#region Effects

	private void ApplyFocus(int duration)
	{
		ZoomSystem.AddModifier
		(
			"SunDevourer", 
			duration,
			(ref SpriteViewMatrix matrix, float progress) =>
			{
				var multiplier = 1f - progress; 
				var fade = MathF.Sin(multiplier * MathF.PI); 

				matrix.Zoom = new Vector2(1f + 1f * fade);
			}
		);
		
		Main.instance.CameraModifiers.Add(new FocusCameraModifier(() => NPC.Center + NPC.Size / 2f, duration / 2));
	}
	
	#endregion

	#region Loot

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		base.ModifyNPCLoot(npcLoot);
	}

	public override void BossLoot(ref string name, ref int potionType)
	{
		base.BossLoot(ref name, ref potionType);

		potionType = ItemID.SuperHealingPotion;
	}
	
	#endregion
	
	#region Rendering
	
	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
	{
		scale = 1.5f;
		
		return null;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (State != STATE_IDLE)
		{
			Opacity += 0.05f;
		}
		else
		{
			Opacity -= 0.01f;
		}
		
		DrawNPCAfterimage(in screenPos, in drawColor);
		DrawNPC(in screenPos, in drawColor);
		
		return false;
	}

	private void DrawNPC(in Vector2 screenPosition, in Color drawColor)
	{
		var texture = TextureAssets.Npc[Type].Value;

		var position = NPC.Center - Main.screenPosition + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);

		var origin = NPC.frame.Size() / 2f;

		var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		
		Main.EntitySpriteDraw(texture, position, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);
	}
	
	private void DrawNPCAfterimage(in Vector2 screenPosition, in Color drawColor)
	{
		var texture = TextureAssets.Npc[Type].Value;

		var origin = NPC.frame.Size() / 2f;
		
		var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		
		var length = NPCID.Sets.TrailCacheLength[Type];

		for (var i = 0; i < length; i += 2)
		{
			var multiplier = 1f - i / (float)length;
			
			var position = NPC.oldPos[i] + NPC.Size / 2f - Main.screenPosition + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);

			var color = NPC.GetAlpha(drawColor) * multiplier * Opacity;
			
			Main.EntitySpriteDraw(texture, position, NPC.frame, color, NPC.oldRot[i], origin, NPC.scale, effects);
		}
	}
	
	#endregion
}