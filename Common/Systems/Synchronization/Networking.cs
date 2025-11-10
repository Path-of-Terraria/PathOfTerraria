using SubworldLibrary;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization;

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
		/// Syncs map devices.
		/// <br/> Does not implement <see cref="Handler.Send"/>.
		/// </summary>
		MapDeviceSync,
		/// <summary>
		/// Reports various map interactions.
		/// <br/> Does not implement <see cref="Handler.Send"/>.
		/// </summary>
		MapDeviceInteraction,

		/// <summary>
		/// Sets the index of a given Ravencrest structure.<br/>Signature:<br/>
		/// <c>string name, int index</c>
		/// </summary>
		SetRavencrestBuildingIndex,

		/// <summary>
		/// Syncs a player's conditional drop to the server and all other clients.<br/>Signature:<br/>
		/// <c>byte who, int id, bool add</c>
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
		RequestOthersSkillSpecialization,

		/// <summary>
		/// Requests all other players to sync their conditional drops. Does not recieve on clients; forwards a <see cref="SyncConditionalDrop"/> instead.<br/>Signature:<br/>
		/// <c>byte who, int[] drops</c>
		/// </summary>
		SyncNewConditionalDropPlayer,

		/// <summary>
		/// Marks a boss as downed. This is used for sending boss downs to the main server through <see cref="SendPacketToMainServer(ModPacket)"/>.
		/// <br/>Signature:<br/>
		/// <c>int id</c>
		/// </summary>
		SyncBossDowned,

		/// <summary>
		/// Marks an event as completed. This is used for sending boss downs to the main server through <see cref="SendPacketToMainServer(ModPacket)"/>.
		/// <br/>Signature:<br/>
		/// <c>EventFlags flag, int gameEventId</c>
		/// </summary>
		SyncEventCompletion,

		/// <summary>
		/// Tells Morven to get stuck in rock by way of the server. Takes no parameters.
		/// </summary>
		TellMorvenToSpawn,

		/// <summary>
		/// Sends a message to let the server allow Shadow Orbs (and Crimson Hearts) to be broken. Takes no parameters.
		/// </summary>
		BreakableOrbs,

		/// <summary>
		/// Adds a tier to the completion tracker. Meant to be used through <see cref="SendPacketToMainServer(ModPacket)"/>.<br/>Signature:<br/>
		/// <c>short tier</c><br/><br/>
		/// <b>Note:</b> This packet sends an additional short, count, to clients so that their downed count is forcefully set to the server's instead of allowing a desync.<br/>
		/// This should not affect any normal use of this packet however.
		/// </summary>
		SendMappingTierDown,

		/// <summary>
		/// Requests all mapping tier downs from the server.<br/>Signature:<br/>
		/// <c>byte who</c>
		/// </summary>
		RequestMappingTiers,

		/// <summary>
		/// Sends mapping domain info (Level, Tier, Affixes) to the server. <br/>Signature:<br/>
		/// <c>short level, short tier, List{MapAffix}</c>
		/// </summary>
		SendMappingDomainInfo,

		/// <summary>
		/// Requests mapping domain info (Level, Tier, Affixes) from the server.<br/>Signature:<br/>
		/// <c>byte who</c>
		/// </summary>
		RequestMappingDomainInfo,

		/// <summary>
		/// Synchronizes <see cref="Encounters.EnemySpawning"/>'s enemy spawn effects.
		/// <br/><b>Signature:</b> <c>NPC npc, EnemySpawnEffect effect, Vector2 position</c>
		/// </summary>
		EnemySpawn,

		/// Calls <see cref="RemoteClient.CheckSection(int, Vector2, int)"/> on the server for the given player.<br/>Signature:<br/>
		/// <c>byte who, Vector2 position</c>
		/// </summary>
		RequestCheckSection,

		/// <summary>
		/// Adds a stack of Bleed to an NPC.<br/>Signature:<br/>
		/// <c>byte who, short npcWho, ushort time, ushort damage</c>
		/// </summary>
		BleedStack,

		/// Spawns a Sentry NPC on the server. All NPCs need to be spawned on the server.<br/>Signature:<br/>
		/// <c>ushort type, byte owner, Vector2 position, ushort timeLeft, ushort damage</c>
		/// </summary>
		SpawnSentryNPC,

		/// Synchronizes right click interactions with rifts.<br/>Signature:<br/>
		/// <c>byte sender, int riftIdentity</c>
		/// </summary>
		RiftInteraction,

		/// <summary>
		/// Spawns arbitrary, pre-defined VFX on all clients and server. 
		/// Used for code run on only one client that should be shown on all clients, such as projectile spawning VFX, or server-side operations.<br/>Signature:<br/>
		/// <c>Vector2 position, SendSpawnVFXModule.VFXType type</c>
		/// </summary>
		SpawnVFX,

		/// <summary>
		/// Enables or disables the player's <see cref="UI.VirtualBagUI.VirtualBagStoragePlayer.UsesVirtualBag"/> for all clients.<br/>Signature:<br/>
		/// <c>byte player, bool enabled</c>
		/// </summary>
		PlayerUseSackOfHolding,
	}

	internal static void HandlePacket(BinaryReader reader, byte sender)
	{
		var message = (Message)reader.ReadByte();
		Handler handler = Handler.HandlerForMessage[message];

		handler.Receive(reader, sender);

#if DEBUG
		PoTMod.Instance.Logger.Debug($"[PoT] Network got: {message}");
#endif
		return;
	}

	internal static ModPacket GetPacket(Message type, byte capacity = 255)
	{
		ModPacket packet = PoTMod.Instance.GetPacket(capacity);
		packet.Write((byte)type);
		return packet;
	}

	/// <summary>
	/// Takes a <see cref="ModPacket"/> and sends it to the main server. Behaves as if using packet.Send() otherwise.
	/// </summary>
	/// <param name="packet">The packet to forward.</param>
	internal static void SendPacketToMainServer(ModPacket packet)
	{
		byte[] data = (packet.BaseStream as MemoryStream).GetBuffer();
		data = data[4..]; // Packets have a bunch of garbage data for some reason?

#if DEBUG
		string text = "";

		foreach (byte b in data)
		{
			text += $"{b}, ";
		}

		text = text[..^2];
		PoTMod.Instance.Logger.Debug(text);
#endif

		SubworldSystem.SendToMainServer(PoTMod.Instance, data);
	}
}
