using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.BuffBarSystem;

public class BuffBarSystem : GlobalBuff
{
	/// <summary>
	///	Overrides the existing buff bar with an offset to account for the custom hotbar
	/// </summary>
	public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
	{
		const int buffPositionOffsetY = 20;
		drawParams.Position = new Vector2(drawParams.Position.X, drawParams.Position.Y + buffPositionOffsetY);
		drawParams.MouseRectangle.Y += buffPositionOffsetY;
		return true;
	}
}