using Terraria.GameContent;

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

		Main.EntitySpriteDraw(texture, position, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);

		if (State == DevourerState.Godrays && Timer <= 120f)
		{
			Color maskColor = Color.White * (Timer / 120f);
			Main.EntitySpriteDraw(MaskTexture.Value, position, NPC.frame, maskColor, NPC.rotation, origin, NPC.scale, effects);
		}
	}
}