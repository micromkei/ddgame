using System;
using Box2D.Interop;

namespace Box2D
{
	/// Body B is a wheel that rotates freely and slides along the local x-axis of frame A.
	/// Supports a linear spring, linear limits, and a rotational motor - used to simulate vehicle
	/// wheels.
	public readonly struct B2WheelJoint : IEquatable<B2WheelJoint>
	{
		public readonly B2JointId Id;

		public B2WheelJoint(B2JointId id)
		{
			Id = id;
		}

		public static B2WheelJoint Create(B2World world, B2WheelJointDef def) =>
			new B2WheelJoint(B2Native.b2CreateWheelJoint(world.Id, in def));

		public B2Joint Generic => new B2Joint(Id);

		public bool EnableSpring
		{
			get => B2Native.b2WheelJoint_IsSpringEnabled(Id);
			set => B2Native.b2WheelJoint_EnableSpring(Id, value);
		}

		public float SpringHertz
		{
			get => B2Native.b2WheelJoint_GetSpringHertz(Id);
			set => B2Native.b2WheelJoint_SetSpringHertz(Id, value);
		}

		public float SpringDampingRatio
		{
			get => B2Native.b2WheelJoint_GetSpringDampingRatio(Id);
			set => B2Native.b2WheelJoint_SetSpringDampingRatio(Id, value);
		}

		public bool EnableLimit
		{
			get => B2Native.b2WheelJoint_IsLimitEnabled(Id);
			set => B2Native.b2WheelJoint_EnableLimit(Id, value);
		}

		public float LowerLimit => B2Native.b2WheelJoint_GetLowerLimit(Id);
		public float UpperLimit => B2Native.b2WheelJoint_GetUpperLimit(Id);
		public void SetLimits(float lower, float upper) => B2Native.b2WheelJoint_SetLimits(Id, lower, upper);

		public bool EnableMotor
		{
			get => B2Native.b2WheelJoint_IsMotorEnabled(Id);
			set => B2Native.b2WheelJoint_EnableMotor(Id, value);
		}

		public float MotorSpeed
		{
			get => B2Native.b2WheelJoint_GetMotorSpeed(Id);
			set => B2Native.b2WheelJoint_SetMotorSpeed(Id, value);
		}

		public float MaxMotorTorque
		{
			get => B2Native.b2WheelJoint_GetMaxMotorTorque(Id);
			set => B2Native.b2WheelJoint_SetMaxMotorTorque(Id, value);
		}

		public float MotorTorque => B2Native.b2WheelJoint_GetMotorTorque(Id);

		public bool Equals(B2WheelJoint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2WheelJoint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2WheelJoint a, B2WheelJoint b) => a.Equals(b);
		public static bool operator !=(B2WheelJoint a, B2WheelJoint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2WheelJoint joint) => joint.Id;
		public static implicit operator B2Joint(B2WheelJoint joint) => new B2Joint(joint.Id);
	}
}
