using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.NPCCritFunctionality;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

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

	protected override void InternalSaveTo(TagCompound tag)
	{
		base.InternalSaveTo(tag);

		tag.Add("strength", Strength);
	}

	protected override void InternalLoadFrom(TagCompound tag)
	{
		base.InternalLoadFrom(tag);

		Strength = tag.GetFloat("strength");
	}

	public override void NetSend(BinaryWriter writer)
	{
		base.NetSend(writer);

		writer.Write((Half)Strength);
	}

	public override void NetReceive(BinaryReader reader)
	{
		base.NetReceive(reader);

		Strength = (float)reader.ReadHalf();
	}
}

public class MapDamageAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		npc.damage = (int)(npc.damage * (1 + (Value / 100f)));
	}
}

public class MapBossHealthAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type])
		{
			npc.lifeMax = (int)(npc.lifeMax * (1 + Value / 100f));
			npc.life = npc.lifeMax;
		}
	}
}

public class MapMobCritChanceAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out CriticalStrikeNPC crit))
		{
			crit.CriticalStrikeChance += Value / 100f;
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

public class EoLDaylightAffix : MapAffix
{
	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = Value,
			Tier = null,
			ValueRollRange = null,
			Corrupt = IsCorruptedAffix,
			Implicit = IsImplicit,
		};
	}
}

public class MapFireConversionAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC ele))
		{
			ele.Container.AddElementalValues((ElementType.Fire, 0, Value / 100f));
		}
	}
}

public class MapColdConversionAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC ele))
		{
			ele.Container.AddElementalValues((ElementType.Cold, 0, Value / 100f));
		}
	}
}

public class MapLightningConversionAffix : MapAffix
{
	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC ele))
		{
			ele.Container.AddElementalValues((ElementType.Lightning, 0, Value / 100f));
		}
	}
}
