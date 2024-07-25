using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

internal abstract class AbstractVanillaInterfaceHandler<T> : ILoadable
{
	private sealed class ItemVanillaInterfaceHandler : AbstractVanillaInterfaceHandler<Item>
	{
		protected override bool TryGetModInterface<TInterface>(Item instance, [NotNullWhen(true)] out TInterface value)
		{
			if (instance.ModItem is TInterface @interface)
			{
				value = @interface;
				return true;
			}

			value = default;
			return false;
		}

		protected override int GetType(Item instance)
		{
			return instance.type;
		}
	}

	private static AbstractVanillaInterfaceHandler<T> _instance;

	internal static AbstractVanillaInterfaceHandler<T> Instance
	{
		get => _instance ?? throw new InvalidOperationException($"Cannot handle interfaces for {typeof(T).FullName}");

		private set
		{
			if (_instance is null)
			{
				_instance = value;
				return;
			}

			throw new InvalidOperationException($"Cannot set interface handler type twice for {typeof(T).FullName}");
		}
	}

	private static Dictionary<int, Dictionary<Type, object>> _interfaceTypeMap = [];

	public AbstractVanillaInterfaceHandler()
	{
		Instance = this;
	}

	public bool TryGetInterface<TInterface>(T instance, [NotNullWhen(returnValue: true)] out TInterface value)
	{
		if (TryGetModInterface(instance, out value))
		{
			return true;
		}

		if (!_interfaceTypeMap.TryGetValue(GetType(instance), out Dictionary<Type, object> interfaceMap))
		{
			return false;
		}

		if (!interfaceMap.TryGetValue(typeof(TInterface), out object interfaceValue))
		{
			return false;
		}

		value = (TInterface)interfaceValue;
		return true;
	}

	protected abstract bool TryGetModInterface<TInterface>(T instance, [NotNullWhen(returnValue: true)] out TInterface value);

	protected abstract int GetType(T instance);

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_interfaceTypeMap = null;
	}

	public static void AddInterface<TInterface>(int type, TInterface value)
	{
		if (!_interfaceTypeMap.TryGetValue(type, out Dictionary<Type, object> interfaceMap))
		{
			_interfaceTypeMap[type] = interfaceMap = [];
		}

		interfaceMap.Add(typeof(TInterface), value);
	}
}

/// <summary>
///		Provides methods for easily interacting with interfaces that may be
///		applied to vanilla content.
/// </summary>
public static class VanillaInterfaceHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetInterface<TType, TInterface>(this TType instance, [NotNullWhen(returnValue: true)] out TInterface value)
	{
		return AbstractVanillaInterfaceHandler<TType>.Instance.TryGetInterface(instance, out value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddItemInterface<TInterface>(int itemType, TInterface value)
	{
		AbstractVanillaInterfaceHandler<Item>.AddInterface(itemType, value);
	}
}
