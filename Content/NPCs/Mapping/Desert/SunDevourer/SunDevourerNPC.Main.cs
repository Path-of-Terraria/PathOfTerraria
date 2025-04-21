using NPCUtils;
using PathOfTerraria.Core.Graphics.Camera.Modifiers;
using PathOfTerraria.Core.Graphics.Zoom;
using PathOfTerraria.Core.Graphics.Zoom.Modifiers;
using PathOfTerraria.Core.Physics.Verlet;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

[AutoloadBossHead]
public sealed partial class SunDevourerNPC : ModNPC
{
	public enum DevourerState : byte
	{
		Trapped,
		Rise,
		ReturnToIdle,
		Firefall,
		FlameAdds,
	}

	public const int COOLDOWN_CHARGE = 90;

	/// <summary>
	///		Gets the <see cref="Player"/> instance that the NPC is targeting. Shorthand for <c>Main.player[NPC.target]</c>.
	/// </summary>
	public Player Player => Main.player[NPC.target];

	public Vector2 IdleSpot => new(NPC.ai[0], NPC.ai[1]);

	/// <summary>
	///		Gets or sets the state of the NPC. Shorthand for <c>NPC.ai[1]</c>.
	/// </summary>
	public DevourerState State
	{
		get => (DevourerState)NPC.ai[2];
		set => NPC.ai[2] = (float)value;
	}

	/// <summary>
	///		Gets or sets the timer of the NPC. Shorthand for <c>NPC.ai[2]</c>.
	/// </summary>
	public ref float Timer => ref NPC.ai[3];

	/// <summary>
	///		Gets or sets the previous state of the NPC. Shorthand for <c>NPC.localAI[0]</c>.
	/// </summary>
	public ref float MiscData => ref NPC.localAI[0];

	private VerletChain chain;
	private Vector2[] bezier;

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
		NPC.knockBackResist = 0;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Desert");
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override void BossHeadRotation(ref float rotation)
	{
		rotation = NPC.rotation;
	}

	public override void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
	{
		spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
	}

	public override void BossLoot(ref string name, ref int potionType)
	{
		base.BossLoot(ref name, ref potionType);

		potionType = ItemID.GreaterHealingPotion;
	}

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
	{
		scale = 1.5f;

		return null;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		//if (chain == null)
		//{
		//	chain = VerletChainBuilder.CreatePinnedRope(NPC.Center, 10, 32f, 0.9f);
		//}
		//else
		//{
		//	// TODO: Just for testing purposes. Eventually make this follow the NPC's tail position.
		//	chain.Points[0].Position = Main.MouseWorld;

		//	chain.Update();
		//	chain.Render(new SunDevourerVerletRenderer());
		//}

		DrawNPC(in screenPos, in drawColor);

		return false;
	}

	private void ApplyFocus(int duration)
	{
		ZoomSystem.AddModifier(new FocusZoomModifier($"{PoTMod.ModName}:{nameof(SunDevourerNPC)}_Zoom", duration));

		Main.instance.CameraModifiers.Add(new FocusCameraModifier($"{PoTMod.ModName}:{nameof(SunDevourerNPC)}_Camera", duration, () => NPC.Center + NPC.Size / 2f));
	}

	private void DrawNPC(in Vector2 screenPosition, in Color drawColor)
	{
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Vector2 position = NPC.Center - screenPosition + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);
		Vector2 origin = NPC.frame.Size() / 2f;
		SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		Main.EntitySpriteDraw(texture, position, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);
	}
}