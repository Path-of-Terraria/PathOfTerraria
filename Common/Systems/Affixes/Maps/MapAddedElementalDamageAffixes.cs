using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public abstract class MapAddedElementalDamageAffix : MapAffix
{
	protected abstract ElementType AddedElementType { get; }

	public override void ModifyNewNPC(NPC npc)
	{
		if (npc.TryGetGlobalNPC(out ElementalNPC ele))
		{
			int baseDamage = npc.defDamage > 0 ? npc.defDamage : npc.damage;
			int addedDamage = (int)(baseDamage * Value / 100f);

			if (addedDamage > 0)
			{
				ele.Container.AddElementalValues((AddedElementType, addedDamage, 0f));
			}
		}
	}
}

public class MapFireAddedDamageAffix : MapAddedElementalDamageAffix
{
	protected override ElementType AddedElementType => ElementType.Fire;
}

public class MapColdAddedDamageAffix : MapAddedElementalDamageAffix
{
	protected override ElementType AddedElementType => ElementType.Cold;
}

public class MapLightningAddedDamageAffix : MapAddedElementalDamageAffix
{
	protected override ElementType AddedElementType => ElementType.Lightning;
}
