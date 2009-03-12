using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using System.Collections;
using cogbot.TheOpenSims.Navigation.Debug;
using System.Drawing;

namespace cogbot.TheOpenSims.Navigation
{
    /// <summary>
    /// Basically a node is defined with a geographical position in space.
    /// It is also characterized with both collections of outgoing arcs and incoming arcs.
    /// </summary>
    [Serializable]
    public class SimWaypoint : SimPosition
    {
        public byte GetMatrix()
        {
            return PathStore.mMatrix[PX, PY];
        }

        public void SetMatrix(int v)
        {
            if (v < 0) v = 0; else if (v > 255) v = 255;
            PathStore.mMatrix[PX, PY] = (byte)v;
        }

        public void UpdateMatrix()
        {
            bool IsBlocked = false;
            bool WasBlocked = GetMatrix() == 0;
            foreach (SimObject O in OccupiedList)
            {
                if (!O.IsPassable) IsBlocked = true;
            }
            if (WasBlocked && !IsBlocked)
            {
                SetMatrix(2 + OccupiedList.Count * 2);
                return;
            }
            else if (!WasBlocked && IsBlocked) SetMatrix(0);
        }

        internal int AddShadow(SimObject blocker)
        {
            IList ShadowList = OccupiedList;
            if (!ShadowList.Contains(blocker))
            {
                ShadowList.Add(blocker);
            }
            return ShadowList.Count;
        }

        internal bool AddOccupied(SimObject simObject)
        {
            if (OccupiedList.Contains(simObject)) return false;
            OccupiedList.Add(simObject);
            return true;
        }

        // private IList ShadowList = new List<SimObject>();
        internal IList OccupiedList = new List<SimObject>();

        public bool NoObjectsBlock()
        {
            foreach (SimObject O in OccupiedList)
            {
                if (!O.IsPassable)
                {
                    return false;
                }
            }
            return true;
        }

        public string OccupiedString()
        {
            string S = "";
            foreach (SimObject O in OccupiedList)
            {
                S += O.ToString();
                S += "\n";
            }
            return S.TrimEnd();
        }


        public SimWaypoint GetWaypoint()
        {
            return this;
        }


        public bool IsRegionAttached()
        {
            return PathStore != null;
        }

        Vector3d _Position;
        bool _Passable = true;
        ArrayList _IncomingArcs, _OutgoingArcs;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="PositionX">X coordinate.</param>
        /// <param name="PositionY">Y coordinate.</param>
        /// <param name="PositionZ">Z coordinate.</param>
        static public SimWaypoint CreateLocal(float PositionX, float PositionY, float PositionZ, SimPathStore PathStore)
        {
            return CreateLocal(new Vector3(PositionX, PositionY, PositionZ), PathStore);
        }

        public int ArcCount()
        {
            if (_OutgoingArcs != null)
                return _OutgoingArcs.Count;
            return 0;
        }
        /// <summary>
        /// Gets the list of the arcs that lead to this node.
        /// </summary>
        public IList IncomingArcs
        {
            get
            {
                if (_IncomingArcs == null) _IncomingArcs = new ArrayList();
                return _IncomingArcs;
            }
        }

        /// <summary>
        /// Gets the list of the arcs that start from this node.
        /// </summary>
        public IList OutgoingArcs
        {
            get
            {
                if (_OutgoingArcs == null) _OutgoingArcs = new ArrayList();
                return _OutgoingArcs;
            }
        }

        /// Gets/Sets the functional state of the node.
        /// 'true' means that the node is in its normal state.
        /// 'false' means that the node will not be taken into account (as if it did not exist).
        public bool Passable
        {
            set
            {
                if (_IncomingArcs != null) foreach (SimRoute A in _IncomingArcs) A.Passable = value;
                if (_OutgoingArcs != null) foreach (SimRoute A in _OutgoingArcs) A.Passable = value;
                _Passable = value;
                SetMatrix((byte)(value ? 2 : 0));
            }
            get
            {
                if (!_Passable) return false;
                if (_IncomingArcs != null) foreach (SimRoute A in _IncomingArcs) if (A.Passable) return true;
                if (_OutgoingArcs != null) foreach (SimRoute A in _OutgoingArcs) if (A.Passable) return true;
                return GetMatrix() != 0;
            }
        }

        /// <summary>
        /// Gets Point.X coordinate on the PathStore.
        /// </summary>
        readonly public int PX;// { get { return (int)Math.Round(Position.X * PathStore.POINTS_PER_METER); } }

