using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class HotbarPotionHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SetHotbarPotionUse;

	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte playerWhoAmI, out bool isHealingPotion, out byte newValue);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(playerWhoAmI);
		packet.Write(isHealingPotion);
		packet.Write(newValue);
		packet.Send();

		if (TryGetOptionalValue(parameters, 3, out bool runLocally) && runLocally)
		{
			SetHotbarPotion(playerWhoAmI, isHealingPotion, newValue);
		}
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		byte who = reader.ReadByte();
		bool isHeal = reader.ReadBoolean();
		byte newValue = reader.ReadByte();

		SetHotbarPotion(who, isHeal, newValue);

		ModPacket packet = Networking.GetPacket(MessageType);

		packet.Write(who);
		packet.Write(isHeal);
		packet.Write(newValue);
		packet.Send(-1, who);
	}
	
	internal override void ClientRecieve(BinaryReader reader)
	{
		SetHotbarPotion(reader.ReadByte(), reader.ReadBoolean(), reader.ReadByte());
	}

	public static void SetHotbarPotion(byte playerWhoAmI, bool isHealingPotion, int newValue)
	{
		PotionPlayer player = Main.player[playerWhoAmI].GetModPlayer<PotionPlayer>();
		
		if (isHealingPotion)
		{
			player.HealingLeft = newValue;
		}
		else
		{
			player.ManaLeft = newValue;
		}
	}
}
