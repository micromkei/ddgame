using System;
using System.Runtime.InteropServices;
using Box2D.Interop;

namespace Box2D
{
	/// Base joint definition embedded as the first field of every specific joint def, matching
	/// how b2JointDef is embedded as `base` in the C structs (renamed here since `base` is a
	/// reserved word in C#).
	[StructLayout(LayoutKind.Sequential)]
	public struct B2JointDef
	{
		public IntPtr userData;
		public B2BodyId bodyIdA;
		public B2BodyId bodyIdB;
		public B2Transform localFrameA;
		public B2Transform localFrameB;
		public float forceThreshold;
		public float torqueThreshold;
		public float constraintHertz;
		public float constraintDampingRatio;
		public float drawScale;
		private byte collideConnected;

		public bool CollideConnected
		{
			get => collideConnected != 0;
			set => collideConnected = B2Bool.From(value);
		}
	}

	/// Connects a point on body A with a point on body B by a segment. Matches b2DistanceJointDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2DistanceJointDef
	{
		public B2JointDef Base;
		public float length;
		private byte enableSpring;
		public float lowerSpringForce;
		public float upperSpringForce;
		public float hertz;
		public float dampingRatio;
		private byte enableLimit;
		public float minLength;
		public float maxLength;
		private byte enableMotor;
		public float maxMotorForce;
		public float motorSpeed;
		internal int internalValue;

		public bool EnableSpring { get => enableSpring != 0; set => enableSpring = B2Bool.From(value); }
		public bool EnableLimit { get => enableLimit != 0; set => enableLimit = B2Bool.From(value); }
		public bool EnableMotor { get => enableMotor != 0; set => enableMotor = B2Bool.From(value); }

		public static B2DistanceJointDef Default() => B2Native.b2DefaultDistanceJointDef();
	}

	/// Controls the relative velocity and/or transform between two bodies. Matches b2MotorJointDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2MotorJointDef
	{
		public B2JointDef Base;
		public B2Vec2 linearVelocity;
		public float maxVelocityForce;
		public float angularVelocity;
		public float maxVelocityTorque;
		public float linearHertz;
		public float linearDampingRatio;
		public float maxSpringForce;
		public float angularHertz;
		public float angularDampingRatio;
		public float maxSpringTorque;
		internal int internalValue;

		public static B2MotorJointDef Default() => B2Native.b2DefaultMotorJointDef();
	}

	/// Disables collision between two specific bodies. Matches b2FilterJointDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2FilterJointDef
	{
		public B2JointDef Base;
		internal int internalValue;

		public static B2FilterJointDef Default() => B2Native.b2DefaultFilterJointDef();
	}

	/// Body B slides along the local x-axis of frame A with no relative rotation. Matches
	/// b2PrismaticJointDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2PrismaticJointDef
	{
		public B2JointDef Base;
		private byte enableSpring;
		public float hertz;
		public float dampingRatio;
		public float targetTranslation;
		private byte enableLimit;
		public float lowerTranslation;
		public float upperTranslation;
		private byte enableMotor;
		public float maxMotorForce;
		public float motorSpeed;
		internal int internalValue;

		public bool EnableSpring { get => enableSpring != 0; set => enableSpring = B2Bool.From(value); }
		public bool EnableLimit { get => enableLimit != 0; set => enableLimit = B2Bool.From(value); }
		public bool EnableMotor { get => enableMotor != 0; set => enableMotor = B2Bool.From(value); }

		public static B2PrismaticJointDef Default() => B2Native.b2DefaultPrismaticJointDef();
	}

	/// A point on body B is pinned to a point on body A, allowing relative rotation. Matches
	/// b2RevoluteJointDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2RevoluteJointDef
	{
		public B2JointDef Base;
		public float targetAngle;
		private byte enableSpring;
		public float hertz;
		public float dampingRatio;
		private byte enableLimit;
		public float lowerAngle;
		public float upperAngle;
		private byte enableMotor;
		public float maxMotorTorque;
		public float motorSpeed;
		internal int internalValue;

		public bool EnableSpring { get => enableSpring != 0; set => enableSpring = B2Bool.From(value); }
		public bool EnableLimit { get => enableLimit != 0; set => enableLimit = B2Bool.From(value); }
		public bool EnableMotor { get => enableMotor != 0; set => enableMotor = B2Bool.From(value); }

		public static B2RevoluteJointDef Default() => B2Native.b2DefaultRevoluteJointDef();
	}

	/// Rigidly connects two bodies, optionally with springy give. Matches b2WeldJointDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2WeldJointDef
	{
		public B2JointDef Base;
		public float linearHertz;
		public float angularHertz;
		public float linearDampingRatio;
		public float angularDampingRatio;
		internal int internalValue;

		public static B2WeldJointDef Default() => B2Native.b2DefaultWeldJointDef();
	}

	/// Body B is a wheel that rotates freely and slides along the local x-axis of frame A.
	/// Matches b2WheelJointDef.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2WheelJointDef
	{
		public B2JointDef Base;
		private byte enableSpring;
		public float hertz;
		public float dampingRatio;
		private byte enableLimit;
		public float lowerTranslation;
		public float upperTranslation;
		private byte enableMotor;
		public float maxMotorTorque;
		public float motorSpeed;
		internal int internalValue;

		public bool EnableSpring { get => enableSpring != 0; set => enableSpring = B2Bool.From(value); }
		public bool EnableLimit { get => enableLimit != 0; set => enableLimit = B2Bool.From(value); }
		public bool EnableMotor { get => enableMotor != 0; set => enableMotor = B2Bool.From(value); }

		public static B2WheelJointDef Default() => B2Native.b2DefaultWheelJointDef();
	}
}
