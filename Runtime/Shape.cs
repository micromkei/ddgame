using System;
using Box2D.Interop;

namespace Box2D
{
	/// A shape attached to a body. Thin, cheap-to-copy wrapper around a b2ShapeId handle.
	public readonly struct B2Shape : IEquatable<B2Shape>
	{
		public readonly B2ShapeId Id;

		public B2Shape(B2ShapeId id)
		{
			Id = id;
		}

		public static B2Shape CreateCircle(B2Body body, B2ShapeDef def, B2Circle circle) =>
			new B2Shape(B2Native.b2CreateCircleShape(body.Id, in def, in circle));

		public static B2Shape CreateSegment(B2Body body, B2ShapeDef def, B2Segment segment) =>
			new B2Shape(B2Native.b2CreateSegmentShape(body.Id, in def, in segment));

		public static B2Shape CreateChainSegment(B2Body body, B2ShapeDef def, B2ChainSegment chainSegment) =>
			new B2Shape(B2Native.b2CreateChainSegmentShape(body.Id, in def, in chainSegment));

		public static B2Shape CreateCapsule(B2Body body, B2ShapeDef def, B2Capsule capsule) =>
			new B2Shape(B2Native.b2CreateCapsuleShape(body.Id, in def, in capsule));

		public static B2Shape CreatePolygon(B2Body body, B2ShapeDef def, B2Polygon polygon) =>
			new B2Shape(B2Native.b2CreatePolygonShape(body.Id, in def, in polygon));

		public void Destroy(bool updateBodyMass = true) => B2Native.b2DestroyShape(Id, updateBodyMass);

		public bool IsValid => B2Native.b2Shape_IsValid(Id);

		public B2ShapeType Type => B2Native.b2Shape_GetType(Id);
		public B2Body GetBody() => new B2Body(B2Native.b2Shape_GetBody(Id));
		public B2World GetWorld() => new B2World(B2Native.b2Shape_GetWorld(Id));
		public bool IsSensor => B2Native.b2Shape_IsSensor(Id);

		/// Wraps an arbitrary managed object in a GCHandle stored as Box2D's userData pointer.
		public object UserData
		{
			get => B2Marshal.GetUserData(B2Native.b2Shape_GetUserData(Id));
			set => B2Native.b2Shape_SetUserData(Id, B2Marshal.SetUserData(B2Native.b2Shape_GetUserData(Id), value));
		}

		public void SetDensity(float density, bool updateBodyMass = true) => B2Native.b2Shape_SetDensity(Id, density, updateBodyMass);
		public float Density => B2Native.b2Shape_GetDensity(Id);

		public float Friction
		{
			get => B2Native.b2Shape_GetFriction(Id);
			set => B2Native.b2Shape_SetFriction(Id, value);
		}

		public float Restitution
		{
			get => B2Native.b2Shape_GetRestitution(Id);
			set => B2Native.b2Shape_SetRestitution(Id, value);
		}

		public ulong UserMaterial
		{
			get => B2Native.b2Shape_GetUserMaterial(Id);
			set => B2Native.b2Shape_SetUserMaterial(Id, value);
		}

		public B2SurfaceMaterial SurfaceMaterial
		{
			get => B2Native.b2Shape_GetSurfaceMaterial(Id);
			set => B2Native.b2Shape_SetSurfaceMaterial(Id, in value);
		}

		public B2Filter Filter
		{
			get => B2Native.b2Shape_GetFilter(Id);
			set => B2Native.b2Shape_SetFilter(Id, value);
		}

		public bool EnableSensorEvents
		{
			get => B2Native.b2Shape_AreSensorEventsEnabled(Id);
			set => B2Native.b2Shape_EnableSensorEvents(Id, value);
		}

		public bool EnableContactEvents
		{
			get => B2Native.b2Shape_AreContactEventsEnabled(Id);
			set => B2Native.b2Shape_EnableContactEvents(Id, value);
		}

		public bool EnablePreSolveEvents
		{
			get => B2Native.b2Shape_ArePreSolveEventsEnabled(Id);
			set => B2Native.b2Shape_EnablePreSolveEvents(Id, value);
		}

		public bool EnableHitEvents
		{
			get => B2Native.b2Shape_AreHitEventsEnabled(Id);
			set => B2Native.b2Shape_EnableHitEvents(Id, value);
		}

		public bool TestPoint(B2Vec2 point) => B2Native.b2Shape_TestPoint(Id, point);
		public B2CastOutput RayCast(B2Vec2 origin, B2Vec2 translation) => B2Native.b2Shape_RayCast(Id, origin, translation);

		public B2Circle GetCircle() => B2Native.b2Shape_GetCircle(Id);
		public B2Segment GetSegment() => B2Native.b2Shape_GetSegment(Id);
		public B2ChainSegment GetChainSegment() => B2Native.b2Shape_GetChainSegment(Id);
		public B2Capsule GetCapsule() => B2Native.b2Shape_GetCapsule(Id);
		public B2Polygon GetPolygon() => B2Native.b2Shape_GetPolygon(Id);

		public void SetCircle(B2Circle circle) => B2Native.b2Shape_SetCircle(Id, in circle);
		public void SetCapsule(B2Capsule capsule) => B2Native.b2Shape_SetCapsule(Id, in capsule);
		public void SetSegment(B2Segment segment) => B2Native.b2Shape_SetSegment(Id, in segment);
		public void SetPolygon(B2Polygon polygon) => B2Native.b2Shape_SetPolygon(Id, in polygon);
		public void SetChainSegment(B2ChainSegment chainSegment) => B2Native.b2Shape_SetChainSegment(Id, in chainSegment);

		public B2ChainId GetParentChain() => B2Native.b2Shape_GetParentChain(Id);

		public int ContactCapacity => B2Native.b2Shape_GetContactCapacity(Id);

		public B2ContactData[] GetContactData()
		{
			var data = new B2ContactData[ContactCapacity];
			int n = B2Native.b2Shape_GetContactData(Id, data, data.Length);
			if (n != data.Length) Array.Resize(ref data, n);
			return data;
		}

		public int SensorCapacity => B2Native.b2Shape_GetSensorCapacity(Id);

		public B2Shape[] GetSensorData()
		{
			var ids = new B2ShapeId[SensorCapacity];
			int n = B2Native.b2Shape_GetSensorData(Id, ids, ids.Length);
			var shapes = new B2Shape[n];
			for (int i = 0; i < n; i++) shapes[i] = new B2Shape(ids[i]);
			return shapes;
		}

		public B2AABB AABB => B2Native.b2Shape_GetAABB(Id);
		public B2MassData ComputeMassData() => B2Native.b2Shape_ComputeMassData(Id);
		public B2Vec2 GetClosestPoint(B2Vec2 target) => B2Native.b2Shape_GetClosestPoint(Id, target);

		public void ApplyWind(B2Vec2 wind, float drag, float lift, bool wake = true) =>
			B2Native.b2Shape_ApplyWind(Id, wind, drag, lift, wake);

		public bool Equals(B2Shape other) => Id.Equals(other.Id);
		public override bool Equals(object obj) => obj is B2Shape other && Equals(other);
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(B2Shape a, B2Shape b) => a.Equals(b);
		public static bool operator !=(B2Shape a, B2Shape b) => !a.Equals(b);

		public static implicit operator B2ShapeId(B2Shape shape) => shape.Id;
	}
}