        /// <summary>
        /// Gets Point.Y coordinate on the PathStore.
        /// </summary>
        readonly public int PY;// { get { return (int)Math.Round(Position.Y * PathStore.POINTS_PER_METER); } }


        /// <summary>
        /// Gets X coordinate on the Graph debugger (whole world position).
        /// </summary>
        public double DX { get { return (Position.X / GraphFormer.DSCALE) - GraphFormer.StartX; } }

        /// <summary>
        /// Gets Y coordinate on the Graph debugger
        /// </summary>
        public double DY { get { return (Position.Y / GraphFormer.DSCALE) - GraphFormer.StartY; } }

        /// <summary>
        /// Gets Z coordinate.
        /// </summary>
        ///public double DZ { get { return Position.Z; } }

        /// <summary>
        /// Modifies X, Y and Z coordinates
        /// </summary>
        /// <param name="PositionX">X coordinate.</param>
        /// <param name="PositionY">Y coordinate.</param>
        /// <param name="PositionZ">Z coordinate.</param>
        public void ChangeXYZDebug(double PositionX, double PositionY, double PositionZ)
        {
            Vector3d v3d = new Vector3d(PositionX, PositionY, PositionZ);
            PathStore = SimRegion.GetRegion(v3d).PathStore;
            _Position = v3d;// SimRegion.GlobaltoLocalPosition(v3d);
        }

        /// <summary>
        /// Gets/Sets the geographical position of the node.
        /// </summary>
        /// <exception cref="ArgumentNullException">Cannot set the Position to null.</exception>
        public Vector3d Position
        {
            set
            {
                //if (value == null) throw new ArgumentNullException();
                if (_IncomingArcs != null) foreach (SimRoute A in _IncomingArcs) A.LengthUpdated = false;
                if (_OutgoingArcs != null) foreach (SimRoute A in _OutgoingArcs) A.LengthUpdated = false;
                ChangeXYZDebug(value.X, value.Y, value.Z);
            }
            get { return _Position; }
        }

        /// <summary>
        /// Gets the array of nodes that can be directly reached from this one.
        /// </summary>
        public SimWaypoint[] AccessibleNodes
        {
            get
            {
                SimWaypoint[] Tableau = new SimWaypoint[OutgoingArcs.Count];
                int i = 0;
                foreach (SimRoute A in OutgoingArcs) Tableau[i++] = A.EndNode;
                return Tableau;
            }
        }

        /// <summary>
        /// Gets the array of nodes that can directly reach this one.
        /// </summary>
        public SimWaypoint[] AccessingNodes
        {
            get
            {
                SimWaypoint[] Tableau = new SimWaypoint[IncomingArcs.Count];
                int i = 0;
                foreach (SimRoute A in IncomingArcs) Tableau[i++] = A.StartNode;
                return Tableau;
            }
        }

        /// <summary>
        /// Gets the array of nodes directly linked plus this one.
        /// </summary>
        public SimWaypoint[] Molecule
        {
            get
            {
                int NbNodes = 1 + OutgoingArcs.Count + IncomingArcs.Count;
                SimWaypoint[] Tableau = new SimWaypoint[NbNodes];
                Tableau[0] = this;
                int i = 1;
                foreach (SimRoute A in OutgoingArcs) Tableau[i++] = A.EndNode;
                foreach (SimRoute A in IncomingArcs) Tableau[i++] = A.StartNode;
                return Tableau;
            }
        }

        /// <summary>
        /// Unlink this node from all current connected arcs.
        /// </summary>
        public void Isolate()
        {
            UntieIncomingArcs();
            UntieOutgoingArcs();
        }

        /// <summary>
        /// Unlink this node from all current incoming arcs.
        /// </summary>
        public void UntieIncomingArcs()
        {
            if (_IncomingArcs != null)
            {
                foreach (SimRoute A in _IncomingArcs)
                    A.StartNode.OutgoingArcs.Remove(A);
                _IncomingArcs.Clear();
            }
        }

        /// <summary>
        /// Unlink this node from all current outgoing arcs.
        /// </summary>
        public void UntieOutgoingArcs()
        {
            if (_OutgoingArcs != null)
            {
                foreach (SimRoute A in _OutgoingArcs)
                    A.EndNode.IncomingArcs.Remove(A);
                _OutgoingArcs.Clear();
            }
        }

