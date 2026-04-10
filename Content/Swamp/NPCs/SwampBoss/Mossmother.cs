using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

#nullable enable

[AutoloadBossHead]
internal partial class Mossmother : ModNPC
{
	public enum BehaviorState : byte
	{
		Huddled,
		SpawnAnimation,
		MoveToWall,
		IdleInWall,
		GasCrawl,
		Desperation,
		PreDesperation,
	}

	public BehaviorState State
	{
		get => (BehaviorState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	public const int SplineCountMax = 30;
	public const int TrailingIndex = 20;
	public const int PosionAuraRadiusSize = 340;

	private static Asset<Texture2D> Face = null;

	private ref float Timer => ref NPC.ai[1];
	private ref float MiscNumber => ref NPC.ai[2];

	private ref float LastWallNodeSelected => ref NPC.ai[3];
	
	private bool ReadyForGas
	{
		get => NPC.localAI[0] == 1;
		set => NPC.localAI[0] = value ? 1 : 0;
	}

	private ref float TimesWallCrawled => ref NPC.localAI[1];
	private ref float MiscTwo => ref NPC.localAI[2];
	private ref float VisualVariant => ref NPC.localAI[3];

	private Vector2[] movementSpline = new Vector2[30];

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TrailCacheLength[Type] = TrailingIndex;
		NPCID.Sets.TrailingMode[Type] = 1;

		NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { PortraitPositionYOverride = -50, PortraitPositionXOverride = 4 };

		Face = ModContent.Request<Texture2D>(Texture + "_Face");
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(180);
		NPC.lifeMax = 80_000;
		NPC.defense = 15;
		NPC.aiStyle = -1;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.boss = true;
		NPC.knockBackResist = 0f;
		NPC.damage = 1;
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.damage = ModeUtils.ByMode(60, 75, 100, 150);
		NPC.lifeMax = 40_000;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
	{
		if (State == BehaviorState.MoveToWall)
		{
			modifiers.FinalDamage -= 0.4f;
		}
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return State == BehaviorState.Desperation;
	}

	public override bool? CanBeHitByProjectile(Projectile projectile)
	{
		return State is BehaviorState.Huddled or BehaviorState.SpawnAnimation ? false : null;
	}

	public override bool? CanBeHitByItem(Player player, Item item)
	{
		return State is BehaviorState.Huddled or BehaviorState.SpawnAnimation ? false : null;
	}

	public override bool CanBeHitByNPC(NPC attacker)
	{
		return State is not BehaviorState.Huddled and not BehaviorState.SpawnAnimation;
	}

	public override void BossHeadRotation(ref float rotation)
	{
		rotation = NPC.rotation;
	}

	public override void OnKill()
	{
		if (NPC.CountNPCS(Type) > 1)
		{
			return;
		}

		Vector2 pos = new(SwampArea.ArenaMiddleX * 16, (SwampArea.FloorY - 20) * 16);
		Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter += 0.2f;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		Rectangle frame = NPC.frame with { Width = 302 };
		Vector2 basePosition = NPC.Center - screenPos;

		NPC.rotation = NPC.AngleTo(Main.MouseWorld);
		VisualVariant = Main.LocalPlayer.selectedItem % 3;

		DrawTail(tex, basePosition, drawColor);

		DrawForelimb(tex, basePosition, new Vector2(50, 16), new Vector2(30, -6), drawColor, false); // Forelimb
		DrawForelimb(tex, basePosition, new Vector2(-70, 16), new Vector2(-8, -8), drawColor, true); // Flipped forelimb

		DrawBackLimb(tex, basePosition, [new Vector2(40, -26), new(14, -6), new Vector2(24, -2)], drawColor, false, false); // Middle limb
		DrawBackLimb(tex, basePosition, [new Vector2(-46, -26), new(-8, -6), new Vector2(-18, -2)], drawColor, true, false); // Flipped middle limb

		DrawBackLimb(tex, basePosition, [new Vector2(24, -56), new(10, -4), new Vector2(14, 2)], drawColor, false, true); // Back limb
		DrawBackLimb(tex, basePosition, [new Vector2(-24, -56), new(-6, -4), new Vector2(-8, -2)], drawColor, true, true); // Flipped back limb

		DrawIndividualSegment(new Rectangle(0, 0, 136, 134), drawColor);
		DrawIndividualSegment(new Rectangle(0, 136, 136, 134), Color.White);

		var faceFrame = new Rectangle(82 * (int)(NPC.frameCounter % 8), (int)(84 * VisualVariant), 82, 84);
		DrawIndividualSegment(faceFrame, drawColor, -new Vector2(0, 40), Face.Value);

		return false;

		void DrawIndividualSegment(Rectangle src, Color color, Vector2? offset = null, Texture2D? textureOverride = null)
		{
			spriteBatch.Draw(textureOverride ?? tex, basePosition, src, color, NPC.rotation, src.Size() / 2f + (offset ?? Vector2.Zero), 1f, SpriteEffects.None, 0);
		}
	}

	private void DrawBackLimb(Texture2D texture, Vector2 basePosition, Vector2[] offsets, Color drawColor, bool flipped, bool wayBack)
	{
		Rectangle src = wayBack ? new(138, 10, 18, 18) : new(150, 28, 18, 18);
		Vector2 origin = wayBack ? new(6, 10) : new(6, 12);
		SpriteEffects effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		Vector2 offset = offsets[0].RotatedBy(NPC.rotation);

		Main.spriteBatch.Draw(texture, basePosition + offset, src, drawColor, NPC.rotation, origin, 1f, effects, 0);

		float sineRotation = (float)Math.Sin(NPC.frameCounter * 0.8f - (!flipped ? MathHelper.PiOver2 : 0) + (wayBack ? 2.3f : 3)) * 0.15f;
		float rotation = NPC.rotation + sineRotation;

		basePosition += offset;
		offset = offsets[1].RotatedBy(NPC.rotation);
		src = wayBack ? new(160, 2, 22, 14) : new(170, 20, 28, 14);
		origin = wayBack ? new(4, 12) : new(6, 8);

		if (flipped)
		{
			origin = new Vector2(src.Width - origin.X, origin.Y);
		}

		Main.spriteBatch.Draw(texture, basePosition + offset, src, drawColor, rotation, origin, 1f, effects, 0);

		rotation = NPC.rotation + sineRotation * 1.5f;
		basePosition += offset.RotatedBy(sineRotation * 1.6f);
		src = wayBack ? new(186, 2, 38, 6) : new(202, 18, 48, 26);
		offset = offsets[2].RotatedBy(NPC.rotation);

		if (flipped)
		{
			origin = new Vector2(44, origin.Y);
		}

		Main.spriteBatch.Draw(texture, basePosition + offset, src, drawColor, rotation, origin, 1f, effects, 0);
	}

	private void DrawForelimb(Texture2D texture, Vector2 basePosition, Vector2 offset, Vector2 hinge, Color drawColor, bool flipped)
	{
		Rectangle baseFrame = new(150, 50, 36, 34);
		Vector2 origin = new Vector2(8, 24);
		SpriteEffects effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		offset = offset.RotatedBy(NPC.rotation);

		Main.spriteBatch.Draw(texture, basePosition + offset, baseFrame, drawColor, NPC.rotation, origin, 1f, effects, 0);

		float rotation = NPC.rotation + (float)Math.Sin(NPC.frameCounter * 0.55f - (!flipped ? MathHelper.PiOver2 : 0)) * 0.35f;

		offset += hinge.RotatedBy(NPC.rotation);
		origin = new Vector2(8);
		baseFrame = new Rectangle(188, 62, 74, 78);

		if (flipped)
		{
			origin = new Vector2(baseFrame.Width - 8, 8);
		}

		Main.spriteBatch.Draw(texture, basePosition + offset, baseFrame, drawColor, rotation, origin, 1f, effects, 0);
	}

	private void DrawTail(Texture2D texture, Vector2 basePosition, Color drawColor)
	{
		float animSpeed = MathHelper.Lerp(0.14f, 0.25f, NPC.life / (float)NPC.lifeMax);
		float offset = (float)Math.Sin(NPC.frameCounter * 0.14f) * 0.1f;
		float offsetOpp = (float)Math.Sin(NPC.frameCounter * 0.14f + MathHelper.Pi) * 0.1f;

		Rectangle baseFrame = new(148, 104, 32, 16);
		Vector2[] bezierPoints = [new Vector2(0, 54), new Vector2(0, 84).RotatedBy(offset), new Vector2(0, 134), new Vector2(0, 174).RotatedBy(offsetOpp)];
		Vector2[] equidistantPoints = Tunnel.CreateEquidistantSet(bezierPoints, 16, false);
		Vector2 priorPosition = basePosition;

		for (int i = 0; i < equidistantPoints.Length; ++i)
		{
			Vector2 position = equidistantPoints[i].RotatedBy(NPC.rotation - MathHelper.Pi) + basePosition;
			float rotation = priorPosition.AngleTo(position) + MathHelper.PiOver2;
			Rectangle frame = baseFrame with { Y = 104 + (7 - i) * 18 };
			
			Main.spriteBatch.Draw(texture, position, frame, drawColor, rotation, baseFrame.Size() / 2f, 1f, SpriteEffects.None, 0);
			priorPosition = position;
		}
	}

	public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
	{
		boundingBox.Width /= 2;
		boundingBox.X += boundingBox.Width / 2;
	}
}
