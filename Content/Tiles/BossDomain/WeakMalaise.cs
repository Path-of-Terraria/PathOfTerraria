﻿using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class WeakMalaise : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;

		AddMapEntry(new Color(116, 63, 136));

		DustType = DustID.PurpleMoss;
		HitSound = SoundID.NPCHit1;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		float sine = MathF.Sin((i + j) * 20 + Main.GameUpdateCount * 0.06f) * 0.3f;
		(r, g, b) = (sine * 0.67f, sine * 0.2f, sine * 0.9f);
	}
}