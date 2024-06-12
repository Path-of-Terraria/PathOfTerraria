using Terraria.DataStructures;

namespace PathOfTerraria.Core.Systems.BuffBarSystem;

public class BuffBarSystem : GlobalBuff
{
	public const int BuffPositionOffsetY = 20;

	/// <summary>
	///	Overrides the existing buff bar with an offset to account for the custom hotbar
	/// </summary>
	public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
	{
		drawParams.Position = new Vector2(drawParams.Position.X, drawParams.Position.Y + BuffPositionOffsetY);
		return true;
	}
}