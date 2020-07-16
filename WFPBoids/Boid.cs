using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using SharpGL.SceneGraph;

namespace WFPBoids
{
    public struct BoidParameters
    {
        public float DesiredSeparation;
        public float FriendRadius;
        public float SpeedLimit;
        public float CenterPower;
        public float AlignPower;
        public float CohesionPower;
        public float SeparationPower;
        public float AvoidPower;
    }

    public class Boid
    {

        public Vertex Position;
        public Vertex Velocity;
        public Vertex Acceleration;
        public float Mass;

        public GLColor UnitColor;
        public List<Boid> Friends;

        protected float FORCE_SCALE = 2.0f;
        protected float HUNT_PRIORITY = 1.0f;
        protected static Random r = new Random();

        public Boid(double x, double y, double z)
            : this(new Vertex((float)x, (float)y, (float)z))
        { }
        public Boid(Vertex InitPos)
        {
            Position = InitPos;
            Velocity = RandomVertex(1f);         
            Mass = 3.0f + (float)r.NextDouble() * 3.0f;

            Friends = new List<Boid>();
            UnitColor = new GLColor(
                0.5f + (float)r.NextDouble() / 2.0f,
                0.5f + (float)r.NextDouble() / 2.0f,
                0.5f + (float)r.NextDouble() / 2.0f,
                1f
                );
        }

        public void Run(List<Boid> AllBoids)
        {
            SearchFriends(AllBoids);
            flock();
        }

        void flock()
        {
            SeparationForce();
            AlignmentForce();
            CohesionForce();
            CenteringForce();
            Velocity += Acceleration;
            Position += Velocity;
            Acceleration *= 0f;
            Velocity = Limit(Velocity, Boid._parameters.SpeedLimit);

        }

        void SearchFriends(List<Boid> AllBoids)
        {
            Friends.Clear();
            Friends = GetNearBy(AllBoids,Boid._parameters.FriendRadius);
        }
       
        #region Rules
        public void Border(float left, float right, float bottom, float top)
        {
            if (this.Position.X < left) this.Position.X = right;
            if (this.Position.X > right) this.Position.X = left;

            if (this.Position.Y < bottom) this.Position.Y = top;
            if (this.Position.Y > top) this.Position.Y = bottom;
        }
        void CohesionForce()
        {
            if (Friends.Count == 0)
                return;

            Vertex coh = new Vertex(0, 0, 0);
            Vertex MeanPos = Friends
                        .Select(x => x.Position)
                        .Aggregate((cur, next) => cur + next) / Friends.Count;

            coh = MeanPos - this.Position;
            coh = Limit(coh, FORCE_SCALE * HUNT_PRIORITY);
            ApplyForce(coh * Boid._parameters.CohesionPower);
        }
        void AlignmentForce()
        {
            if (Friends.Count == 0)
                return;

            Vertex MeanVelocity = Friends
              .Select(x => x.Velocity)
              .Aggregate((cur, next) => cur + next) / Friends.Count;

            MeanVelocity = Limit(MeanVelocity, FORCE_SCALE);
            ApplyForce(MeanVelocity * Boid._parameters.AlignPower);
        }

        void SeparationForce()
        {
            if (Friends.Count == 0)
                return;

            List<Boid> ToClose = GetNearBy(Friends, Boid._parameters.DesiredSeparation);

            if (ToClose.Count == 0)
                return;
            Vertex MeanPos = ToClose
                        .Select(x => x.Position)
                        .Aggregate((cur, next) => cur + next) / ToClose.Count;

            Vertex sep = Limit(this.Position - MeanPos, FORCE_SCALE);
            ApplyForce(sep * Boid._parameters.SeparationPower);
        }

        void CenteringForce()
        {
            Vertex tend = this.Position / -10.0f;
            tend = Limit(tend, FORCE_SCALE);
            ApplyForce(tend * Boid._parameters.CenterPower);
        }

        public void Avoid(List<Boid> AvoidBoids, float range = 60)
        {
            Vertex repV;
            List<Boid> Danger = GetNearBy(AvoidBoids, range);
            foreach (Boid Test in Danger)
            {
                repV = this.Position - Test.Position;
                repV.Normalize();
                ApplyForce(repV * FORCE_SCALE * Boid._parameters.AvoidPower);
            }
        }
        #endregion

        #region Misc
        List<Boid> GetNearBy(List<Boid> AllBoids, float Radius = 40)
        {
            List<Boid> NewFriends = new List<Boid>();
            foreach (Boid Test in AllBoids)
            {
                if (Test == this)
                    continue;

                if ((Test.Position - this.Position).Magnitude() < Radius)
                    NewFriends.Add(Test);
            }

            return NewFriends;
        }
        void ApplyForce(Vertex Force)
        {
            this.Acceleration += Force / Mass;
        }
        Vertex Limit(Vertex V, float Limitation)
        {
            if (V.Magnitude() > Limitation)
            {
                V.Normalize();
                V *= Limitation;
            }
            return V;
        }

        Vertex RandomVertex(float Magnitude)
        {
            Vertex RV = new Vertex((float)r.NextDouble(), (float)r.NextDouble(), 0);
            RV.Normalize();
            return RV * Magnitude;
        }
        #endregion

        static BoidParameters _parameters;
        public static void SetOptions(BoidParameters Params)
        {
            _parameters = Params;
        }
    }
}
