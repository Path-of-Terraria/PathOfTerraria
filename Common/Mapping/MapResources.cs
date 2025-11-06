using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using PathOfTerraria.Common.Systems.Synchronization;
using SubworldLibrary;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Mapping;

internal record struct MapResource()
{
	/// <summary> The item ID that will be used to refer to this resource. </summary>
	public required int AssociatedItem;
	/// <summary> The color to use when referring to this resource in text or sprite rendering. </summary>
	public required Color AccentColor;
	/// <summary> The current amount of this resource. </summary>
	public int Value;
	/// <summary> The minimum amount of this resource that can be. </summary>
	public int MinValue = 0;
	/// <summary> The maximum amount of this resource that can be. </summary>
	public int MaxValue = int.MaxValue;
	/// <summary> Whether this resource has been discovered by the player. Will affect how and whether it will be displayed. </summary>
	public bool Discovered;
}

/// <summary> Tracks and synchronizes <see cref="MapResource"/>s across servers and clients. </summary>
internal sealed class MapResources : ModSystem
{
	/// <summary> Synchronizes all resource values. Can only be sent by servers. </summary>
	internal sealed class SynchronizeMessage : Handler
	{
		public static void Send(bool sendToSubworlds)
		{
			ModPacket packet = Networking.GetPacket<SynchronizeMessage>();
			ModContent.GetInstance<MapResources>().NetSend(packet);
			packet.Send();

			if (sendToSubworlds) { SubworldSystem.SendToAllSubservers(PoTMod.Instance, Networking.GetFinalPacketBuffer(packet)); }
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			// Only servers can send this.
			if (sender < byte.MaxValue) { return; }

			ModContent.GetInstance<MapResources>().NetReceive(reader);

			// Resend to clients in case we are a subworld that received this from the main server.
			if (Main.netMode == NetmodeID.Server)
			{
				Send(sendToSubworlds: false);
			}
		}
	}

	/// <summary> Requests so that map resources are sent to this client or subserver. </summary>
	internal sealed class RequestMessage : Handler
	{
		public static void Send()
		{
			ModPacket packet = Networking.GetPacket<RequestMessage>();

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				packet.Send();
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				SubworldSystem.SendToMainServer(PoTMod.Instance, Networking.GetFinalPacketBuffer(packet));
			}
		}

