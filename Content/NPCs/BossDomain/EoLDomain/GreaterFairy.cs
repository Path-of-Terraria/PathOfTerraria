using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using ReLogic.Content;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;

internal class GreaterFairy : ModNPC
{
	private static Asset<Texture2D> WingTex = null;
	private static Asset<Texture2D> GlowTex = null;

	private Player Target => Main.player[NPC.target];

	private ref float Timer => ref NPC.ai[0];

	public override void SetStaticDefaults()
	{
		WingTex = ModContent.Request<Texture2D>(Texture + "_Wings");
		GlowTex = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(26);
		NPC.damage = 0;
		NPC.aiStyle = -1;
		NPC.defense = 50;
		NPC.lifeMax = 1000;
		NPC.noGravity = true;
		NPC.knockBackResist = 0f;
		NPC.noTileCollide = true;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Pixie, 4));
			c.AddDust(new(DustID.Pixie, 16, NPCHitEffects.OnDeath));

			for (int i = 0; i < 4; ++i) 
			{ 
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
			}
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "TheHallow");
	}

	public override void AI()
	{
		NPC.TargetClosest(false);
		
		if (NPC.direction == 0)
		{
			NPC.direction = 1;
		}

		Timer++;

		if (Math.Abs(NPC.velocity.X) > 0.001f)
		{
			NPC.direction = Math.Sign(NPC.velocity.X);
		}

		if (Timer < 160)
		{
			NPC.velocity *= 0.98f;
			NPC.damage = 0;
		}
		else if (Timer < 400)
		{
			NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center) * 12, (Timer - 160) / 340f * 0.2f);
			NPC.damage = ModeUtils.ByMode(60, 90, 140, 190);
		}
		else if (Timer == 400)
		{
			Timer = 0;
		}
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter++;
	}

	public override Color? GetAlpha(Color drawColor)
	{
		return NPC.color == Color.Transparent ? Color.White : NPC.color;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		SpriteEffects effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		int frameHeight = WingTex.Height() / 4;
		var src = new Rectangle(0, frameHeight * (int)(NPC.frameCounter * 0.2f % 4), WingTex.Width(), frameHeight);
		Vector2 origin = new(NPC.direction == 1 ? 42 : 36, 22);
		Main.EntitySpriteDraw(WingTex.Value, NPC.Center - screenPos, src, NPC.GetAlpha(Color.White), NPC.rotation, origin, 1f, effects, 0);
		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Vector2 pos = NPC.Center - screenPos + new Vector2(0, 4);
		Main.EntitySpriteDraw(GlowTex.Value, pos, null, NPC.GetAlpha(Color.White), NPC.rotation, GlowTex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}
