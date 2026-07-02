using System;
using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// Small helpers for marshaling the "pointer + count" style buffers Box2D returns for
	/// per-step event arrays (b2World_GetBodyEvents and friends).
	internal static class B2Marshal
	{
		public static T[] ToArray<T>(IntPtr ptr, int count) where T : struct
		{
			var result = new T[count];
			if (count == 0 || ptr == IntPtr.Zero)
			{
				return result;
			}

			int stride = Marshal.SizeOf<T>();
			for (int i = 0; i < count; i++)
			{
				result[i] = Marshal.PtrToStructure<T>(IntPtr.Add(ptr, i * stride));
			}

			return result;
		}

		public static object GetUserData(IntPtr ptr) => ptr == IntPtr.Zero ? null : GCHandle.FromIntPtr(ptr).Target;

		public static IntPtr SetUserData(IntPtr previous, object value)
		{
			if (previous != IntPtr.Zero)
			{
				GCHandle.FromIntPtr(previous).Free();
			}

			return value != null ? GCHandle.ToIntPtr(GCHandle.Alloc(value)) : IntPtr.Zero;
		}
	}
}
