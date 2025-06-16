using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.EoLDomain;

internal class EoLDomainSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (SubworldSystem.Current is not EmpressDomain)
		{
			return;
		}

		tileColor = new Color(50, 50, 50);
		backgroundColor = new Color(50, 50, 50);
	}
}
