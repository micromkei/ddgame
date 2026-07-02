using System;
using Box2D.Interop;

namespace Box2D
{
	/// A transient contact between two shapes. Contact ids are only valid for a short time - the
	/// contact graph is rebuilt every step, so always check IsValid before use, especially when
	/// holding an id from a previous step (e.g. from a B2ContactBeginTouchEvent).
	public readonly struct B2Contact : IEquatable<B2Contact>
	{
		public readonly B2ContactId Id;

		public B2Contact(B2ContactId id)
		{
			Id = id;
		}

		public bool IsValid => B2Native.b2Contact_IsValid(Id);
		public B2ContactData GetData() => B2Native.b2Contact_GetData(Id);

		public bool Equals(B2Contact other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2Contact other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2Contact a, B2Contact b) => a.Equals(b);
		public static bool operator !=(B2Contact a, B2Contact b) => !a.Equals(b);

		public static implicit operator B2ContactId(B2Contact contact) => contact.Id;
	}
}
