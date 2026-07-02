using System;
using System.Runtime.InteropServices;
using Box2D.Interop;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Box2D
{
	/// Cosine and sine pair, as returned by the native (cross-platform deterministic) trig functions.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2CosSin
	{
		public float cosine;
		public float sine;
	}

	/// 2D rotation represented as a unit complex number (cosine, sine).
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Rot : IEquatable<B2Rot>
	{
		public float c;
		public float s;

		public static readonly B2Rot Identity = new B2Rot(1f, 0f);

		public B2Rot(float c, float s)
		{
			this.c = c;
			this.s = s;
		}

		/// Uses Box2D's own cosine/sine implementation so rotations built here match the solver exactly.
		public static B2Rot FromAngle(float radians)
		{
			B2CosSin cs = B2Native.b2ComputeCosSin(radians);
			return new B2Rot(cs.cosine, cs.sine);
		}

		public static B2Rot FromUnitVector(B2Vec2 unitVector) => new B2Rot(unitVector.x, unitVector.y);

		public float Angle => B2Native.b2Atan2(s, c);

		public B2Vec2 XAxis => new B2Vec2(c, s);
		public B2Vec2 YAxis => new B2Vec2(-s, c);

		public bool IsNormalized()
		{
			float qq = s * s + c * c;
			return 1f - 0.0006f < qq && qq < 1f + 0.0006f;
		}

		public B2Rot Normalized()
		{
			float mag = MathF.Sqrt(s * s + c * c);
			float invMag = mag > 0f ? 1f / mag : 0f;
			return new B2Rot(c * invMag, s * invMag);
		}

		public B2Rot Inverted() => new B2Rot(c, -s);

		/// Integrate rotation from angular velocity (radians) over one step.
		public B2Rot Integrate(float deltaAngle)
		{
			B2Rot q2 = new B2Rot(c - deltaAngle * s, s + deltaAngle * c);
			return q2.Normalized();
		}

		public static B2Rot NLerp(B2Rot q1, B2Rot q2, float t)
		{
			float omt = 1f - t;
			B2Rot q = new B2Rot(omt * q1.c + t * q2.c, omt * q1.s + t * q2.s);
			return q.Normalized();
		}

		/// Angular velocity needed to rotate from q1 to q2 over 1/invH seconds.
		public static float ComputeAngularVelocity(B2Rot q1, B2Rot q2, float invH) =>
			invH * (q2.s * q1.c - q2.c * q1.s);

		public static B2Rot operator *(B2Rot q, B2Rot r) =>
			new B2Rot(q.c * r.c - q.s * r.s, q.s * r.c + q.c * r.s);

		/// inv(a) * b : rotates a vector local to frame b into frame a.
		public static B2Rot InvMul(B2Rot a, B2Rot b) =>
			new B2Rot(a.c * b.c + a.s * b.s, a.c * b.s - a.s * b.c);

		/// Relative angle from a to b, in radians.
		public static float RelativeAngle(B2Rot a, B2Rot b)
		{
			float s = a.c * b.s - a.s * b.c;
			float c = a.c * b.c + a.s * b.s;
			return B2Native.b2Atan2(s, c);
		}

		public static float UnwindAngle(float radians)
		{
			float twoPi = 2f * MathF.PI;
			float r = radians % twoPi;
			if (r < -MathF.PI) r += twoPi;
			else if (r > MathF.PI) r -= twoPi;
			return r;
		}

		public B2Vec2 RotateVector(B2Vec2 v) => new B2Vec2(c * v.x - s * v.y, s * v.x + c * v.y);
		public B2Vec2 InvRotateVector(B2Vec2 v) => new B2Vec2(c * v.x + s * v.y, -s * v.x + c * v.y);

		public bool Equals(B2Rot other) => c == other.c && s == other.s;
		public override bool Equals(object obj) => obj is B2Rot other && Equals(other);
		public override int GetHashCode() => c.GetHashCode() ^ (s.GetHashCode() << 2);
		public override string ToString() => $"(angle: {Angle * (180f / MathF.PI):0.##} deg)";

#if UNITY_5_3_OR_NEWER
		/// Rotation around the Z axis, matching Unity's 2D convention.
		public static implicit operator Quaternion(B2Rot q) => Quaternion.AngleAxis(q.Angle * Mathf.Rad2Deg, Vector3.forward);
		public static implicit operator B2Rot(Quaternion q) => FromAngle(q.eulerAngles.z * Mathf.Deg2Rad);
#endif
	}

	/// A 2D rigid transform (position + rotation). Also serves as Box2D's "world transform"
	/// since this binding targets single-precision Box2D, where b2WorldTransform == b2Transform.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Transform
	{
		public B2Vec2 p;
		public B2Rot q;

		public static readonly B2Transform Identity = new B2Transform(B2Vec2.Zero, B2Rot.Identity);

		public B2Transform(B2Vec2 p, B2Rot q)
		{
			this.p = p;
			this.q = q;
		}

		public B2Vec2 TransformPoint(B2Vec2 point) => q.RotateVector(point) + p;
		public B2Vec2 InvTransformPoint(B2Vec2 point) => q.InvRotateVector(point - p);

		/// Combine transforms: applies B first (local to A), then A.
		public static B2Transform Mul(B2Transform a, B2Transform b) =>
			new B2Transform(a.TransformPoint(b.p), a.q * b.q);

		/// Relative transform of B in the frame of A.
		public static B2Transform InvMul(B2Transform a, B2Transform b) =>
			new B2Transform(a.q.InvRotateVector(b.p - a.p), B2Rot.InvMul(a.q, b.q));
	}

	/// A 2-by-2 matrix, stored column-major (cx, cy) to match b2Mat22.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Mat22
	{
		public B2Vec2 cx;
		public B2Vec2 cy;

		public static readonly B2Mat22 Zero = new B2Mat22(B2Vec2.Zero, B2Vec2.Zero);

		public B2Mat22(B2Vec2 cx, B2Vec2 cy)
		{
			this.cx = cx;
			this.cy = cy;
		}

		public static B2Vec2 operator *(B2Mat22 a, B2Vec2 v) =>
			new B2Vec2(a.cx.x * v.x + a.cy.x * v.y, a.cx.y * v.x + a.cy.y * v.y);

		public B2Mat22 Inverse()
		{
			float a = cx.x, b = cy.x, c = cx.y, d = cy.y;
			float det = a * d - b * c;
			if (det != 0f) det = 1f / det;
			return new B2Mat22(new B2Vec2(det * d, -det * c), new B2Vec2(-det * b, det * a));
		}

		/// Solve A * x = b for x.
		public B2Vec2 Solve(B2Vec2 b)
		{
			float a11 = cx.x, a12 = cy.x, a21 = cx.y, a22 = cy.y;
			float det = a11 * a22 - a12 * a21;
			if (det != 0f) det = 1f / det;
			return new B2Vec2(det * (a22 * b.x - a12 * b.y), det * (a11 * b.y - a21 * b.x));
		}
	}

	/// Axis-aligned bounding box.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2AABB
	{
		public B2Vec2 lowerBound;
		public B2Vec2 upperBound;

		public B2AABB(B2Vec2 lowerBound, B2Vec2 upperBound)
		{
			this.lowerBound = lowerBound;
			this.upperBound = upperBound;
		}

		public B2Vec2 Center => 0.5f * (lowerBound + upperBound);
		public B2Vec2 Extents => 0.5f * (upperBound - lowerBound);

		public bool Contains(B2AABB other) =>
			lowerBound.x <= other.lowerBound.x && lowerBound.y <= other.lowerBound.y &&
			other.upperBound.x <= upperBound.x && other.upperBound.y <= upperBound.y;

		public bool Overlaps(B2AABB other) =>
			!(other.lowerBound.x > upperBound.x || other.lowerBound.y > upperBound.y ||
			  lowerBound.x > other.upperBound.x || lowerBound.y > other.upperBound.y);

		public static B2AABB Union(B2AABB a, B2AABB b) =>
			new B2AABB(B2Vec2.Min(a.lowerBound, b.lowerBound), B2Vec2.Max(a.upperBound, b.upperBound));
	}

	/// A plane in 2D: separation = dot(normal, point) - offset.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Plane
	{
		public B2Vec2 normal;
		public float offset;

		public float Separation(B2Vec2 point) => B2Vec2.Dot(normal, point) - offset;
	}

	public static class B2MathUtil
	{
		/// One-dimensional mass-spring-damper step. Returns the new velocity; integrate position yourself.
		public static float SpringDamper(float hertz, float dampingRatio, float position, float velocity, float timeStep)
		{
			float omega = 2f * MathF.PI * hertz;
			float omegaH = omega * timeStep;
			return (velocity - omega * omegaH * position) / (1f + 2f * dampingRatio * omegaH + omegaH * omegaH);
		}
	}
}
