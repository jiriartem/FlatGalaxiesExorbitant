using System.Drawing;

namespace SolarSystemSim.Models
{
    public class CelestialBody
    {
        public string Name { get; set; } = "";
        public float OrbitRadius { get; set; }
        public float OrbitSpeed { get; set; } // radians per tick
        public float Angle { get; set; } = 0f;
        public Color Color { get; set; } = Color.White;
        public float Size { get; set; } = 6f;

        public void Update() => Angle += OrbitSpeed;

        public PointF GetPosition(PointF center, float flatten)
        {
            float x = center.X + (float)(OrbitRadius * System.Math.Cos(Angle));
            float y = center.Y + (float)(OrbitRadius * System.Math.Sin(Angle) * flatten);
            return new PointF(x, y);
        }
    }
}