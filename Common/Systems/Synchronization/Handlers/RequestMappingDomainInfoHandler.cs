using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using SubworldLibrary;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.RequestMappingDomainInfo"/>
internal class RequestMappingDomainInfoHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.RequestMappingDomainInfo;

	public override void Load(Mod mod)
	{
		base.Load(mod);

		PathOfTerrariaPlayerEvents.OnEnterWorldEvent += player =>
		{
			if (Main.netMode != NetmodeID.SinglePlayer && SubworldSystem.Current is MappingWorld)
			{
				ModContent.GetInstance<RequestMappingDomainInfoHandler>().Send((byte)player.whoAmI);
			}
		};
	}

	/// <inheritdoc cref="Networking.Message.RequestMappingDomainInfo"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte who);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(who);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		byte who = reader.ReadByte();

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write((short)MappingWorld.AreaLevel);
		packet.Write((short)MappingWorld.MapTier);
		packet.Write((byte)MappingWorld.Affixes.Count);

		foreach (MapAffix item in MappingWorld.Affixes)
		{
			item.NetSend(packet);
		}

		packet.Send(who);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		SendMappingDomainInfoHandler.GetAndSetMappingDomainInfo(reader);
	}
}
