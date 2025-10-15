using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed partial class SunDevourerNPC : ModNPC
{
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		DrawNPC(in screenPos, in drawColor);
		return false;
	}

	public override void FindFrame(int frameHeight)
	{
		float target = 1;

		if (State == DevourerState.Trapped)
		{
			target = 0;
			NPC.frameCounter = 8;
		}
		else if (State == DevourerState.LightningAdds)
		{
			target = 0.5f;
		}
		else if (State == DevourerState.ReturnToIdle)
		{
			target = 0.8f;
		}
		else if (State == DevourerState.BallLightning)
		{
			target = 1.1f;
		}
		else if (State is DevourerState.Firefall or DevourerState.Dawning)
		{
			target = 1.2f;

			if (State == DevourerState.Firefall && AdditionalData == 1)
			{
				target = 1.3f;
			}
		}

		animSpeed = MathHelper.Lerp(animSpeed, target, 0.06f);
		
		NPC.frameCounter += animSpeed;
		NPC.frame.Y = frameHeight * (int)(NPC.frameCounter % 24f / 4f);
	}

	private void DrawNPC(in Vector2 screenPosition, in Color drawColor)
	{
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Vector2 position = NPC.Center - screenPosition + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);
		Rectangle frame = NPC.frame with { Width = 300 };
		Vector2 origin = frame.Size() / 2f;
		SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		if (flipVert)
		{
			effects |= SpriteEffects.FlipVertically;
		}

		if (State == DevourerState.Trapped)
		{
			DoChainActions(position, false);
		}

		Main.EntitySpriteDraw(TailTexture.Value, position, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);
		Main.EntitySpriteDraw(WingsTexture.Value, position, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);
		Rectangle bodyFrame = State == DevourerState.Trapped ? frame with { X = 300, Y = 0 } : frame;
		Main.EntitySpriteDraw(texture, position, bodyFrame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);

		if (State == DevourerState.Godrays && Timer <= GodrayHideTime)
		{
			Color maskColor = Color.White * (Timer / GodrayHideTime);
			Main.EntitySpriteDraw(TailTexture.Value, position, frame with { X = 300 }, maskColor, NPC.rotation, origin, NPC.scale, effects);
			Main.EntitySpriteDraw(WingsTexture.Value, position, frame with { X = 300 }, maskColor, NPC.rotation, origin, NPC.scale, effects);
			Main.EntitySpriteDraw(MaskTexture.Value, position, frame, maskColor, NPC.rotation, origin, NPC.scale, effects);
		}
	}

	private void DoChainActions(Vector2 position, bool gore)
	{
		if (TimerCheck(30))
		{
			DrawOrBreakChains(position + new Vector2(40, -50), position + new Vector2(190, -120), gore);
		}

		if (TimerCheck(80))
		{
			DrawOrBreakChains(position - new Vector2(0, 20), position + new Vector2(-160, 140), gore);
		}

		if (TimerCheck(110))
		{
			DrawOrBreakChains(position - new Vector2(70, 50), position - new Vector2(160, 140), gore);
		}

		if (TimerCheck(130))
		{
			DrawOrBreakChains(position - new Vector2(20, 0), position + new Vector2(20, -140), gore);
		}

		if (TimerCheck(140))
		{
			DrawOrBreakChains(position + new Vector2(00, 30), position + new Vector2(120, 140), gore);
		}

		bool TimerCheck(int threshold)
		{
			return gore ? Timer == threshold : Timer < threshold;
		}
	}

	private void DrawOrBreakChains(Vector2 start, Vector2 end, bool gore = false)
	{
		if (gore && Main.dedServ)
		{
			return;
		}

		Vector2 current = start;
		Vector2 dir = start.DirectionTo(end) * 24;
		float angle = start.AngleTo(end) + MathHelper.PiOver2;
		float length = ChainTexture.Value.Height;

		if (gore)
		{
			SoundEngine.PlaySound(SoundID.Item52, (start + end) / 2 + Main.screenPosition);
		}

		while (true)
		{
			if (!gore)
			{
				Color col = Lighting.GetColor((current + Main.screenPosition).ToTileCoordinates());
				Main.spriteBatch.Draw(ChainTexture.Value, current, null, col, angle, ChainTexture.Size() / 2f, 1f, SpriteEffects.None, 0);
			}
			else
			{
				Gore.NewGore(NPC.GetSource_FromAI(), current + Main.screenPosition, Vector2.Zero, Mod.Find<ModGore>("GoldChainLink").Type);
			}

			current += dir;

			if (current.DistanceSQ(end) < length * length)
			{
				break;
			}
		}
	}
}