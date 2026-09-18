using System;
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
        public float RotationDegrees { get; set; } = 0f;
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

        // Firma extendida: ahora recibe inclinación del sistema y flatten para proyectar
        public void Draw(Graphics g, float globalDistance, float time, PointF systemPosNormalized, Size canvasSize, float systemTiltDeg, float systemFlatten)
        {
            switch (Type)
            {
                case GalaxyType.Spiral:
                    DrawSpiral(g, globalDistance, time, systemPosNormalized, canvasSize, systemTiltDeg, systemFlatten);
                    break;
                case GalaxyType.Elliptical:
                    DrawElliptical(g, globalDistance, systemTiltDeg, systemFlatten);
                    break;
                case GalaxyType.Ring:
                    DrawRing(g, globalDistance, time, systemPosNormalized, canvasSize, systemTiltDeg, systemFlatten);
                    break;
                case GalaxyType.Irregular:
                    DrawIrregular(g, globalDistance, systemPosNormalized, canvasSize, systemTiltDeg, systemFlatten);
                    break;
            }
        }

        // Compatibilidad: sobrecarga simplificada (mantener si hay llamadas antiguas)
        public void Draw(Graphics g, float globalDistance, float time)
        {
            var vb = g.VisibleClipBounds;
            var canvas = new Size(Math.Max(1, (int)vb.Width), Math.Max(1, (int)vb.Height));
            var sysPos = new PointF(0.5f, 0.5f);
            Draw(g, globalDistance, time, sysPos, canvas, 0f, 1f);
        }

        private void DrawSpiral(Graphics g, float distance, float time, PointF systemPosNormalized, Size canvasSize, float systemTiltDeg, float systemFlatten)
        {
            var state = g.Save();

            // Proximidad del sistema a la galaxia [0..1]
            var sysPx = new PointF(systemPosNormalized.X * canvasSize.Width, systemPosNormalized.Y * canvasSize.Height);
            var dx = Center.X - sysPx.X;
            var dy = Center.Y - sysPx.Y;
            var distToSystem = MathF.Sqrt(dx * dx + dy * dy);
            var maxDiag = MathF.Sqrt(canvasSize.Width * canvasSize.Width + canvasSize.Height * canvasSize.Height);
            var proximity = 1f - MathF.Min(1f, distToSystem / (maxDiag * 0.5f));

            // rotación combinada: rotación propia + influencia del tilt del sistema (proximity-weighted)
            float combinedRotation = RotationDegrees + systemTiltDeg * (0.6f * proximity);
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(combinedRotation);

            int stars = (int)(Size * 18);
            double turns = 2.5 + (_rng.NextDouble() * 2.0);
            float growthFactor = 1f + (distance - 1f) * 0.8f;
            float wobble = (float)(Math.Sin(time * 0.6 + _seed) * 0.6);

            for (int i = 0; i < stars; i++)
            {
                double t = i / (double)stars;
                double theta = t * turns * Math.PI * 2 + time * 0.15 * (1 + _rng.NextDouble());
                double baseRadius = t * Size;
                double radialNoise = baseRadius * (0.05 + 0.25 * _rng.NextDouble());
                float r = (float)((baseRadius + radialNoise) * (growthFactor + wobble * 0.02));

                double armOffset = (theta * Arms) % (Math.PI * 2);
                double armDensity = Math.Pow(Math.Cos(armOffset), 10);
                if (_rng.NextDouble() < 0.25 + 0.65 * armDensity)
                {
                    float x = (float)(Math.Cos(theta) * r);
                    float y = (float)(Math.Sin(theta) * r);

                    // aplicar flatten del sistema sobre Y (proyección)
                    y *= systemFlatten;

                    // artefacto orbital: desplazamiento oscilatorio cuando la galaxia está cerca del sistema
                    if (proximity > 0.05f)
                    {
                        var dirX = -dx; var dirY = -dy;
                        var len = MathF.Sqrt(dirX * dirX + dirY * dirY) + 0.0001f;
                        dirX /= len; dirY /= len;
                        var perpX = -dirY; var perpY = dirX;
                        float amplitude = 6f * proximity * (0.6f + (float)(Math.Sin(time * 1.5 + i * 0.03)));
                        x += perpX * amplitude * (float)(0.3 + 0.7 * _rng.NextDouble());
                        y += perpY * amplitude * (float)(0.3 + 0.7 * _rng.NextDouble());
                    }

                    int s = _rng.Next(1, 4);
                    int alpha = 120 + (int)(120 * (1 - t));
                    using var b = new SolidBrush(Color.FromArgb(Math.Min(255, Math.Max(40, alpha)), BaseColor));
                    g.FillEllipse(b, x - s / 2f, y - s / 2f, s, s);
                }
            }

            float nuc = Size * 0.14f * (0.8f + (distance - 1f) * 0.4f);
            using (var b = new SolidBrush(Color.FromArgb(180, 255, 230, 140)))
                g.FillEllipse(b, -nuc, -nuc * systemFlatten, nuc * 2, nuc * 2 * systemFlatten);

            g.Restore(state);
        }

        private void DrawElliptical(Graphics g, float distance, float systemTiltDeg, float systemFlatten)
        {
            var state = g.Save();

            var combinedRotation = RotationDegrees + systemTiltDeg * 0.4f;
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(combinedRotation);

            float w = Size * (0.9f + (distance - 1f) * 0.2f);
            float h = Size * (0.6f * (0.7f + (1f / (0.8f + distance * 0.2f))));
            // aplicar flatten de sistema sobre la altura
            h *= systemFlatten;

            using var b = new SolidBrush(Color.FromArgb(40, BaseColor));
            g.FillEllipse(b, -w / 2, -h / 2, w, h);
            using var p = new Pen(Color.FromArgb(120, BaseColor));
            g.DrawEllipse(p, -w / 2, -h / 2, w, h);

            g.Restore(state);
        }

        private void DrawRing(Graphics g, float distance, float time, PointF systemPosNormalized, Size canvasSize, float systemTiltDeg, float systemFlatten)
        {
            var state = g.Save();

            var combinedRotation = RotationDegrees + (float)(time * 8.0 % 360) + systemTiltDeg * 0.5f;
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(combinedRotation);

            float outer = Size * (0.9f + (distance - 1f) * 0.3f);
            float inner = outer * 0.55f;
            var rnd = _rng;

            for (int i = 0; i < 140; i++)
            {
                double a = i / 140.0 * Math.PI * 2 + time * 0.4;
                double r = inner + (outer - inner) * (0.4 + 0.6 * rnd.NextDouble());
                float x = (float)(r * Math.Cos(a));
                float y = (float)(r * Math.Sin(a));
                y *= systemFlatten;

                int s = rnd.Next(1, 4);
                using var b = new SolidBrush(Color.FromArgb(120 + rnd.Next(80), BaseColor));
                g.FillEllipse(b, x - s / 2f, y - s / 2f, s, s);
            }

            g.Restore(state);
        }

        private void DrawIrregular(Graphics g, float distance, PointF systemPosNormalized, Size canvasSize, float systemTiltDeg, float systemFlatten)
        {
            var state = g.Save();

            var combinedRotation = RotationDegrees + systemTiltDeg * 0.4f;
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(combinedRotation);

            int pts = 80;
            for (int i = 0; i < pts; i++)
            {
                double a = i / (double)pts * Math.PI * 2;
                double r = Size * (0.2 + 0.8 * _rng.NextDouble()) * (0.8 + (distance - 1f) * 0.3);
                float x = (float)(r * Math.Cos(a));
                float y = (float)(r * Math.Sin(a));
                y *= systemFlatten;

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
            var g = new Galaxy(type, center, size, baseColor)
            {
                Arms = R.Next(2, 5),
                RotationDegrees = (float)(R.NextDouble() * 360f)
            };
            return g;
        }
    }
}