        /// <summary>
        /// Returns the arc that leads to the specified node if it exists.
        /// </summary>
        /// <exception cref="ArgumentNullException">Argument node must not be null.</exception>
        /// <param name="N">A node that could be reached from this one.</param>
        /// <returns>The arc leading to N from this / null if there is no solution.</returns>
        public SimRoute ArcGoingTo(SimWaypoint N)
        {
            if (N == null) throw new ArgumentNullException();
            if (_OutgoingArcs != null) foreach (SimRoute A in _OutgoingArcs)
                    if (A.EndNode == N) return A;
            return null;
        }

        /// <summary>
        /// Returns the arc that arc that comes to this from the specified node if it exists.
        /// </summary>
        /// <exception cref="ArgumentNullException">Argument node must not be null.</exception>
        /// <param name="N">A node that could reach this one.</param>
        /// <returns>The arc coming to this from N / null if there is no solution.</returns>
        public SimRoute ArcComingFrom(SimWaypoint N)
        {
            if (N == null) throw new ArgumentNullException();
            if (_IncomingArcs != null) foreach (SimRoute A in _IncomingArcs)
                    if (A.StartNode == N) return A;
            return null;
        }

        void Invalidate()
        {
            if (_IncomingArcs != null) foreach (SimRoute A in _IncomingArcs) A.LengthUpdated = false;
            if (_OutgoingArcs != null) foreach (SimRoute A in _OutgoingArcs) A.LengthUpdated = false;
        }

        /// <summary>
        /// object.ToString() override.
        /// Returns the textual description of the node.
        /// </summary>
        /// <returns>String describing this node.</returns>
        public override string ToString() { return "(" + Position.ToRawString() + ")"; }

        /// <summary>
        /// Object.Equals override.
        /// Tells if two nodes are equal by comparing positions.
        /// </summary>
        /// <exception cref="ArgumentException">A Node cannot be compared with another type.</exception>
        /// <param name="O">The node to compare with.</param>
        /// <returns>'true' if both nodes are equal.</returns>
        public override bool Equals(object O)
        {
            if (O is SimPosition)
            {
                return Position == ((SimPosition)O).GetWorldPosition();
            }
            //if (O is Vector3d)
            //{
            //    return Create((Vector3d)O).Equals(this);
            //}

            throw new ArgumentException("Type " + O.GetType() + " cannot be compared with type " + GetType() + " !");
        }


        /// <summary>
        /// Returns a copy of this node.
        /// </summary>
        /// <returns>The reference of the new object.</returns>
        public object Clone()
        {
            SimWaypoint N = new SimWaypoint(_Position, PathStore);
            N._Passable = _Passable;
            return N;
        }

        /// <summary>
        /// Object.GetHashCode override.
        /// </summary>
        /// <returns>HashCode value.</returns>
        public override int GetHashCode() { return Position.GetHashCode(); }

        /// <summary>
        /// Returns the euclidian distance between two nodes : Sqrt(Dx�+Dy�+Dz�)
        /// </summary>
        /// <param name="N1">First node.</param>
        /// <param name="N2">Second node.</param>
        /// <returns>Distance value.</returns>
        public static double EuclidianDistance(SimWaypoint N1, SimWaypoint N2)
        {
            return (double)Math.Sqrt(SquareEuclidianDistance(N1, N2));
        }

        /// <summary>
        /// Returns the square euclidian distance between two nodes : Dx�+Dy�+Dz�
        /// </summary>
        /// <exception cref="ArgumentNullException">Argument nodes must not be null.</exception>
        /// <param name="N1">First node.</param>
        /// <param name="N2">Second node.</param>
        /// <returns>Distance value.</returns>
        public static double SquareEuclidianDistance(SimWaypoint N1, SimWaypoint N2)
        {
            if (N1 == null || N2 == null) throw new ArgumentNullException();
            double DX = N1.Position.X - N2.Position.X;
            double DY = N1.Position.Y - N2.Position.Y;
            double DZ = N1.Position.Z - N2.Position.Z;
            return (double)(DX * DX + DY * DY + DZ * DZ);
        }

