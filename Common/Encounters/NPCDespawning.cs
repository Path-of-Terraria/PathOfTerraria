#nullable enable

using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Encounters;

public sealed class NPCDespawning : GlobalNPC
{
    public bool NeverDespawn { get; set; } = false;

	public override bool InstancePerEntity => true;

	public override bool CheckActive(NPC npc)
	{
		return !NeverDespawn;
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(NeverDespawn);
    }
	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        NeverDespawn = bitReader.ReadBit();
    }
}
