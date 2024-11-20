using PathOfTerraria.Common.Systems.DrawLayers;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class SimpleCompass : ModItem
{
	private static Asset<Texture2D> _arrowTex = null;

	public override void Load()
	{
		_arrowTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CompassArrow");

		MiscDrawLayer.OnDraw += DrawCompassPoint;
	}

	private void DrawCompassPoint(ref Terraria.DataStructures.PlayerDrawSet drawInfo)
	{
		if (drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<SimpleCompass>())
		{
			Vector2 dir = drawInfo.Center.DirectionTo(Main.MouseWorld);

			Main.spriteBatch.Draw(_arrowTex.Value, drawInfo.Center - Main.screenPosition + dir * 40, null, Color.White,
				dir.ToRotation() + MathHelper.PiOver2, new Vector2(11, 38), 1f, SpriteEffects.None, 0);
		}
	}

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.rare = ItemRarityID.Quest;
		Item.Size = new Vector2(24, 18);
	}
}