        /// <summary>
        /// Returns the manhattan distance between two nodes : |Dx|+|Dy|+|Dz|
        /// </summary>
        /// <exception cref="ArgumentNullException">Argument nodes must not be null.</exception>
        /// <param name="N1">First node.</param>
        /// <param name="N2">Second node.</param>
        /// <returns>Distance value.</returns>
        public static double ManhattanDistance(SimWaypoint N1, SimWaypoint N2)
        {
            if (N1 == null || N2 == null) throw new ArgumentNullException();
            double DX = N1.Position.X - N2.Position.X;
            double DY = N1.Position.Y - N2.Position.Y;
            double DZ = N1.Position.Z - N2.Position.Z;
            return (double)(Math.Abs(DX) + Math.Abs(DY) + Math.Abs(DZ));
        }

        /// <summary>
        /// Returns the maximum distance between two nodes : Max(|Dx|, |Dy|, |Dz|)
        /// </summary>
        /// <exception cref="ArgumentNullException">Argument nodes must not be null.</exception>
        /// <param name="N1">First node.</param>
        /// <param name="N2">Second node.</param>
        /// <returns>Distance value.</returns>
        public static double MaxDistanceAlongAxis(SimWaypoint N1, SimWaypoint N2)
        {
            if (N1 == null || N2 == null) throw new ArgumentNullException();
            double DX = Math.Abs(N1.Position.X - N2.Position.X);
            double DY = Math.Abs(N1.Position.Y - N2.Position.Y);
            double DZ = Math.Abs(N1.Position.Z - N2.Position.Z);
            return (double)Math.Max(DX, Math.Max(DY, DZ));
        }

        /// <summary>
        /// Returns the bounding box that wraps the specified list of nodes.
        /// </summary>
        /// <exception cref="ArgumentException">The list must only contain elements of type Node.</exception>
        /// <exception cref="ArgumentException">The list of nodes is empty.</exception>
        /// <param name="NodesGroup">The list of nodes to wrap.</param>
        /// <param name="MinPoint">The point of minimal coordinates for the box.</param>
        /// <param name="MaxPoint">The point of maximal coordinates for the box.</param>
        static public void BoundingBox(ICollection<SimWaypoint> NodesGroup, out double[] MinPoint, out double[] MaxPoint)
        {
            IEnumerator E = NodesGroup.GetEnumerator();
            E.MoveNext();
            SimWaypoint N1 = E.Current as SimWaypoint;
            if (N1 == null) throw new ArgumentException("The list must only contain elements of type Node.");
            if (NodesGroup.Count == 0) throw new ArgumentException("The list of nodes is empty.");
            int Dim = 3;
            MinPoint = new double[Dim];
            MaxPoint = new double[Dim];
            for (int i = 0; i < Dim; i++) MinPoint[i] = MaxPoint[i] = N1.GlobalXYZ(i);
            foreach (SimWaypoint N in NodesGroup)
            {
                for (int i = 0; i < Dim; i++)
                {
                    if (MinPoint[i] > N.GlobalXYZ(i)) MinPoint[i] = N.GlobalXYZ(i);
                    if (MaxPoint[i] < N.GlobalXYZ(i)) MaxPoint[i] = N.GlobalXYZ(i);
                }
            }
        }

        private double GlobalXYZ(int i)
        {
            switch (i)
            {
                case 0:
                    return Position.X;
                case 1:
                    return Position.Y;
                case 2:
                    return Position.Z;
                default:
                    throw new ArgumentException("Waypoints dont have arg " + i);
            }
        }

        //public static implicit operator Vector3d(SimWaypoint m)
        //{
        //    return m.GetSimPosition();
        //}
        //public static implicit operator Vector2(SimWaypoint m)
        //{
        //    Vector3d v3 = m.GetSimPosition();
        //    return new Vector2(v3.X, v3.Y);
        //}
        //public static implicit operator Vector3d(SimWaypoint m)
        //{
        //    Vector3d v3 = m.GetSimPosition();
        //    return new Vector3d(v3.X, v3.Y, v3.Z);
        //}

        SimPathStore PathStore;
        private SimWaypoint(Vector3d firstP, SimPathStore pathStore)
        {
            PathStore = pathStore;
            _Position = firstP;// RoundPoint(firstP, PathStore);
            Vector3 LocalPos = GetSimPosition();
            PX = (int)Math.Round(LocalPos.X * PathStore.POINTS_PER_METER);
            PY = (int)Math.Round(LocalPos.Y * PathStore.POINTS_PER_METER);
        }

        //protected Vector3d _Position;

