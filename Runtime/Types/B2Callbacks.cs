using System;
using System.Runtime.InteropServices;

namespace Box2D
{
	/// Called for each shape found by an overlap query. Return false to stop the query early.
	/// Matches b2OverlapResultFcn.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public delegate bool B2OverlapResultFcn(B2ShapeId shapeId, IntPtr context);

	/// Called for each shape hit by a ray or shape cast. Return -1 to ignore and continue,
	/// 0 to terminate, the given fraction to clip, or 1 to continue without clipping.
	/// Matches b2CastResultFcn.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate float B2CastResultFcn(B2ShapeId shapeId, B2Vec2 point, B2Vec2 normal, float fraction, IntPtr context);

	/// Called when a contact pair is considered for collision, if either shape enabled custom
	/// filtering. Must be thread-safe. Return false to disable collision. Matches b2CustomFilterFcn.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public delegate bool B2CustomFilterFcn(B2ShapeId shapeIdA, B2ShapeId shapeIdB, IntPtr context);

	/// Called after a contact is updated, before it reaches the solver, if the shape enabled
	/// pre-solve events. Must be thread-safe. Return false to disable the contact this step.
	/// Matches b2PreSolveFcn.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public delegate bool B2PreSolveFcn(B2ShapeId shapeIdA, B2ShapeId shapeIdB, B2Vec2 point, B2Vec2 normal, IntPtr context);

	/// Optional friction mixing callback, called from worker threads. Matches b2FrictionCallback.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate float B2FrictionCallback(float frictionA, ulong userMaterialIdA, float frictionB, ulong userMaterialIdB);

	/// Optional restitution mixing callback, called from worker threads. Matches b2RestitutionCallback.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate float B2RestitutionCallback(float restitutionA, ulong userMaterialIdA, float restitutionB, ulong userMaterialIdB);
}
