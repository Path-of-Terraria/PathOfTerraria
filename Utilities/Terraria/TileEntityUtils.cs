using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Utilities.Terraria;

#nullable enable

internal static class TileEntityUtils
{
	public static bool Remove(Point16 point, bool sync = true)
	{
		if (TileEntity.ByPosition.TryGetValue(point, out TileEntity? entity))
		{
			return Remove(entity.ID, sync: sync);
		}
		
		return false;
	}

	public static bool Remove(int id, bool sync = true)
	{
		lock (TileEntity.EntityCreationLock)
		{
			if (TileEntity.ByID.Remove(id, out TileEntity? entity))
			{
				TileEntity.ByPosition.Remove(entity.Position);

                if (Main.netMode == NetmodeID.Server && sync)
                {
					NetMessage.SendData(MessageID.TileEntitySharing, number: entity.ID);
                }
                
				return true;
			}
		}
		
		return false;
	}
}
