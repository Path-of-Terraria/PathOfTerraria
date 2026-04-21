using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Map;
using Terraria.UI;

namespace PathOfTerraria.Common.Systems.MapContent;

/// <summary>
/// Gives a projectile an icon on the map, loaded as <see cref="ModProjectile.Texture"/> + "_Map".<br/>
/// The map icon won't draw if not revealed, and will show the projectile's name if hovered over.
/// </summary>
internal interface IMapIcon
{
	bool ShowMapIcon(Projectile projectile)
	{
		return true;
	}
}

internal class ProjectileMapIconLoader : ModSystem
{
	public override void SetStaticDefaults()
	{
		ProjectileMapLayer.IconsByType.Clear();

		for (int i = ProjectileID.Count; i < ProjectileLoader.ProjectileCount; ++i)
		{
			Projectile proj = new();
			proj.SetDefaults(i);

			if (proj.ModProjectile is { } modProj and IMapIcon)
			{
				ProjectileMapLayer.IconsByType.Add(i, ModContent.Request<Texture2D>(modProj.Texture + "_Map"));
			}
		}
	}
}

internal class ProjectileMapLayer : ModMapLayer
{
	internal static Dictionary<int, Asset<Texture2D>> IconsByType = [];

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			Point mapPos = projectile.Center.ToTileCoordinates();

			if (!WorldGen.InWorld(mapPos.X, mapPos.Y)) { continue; }
			if (!Main.Map.IsRevealed(mapPos.X, mapPos.Y)) { continue; }
			if (!IconsByType.TryGetValue(projectile.type, out Asset<Texture2D> asset)) { continue; }
			if (projectile.ModProjectile is IMapIcon mapIcon && !mapIcon.ShowMapIcon(projectile)) { continue; }

			if (context.Draw(asset.Value, projectile.Center / 16f, Alignment.Center).IsMouseOver)
			{
				text = projectile.Name;
			}
		}
	}
}
