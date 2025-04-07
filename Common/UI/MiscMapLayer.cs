using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.Map;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

internal class MiscMapLayer : ModMapLayer
{
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		Alignment alignment = Alignment.Center;

		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.ModProjectile is not ShrineAoE)
			{
				continue;
			}

			Texture2D tex = ShrineAoE.MapIconsByType[proj.type].Value;
			Point16 center = proj.Center.ToTileCoordinates16();

			if (Main.Map.IsRevealed(center.X, center.Y) && context.Draw(tex, center.ToVector2(), alignment).IsMouseOver)
			{
				text = Language.GetTextValue(proj.Name);
			}
		}
	}
}
