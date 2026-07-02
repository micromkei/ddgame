using System;
using Box2D.Interop;

namespace Box2D
{
	/// Connects a point on body A with a point on body B by a segment. Useful for ropes and springs.
	public readonly struct B2DistanceJoint : IEquatable<B2DistanceJoint>
	{
		public readonly B2JointId Id;

		public B2DistanceJoint(B2JointId id)
		{
			Id = id;
		}

		public static B2DistanceJoint Create(B2World world, B2DistanceJointDef def) =>
			new B2DistanceJoint(B2Native.b2CreateDistanceJoint(world.Id, in def));

		public B2Joint Generic => new B2Joint(Id);

		public float Length
		{
			get => B2Native.b2DistanceJoint_GetLength(Id);
			set => B2Native.b2DistanceJoint_SetLength(Id, value);
		}

		public bool EnableSpring
		{
			get => B2Native.b2DistanceJoint_IsSpringEnabled(Id);
			set => B2Native.b2DistanceJoint_EnableSpring(Id, value);
		}

		public void SetSpringForceRange(float lowerForce, float upperForce) => B2Native.b2DistanceJoint_SetSpringForceRange(Id, lowerForce, upperForce);
		public void GetSpringForceRange(out float lowerForce, out float upperForce) => B2Native.b2DistanceJoint_GetSpringForceRange(Id, out lowerForce, out upperForce);

		public float SpringHertz
		{
			get => B2Native.b2DistanceJoint_GetSpringHertz(Id);
			set => B2Native.b2DistanceJoint_SetSpringHertz(Id, value);
		}

		public float SpringDampingRatio
		{
			get => B2Native.b2DistanceJoint_GetSpringDampingRatio(Id);
			set => B2Native.b2DistanceJoint_SetSpringDampingRatio(Id, value);
		}

		public bool EnableLimit
		{
			get => B2Native.b2DistanceJoint_IsLimitEnabled(Id);
			set => B2Native.b2DistanceJoint_EnableLimit(Id, value);
		}

		public void SetLengthRange(float minLength, float maxLength) => B2Native.b2DistanceJoint_SetLengthRange(Id, minLength, maxLength);
		public float MinLength => B2Native.b2DistanceJoint_GetMinLength(Id);
		public float MaxLength => B2Native.b2DistanceJoint_GetMaxLength(Id);
		public float CurrentLength => B2Native.b2DistanceJoint_GetCurrentLength(Id);

		public bool EnableMotor
		{
			get => B2Native.b2DistanceJoint_IsMotorEnabled(Id);
			set => B2Native.b2DistanceJoint_EnableMotor(Id, value);
		}

		public float MotorSpeed
		{
			get => B2Native.b2DistanceJoint_GetMotorSpeed(Id);
			set => B2Native.b2DistanceJoint_SetMotorSpeed(Id, value);
		}

		public float MaxMotorForce
		{
			get => B2Native.b2DistanceJoint_GetMaxMotorForce(Id);
			set => B2Native.b2DistanceJoint_SetMaxMotorForce(Id, value);
		}

		public float MotorForce => B2Native.b2DistanceJoint_GetMotorForce(Id);

		public bool Equals(B2DistanceJoint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2DistanceJoint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2DistanceJoint a, B2DistanceJoint b) => a.Equals(b);
		public static bool operator !=(B2DistanceJoint a, B2DistanceJoint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2DistanceJoint joint) => joint.Id;
		public static implicit operator B2Joint(B2DistanceJoint joint) => new B2Joint(joint.Id);
	}
}
