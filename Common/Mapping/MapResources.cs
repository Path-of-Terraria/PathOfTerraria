#define DEBUG_COMMANDS

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
	/// <summary> Path of the texture used to display this resource inside a map device canister. </summary>
	public required string CanisterLiquidTexture;
	/// <summary> How much of the resource has to be used to be injected into a map device. </summary>
	public required int Cost;
	/// <summary> The current amount of this resource. </summary>
	public int Value;
	/// <summary> The minimum amount of this resource that can be. </summary>
	public int MinValue = 0;
	/// <summary> The maximum amount of this resource that can be. </summary>
	public int MaxValue = int.MaxValue;
	/// <summary> How many uses the portal spawned from this resource should have. </summary>
	public int PortalUses = int.MaxValue;
	/// <summary> The FullName of the subworld that portals using this resource will lead to. </summary>
	public string PortalDestination = string.Empty;
	/// <summary> Whether this resource has been discovered by the player. Will affect how and whether it will be displayed. </summary>
	public bool Discovered;
}

internal enum ResourceDiscovery : byte
{
	Auto,
	Always,
	Never,
	Undiscover,
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
				// Redirect 'needsSync' flag to the main server, or just set it right away.
				if (SubworldSystem.Current == null) { needsSync = true; }
				else { SubworldSystem.SendToMainServer(PoTMod.Instance, Networking.GetFinalPacketBuffer(packet)); }
			}
		}

		internal override void ServerReceive(BinaryReader reader, byte sender)
		{
			if (SubworldSystem.Current == null) { needsSync = true; }
			else { Send(); }
		}
	}

	/// <inheritdoc cref="ModifyValue"/>
	internal sealed class ModifyValueMessage : Handler
	{
		public static ModPacket Write(int itemId, int delta, ResourceDiscovery discover)
		{
			ModPacket packet = Networking.GetPacket<ModifyValueMessage>();
			packet.Write7BitEncodedInt(itemId);
			packet.Write7BitEncodedInt(delta);
			packet.Write((byte)discover);
			return packet;
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			int itemId = reader.Read7BitEncodedInt();
			int delta = reader.Read7BitEncodedInt();
			var discovery = (ResourceDiscovery)reader.ReadByte();
			ModifyValue(itemId, delta, discovery: discovery, netSender: sender);
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

	/// <summary> Conditionally returns the globally stored amount of the resource associated with the provided item. </summary>
	public static bool TryGet(int itemType, [MaybeNullWhen(false)] out MapResource result)
	{
		if (!resourcesByItem.TryGetValue(itemType, out int idx))
		{
			result = default;
			return false;
		}

		result = resources[idx];
		return true;
	}
	/// <inheritdoc cref="TryGet"/>
	public static bool TryGet<T>([MaybeNullWhen(false)] out MapResource result) where T : ModItem
	{
		return TryGet(ModContent.GetInstance<T>().Item.type, out result);
	}
	/// <summary> Returns a copy of the resource associated with the provided item. </summary>
	public static MapResource Get(int itemType)
	{
		return TryGet(itemType, out MapResource result) ? result : throw new InvalidOperationException($"Invalid resource: {ModContent.GetModItem(itemType).FullName}");
	}
	/// <inheritdoc cref="Get"/>
	public static MapResource Get<T>() where T : ModItem
	{
		return Get(ModContent.GetInstance<T>().Item.type);
	}

	/// <summary> Returns the array index of the resource associated with the given item ID. </summary>
	public static int IndexOf(int itemType)
	{
		return resourcesByItem[itemType];
	}
	/// <inheritdoc cref="IndexOf"/>
	public static int IndexOf<T>() where T : ModItem
	{
		return IndexOf(ModContent.GetInstance<T>().Item.type);
	}

	/// <inheritdoc cref="ModifyValue"/>
	public static void ModifyValue<T>(int delta, ResourceDiscovery discovery = 0) where T : ModItem
	{
		ModifyValue(ModContent.GetInstance<T>().Item.type, delta, discovery: discovery);
	}
	/// <summary> Atomically adds the given value to the resource associated with the provided item, synchronizing across all servers and clients. </summary>
	public static void ModifyValue(int itemType, int delta, ResourceDiscovery discovery = 0, byte? netSender = null)
	{
		// Only servers can send this.
		if (netSender != null && netSender < byte.MaxValue) { return; }

		// Short-circuit if unrecognized.
		if (!resourcesByItem.TryGetValue(itemType, out int index)) { return; }

		// Redirect to main server.
		if (Main.netMode == NetmodeID.Server && SubworldSystem.Current != null)
		{
			SubworldSystem.SendToMainServer(PoTMod.Instance, Networking.GetFinalPacketBuffer(ModifyValueMessage.Write(itemType, delta, discovery)));
			return;
		}

		// Perform function.
		ref MapResource resource = ref ResourcesMut[index];
		resource.Value = Math.Clamp(resource.Value + delta, resource.MinValue, resource.MaxValue);
		resource.Discovered = discovery switch
		{
			ResourceDiscovery.Always => true,
			ResourceDiscovery.Never => resource.Discovered,
			ResourceDiscovery.Undiscover => false,
			_ => resource.Value > resource.MinValue || resource.Discovered,
		};

		// Enqueue a full sync.
		needsSync |= Main.netMode == NetmodeID.Server;
	}

	public static bool AnyResourceDiscovered()
	{
		return resources.Any(r => r.Discovered);
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
				&& resourcesByItem.TryGetValue(item.Item.type, out int index)
				&& pair.Value is TagCompound resourceTag)
				{
					ref MapResource resource = ref ResourcesMut[index];
					resource.Value = resourceTag.GetInt("value");
					resource.Discovered = resourceTag.GetBool("discovered");
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
			writer.Write((bool)resource.Discovered);
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
			bool discovered = reader.ReadBoolean();

			if (resourcesByItem.TryGetValue(item, out int index))
			{
				ref MapResource resource = ref ResourcesMut[index];
				resource.Value = value;
				resource.Discovered = true;
			}
		}
	}
}

