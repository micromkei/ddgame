namespace Box2D
{
	/// The body simulation type. Matches b2BodyType.
	public enum B2BodyType
	{
		Static = 0,
		Kinematic = 1,
		Dynamic = 2,
	}

	/// Matches b2ShapeType.
	public enum B2ShapeType
	{
		Circle = 0,
		Capsule = 1,
		Segment = 2,
		Polygon = 3,
		ChainSegment = 4,
	}

	/// Matches b2JointType.
	public enum B2JointType
	{
		Distance = 0,
		Filter = 1,
		Motor = 2,
		Prismatic = 3,
		Revolute = 4,
		Weld = 5,
		Wheel = 6,
	}
}
