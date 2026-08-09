using PathOfTerraria.Common.Systems.ElementalDamage;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class ScorchingFlamesDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (!npc.TryGetGlobalNPC(out ElementalNPC element)) { return; }
		
		element.Container[ElementType.Fire].Resistance -= 0.3f;

		Visuals(npc);
	}

	public override void Update(Player player, ref int buffIndex)
	{
		if (!player.TryGetModPlayer(out ElementalPlayer element)) { return; }
		
		element.Container[ElementType.Fire].Resistance -= 0.3f;

		Visuals(player);
	}

	//TODO: Upgrade visuals.
	private static void Visuals(Entity entity)
	{
		if (Main.dedServ) { return; }
		if (Main.GameUpdateCount % 3 != 0) { return; }

		if (entity is NPC npc)
		{
			if (npc.alpha == byte.MaxValue) { return; }
		}
		
		Rectangle rect = new((int)entity.position.X, (int)entity.position.Y, entity.width, entity.height);
		Vector2 dustPos = Main.rand.NextVector2FromRectangle(rect);
		Vector2 dustVel = Main.rand.NextVector2Circular(1.5f, 0.5f) + Vector2.UnitY;
		Dust.NewDustPerfect(dustPos, DustID.RedMoss);
	}
}
