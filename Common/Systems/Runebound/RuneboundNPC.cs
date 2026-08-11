using System.IO;
using PathOfTerraria.Common.Encounters;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Runebound;

internal sealed class RuneboundNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public int EncounterId = -1;
	public bool Sealed;
	public RuneboundFamily Family;
	public RunestoneGrade Grade;
	private int storedDamage;

	public void Bind(NPC npc, int encounterId, RuneboundFamily family, RunestoneGrade grade)
	{
		EncounterId = encounterId;
		Family = family;
		Grade = grade;
		Sealed = true;
		storedDamage = npc.damage;
		npc.damage = 0;
		npc.dontTakeDamage = true;
		npc.velocity = Vector2.Zero;
		npc.GetGlobalNPC<NPCDespawning>().NeverDespawn = true;
		npc.netUpdate = true;
	}

	public void Release(NPC npc)
	{
		if (Main.netMode != NetmodeID.Server)
		{
			RuneboundPrisonVisuals.CreateReleaseBurst(npc, Family, Grade);
		}

		Sealed = false;
		npc.damage = storedDamage;
		npc.dontTakeDamage = false;
		npc.netUpdate = true;
	}

	public override bool PreAI(NPC npc)
	{
		if (!Sealed)
		{
			return true;
		}

		npc.velocity = Vector2.Zero;
		RuneboundPrisonVisuals.UpdateMaterialEffects(npc, Family, Grade);
		return false;
	}

	public override void DrawEffects(NPC npc, ref Color drawColor)
	{
		if (Sealed)
		{
			Color familyColor = RuneboundSystem.GetFamilyColor(Family);
			float pulse = 0.24f + (int)Grade * 0.045f
				+ MathF.Sin((float)Main.timeForVisualEffects * 0.055f + npc.whoAmI) * 0.06f;
			drawColor = Color.Lerp(drawColor, familyColor, pulse);
		}
	}

	public override void OnKill(NPC npc)
	{
		if (EncounterId >= 0 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			RuneboundSystem.NotifyEnemyKilled(EncounterId, npc.whoAmI);
		}
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		bitWriter.WriteBit(Sealed);
		binaryWriter.Write(EncounterId);
		binaryWriter.Write((byte)Family);
		binaryWriter.Write((byte)Grade);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		bool wasSealed = Sealed;
		Sealed = bitReader.ReadBit();
		EncounterId = binaryReader.ReadInt32();
		Family = (RuneboundFamily)binaryReader.ReadByte();
		Grade = (RunestoneGrade)binaryReader.ReadByte();

		if (wasSealed && !Sealed && Main.netMode == NetmodeID.MultiplayerClient)
		{
			RuneboundPrisonVisuals.CreateReleaseBurst(npc, Family, Grade);
		}
	}
}
