﻿using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Forest;

internal class Runestone : ModTile
{
	private static Asset<Texture2D> Glow = null;

	public override void Load()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileBlendAll[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;

		AddMapEntry(new Color(56, 66, 66));

		DustType = DustID.Lead;
		HitSound = SoundID.Tink;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 worldPos = new Vector2(i, j).ToWorldCoordinates();
		int plr = Player.FindClosest(worldPos - new Vector2(8), 16, 16);
		float alpha = 1 - MathHelper.Clamp(Main.player[plr].Distance(worldPos) / 250f, 0, 1f);
		Color baseColor = Color.White;

		if (ModContent.GetInstance<GrovetenderSystem>().GrovetenderWhoAmI != -1)
		{
			const float MaxDistance = 200;

			NPC npc = Main.npc[ModContent.GetInstance<GrovetenderSystem>().GrovetenderWhoAmI];
			var tender = npc.ModNPC as Grovetender;

			if (tender is null)
			{
				return;
			}

			foreach (Point16 poweredRunestonePos in tender.PoweredRunestonePositions.Keys)
			{
				Vector2 pos = poweredRunestonePos.ToWorldCoordinates();

				float distance = MathHelper.Clamp(1 - pos.Distance(new Vector2(i, j) * 16) / MaxDistance, 0, 1);
				float newAlpha = tender.PoweredRunestonePositions[poweredRunestonePos] / (float)Grovetender.MaxRunestoneWait * distance;

				if (alpha < newAlpha)
				{
					alpha = newAlpha;
					baseColor = Color.Lerp(Color.White, Color.OrangeRed, newAlpha);
				}
			}
		}

		if (OpenExtensions.GetOpenings(i, j, false) != OpenFlags.None)
		{
			this.DrawSloped(i, j, Glow.Value, baseColor * alpha, Vector2.Zero);
		}
		else
		{
			float GetTime(float offset)
			{
				return MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.03f + offset + i * MathHelper.PiOver2 + j * MathHelper.PiOver2 + i + j * 0.3f), 2) * 0.75f;
			}

			Color color = new Color((float)(GetTime(0.1f) % 1), (float)(GetTime(0.2f) % 1), (float)(GetTime(-0.3f) % 1)).MultiplyRGB(baseColor);
			this.DrawSloped(i, j, Glow.Value, color * alpha, Vector2.Zero);
		}
	}
}