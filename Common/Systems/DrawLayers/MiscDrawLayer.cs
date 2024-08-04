using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.DrawLayers;

internal class MiscDrawLayer : PlayerDrawLayer
{
	public delegate void DrawDelegate(ref PlayerDrawSet drawInfo);

	public static event DrawDelegate OnDraw;

	public override Position GetDefaultPosition()
	{
		return new AfterParent(PlayerDrawLayers.ArmOverItem);
	}

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		OnDraw(ref drawInfo);
	}
}