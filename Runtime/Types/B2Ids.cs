using System;
using System.Runtime.InteropServices;

namespace Box2D
{
	/// Opaque handle to a world. Treat as a value type; null-check with IsNull.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2WorldId : IEquatable<B2WorldId>
	{
		public ushort index1;
		public ushort generation;

		public static readonly B2WorldId Null = default;
		public bool IsNull => index1 == 0;
		public bool IsNonNull => index1 != 0;

		public bool Equals(B2WorldId other) => index1 == other.index1 && generation == other.generation;
		public override bool Equals(object obj) => obj is B2WorldId other && Equals(other);
		public override int GetHashCode() => (index1 << 16) | generation;
		public static bool operator ==(B2WorldId a, B2WorldId b) => a.Equals(b);
		public static bool operator !=(B2WorldId a, B2WorldId b) => !a.Equals(b);
		public override string ToString() => $"WorldId({index1}:{generation})";
	}

	/// Opaque handle to a body. Treat as a value type; null-check with IsNull.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2BodyId : IEquatable<B2BodyId>
	{
		public int index1;
		public ushort world0;
		public ushort generation;

		public static readonly B2BodyId Null = default;
		public bool IsNull => index1 == 0;
		public bool IsNonNull => index1 != 0;

		public bool Equals(B2BodyId other) => index1 == other.index1 && world0 == other.world0 && generation == other.generation;
		public override bool Equals(object obj) => obj is B2BodyId other && Equals(other);
		public override int GetHashCode() => HashCombine(index1, world0, generation);
		public static bool operator ==(B2BodyId a, B2BodyId b) => a.Equals(b);
		public static bool operator !=(B2BodyId a, B2BodyId b) => !a.Equals(b);
		public override string ToString() => $"BodyId({index1}:{world0}:{generation})";

		internal static int HashCombine(int a, int b, int c) => (a * 397 ^ b) * 397 ^ c;
	}

	/// Opaque handle to a shape. Treat as a value type; null-check with IsNull.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ShapeId : IEquatable<B2ShapeId>
	{
		public int index1;
		public ushort world0;
		public ushort generation;

		public static readonly B2ShapeId Null = default;
		public bool IsNull => index1 == 0;
		public bool IsNonNull => index1 != 0;

		public bool Equals(B2ShapeId other) => index1 == other.index1 && world0 == other.world0 && generation == other.generation;
		public override bool Equals(object obj) => obj is B2ShapeId other && Equals(other);
		public override int GetHashCode() => B2BodyId.HashCombine(index1, world0, generation);
		public static bool operator ==(B2ShapeId a, B2ShapeId b) => a.Equals(b);
		public static bool operator !=(B2ShapeId a, B2ShapeId b) => !a.Equals(b);
		public override string ToString() => $"ShapeId({index1}:{world0}:{generation})";
	}

	/// Opaque handle to a chain shape. Treat as a value type; null-check with IsNull.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ChainId : IEquatable<B2ChainId>
	{
		public int index1;
		public ushort world0;
		public ushort generation;

		public static readonly B2ChainId Null = default;
		public bool IsNull => index1 == 0;
		public bool IsNonNull => index1 != 0;

		public bool Equals(B2ChainId other) => index1 == other.index1 && world0 == other.world0 && generation == other.generation;
		public override bool Equals(object obj) => obj is B2ChainId other && Equals(other);
		public override int GetHashCode() => B2BodyId.HashCombine(index1, world0, generation);
		public static bool operator ==(B2ChainId a, B2ChainId b) => a.Equals(b);
		public static bool operator !=(B2ChainId a, B2ChainId b) => !a.Equals(b);
		public override string ToString() => $"ChainId({index1}:{world0}:{generation})";
	}

	/// Opaque handle to a joint. Treat as a value type; null-check with IsNull.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2JointId : IEquatable<B2JointId>
	{
		public int index1;
		public ushort world0;
		public ushort generation;

		public static readonly B2JointId Null = default;
		public bool IsNull => index1 == 0;
		public bool IsNonNull => index1 != 0;

		public bool Equals(B2JointId other) => index1 == other.index1 && world0 == other.world0 && generation == other.generation;
		public override bool Equals(object obj) => obj is B2JointId other && Equals(other);
		public override int GetHashCode() => B2BodyId.HashCombine(index1, world0, generation);
		public static bool operator ==(B2JointId a, B2JointId b) => a.Equals(b);
		public static bool operator !=(B2JointId a, B2JointId b) => !a.Equals(b);
		public override string ToString() => $"JointId({index1}:{world0}:{generation})";
	}

	/// Opaque handle to a transient contact. Note the explicit padding field, which mirrors
	/// the C struct exactly (b2ContactId has an int16 pad before the uint32 generation) so the
	/// managed and native layouts agree byte-for-byte.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ContactId : IEquatable<B2ContactId>
	{
		public int index1;
		public ushort world0;
		public short padding;
		public uint generation;

		public static readonly B2ContactId Null = default;
		public bool IsNull => index1 == 0;
		public bool IsNonNull => index1 != 0;

		public bool Equals(B2ContactId other) => index1 == other.index1 && world0 == other.world0 && generation == other.generation;
		public override bool Equals(object obj) => obj is B2ContactId other && Equals(other);
		public override int GetHashCode() => B2BodyId.HashCombine(index1, world0, (int)generation);
		public static bool operator ==(B2ContactId a, B2ContactId b) => a.Equals(b);
		public static bool operator !=(B2ContactId a, B2ContactId b) => !a.Equals(b);
		public override string ToString() => $"ContactId({index1}:{world0}:{generation})";
	}
}
