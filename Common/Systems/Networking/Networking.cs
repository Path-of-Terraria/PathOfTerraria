using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Networking;

internal static class Networking
{
	public enum Message : byte
	{
		/// <summary>
		/// Spawns experience.<br/>Signature:<br/>
		/// <c>byte target, int xpValue, Vector2 position, Vector2 velocity, bool spawnLocally = false</c>
		/// </summary>
		SpawnExperience,

		/// <summary>
		/// Syncs the usage of hotbar potions.<br/>Signature:<br/>
		/// <c>byte playerWhoAmI, bool isHealingPotion, byte newValue, bool runLocally = false</c>
		/// </summary>
		SetHotbarPotionUse,

		/// <summary>
		/// Syncs the rings of the Guardian Angel's hits.<br/>Signature:<br/>
		/// <c>byte playerWhoAmI, short npcWho, bool runLocally = false</c>
		/// </summary>
		SyncGuardianAngelHit,

		/// <summary>
		/// Spawns an NPC on the server. Note that this spawns all NPCs with a Start value of 1, so worms spawn normally.<br/>Signatures:<br/>
		/// <c>short npcId, Vector2 position</c><br/>
		/// <c>short npcId, Vector2 position, Vector2 velocity</c>
		/// </summary>
		SpawnNPCOnServer,

		/// <summary>
		/// Syncs placing an item in a map device.<br/>Signature:<br/>
		/// <c>byte fromWho, short itemId, Point16 entityKey</c>
		/// </summary>
		SyncMapDevicePlaceMap,

		/// <summary>
		/// Takes 1 "use" off of a given map device.<br/>Signature:<br/>
		/// <c>byte fromWho, Point16 entityKey</c>
		/// </summary>
		ConsumeMapOffOfDevice,

		/// <summary>
		/// Sets the index of a given Ravencrest structure.<br/>Signature:<br/>
		/// <c>string name, int index</c>
		/// </summary>
		SetRavencrestBuildingIndex,

		/// <summary>
		/// Syncs a condition drop to the server.<br/>Signature:<br/>
		/// <c>int id, bool add</c>
		/// </summary>
		SyncConditionalDrop,

		/// <summary>
		/// Syncs the usage of a staff's alt use. This is invariable and always sets to the same value.<br/>Signature:<br/>
		/// <c>byte whoAmI</c>
		/// </summary>
		SyncUseStaffAltUse,

		/// <summary>
		/// Changes the pathfinder state on the given NPC. This is used over <see cref="NPC.netUpdate"/> as it guarantees precision information.<br/>Signature:<br/>
		/// <c>byte followPlayer, byte who, bool enable</c>
		/// </summary>
		PathfindChangeState,

		/// <summary>
		/// Syncs any use of <see cref="AltUsePlayer"/>'s <see cref="AltUsePlayer.SetAltCooldown(int, int)"/>.<br/>Signature:<br/>
		/// <c>byte player, short cooldown, short activeTime</c>
		/// </summary>
		SyncAltUse,

		/// <summary>
		/// Syncs all of one player's passives with everyone else + the server.<br/>Signature:<br/>
		/// <c>byte player, string treeName, string nodeName, byte level, bool runLocally = false</c>
		/// </summary>
		SkillPassiveValue,

		/// <summary>
		/// Requests all other players to sync their skill passives.<br/>Signature:<br/>
		/// <c>byte player</c>
		/// </summary>
		RequestOthersSkillPassives,

		/// <summary>
		/// Syncs all of one player's skill specializations with everyone else + the server.<br/>Signature:<br/>
		/// <c>byte player, string skillName, string specName</c>
		/// </summary>
		SyncSkillSpecialization,

		/// <summary>
		/// Requests all other players to sync their skill specializations.<br/>Signature:<br/>
		/// <c>byte player</c>
		/// </summary>
		RequestOthersSkillSpecialization
	}

	internal static void HandlePacket(BinaryReader reader)
	{
		var message = (Message)reader.ReadByte();
		Handler handler = Handler.HandlerForMessage[message];

		if (Main.netMode == NetmodeID.Server)
		{
			handler.ServerRecieve(reader);
		}
		else
		{
			handler.ClientRecieve(reader);
		}

		return;
	}

	internal static ModPacket GetPacket(Message type, byte capacity = 255)
	{
		ModPacket packet = PoTMod.Instance.GetPacket(capacity);
		packet.Write((byte)type);
		return packet;
	}
}
