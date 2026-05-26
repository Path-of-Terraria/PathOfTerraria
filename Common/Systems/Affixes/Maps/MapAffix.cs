using System.IO;
using PathOfTerraria.Common.Systems.Affixes;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

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
