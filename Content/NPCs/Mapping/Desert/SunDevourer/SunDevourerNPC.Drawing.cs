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

	private void DrawNPC(in Vector2 screenPosition, in Color drawColor)
	{
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Vector2 position = NPC.Center - screenPosition + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);
		Vector2 origin = NPC.frame.Size() / 2f;
		SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		if (State == DevourerState.Trapped)
		{
			DrawAllChains(position);
		}

		Main.EntitySpriteDraw(texture, position, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);

		if (State == DevourerState.Godrays && Timer <= GodrayHideTime)
		{
			Color maskColor = Color.White * (Timer / GodrayHideTime);
			Main.EntitySpriteDraw(MaskTexture.Value, position, NPC.frame, maskColor, NPC.rotation, origin, NPC.scale, effects);
		}
	}

	private void DrawAllChains(Vector2 position)
	{
		if (Timer < 30)
		{
			DrawOrBreakChains(position + new Vector2(140, 0), position + new Vector2(190, -120));
		}

		if (Timer < 80)
		{
			DrawOrBreakChains(position - new Vector2(120, -20), position + new Vector2(-160, 140));
		}

		if (Timer < 110)
		{
			DrawOrBreakChains(position - new Vector2(120, -20), position + new Vector2(-160, -140));
		}

		if (Timer < 130)
		{
			DrawOrBreakChains(position - new Vector2(20, 0), position + new Vector2(20, -140));
		}

		if (Timer < 140)
		{
			DrawOrBreakChains(position + new Vector2(00, 30), position + new Vector2(120, 140));
		}
	}

	private void DrawOrBreakChains(Vector2 start, Vector2 end, bool gore = false)
	{
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