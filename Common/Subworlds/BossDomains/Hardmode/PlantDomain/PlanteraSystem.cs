using SubworldLibrary;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.PlantDomain;

internal class PlanteraSystem : ModSystem
{
	public override void NetSend(BinaryWriter writer)
	{
		bool inDomain = SubworldSystem.Current is PlanteraDomain;
		writer.Write(inDomain);

		if (inDomain)
		{
			writer.Write((byte)PlanteraDomain.BulbsBroken);
			writer.Write(PlanteraDomain.BulbPosition.X);
			writer.Write(PlanteraDomain.BulbPosition.Y);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		if (reader.ReadBoolean())
		{
			PlanteraDomain.BulbsBroken = reader.ReadByte();
			PlanteraDomain.BulbPosition = new Point16(reader.ReadInt16(), reader.ReadInt16());
		}
	}
}

internal class PlanteraPlayer : ModPlayer
{
	public override void OnEnterWorld()
	{
		if (SubworldSystem.Current is PlanteraDomain)
		{
			NetMessage.SendData(MessageID.RequestWorldData, -1, -1, null, Player.whoAmI);
		}
	}
}