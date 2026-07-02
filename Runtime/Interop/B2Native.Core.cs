using System;
using System.Runtime.InteropServices;

namespace Box2D.Interop
{
	/// Raw P/Invoke declarations for the Box2D C API. Method names match the native
	/// symbols exactly (see box2d/include/box2d/*.h) so they can be cross-referenced
	/// directly against the upstream Box2D documentation and headers.
	///
	/// This binding assumes a single-precision build of Box2D (BOX2D_DOUBLE_PRECISION off,
	/// the default). Do not use it against a double-precision native build.
	///
	/// Every native `bool` is 1 byte (C99 stdbool). C#'s default marshaling for `bool` is a
	/// 4-byte Win32 BOOL, so every bool field in an interop-facing struct is declared here as
	/// `byte` (see the various B2*Def / event structs), and every bool parameter/return on a
	/// function is explicitly marshaled with [MarshalAs(UnmanagedType.I1)].
	internal static partial class B2Native
	{
		internal const string LIB = "box2d";

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2SetAllocator(B2AllocFcn allocFcn, B2FreeFcn freeFcn);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern long b2GetByteCount();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2SetAssertFcn(B2AssertFcn assertFcn);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2SetLogFcn(B2LogFcn logFcn);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Version b2GetVersion();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsDoublePrecision();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong b2GetTicks();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2GetMilliseconds(ulong ticks);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2GetMillisecondsAndReset(ref ulong ticks);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2Yield();

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint b2Hash(uint hash, byte[] data, int count);

		// ---- math_functions.h exported functions ----

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsValidFloat(float a);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsValidVec2(B2Vec2 v);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsValidRotation(B2Rot q);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsValidTransform(B2Transform t);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsValidAABB(B2AABB aabb);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool b2IsValidPlane(B2Plane a);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2Atan2(float y, float x);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2CosSin b2ComputeCosSin(float radians);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern B2Rot b2ComputeRotationBetweenUnitVectors(B2Vec2 v1, B2Vec2 v2);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern void b2SetLengthUnitsPerMeter(float lengthUnits);

		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		public static extern float b2GetLengthUnitsPerMeter();
	}
}
