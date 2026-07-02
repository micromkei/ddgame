using System;
using System.Runtime.InteropServices;

namespace Box2D
{
	/// A contact point belonging to a contact manifold. Matches b2ManifoldPoint (collision.h).
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ManifoldPoint
	{
		public B2Vec2 anchorA;
		public B2Vec2 anchorB;
		public float separation;
		public float baseSeparation;
		public float normalImpulse;
		public float tangentImpulse;
		public float totalNormalImpulse;
		public float normalVelocity;
		public ushort id;
		private byte persisted;

		public bool Persisted => persisted != 0;
	}

	/// Contact manifold between two colliding shapes. Matches b2Manifold (collision.h).
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Manifold
	{
		public B2Vec2 normal;
		public float rollingImpulse;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public B2ManifoldPoint[] points;

		public int pointCount;
	}

	/// Result from B2World.CastRayClosest. Matches b2RayResult.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2RayResult
	{
		public B2ShapeId shapeId;
		public B2Vec2 point;
		public B2Vec2 normal;
		public float fraction;
		public int nodeVisits;
		public int leafVisits;
		private byte hit;

		public bool Hit => hit != 0;
	}

	/// Matches b2SensorBeginTouchEvent.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2SensorBeginTouchEvent
	{
		public B2ShapeId sensorShapeId;
		public B2ShapeId visitorShapeId;
	}

	/// Matches b2SensorEndTouchEvent.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2SensorEndTouchEvent
	{
		public B2ShapeId sensorShapeId;
		public B2ShapeId visitorShapeId;
	}

	/// Raw event buffer returned by b2World_GetSensorEvents. Use B2World.GetSensorEvents() for
	/// managed arrays instead of reading the pointers here directly.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2SensorEvents
	{
		public IntPtr beginEvents;
		public IntPtr endEvents;
		public int beginCount;
		public int endCount;
	}

	/// Matches b2ContactBeginTouchEvent.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ContactBeginTouchEvent
	{
		public B2ShapeId shapeIdA;
		public B2ShapeId shapeIdB;
		public B2ContactId contactId;
	}

	/// Matches b2ContactEndTouchEvent.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ContactEndTouchEvent
	{
		public B2ShapeId shapeIdA;
		public B2ShapeId shapeIdB;
		public B2ContactId contactId;
	}

	/// Matches b2ContactHitEvent.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ContactHitEvent
	{
		public B2ShapeId shapeIdA;
		public B2ShapeId shapeIdB;
		public B2ContactId contactId;
		public B2Vec2 point;
		public B2Vec2 normal;
		public float approachSpeed;
	}

	/// Raw event buffer returned by b2World_GetContactEvents. Use B2World.GetContactEvents() for
	/// managed arrays instead of reading the pointers here directly.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ContactEvents
	{
		public IntPtr beginEvents;
		public IntPtr endEvents;
		public IntPtr hitEvents;
		public int beginCount;
		public int endCount;
		public int hitCount;
	}

	/// Matches b2BodyMoveEvent.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2BodyMoveEvent
	{
		public IntPtr userData;
		public B2Transform transform;
		public B2BodyId bodyId;
		private byte fellAsleep;

		public bool FellAsleep => fellAsleep != 0;
	}

	/// Raw event buffer returned by b2World_GetBodyEvents. Use B2World.GetBodyEvents() for a
	/// managed array instead of reading the pointer here directly.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2BodyEvents
	{
		public IntPtr moveEvents;
		public int moveCount;
	}

	/// Matches b2JointEvent.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2JointEvent
	{
		public B2JointId jointId;
		public IntPtr userData;
	}

	/// Raw event buffer returned by b2World_GetJointEvents. Use B2World.GetJointEvents() for a
	/// managed array instead of reading the pointer here directly.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2JointEvents
	{
		public IntPtr jointEvents;
		public int count;
	}

	/// Contact data for two shapes. Matches b2ContactData.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2ContactData
	{
		public B2ContactId contactId;
		public B2ShapeId shapeIdA;
		public B2ShapeId shapeIdB;
		public B2Manifold manifold;
	}

	/// Profiling data, times in milliseconds. Matches b2Profile.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Profile
	{
		public float step;
		public float pairs;
		public float collide;
		public float solve;
		public float solverSetup;
		public float constraints;
		public float prepareConstraints;
		public float integrateVelocities;
		public float warmStart;
		public float solveImpulses;
		public float integratePositions;
		public float relaxImpulses;
		public float applyRestitution;
		public float storeImpulses;
		public float splitIslands;
		public float transforms;
		public float sensorHits;
		public float jointEvents;
		public float hitEvents;
		public float refit;
		public float bullets;
		public float sleepIslands;
		public float sensors;
	}

	/// Counters describing the simulation size. Matches b2Counters.
	[StructLayout(LayoutKind.Sequential)]
	public struct B2Counters
	{
		public long byteCount;
		public int bodyCount;
		public int shapeCount;
		public int contactCount;
		public int jointCount;
		public int islandCount;
		public int stackUsed;
		public int staticTreeHeight;
		public int treeHeight;
		public int taskCount;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public int[] colorCounts;

		public int awakeContactCount;
		public int recycledContactCount;
	}
}
