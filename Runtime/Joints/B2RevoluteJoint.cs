using System;
using Box2D.Interop;

namespace Box2D
{
	/// A point on body B is pinned to a point on body A, allowing relative rotation. The most
	/// common joint - used for ragdolls, chains, and hinges. Also called a hinge or pin joint.
	public readonly struct B2RevoluteJoint : IEquatable<B2RevoluteJoint>
	{
		public readonly B2JointId Id;

		public B2RevoluteJoint(B2JointId id)
		{
			Id = id;
		}

		public static B2RevoluteJoint Create(B2World world, B2RevoluteJointDef def) =>
			new B2RevoluteJoint(B2Native.b2CreateRevoluteJoint(world.Id, in def));

		public B2Joint Generic => new B2Joint(Id);

		public bool EnableSpring
		{
			get => B2Native.b2RevoluteJoint_IsSpringEnabled(Id);
			set => B2Native.b2RevoluteJoint_EnableSpring(Id, value);
		}

		public float SpringHertz
		{
			get => B2Native.b2RevoluteJoint_GetSpringHertz(Id);
			set => B2Native.b2RevoluteJoint_SetSpringHertz(Id, value);
		}

		public float SpringDampingRatio
		{
			get => B2Native.b2RevoluteJoint_GetSpringDampingRatio(Id);
			set => B2Native.b2RevoluteJoint_SetSpringDampingRatio(Id, value);
		}

		public float TargetAngle
		{
			get => B2Native.b2RevoluteJoint_GetTargetAngle(Id);
			set => B2Native.b2RevoluteJoint_SetTargetAngle(Id, value);
		}

		public float Angle => B2Native.b2RevoluteJoint_GetAngle(Id);

		public bool EnableLimit
		{
			get => B2Native.b2RevoluteJoint_IsLimitEnabled(Id);
			set => B2Native.b2RevoluteJoint_EnableLimit(Id, value);
		}

		public float LowerLimit => B2Native.b2RevoluteJoint_GetLowerLimit(Id);
		public float UpperLimit => B2Native.b2RevoluteJoint_GetUpperLimit(Id);
		public void SetLimits(float lower, float upper) => B2Native.b2RevoluteJoint_SetLimits(Id, lower, upper);

		public bool EnableMotor
		{
			get => B2Native.b2RevoluteJoint_IsMotorEnabled(Id);
			set => B2Native.b2RevoluteJoint_EnableMotor(Id, value);
		}

		public float MotorSpeed
		{
			get => B2Native.b2RevoluteJoint_GetMotorSpeed(Id);
			set => B2Native.b2RevoluteJoint_SetMotorSpeed(Id, value);
		}

		public float MotorTorque => B2Native.b2RevoluteJoint_GetMotorTorque(Id);

		public float MaxMotorTorque
		{
			get => B2Native.b2RevoluteJoint_GetMaxMotorTorque(Id);
			set => B2Native.b2RevoluteJoint_SetMaxMotorTorque(Id, value);
		}

		public bool Equals(B2RevoluteJoint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2RevoluteJoint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2RevoluteJoint a, B2RevoluteJoint b) => a.Equals(b);
		public static bool operator !=(B2RevoluteJoint a, B2RevoluteJoint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2RevoluteJoint joint) => joint.Id;
		public static implicit operator B2Joint(B2RevoluteJoint joint) => new B2Joint(joint.Id);
	}
}
