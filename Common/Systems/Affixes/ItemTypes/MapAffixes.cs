using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

public abstract class MapAffix : ItemAffix
{
	/// <summary>
	/// Used to modify the resulting reward for map affix difficulty.<br/>
	/// Experience boost: <see cref="MobSystem.MobExperienceGlobalNPC.OnKill(NPC)"/><br/>
	/// Drop rarity and quantity boost: <see cref="MobSystem.ArpgNPC.DropQuantity"/> and <see cref="MobSystem.ArpgNPC.OnKill(NPC)"/>
	/// </summary>
	public float Strength;

	public virtual void ModifyNewNPC(NPC npc)
	{
	}

	public virtual void ModifyHitPlayer(NPC npc, Player player, ref Player.HurtModifiers modifiers)
	{
	}

	public virtual void OnHitPlayer(NPC npc, Player player, Player.HurtInfo info)
	{
	}

	public virtual void PreAI(NPC npc)
	{
	}
}

public class MapDamageAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		npc.damage = (int)(npc.damage * (1 + Value / 100f));
	}
}

public class MapBossHealthAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.boss)
		{
			npc.lifeMax = (int)(npc.lifeMax * (1 + Value / 100f));
			npc.life = npc.lifeMax;
		}
	}
}

public class MapMobCritChanceAffix : MapAffix
{
	public override void ModifyHitPlayer(NPC npc, Player player, ref Player.HurtModifiers modifiers)
	{
		if (Main.rand.NextFloat() < Value / 100f)
		{
			modifiers.FinalDamage += 1.5f;
		}
	}
}

public class MapMobChillChanceAffix : MapAffix
{
	public override void OnHitPlayer(NPC npc, Player player, Player.HurtInfo info)
	{
		if (Main.rand.NextFloat() < Value / 100f)
		{
			player.AddBuff(BuffID.Chilled, 4 * 60);
		}
	}
}

public class MapIncreasedBehaviourAffix : MapAffix
{
	public override void PreAI(NPC npc)
	{
		npc.GetGlobalNPC<SpeedUpNPC>().ExtraAISpeed += Value / 100f;
	}
}