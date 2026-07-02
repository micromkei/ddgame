using System.Runtime.InteropServices;

namespace Box2D
{
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Version
	{
		public int major;
		public int minor;
		public int revision;

		public override string ToString() => $"{major}.{minor}.{revision}";
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate System.IntPtr B2AllocFcn(System.UIntPtr size, int alignment);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void B2FreeFcn(System.IntPtr mem, System.UIntPtr size);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int B2AssertFcn([MarshalAs(UnmanagedType.LPStr)] string condition, [MarshalAs(UnmanagedType.LPStr)] string fileName, int lineNumber);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void B2LogFcn([MarshalAs(UnmanagedType.LPStr)] string message);
}
