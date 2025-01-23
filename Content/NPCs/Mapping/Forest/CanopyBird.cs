using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using ReLogic.Content;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest;

//[AutoloadBanner]
internal class CanopyBird : ModNPC
{
	public const int SwingWaitTimer = 60;
	public const int SlamWaitTimer = 40;

	private static Asset<Texture2D> Glow = null;

	public enum AIState
	{
		Chase,
		SwingProj,
		Slam
	}

	public Player Target => Main.player[NPC.target];

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
		Main.npcFrameCount[Type] = 1;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 2f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(50, 124);
		NPC.aiStyle = -1;
		NPC.lifeMax = 450;
		NPC.defense = 35;
		NPC.damage = 65;
		NPC.knockBackResist = 0.3f;
		NPC.HitSound = SoundID.NPCHit30;

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				//for (int i = 0; i < 6; ++i)
				//{
				//	c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
				//}

				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 5));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 1));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 20, NPCHitEffects.OnDeath));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 10, NPCHitEffects.OnDeath));
			}
		);
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(450, 600, 900);
		NPC.damage = ModeUtils.ByMode(65, 130, 195);
		NPC.knockBackResist = ModeUtils.ByMode(0.3f, 0.27f, 0.23f);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void AI()
	{
	}

	public override void FindFrame(int frameHeight)
	{

	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		spriteBatch.Draw(Glow.Value, NPC.Center - screenPos, drawColor);

		return true;
	}
}
