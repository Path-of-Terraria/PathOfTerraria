using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs the usage of hotbar potions.
/// </summary>
internal class HotbarPotionHandler : Handler
{
	public static void Send(bool isHealingPotion, byte newValue)
	{
		ModPacket packet = Networking.GetPacket<HotbarPotionHandler>();
		packet.Write(isHealingPotion);
		packet.Write(newValue);
		packet.Send();
	}

	internal override void Receive(BinaryReader reader, byte sender)
	{
		byte who = sender >= byte.MaxValue ? reader.ReadByte() : sender;
		bool isHeal = reader.ReadBoolean();
		byte newValue = reader.ReadByte();

		SetHotbarPotion(who, isHeal, newValue);

		if (Main.netMode == NetmodeID.Server)
		{
			ModPacket packet = Networking.GetPacket(Id);

			packet.Write(who);
			packet.Write(isHeal);
			packet.Write(newValue);
			packet.Send(-1, who);
		}
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
