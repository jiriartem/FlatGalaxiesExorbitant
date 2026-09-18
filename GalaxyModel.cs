using System;
using System.Collections.Generic;
using System.Drawing;

namespace GalaxiaplanismoDesorbitante.Models
{
    public enum GalaxyType { Spiral, Elliptical, Ring, Irregular }

    public class Galaxy
    {
        private readonly Random _rng = new();
        public GalaxyType Type { get; set; }
        public PointF Center { get; set; }
        public float Size { get; set; }
        public Color BaseColor { get; set; }
        public int Arms { get; set; } = 3;
        public float RotationDegrees { get; set; } = 0f; // ladeado independiente
        private readonly int _seed;

        public Galaxy(GalaxyType type, PointF center, float size, Color color)
        {
            Type = type;
            Center = center;
            Size = size;
            BaseColor = color;
            RotationDegrees = (float)(_rng.NextDouble() * 360.0);
            _seed = _rng.Next();
        }

        // globalDistance: >1 = cámara más cerca (espirales se abren), <1 = más lejos (se ciñen)
        public void Draw(Graphics g, float globalDistance, float time)
        {
            switch (Type)
            {
                case GalaxyType.Spiral:
                    DrawSpiral(g, globalDistance, time);
                    break;
                case GalaxyType.Elliptical:
                    DrawElliptical(g, globalDistance);
                    break;
                case GalaxyType.Ring:
                    DrawRing(g, globalDistance, time);
                    break;
                case GalaxyType.Irregular:
                    DrawIrregular(g, globalDistance);
                    break;
            }
        }

        private void DrawSpiral(Graphics g, float distance, float time)
        {
            var state = g.Save();
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(RotationDegrees);

            int stars = (int)(Size * 18);
            double turns = 2.5 + (_rng.NextDouble() * 2.0); // vueltas de la espiral
            float growthFactor = 1f + (distance - 1f) * 0.8f; // mapea distancia a tamaño/desenrollado
            float wobble = (float)(Math.Sin(time * 0.6 + _seed) * 0.6);

            for (int i = 0; i < stars; i++)
            {
                double t = i / (double)stars; // 0..1
                // Logarithmic-like spiral: r = a * exp(b * theta) -> aproximamos con potencia para control
                double theta = t * turns * Math.PI * 2 + time * 0.15 * (1 + _rng.NextDouble());
                double baseRadius = t * Size;
                double radialNoise = baseRadius * (0.05 + 0.25 * _rng.NextDouble());
                // Apply growth/zoom: when distance changes, espiral se abre/ciñe.
                float r = (float)((baseRadius + radialNoise) * (growthFactor + wobble * 0.02));

                // Bias towards arms: mayor probabilidad cerca de arm centers
                double armOffset = (theta * Arms) % (Math.PI * 2);
                double armDensity = Math.Pow(Math.Cos(armOffset), 10);
                if (_rng.NextDouble() < 0.25 + 0.65 * armDensity)
                {
                    float x = (float)(Math.Cos(theta) * r);
                    float y = (float)(Math.Sin(theta) * r * (0.6 + (1 - (float)distance) * 0.2)); // flatten visual subtle
                    int s = _rng.Next(1, 4);
                    int alpha = 120 + (int)(120 * (1 - t));
                    using var b = new SolidBrush(Color.FromArgb(Math.Min(255, Math.Max(40, alpha)), BaseColor));
                    g.FillEllipse(b, x - s / 2f, y - s / 2f, s, s);
                }
            }

            // Núcleo
            float nuc = Size * 0.14f * (0.8f + (distance - 1f) * 0.4f);
            using (var b = new SolidBrush(Color.FromArgb(180, 255, 230, 140)))
                g.FillEllipse(b, -nuc, -nuc, nuc * 2, nuc * 2);

            g.Restore(state);
        }

        private void DrawElliptical(Graphics g, float distance)
        {
            var state = g.Save();
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(RotationDegrees);

            float w = Size * (0.9f + (distance - 1f) * 0.2f);
            float h = Size * (0.6f * (0.7f + (1f / (0.8f + distance * 0.2f))));
            using var b = new SolidBrush(Color.FromArgb(40, BaseColor));
            g.FillEllipse(b, -w / 2, -h / 2, w, h);
            using var p = new Pen(Color.FromArgb(120, BaseColor));
            g.DrawEllipse(p, -w / 2, -h / 2, w, h);

            g.Restore(state);
        }

        private void DrawRing(Graphics g, float distance, float time)
        {
            var state = g.Save();
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(RotationDegrees + (float)(time * 8.0 % 360));

            float outer = Size * (0.9f + (distance - 1f) * 0.3f);
            float inner = outer * 0.55f;
            var rnd = _rng;
            for (int i = 0; i < 140; i++)
            {
                double a = i / 140.0 * Math.PI * 2 + time * 0.4;
                double r = inner + (outer - inner) * (0.4 + 0.6 * rnd.NextDouble());
                float x = (float)(r * Math.Cos(a));
                float y = (float)(r * Math.Sin(a) * (0.6f + (1f - distance) * 0.2f));
                int s = rnd.Next(1, 4);
                using var b = new SolidBrush(Color.FromArgb(120 + rnd.Next(80), BaseColor));
                g.FillEllipse(b, x - s / 2f, y - s / 2f, s, s);
            }

            g.Restore(state);
        }

        private void DrawIrregular(Graphics g, float distance)
        {
            var state = g.Save();
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(RotationDegrees);

            int pts = 80;
            for (int i = 0; i < pts; i++)
            {
                double a = i / (double)pts * Math.PI * 2;
                double r = Size * (0.2 + 0.8 * _rng.NextDouble()) * (0.8 + (distance - 1f) * 0.3);
                float x = (float)(r * Math.Cos(a));
                float y = (float)(r * Math.Sin(a) * (0.6f + (1 - distance) * 0.2f));
                int s = _rng.Next(1, 4);
                using var b = new SolidBrush(Color.FromArgb(90 + _rng.Next(160), BaseColor));
                g.FillEllipse(b, x - s / 2f, y - s / 2f, s, s);
            }

            g.Restore(state);
        }
    }

    public static class GalaxyFactory
    {
        private static readonly Random R = new();
        public static Galaxy RandomGalaxy(int w, int h)
        {
            var types = (GalaxyType[])Enum.GetValues(typeof(GalaxyType));
            var type = types[R.Next(types.Length)];
            var center = new PointF(R.Next(60, Math.Max(120, w - 60)), R.Next(60, Math.Max(120, h - 60)));
            float size = R.Next(40, 140);
            Color baseColor = Color.FromArgb(R.Next(140, 255), R.Next(80, 255), R.Next(80, 255), R.Next(255));
            var g = new Galaxy(type, center, size, baseColor);
            g.Arms = R.Next(2, 5);
            g.RotationDegrees = (float)R.NextDouble() * 360f;
            return g;
        }
    }
}