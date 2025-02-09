namespace EasyLayoutNS
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class EnumHelper<T>
#if CSHARP_7_3_OR_NEWER
		where T : struct, Enum
#else
		where T : struct
#endif
	{
		private static readonly Type EnumType = typeof(T);

		private static readonly object sync = new object();

		public static readonly bool IsFlags = GetIsFlags();

		private static T[] values;

		private static T[] Values
		{
			get
			{
				if (values == null)
				{
					values = GetValues();
				}

				return values;
			}
		}

		private static string[] names;

		private static string[] Names
		{
			get
			{
				if (names == null)
				{
					names = GetNames();
				}

				return names;
			}
		}

		private static Dictionary<T, string> value2Name;

		private static Dictionary<T, string> Value2Name
		{
			get
			{
				if (value2Name == null)
				{
					value2Name = GetValue2Name();
				}

				return value2Name;
			}
		}

		private static T[] GetValues()
		{
			lock (sync)
			{
				return (T[])Enum.GetValues(EnumType);
			}

		}

		private static string[] GetNames()
		{
			lock (sync)
			{
				return Enum.GetNames(EnumType);
			}
		}

		private static Dictionary<T, string> GetValue2Name()
		{
			lock (sync)
			{
				var result = value2Name;
				if (result != null)
				{
					return result;
				}

				result = new Dictionary<T, string>(Names.Length, EqualityComparer<T>.Default);
				for (int i = 0; i < Values.Length; i++)
				{
					if (!result.ContainsKey(values[i]))
					{
						result.Add(values[i], names[i]);
					}
				}

				return result;
			}
		}

		private static bool GetIsFlags()
		{
			return EnumType.IsEnum && EnumType.IsDefined(typeof(FlagsAttribute), false);
		}

		public static string ToString(T value)
		{
			string name;
			if (Value2Name.TryGetValue(value, out name))
			{
				return name;
			}

			// optional: flags version
			// optional: int conversion

			return value.ToString();
		}
	}
}