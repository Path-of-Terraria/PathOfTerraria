namespace PathOfTerraria.Common.Mechanics;

[Flags]
public enum SkillTags : ulong
{
	None = 0,
	Fire = 1,
	Cold = 2,
	Lightning = 4,
	Chaos = 8,
	Melee = 16,
	Ranged = 32,
	Magic = 64,
	Summon = 128,
	Projectile = 256,
	Channeling = 512,
	AreaOfEffect = 1024,
	Sentry = 2048,
	Minion = 4096,
	Debuff = 8192,
	Buff = 16384,
	Movement = 32768,
	Critical = 65536,
	Healing = 131072,

	Elemental = Fire | Cold | Lightning | Chaos,
}
