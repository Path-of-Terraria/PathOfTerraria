using System.IO;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.RequestMappingDomainInfo"/>
internal class RequestMappingDomainInfoHandler : Handler
{
	public sealed class RequestMappingDomainInfoHandlerPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Main.netMode != NetmodeID.SinglePlayer && SubworldSystem.Current is MappingWorld)
			{
				ModContent.GetInstance<RequestMappingDomainInfoHandler>().Send((byte)Player.whoAmI);
			}
		}
	}

	public override Networking.Message MessageType => Networking.Message.RequestMappingDomainInfo;

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
