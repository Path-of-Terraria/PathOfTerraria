using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.PlantDomain;

internal class MiniteraHook : ModNPC
{
	private static Asset<Texture2D> Vine = null;

	private NPC Parent => Main.npc[ParentWho];
	private int ParentWho => (int)NPC.ai[0];
	private ref float Timer => ref NPC.ai[1];
	
	private Vector2 Target
	{
		get => new(NPC.ai[2], NPC.ai[3]);
		set => (NPC.ai[2], NPC.ai[3]) = (value.X, value.Y);
	}

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 2;

		Vine = ModContent.Request<Texture2D>(Texture + "_Vine");

		NPCID.Sets.NPCBestiaryDrawModifiers value = new()
		{
			Hide = true
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.PlanterasHook);
		NPC.aiStyle = -1;
		NPC.Size = new Vector2(26, 22);

		AIType = 0;
	}

	public override void AI()
	{
		Timer++;
		NPC.rotation = NPC.AngleTo(Parent.Center) + MathHelper.PiOver2;

		if (!Parent.active || Parent.type != ModContent.NPCType<Minitera>())
		{
			NPC.active = false;

			Vector2 center = NPC.Center;
			Vector2 offset = Parent.Center + new Vector2(-10, 20) - center;
			int count = 0;

			while (true)
			{
				if (RunVine(ref center, ref offset, ref count, false))
				{
					break;
				}
			}
		}

		if (Timer < 40)
		{
			NPC.velocity = Vector2.Zero;
		}
		else if (Timer == 40)
		{
			Vector2 dir = Parent.DirectionTo(Main.player[Parent.target].Center) * 320;
			Tile tile;

			do
			{
				Target = Parent.Center + dir + Main.rand.NextVector2CircularEdge(200, 200) * Main.rand.NextFloat(0.8f, 1.2f);
				tile = Main.tile[Target.ToTileCoordinates16()];
			} while (WorldGen.SolidTile(tile) && tile.TileType == WallID.None);

			NPC.netUpdate = true;
		}
		else if (Timer > 40)
		{
			NPC.velocity = NPC.DirectionTo(Target) * 8;

			if (NPC.DistanceSQ(Target) < 10 * 10)
			{
				Timer = -90;
			}
		}
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frame.Y = NPC.velocity.LengthSquared() > 0 ? frameHeight : 0;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Vector2 center = NPC.Center;
		Vector2 offset = Parent.Center + new Vector2(-10, 20) - center;

		if (Parent.Center.DistanceSQ(center) < 4 * 4 || NPC.Center.HasNaNs() || offset.HasNaNs())
		{
			return false;
		}

		int count = 0;

		while (true)
		{
			if (RunVine(ref center, ref offset, ref count))
			{
				break;
			}
		}

		return true;
	}

	private bool RunVine(ref Vector2 center, ref Vector2 offset, ref int count, bool draw = true)
	{
		const int CordHeight = 10;

		if (offset.Length() < CordHeight + 1)
		{
			return true;
		}
		else
		{
			center += Vector2.Normalize(offset) * CordHeight;
			offset = Parent.Center - center;
			count++;

			if (draw)
			{
				Color color = NPC.GetAlpha(Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16.0)));
				Rectangle source = new(0, count % 3 * (CordHeight + 2), 20, CordHeight);
				Vector2 position = center - Main.screenPosition;
				Main.spriteBatch.Draw(Vine.Value, position, source, color, offset.ToRotation() + MathHelper.PiOver2, source.Size() / 2, 1f, SpriteEffects.None, 0f);
			}
			else if (Main.netMode != NetmodeID.Server)
			{
				int gore = Gore.NewGore(NPC.GetSource_Death(), center, Vector2.Zero, ModContent.Find<ModGore>($"{PoTMod.ModName}/MiniteraVine").Type);
				Main.gore[gore].Frame = new SpriteFrame(1, 3, 0, (byte)(count % 3));
			}
		}

		return false;
	}
}
