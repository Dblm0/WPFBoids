using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.SceneGraph;
namespace WFPBoids
{
    public class Predator : Boid
    {
        public Predator(double x, double y, double z)
            : this(new Vertex((float)x, (float)y, (float)z))
        { }
        public Predator(Vertex InitPos)
            : base(InitPos)
        {
            Velocity *= 2.5f;
            Mass = Mass = 3.0f + (float)r.NextDouble() * 3.0f;
            FORCE_SCALE = 4;
            HUNT_PRIORITY = 3;
            UnitColor = new GLColor(1, 0, 0, 1);
        }
    }
}