#if DEBUG && DEBUG_COMMANDS
internal sealed class MapResourceCommand : ModCommand
{
	public override string Command => "potMapResource";
	public override CommandType Type => CommandType.World;
	public override string Usage => $"/{Command} set/add/undiscover {{itemName}} {{value?}} (/{Command} set InfernalConflux 10)";
	public override bool IsCaseSensitive => true;

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length < 2) { throw new UsageException(); }

		MapResource GetResource(int argIdx)
		{
			string resName = args[argIdx];
			if (!ItemID.Search.TryGetId(resName, out int itemId)
			&& !ItemID.Search.TryGetId($"{Mod.Name}/{resName}", out itemId))
			{
				throw new UsageException($"Item '{resName}' could not be found.");
			}
		
			if (!MapResources.TryGet(itemId, out MapResource res))
			{
				throw new UsageException($"Not a map resource: '{resName}'");
			}

			return res;
		}

		string cmd = args[0];
		switch (cmd)
		{
			case "undiscover":
			{
				MapResource res = GetResource(1);
				MapResources.ModifyValue(res.AssociatedItem, delta: +0, discovery: ResourceDiscovery.Undiscover);
				break;
			}
			case "add":
			{
				MapResource res = GetResource(1);
				CheckArgCount(3);
				if (!int.TryParse(args[2], out int delta)) { throw new UsageException(); }
				MapResources.ModifyValue(res.AssociatedItem, delta);
				break;
			}
			case "set":
			{
				MapResource res = GetResource(1);
				CheckArgCount(3);
				if (!int.TryParse(args[2], out int value)) { throw new UsageException(); }
				value = Math.Clamp(value, res.MinValue, res.MaxValue);
				MapResources.ModifyValue(res.AssociatedItem, value - res.Value);
				break;
			}
		}

		void CheckArgCount(int expected)
		{
			if (args.Length != expected) { throw new UsageException($"Expected {expected} arguments, got {args.Length}."); }
		}

		Main.NewText("Success!", Color.LimeGreen);
	}
}
#endif
