using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.SkeletronDomain;

public sealed class ControllableSpikeball : ModNPC
{
	private ref float Speed => ref NPC.ai[0];

	private Vector2 Anchor
	{
		get => new(NPC.ai[1], NPC.ai[2]);

		set
		{
			NPC.ai[1] = value.X;
			NPC.ai[2] = value.Y;
		}
	}

	private ref float Length => ref NPC.ai[3];

	private bool IsInitialized
	{
		get => NPC.localAI[0] == 1f;
		set => NPC.localAI[0] = value ? 1f : 0f;
	}

	private ref float Timer => ref NPC.localAI[1];

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.SpikeBall);
		NPC.aiStyle = -1;
		NPC.Size = new Vector2(46);
		NPC.scale = 1f;
		NPC.ShowNameOnHover = false;
		NPC.netAlways = true;
	}

	public override void AI()
	{
		if (!IsInitialized && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Anchor = NPC.position - new Vector2(8, 2);

			if (Length == 0f)
			{
				Length = Main.rand.NextFloat(80, 120f);
			}

			NPC.netUpdate = true;
			IsInitialized = true;
		}

		Timer++;
		Vector2 offset = new Vector2(Length, 0).RotatedBy(Timer * Speed);
		NPC.rotation = offset.ToRotation();
		NPC.Center = Anchor + offset;
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Vector2 drawPos = NPC.Center;
		float xDiff = NPC.ai[1] - drawPos.X;
		float yDiff = NPC.ai[2] - drawPos.Y;
		bool continueDrawing = true;

		while (continueDrawing)
		{
			int drawHeight = 12;
			float distance = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff);

			if (distance < 20f)
			{
				drawHeight = (int)distance - 20 + 12;
				continueDrawing = false;
			}

			distance = 12f / distance;
			xDiff *= distance;
			yDiff *= distance;
			drawPos.X += xDiff;
			drawPos.Y += yDiff;
			xDiff = NPC.ai[1] - drawPos.X;
			yDiff = NPC.ai[2] - drawPos.Y;
			Color color = Lighting.GetColor((int)drawPos.X / 16, (int)(drawPos.Y / 16f));
			var origin = new Vector2(TextureAssets.Chain.Width() * 0.5f, TextureAssets.Chain.Height() * 0.5f);
			var source = new Rectangle(0, 0, TextureAssets.Chain.Width(), drawHeight);

			spriteBatch.Draw(TextureAssets.Chain.Value, drawPos - screenPos, source, color, NPC.rotation, origin, 1f, SpriteEffects.None, 0f);
		}

		var baseSource = new Rectangle(0, 0, TextureAssets.SpikeBase.Width(), TextureAssets.SpikeBase.Height());
		Color baseColor = Lighting.GetColor((int)NPC.ai[1] / 16, (int)(NPC.ai[2] / 16f));
		var baseOrigin = new Vector2(TextureAssets.SpikeBase.Width() * 0.5f, TextureAssets.SpikeBase.Height() * 0.5f);
		Vector2 basePosition = new Vector2(NPC.ai[1], NPC.ai[2]) - screenPos;

		spriteBatch.Draw(TextureAssets.SpikeBase.Value, basePosition, baseSource, baseColor, NPC.rotation - 0.75f, baseOrigin, 1f, SpriteEffects.None, 0f);
		return true;
	}
}