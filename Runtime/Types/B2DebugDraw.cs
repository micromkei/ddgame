using System;
using System.Runtime.InteropServices;

namespace Box2D
{
	/// Subclass and override the Draw* methods you care about, then pass an instance to
	/// B2World.Draw(). Matches the callback set in b2DebugDraw, but as ordinary virtual C#
	/// methods instead of raw function pointers - the interop plumbing lives in
	/// Box2D.Interop.B2DebugDrawBridge.
	public class B2DebugDraw
	{
		public B2AABB DrawingBounds;
		public float ForceScale;
		public float JointScale;
		public bool DrawContacts;
		public bool DrawAnchorA;
		public bool DrawShapes = true;
		public bool DrawChainNormals;
		public bool DrawJoints = true;
		public bool DrawJointExtras;
		public bool DrawShapeBounds;
		public bool DrawMass;
		public bool DrawBodyNames;
		public bool DrawGraphColors;
		public bool DrawContactFeatures;
		public bool DrawContactNormals;
		public bool DrawContactForces;
		public bool DrawFrictionForces;
		public bool DrawIslands;

		public virtual void DrawPolygon(B2Transform transform, B2Vec2[] vertices, B2HexColor color)
		{
		}

		public virtual void DrawSolidPolygon(B2Transform transform, B2Vec2[] vertices, float radius, B2HexColor color)
		{
		}

		public virtual void DrawCircle(B2Vec2 center, float radius, B2HexColor color)
		{
		}

		public virtual void DrawSolidCircle(B2Transform transform, B2Vec2 center, float radius, B2HexColor color)
		{
		}

		public virtual void DrawSolidCapsule(B2Vec2 p1, B2Vec2 p2, float radius, B2HexColor color)
		{
		}

		public virtual void DrawLine(B2Vec2 p1, B2Vec2 p2, B2HexColor color)
		{
		}

		public virtual void DrawTransform(B2Transform transform)
		{
		}

		public virtual void DrawPoint(B2Vec2 p, float size, B2HexColor color)
		{
		}

		public virtual void DrawString(B2Vec2 p, string s, B2HexColor color)
		{
		}

		public virtual void DrawBounds(B2AABB aabb, B2HexColor color)
		{
		}
	}
}

