using Microsoft.CodeAnalysis.Differencing;
using Terraria.GameContent;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal partial class Grovetender : ModNPC
{
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		Vector2 position = NPC.Center - screenPos - new Vector2(-20, -NPC.gfxOffY + 200);
		float gradientY = (Main.maxTilesY - 48) * 16 - Main.screenPosition.Y;

		if (!NPC.IsABestiaryIconDummy)
		{
			for (int i = (int)(Main.screenPosition.X / 16f) - 3; i < (int)(Main.screenPosition.X / 16f) + (int)(Main.screenWidth / 16f) + 3; i++)
			{
				if (WorldGen.SolidOrSlopedTile(i, Main.maxTilesY - 50))
				{
					continue;
				}

				var drawPos = new Vector2(i * 16 - Main.screenPosition.X, gradientY);
				int width = 16;

				if (WorldGen.SolidOrSlopedTile(i - 1, Main.maxTilesY - 50))
				{
					drawPos.X -= 8;
					width = 24;
				}
				else if (WorldGen.SolidOrSlopedTile(i + 1, Main.maxTilesY - 50))
				{
					width = 24;
				}

				Lighting.AddLight(drawPos + Main.screenPosition, new Vector3(0.5f, 0.5f, 0.25f) * _eyeOpacity);
				Main.EntitySpriteDraw(Gradient.Value, drawPos, new Rectangle(0, 0, width, 100), Color.White * _eyeOpacity, 0f, Vector2.Zero, new Vector2(1, 1), effect);
			}
		}
		else
		{
			position -= new Vector2(30, -40);
		}

		Main.EntitySpriteDraw(Sockets.Value, position, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);

		DrawEyes(screenPos);

		for (int i = 0; i < tex.Width / 16 + 1; ++i)
		{
			for (int j = 0; j < tex.Height / 16 + 1; ++j)
			{
				var frame = new Rectangle(NPC.frame.X + i * 16, NPC.frame.Y + j * 16, 16, 16);
				Vector2 drawPos = position + new Vector2(i * 16, j * 16);
				Color color = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor((drawPos + screenPos - new Vector2(200, 340)).ToTileCoordinates());

				Main.EntitySpriteDraw(tex, drawPos, frame, color, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);
			}
		}

		return false;
	}

	private void DrawEyes(Vector2 screenPos)
	{
		DrawEye(screenPos, EyeID.Left, true);
		DrawEye(screenPos, EyeID.Right, true);

		if (NPC.life > NPC.lifeMax * 0.66f)
		{
			DrawEye(screenPos, EyeID.SmallLeft, false);
		}

		if (NPC.life > NPC.lifeMax * 0.33f)
		{
			DrawEye(screenPos, EyeID.SmallRight, false);
		}
	}

	private void DrawEye(Vector2 screenPos, EyeID eye, bool large)
	{
		if (State == AIState.Asleep)
		{
			return;
		}

		_eyeOpacity = MathHelper.Lerp(_eyeOpacity, 1, 0.02f);
		Vector2 offset = GetEyeOffset(eye, true);
		Texture2D tex = (large ? EyeLarge : EyeSmall).Value;
		Main.EntitySpriteDraw(tex, NPC.Center - screenPos - offset, null, Color.White * _eyeOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None);
	}

	public Vector2 GetEyeOffset(EyeID eye, bool sightAdjusted)
	{
		Vector2 offset = eye switch
		{
			EyeID.Left => new Vector2(6, 58),
			EyeID.Right => new Vector2(-44, 58),
			EyeID.SmallLeft => new Vector2(62, 36),
			_ => new Vector2(-114, 46),
		};

		if (sightAdjusted)
		{
			Vector2 directionTo = (NPC.Center - offset - Target.Center) / 60;

			if (directionTo.LengthSquared() > 5)
			{
				directionTo = Vector2.Normalize(directionTo) * 5;
			}

			offset += directionTo;
		}

		return offset;
	}
}