        public static Vector3 RoundPoint(Vector3 point, SimPathStore PathStore)
        {
            double POINTS_PER_METER = PathStore.POINTS_PER_METER;
            Vector3 vect3 = new Vector3(point);
            vect3.X = (float)(Math.Round(vect3.X * POINTS_PER_METER, 0) / POINTS_PER_METER);
            vect3.Y = (float)(Math.Round(vect3.Y * POINTS_PER_METER, 0) / POINTS_PER_METER);
            vect3.Z = (float)(Math.Round(vect3.Z * POINTS_PER_METER, 0) / POINTS_PER_METER);
            return vect3;
        }

        static Dictionary<SimPathStore, Dictionary<Vector3, SimWaypoint>> InternedPointsStore = new Dictionary<SimPathStore, Dictionary<Vector3, SimWaypoint>>();
        public static SimWaypoint CreateLocal(Vector3 from, SimPathStore PathStore)
        {
            if (!InternedPointsStore.ContainsKey(PathStore))
            {
                InternedPointsStore[PathStore] = new Dictionary<Vector3, SimWaypoint>();
            }
            Dictionary<Vector3, SimWaypoint> InternedPoints = InternedPointsStore[PathStore];

            Vector3 rounded = RoundPoint(from, PathStore);
            if (InternedPoints.ContainsKey(rounded))
            {
                return InternedPoints[rounded];
            }
            Vector3d GlobalPos = PathStore.GetSimRegion().LocalToGlobal(rounded);
            SimWaypoint wp = new SimWaypoint(GlobalPos, PathStore);
            wp._Passable = true;
            // wp._IncomingArcs = new ArrayList();
            // wp._OutgoingArcs = new ArrayList();
            InternedPoints[rounded] = wp;
            //  PathStore.EnsureKnown(wp);
            return wp;
        }

        public Vector3 GetSimPosition()
        {
            return SimRegion.GlobalToLocal(_Position); ;
        }

        #region SimPosition Members

        //public Vector3 GetUsePosition()
        //{
        //    return GetSimPosition();
        //}

        public float GetSizeDistance()
        {
            return 0.7f;
        }

        #endregion

        public static double Distance(SimWaypoint wp1, SimWaypoint wp2)
        {
            return Vector3d.Distance(wp1.Position, wp2.Position);
        }

        // <summary>

        // </summary>
        // <returns>true if something needed to be changed</returns>
        public bool EnsureAtLeastOnePath()
        {
            bool needIt = true;
            if (_IncomingArcs != null)
                foreach (SimRoute A in _IncomingArcs)
                {
                    if (A.Passable)
                    {
                        needIt = false;
                    }
                }
            if (_OutgoingArcs != null)
                foreach (SimRoute A in _OutgoingArcs)
                {
                    if (A.Passable)
                    {
                        needIt = false;
                    }
                }
            if (needIt)
            {
                Passable = true;
            }
            return needIt;
        }

        public bool GoesTo(SimWaypoint e)
        {

            if (_OutgoingArcs != null)
                foreach (SimRoute r in _OutgoingArcs)
                {
                    if (r.EndNode == e) return true;
                }
            return false;
        }

        public double Distance(Vector3d P)
        {
            return Vector3d.Distance(P, Position);
        }


        #region SimPosition Members


        public Quaternion GetSimRotation()
        {
            return Quaternion.Identity;
        }

        #endregion

        #region SimPosition Members


        public Vector3d GetWorldPosition()
        {
            return Position;
        }

        #endregion

        #region SimPosition Members


        public SimRegion GetSimRegion()
        {
            return PathStore.GetSimRegion();
        }

        #endregion

        internal static SimWaypoint CreateGlobal(double gx, double gy, double gz)
        {
            return CreateGlobal(new Vector3d(gx, gy, gz));
        }

        internal static SimWaypoint CreateGlobal(Vector3d v3d)
        {
            SimRegion R = SimRegion.GetRegion(v3d);
            return CreateLocal(SimRegion.GlobalToLocal(v3d), R.PathStore);
        }
    }

    //public class SimMovementPoints : SimMovement
    //{
    //    List<Vector3d> ThePoints;// = new List<Vector3d>();
    //    public SimMovementPoints(List<Vector3d> points)
    //        : base(points[0], points[points.Count - 1])
    //    {
    //        ThePoints = points;
    //    }

    //    public override List<Vector3d> GetPoints()
    //    {
    //        return ThePoints;
    //    }
    //}
}
