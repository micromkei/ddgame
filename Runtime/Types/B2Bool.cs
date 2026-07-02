namespace Box2D
{
	/// Converts between C# bool and the 1-byte bool representation used by native interop
	/// structs. Never add a C# `bool` field to a struct that crosses the P/Invoke boundary -
	/// use `byte` plus a bool property built on this helper instead, so the struct stays
	/// blittable and matches C's 1-byte stdbool layout exactly.
	internal static class B2Bool
	{
		public static byte From(bool value) => (byte)(value ? 1 : 0);
		public static bool To(byte value) => value != 0;
	}
}
