using ReLogic.Content;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class SkeletronRitualProj : ModProjectile
{
	private static Asset<Texture2D> StarTex;

	public override string Texture => "Terraria/Images/NPC_0";

	private bool HasInit
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	private ref float Timer => ref Projectile.ai[1];

	Point16 anchor = default;
	
	public override void SetStaticDefaults()
	{
		StarTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Projectiles/Utility/Star");
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(80, 80);
		Projectile.Opacity = 0f;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.rotation += 0.15f;
		
		if (!HasInit)
		{
			HasInit = true;

			anchor = Projectile.position.ToTileCoordinates16();
		}

		if (Timer == 60)
		{
			WorldGen.KillTile(anchor.X - 1, anchor.Y, false, false, true);
			WorldGen.KillTile(anchor.X, anchor.Y, false, false, true);
			WorldGen.KillTile(anchor.X + 1, anchor.Y, false, false, true);
			WorldGen.KillTile(anchor.X, anchor.Y + 1, false, false, true);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}
}
