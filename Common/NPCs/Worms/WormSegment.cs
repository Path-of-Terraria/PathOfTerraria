using Microsoft.CodeAnalysis.Operations;
using PathOfTerraria.Common.Systems.MobSystem;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Worms;

internal abstract class WormSegment : ModNPC
{
	public Entity Parent => Main.npc[ParentIndex];

	public ref float Length => ref NPC.ai[0];
	public int ParentIndex => (int)NPC.ai[1];

	public static int SpawnSegment<T>(IEntitySource source, NPC parent, int length) where T : WormSegment
	{
		int npc = NPC.NewNPC(source, (int)parent.Center.X, (int)parent.Center.Y, ModContent.NPCType<T>(), 0, length, parent.whoAmI);
		return npc;
	}

	public static int SpawnWhole<TBody, TTail>(IEntitySource source, NPC spawner, int segmentLength, int segmentCount, bool setRealLife = true) where TBody : WormSegment where TTail : WormSegment
	{
		if (segmentCount <= 0)
		{
			throw new ArgumentException("segmentCount should be >0.");
		}

		int npc = -1;
		NPC parent = spawner;

		for (int i = 0; i < segmentCount; ++i)
		{
			int type = i == segmentCount - 1 ? ModContent.NPCType<TTail>() : ModContent.NPCType<TBody>();
			npc = NPC.NewNPC(source, (int)spawner.Center.X, (int)spawner.Center.Y, type, 0, segmentLength, parent.whoAmI);
			parent = Main.npc[npc];

			if (setRealLife)
			{
				parent.realLife = spawner.whoAmI;
			}
		}

		return npc;
	}

	public override void SetStaticDefaults()
	{
		ArpgNPC.NoAffixesSet.Add(Type);
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 1;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.knockBackResist = 0f;
		NPC.ShowNameOnHover = false;

		Defaults();
	}

	public virtual void Defaults() { }

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
	{
		return false;
	}

	public override void AI()
	{
		if (Parent.DistanceSQ(NPC.Center) > Length * Length)
		{
			NPC.Center = Parent.Center + Parent.DirectionTo(NPC.Center) * Length;
			NPC.rotation = NPC.AngleTo(Parent.Center);
		}

		if (!Parent.active)
		{
			NPC.active = false;
		}
	}

	public virtual void Draw()
	{
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Vector2 position = NPC.Center - Main.screenPosition;
		Main.spriteBatch.Draw(texture, position, null, Color.White, NPC.rotation, texture.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}
