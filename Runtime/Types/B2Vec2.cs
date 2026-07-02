using System;
using System.Runtime.InteropServices;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Box2D
{
	/// 2D vector. Matches the layout of b2Vec2 in math_functions.h exactly.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Vec2 : IEquatable<B2Vec2>
	{
		public float x;
		public float y;

		public static readonly B2Vec2 Zero = new B2Vec2(0f, 0f);

		public B2Vec2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public float Length() => MathF.Sqrt(x * x + y * y);
		public float LengthSquared() => x * x + y * y;

		public B2Vec2 Normalized()
		{
			float length = Length();
			if (length < float.Epsilon)
			{
				return Zero;
			}

			float inv = 1f / length;
			return new B2Vec2(x * inv, y * inv);
		}

		public bool IsNormalized()
		{
			float aa = Dot(this, this);
			return MathF.Abs(1f - aa) < 100f * float.Epsilon;
		}

		public static float Dot(B2Vec2 a, B2Vec2 b) => a.x * b.x + a.y * b.y;
		public static float Cross(B2Vec2 a, B2Vec2 b) => a.x * b.y - a.y * b.x;
		public static B2Vec2 CrossVS(B2Vec2 v, float s) => new B2Vec2(s * v.y, -s * v.x);
		public static B2Vec2 CrossSV(float s, B2Vec2 v) => new B2Vec2(-s * v.y, s * v.x);
		public static B2Vec2 LeftPerp(B2Vec2 v) => new B2Vec2(-v.y, v.x);
		public static B2Vec2 RightPerp(B2Vec2 v) => new B2Vec2(v.y, -v.x);

		public static B2Vec2 Lerp(B2Vec2 a, B2Vec2 b, float t) =>
			new B2Vec2((1f - t) * a.x + t * b.x, (1f - t) * a.y + t * b.y);

		public static B2Vec2 Mul(B2Vec2 a, B2Vec2 b) => new B2Vec2(a.x * b.x, a.y * b.y);
		public static B2Vec2 MulAdd(B2Vec2 a, float s, B2Vec2 b) => new B2Vec2(a.x + s * b.x, a.y + s * b.y);
		public static B2Vec2 MulSub(B2Vec2 a, float s, B2Vec2 b) => new B2Vec2(a.x - s * b.x, a.y - s * b.y);

		public static B2Vec2 Abs(B2Vec2 a) => new B2Vec2(MathF.Abs(a.x), MathF.Abs(a.y));
		public static B2Vec2 Min(B2Vec2 a, B2Vec2 b) => new B2Vec2(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y));
		public static B2Vec2 Max(B2Vec2 a, B2Vec2 b) => new B2Vec2(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y));

		public static B2Vec2 Clamp(B2Vec2 v, B2Vec2 lower, B2Vec2 upper) =>
			new B2Vec2(Math.Clamp(v.x, lower.x, upper.x), Math.Clamp(v.y, lower.y, upper.y));

		public static float Distance(B2Vec2 a, B2Vec2 b) => (b - a).Length();
		public static float DistanceSquared(B2Vec2 a, B2Vec2 b) => (b - a).LengthSquared();

		public static B2Vec2 operator +(B2Vec2 a, B2Vec2 b) => new B2Vec2(a.x + b.x, a.y + b.y);
		public static B2Vec2 operator -(B2Vec2 a, B2Vec2 b) => new B2Vec2(a.x - b.x, a.y - b.y);
		public static B2Vec2 operator -(B2Vec2 a) => new B2Vec2(-a.x, -a.y);
		public static B2Vec2 operator *(float s, B2Vec2 v) => new B2Vec2(s * v.x, s * v.y);
		public static B2Vec2 operator *(B2Vec2 v, float s) => new B2Vec2(s * v.x, s * v.y);

		public static bool operator ==(B2Vec2 a, B2Vec2 b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(B2Vec2 a, B2Vec2 b) => !(a == b);

		public bool Equals(B2Vec2 other) => this == other;
		public override bool Equals(object obj) => obj is B2Vec2 other && Equals(other);
		public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2);
		public override string ToString() => $"({x:0.###}, {y:0.###})";

#if UNITY_5_3_OR_NEWER
		public static implicit operator Vector2(B2Vec2 v) => new Vector2(v.x, v.y);
		public static implicit operator B2Vec2(Vector2 v) => new B2Vec2(v.x, v.y);
		public static explicit operator Vector3(B2Vec2 v) => new Vector3(v.x, v.y, 0f);
		public static explicit operator B2Vec2(Vector3 v) => new B2Vec2(v.x, v.y);
#endif
	}
}
