using System;
using Box2D.Interop;

namespace Box2D
{
	/// Disables collision between two specific bodies. As a side effect of being a joint, it also
	/// keeps the two bodies in the same simulation island.
	public readonly struct B2FilterJoint : IEquatable<B2FilterJoint>
	{
		public readonly B2JointId Id;

		public B2FilterJoint(B2JointId id)
		{
			Id = id;
		}

		public static B2FilterJoint Create(B2World world, B2FilterJointDef def) =>
			new B2FilterJoint(B2Native.b2CreateFilterJoint(world.Id, in def));

		public B2Joint Generic => new B2Joint(Id);

		public bool Equals(B2FilterJoint other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2FilterJoint other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2FilterJoint a, B2FilterJoint b) => a.Equals(b);
		public static bool operator !=(B2FilterJoint a, B2FilterJoint b) => !a.Equals(b);

		public static implicit operator B2JointId(B2FilterJoint joint) => joint.Id;
		public static implicit operator B2Joint(B2FilterJoint joint) => new B2Joint(joint.Id);
	}
}
