using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class HotbarPotionHandler
{
	public static void SendHotbarPotionUse(byte playerWhoAmI, bool isHealingPotion, byte newValue, bool runLocally = false)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SetHotbarPotionUse);

		packet.Write(playerWhoAmI);
		packet.Write(isHealingPotion);
		packet.Write(newValue);
		packet.Send();

		if (runLocally)
		{
			SetHotbarPotion(playerWhoAmI, isHealingPotion, newValue);
		}
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		byte who = reader.ReadByte();
		bool isHeal = reader.ReadBoolean();
		byte newValue = reader.ReadByte();

		SetHotbarPotion(who, isHeal, newValue);

		ModPacket packet = Networking.GetPacket(Networking.Message.SetHotbarPotionUse);

		packet.Write(who);
		packet.Write(isHeal);
		packet.Write(newValue);
		packet.Send(-1, who);
	}
	
	internal static void ClientRecieve(BinaryReader reader)
	{
		SetHotbarPotion(reader.ReadByte(), reader.ReadBoolean(), reader.ReadByte());
	}

	public static void SetHotbarPotion(byte playerWhoAmI, bool isHealingPotion, int newValue)
	{
		PotionSystem player = Main.player[playerWhoAmI].GetModPlayer<PotionSystem>();
		
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