		internal override void ServerReceive(BinaryReader reader, byte sender)
		{
			if (SubworldSystem.Current == null)
			{
				needsSync = true;
			}
			else
			{
				Send();
			}
		}
	}

	/// <inheritdoc cref="AddOrRemove"/>
	internal sealed class AddOrRemoveMessage : Handler
	{
		public static ModPacket Write(int itemId, int delta)
		{
			ModPacket packet = Networking.GetPacket<AddOrRemoveMessage>();
			packet.Write7BitEncodedInt(itemId);
			packet.Write7BitEncodedInt(delta);
			return packet;
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			int itemId = reader.Read7BitEncodedInt();
			int delta = reader.Read7BitEncodedInt();
			AddOrRemove(itemId, delta, netSender: sender);
		}
	}

	private static readonly List<MapResource> resources = [];
	private static readonly Dictionary<int, int> resourcesByItem = [];
	private static bool needsSync;

	public static ReadOnlySpan<MapResource> Resources => CollectionsMarshal.AsSpan(resources);
	private static Span<MapResource> ResourcesMut => CollectionsMarshal.AsSpan(resources);

	public override void ClearWorld()
	{
		foreach (ref MapResource resource in ResourcesMut)
		{
			resource.Value = resource.MinValue;
			resource.Discovered = false;
		}
	}

	public override void PostUpdateEverything()
	{
		if (Main.netMode == NetmodeID.Server && needsSync && SubworldSystem.Current == null)
		{
			SynchronizeMessage.Send(sendToSubworlds: true);
			needsSync = false;
		}
	}

	/// <summary> Registers a map resource with the provided parameters. </summary>
	public static void Register(MapResource resource)
	{
		if (resource.AssociatedItem <= 0)
		{
			throw new InvalidOperationException("Invalid map resource item association.");
		}

		int index = resources.Count;
		resources.Add(resource);
		resourcesByItem.Add(resource.AssociatedItem, index);
	}

	/// <inheritdoc cref="Get"/>
	public static MapResource Get<T>() where T : ModItem
	{
		return Get(ModContent.GetInstance<T>().Item.netID);
	}
	/// <summary> Returns the globally stored amount of the resource associated with the provided item. Safe to use in all contexts. </summary>
	public static MapResource Get(int itemNetId)
	{
		if (!resourcesByItem.TryGetValue(itemNetId, out int idx)) { throw new InvalidOperationException($"Invalid resource: {ModContent.GetModItem(itemNetId).FullName}"); }

		return resources[idx];
	}

	/// <inheritdoc cref="AddOrRemove"/>
	public static void AddOrRemove<T>(int delta) where T : ModItem
	{
		AddOrRemove(ModContent.GetInstance<T>().Item.netID, delta);
	}
	/// <summary> Atomically adds the given value to the resource associated with the provided item, synchronizing across all servers and clients. </summary>
	public static void AddOrRemove(int itemNetId, int delta, byte? netSender = null)
	{
		// Only servers can send this.
		if (netSender != null && netSender < byte.MaxValue) { return; }

		// Short-circuit if unrecognized.
		if (!resourcesByItem.TryGetValue(itemNetId, out int index)) { return; }

		// Redirect to main server.
		if (Main.netMode == NetmodeID.Server && SubworldSystem.Current != null)
		{
			SubworldSystem.SendToMainServer(PoTMod.Instance, Networking.GetFinalPacketBuffer(AddOrRemoveMessage.Write(itemNetId, delta)));
			return;
		}

		// Perform function.
		ref MapResource resource = ref ResourcesMut[index];
		resource.Value = Math.Clamp(resource.Value + delta, resource.MinValue, resource.MaxValue);

		// Enqueue a full sync.
		if (Main.netMode == NetmodeID.Server)
		{
			needsSync = true;
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		var resourcesTag = new TagCompound();
		tag["resources"] = resourcesTag;

		foreach (ref readonly MapResource resource in Resources)
		{
			resourcesTag[ModContent.GetModItem(resource.AssociatedItem).FullName] = new TagCompound
			{
				{ "value", (int)resource.Value },
				{ "discovered", resource.Discovered },
			};
		}
	}
	public override void LoadWorldData(TagCompound tag)
	{
		if (tag.TryGet("resources", out TagCompound resourcesTag))
		{
			resources.EnsureCapacity(tag.Count);

			foreach (KeyValuePair<string, object> pair in resourcesTag)
			{
				if (ModContent.TryFind(pair.Key, out ModItem item)
				&& resourcesByItem.TryGetValue(item.Item.netID, out int index)
				&& pair.Value is TagCompound resourceTag)
				{
					ref MapResource resource = ref ResourcesMut[index];
					resource.Value = (int)resourceTag["value"];
					resource.Discovered = (bool)resourceTag["discovered"];
				}
			}
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(resources.Count);

		foreach (ref readonly MapResource resource in Resources)
		{
			writer.Write7BitEncodedInt((int)resource.AssociatedItem);
			writer.Write7BitEncodedInt((int)resource.Value);
		}
	}
	public override void NetReceive(BinaryReader reader)
	{
		int count = reader.Read7BitEncodedInt();
		resources.EnsureCapacity(count);
		for (int i = 0; i < count; i++)
		{
			int item = reader.Read7BitEncodedInt();
			int value = reader.Read7BitEncodedInt();

			if (resourcesByItem.TryGetValue(item, out int index))
			{
				ref MapResource resource = ref ResourcesMut[index];
				resource.Value = value;
			}
		}
	}
}
