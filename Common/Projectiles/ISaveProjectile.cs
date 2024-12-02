using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Projectiles;

internal interface ISaveProjectile
{
	public void SaveData(TagCompound tag) { }
	public void LoadData(TagCompound tag, Projectile projectile) { }
}