namespace Box2D.Interop
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawPolygonNative(B2Transform transform, IntPtr vertices, int vertexCount, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawSolidPolygonNative(B2Transform transform, IntPtr vertices, int vertexCount, float radius, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawCircleNative(B2Vec2 center, float radius, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawSolidCircleNative(B2Transform transform, B2Vec2 center, float radius, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawSolidCapsuleNative(B2Vec2 p1, B2Vec2 p2, float radius, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawLineNative(B2Vec2 p1, B2Vec2 p2, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawTransformNative(B2Transform transform, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawPointNative(B2Vec2 p, float size, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawStringNative(B2Vec2 p, IntPtr s, B2HexColor color, IntPtr context);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DrawBoundsNative(B2AABB aabb, B2HexColor color, IntPtr context);

	/// Raw layout matching b2DebugDraw exactly: 10 function pointers followed by settings.
	/// Every bool is a byte, per the project-wide interop convention.
	[StructLayout(LayoutKind.Sequential)]
	internal struct B2NativeDebugDraw
	{
		public IntPtr drawPolygon;
		public IntPtr drawSolidPolygon;
		public IntPtr drawCircle;
		public IntPtr drawSolidCircle;
		public IntPtr drawSolidCapsule;
		public IntPtr drawLine;
		public IntPtr drawTransform;
		public IntPtr drawPoint;
		public IntPtr drawString;
		public IntPtr drawBounds;

		public B2AABB drawingBounds;
		public float forceScale;
		public float jointScale;
		public byte drawContacts;
		public byte drawAnchorA;
		public byte drawShapes;
		public byte drawChainNormals;
		public byte drawJoints;
		public byte drawJointExtras;
		public byte drawBoundsFlag;
		public byte drawMass;
		public byte drawBodyNames;
		public byte drawGraphColors;
		public byte drawContactFeatures;
		public byte drawContactNormals;
		public byte drawContactForces;
		public byte drawFrictionForces;
		public byte drawIslands;
		public IntPtr context;
	}

	/// Bridges a managed B2DebugDraw to the native b2DebugDraw callback table for the duration of
	/// a single B2World.Draw() call. Keeps delegate instances alive as fields so the GC cannot
	/// collect them while native code holds their function pointers; the bridge itself must be
	/// kept alive (e.g. via GC.KeepAlive) until b2World_Draw returns.
	internal sealed class B2DebugDrawBridge
	{
		private readonly B2DebugDraw _target;

		private readonly DrawPolygonNative _drawPolygon;
		private readonly DrawSolidPolygonNative _drawSolidPolygon;
		private readonly DrawCircleNative _drawCircle;
		private readonly DrawSolidCircleNative _drawSolidCircle;
		private readonly DrawSolidCapsuleNative _drawSolidCapsule;
		private readonly DrawLineNative _drawLine;
		private readonly DrawTransformNative _drawTransform;
		private readonly DrawPointNative _drawPoint;
		private readonly DrawStringNative _drawString;
		private readonly DrawBoundsNative _drawBounds;

		public B2DebugDrawBridge(B2DebugDraw target)
		{
			_target = target;

			_drawPolygon = (transform, vertices, count, color, _) =>
				_target.DrawPolygon(transform, ReadVec2Array(vertices, count), color);
			_drawSolidPolygon = (transform, vertices, count, radius, color, _) =>
				_target.DrawSolidPolygon(transform, ReadVec2Array(vertices, count), radius, color);
			_drawCircle = (center, radius, color, _) => _target.DrawCircle(center, radius, color);
			_drawSolidCircle = (transform, center, radius, color, _) => _target.DrawSolidCircle(transform, center, radius, color);
			_drawSolidCapsule = (p1, p2, radius, color, _) => _target.DrawSolidCapsule(p1, p2, radius, color);
			_drawLine = (p1, p2, color, _) => _target.DrawLine(p1, p2, color);
			_drawTransform = (transform, _) => _target.DrawTransform(transform);
			_drawPoint = (p, size, color, _) => _target.DrawPoint(p, size, color);
			_drawString = (p, s, color, _) => _target.DrawString(p, Marshal.PtrToStringAnsi(s), color);
			_drawBounds = (aabb, color, _) => _target.DrawBounds(aabb, color);
		}

		public B2NativeDebugDraw BuildNative()
		{
			return new B2NativeDebugDraw
			{
				drawPolygon = Marshal.GetFunctionPointerForDelegate(_drawPolygon),
				drawSolidPolygon = Marshal.GetFunctionPointerForDelegate(_drawSolidPolygon),
				drawCircle = Marshal.GetFunctionPointerForDelegate(_drawCircle),
				drawSolidCircle = Marshal.GetFunctionPointerForDelegate(_drawSolidCircle),
				drawSolidCapsule = Marshal.GetFunctionPointerForDelegate(_drawSolidCapsule),
				drawLine = Marshal.GetFunctionPointerForDelegate(_drawLine),
				drawTransform = Marshal.GetFunctionPointerForDelegate(_drawTransform),
				drawPoint = Marshal.GetFunctionPointerForDelegate(_drawPoint),
				drawString = Marshal.GetFunctionPointerForDelegate(_drawString),
				drawBounds = Marshal.GetFunctionPointerForDelegate(_drawBounds),
				drawingBounds = _target.DrawingBounds,
				forceScale = _target.ForceScale,
				jointScale = _target.JointScale,
				drawContacts = B2Bool.From(_target.DrawContacts),
				drawAnchorA = B2Bool.From(_target.DrawAnchorA),
				drawShapes = B2Bool.From(_target.DrawShapes),
				drawChainNormals = B2Bool.From(_target.DrawChainNormals),
				drawJoints = B2Bool.From(_target.DrawJoints),
				drawJointExtras = B2Bool.From(_target.DrawJointExtras),
				drawBoundsFlag = B2Bool.From(_target.DrawShapeBounds),
				drawMass = B2Bool.From(_target.DrawMass),
				drawBodyNames = B2Bool.From(_target.DrawBodyNames),
				drawGraphColors = B2Bool.From(_target.DrawGraphColors),
				drawContactFeatures = B2Bool.From(_target.DrawContactFeatures),
				drawContactNormals = B2Bool.From(_target.DrawContactNormals),
				drawContactForces = B2Bool.From(_target.DrawContactForces),
				drawFrictionForces = B2Bool.From(_target.DrawFrictionForces),
				drawIslands = B2Bool.From(_target.DrawIslands),
				context = IntPtr.Zero,
			};
		}

		private static B2Vec2[] ReadVec2Array(IntPtr ptr, int count)
		{
			var result = new B2Vec2[count];
			int stride = Marshal.SizeOf<B2Vec2>();
			for (int i = 0; i < count; i++)
			{
				result[i] = Marshal.PtrToStructure<B2Vec2>(IntPtr.Add(ptr, i * stride));
			}

			return result;
		}
	}
}
