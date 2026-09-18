using GalaxiaplanismoDesorbitante.Models;
using SolarSystemSim.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GalaxiaplanismoDesorbitante
{
    public class FormSolarSystem : Form
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly List<CelestialBody> _planets = new();
        private readonly List<Galaxy> _galaxies = new();
        private float _flatten = 1.0f;
        private float _time = 0f;
        private float _cameraDistance = 1.0f; // controla crecimiento de las espirales

        public FormSolarSystem()
        {
            Text = "Sistema Solar - Galaxiaplanismo Desorbitante";
            ClientSize = new Size(1100, 800);
            DoubleBuffered = true;
            InitializeBodies();
            InitializeGalaxies();

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += (s, e) => { UpdateTick(); };
            _timer.Start();

            KeyPreview = true;
            KeyDown += FormSolarSystem_KeyDown;
        }

        private void InitializeBodies()
        {
            _planets.Add(new CelestialBody { Name = "Mercurio", OrbitRadius = 80, OrbitSpeed = 0.03f, Size = 4, Color = Color.LightGray });
            _planets.Add(new CelestialBody { Name = "Venus", OrbitRadius = 120, OrbitSpeed = 0.02f, Size = 6, Color = Color.Orange });
            _planets.Add(new CelestialBody { Name = "Tierra", OrbitRadius = 160, OrbitSpeed = 0.015f, Size = 7, Color = Color.DeepSkyBlue });
            _planets.Add(new CelestialBody { Name = "Marte", OrbitRadius = 200, OrbitSpeed = 0.012f, Size = 6, Color = Color.OrangeRed });
            _planets.Add(new CelestialBody { Name = "Júpiter", OrbitRadius = 260, OrbitSpeed = 0.008f, Size = 14, Color = Color.SandyBrown });
            _planets.Add(new CelestialBody { Name = "Saturno", OrbitRadius = 320, OrbitSpeed = 0.006f, Size = 12, Color = Color.Khaki });
        }

        private void InitializeGalaxies()
        {
            var w = ClientSize.Width;
            var h = ClientSize.Height;
            _galaxies.Add(new Galaxy(GalaxyType.Spiral, new PointF(150, 120), 100, Color.FromArgb(200, 230, 200)) { Arms = 3, RotationDegrees = -12f });
            _galaxies.Add(new Galaxy(GalaxyType.Elliptical, new PointF(1000, 100), 120, Color.FromArgb(200, 200, 255)) { RotationDegrees = 20f });
            _galaxies.Add(new Galaxy(GalaxyType.Ring, new PointF(900, 650), 90, Color.FromArgb(255, 200, 200)) { RotationDegrees = -30f });
            _galaxies.Add(new Galaxy(GalaxyType.Irregular, new PointF(200, 700), 70, Color.FromArgb(220, 200, 255)) { RotationDegrees = 45f });
        }

        private void FormSolarSystem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) { _flatten = Math.Max(0.2f, _flatten - 0.05f); Invalidate(); }
            else if (e.KeyCode == Keys.Right) { _flatten = Math.Min(2.0f, _flatten + 0.05f); Invalidate(); }
            else if (e.KeyCode == Keys.Up) { _cameraDistance = Math.Min(2.5f, _cameraDistance + 0.06f); }   // acercar
            else if (e.KeyCode == Keys.Down) { _cameraDistance = Math.Max(0.3f, _cameraDistance - 0.06f); } // alejar
            else if (e.KeyCode == Keys.PageUp) { foreach (var p in _planets) p.OrbitSpeed *= 1.05f; }
            else if (e.KeyCode == Keys.PageDown) { foreach (var p in _planets) p.OrbitSpeed *= 0.95f; }
        }

        private void UpdateTick()
        {
            _time += 0.03f;
            foreach (var p in _planets) p.Update();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            // Dibujar galaxias de fondo con influencia de cameraDistance
            foreach (var gal in _galaxies)
            {
                gal.Draw(g, _cameraDistance, _time);
            }

            PointF center = new PointF(ClientSize.Width / 2f, ClientSize.Height / 2f);
            // Sol (tiene en Y un ligero escalado por flatten)
            g.FillEllipse(Brushes.Yellow, center.X - 24, center.Y - 24 * _flatten, 48, 48 * _flatten);

            using var orbitPen = new Pen(Color.FromArgb(60, 200, 200, 200));
            foreach (var p in _planets)
            {
                float rx = p.OrbitRadius;
                float ry = p.OrbitRadius * _flatten;
                g.DrawEllipse(orbitPen, center.X - rx, center.Y - ry, rx * 2, ry * 2);

                var pos = p.GetPosition(center, _flatten);
                using var b = new SolidBrush(p.Color);
                g.FillEllipse(b, pos.X - p.Size / 2f, pos.Y - p.Size / 2f, p.Size, p.Size);
            }

            using var font = new Font("Consolas", 12, FontStyle.Bold);
            g.DrawString($"Achatar (Y): {_flatten:0.00}    Distancia cam: {_cameraDistance:0.00}", font, Brushes.LightGreen, 10, 10);
            g.DrawString("← →: achatar | ↑ ↓: acercar/alejar (afecta espirales) | PgUp/PgDn: velocidad orbital", font, Brushes.LightGray, 10, 30);
        }
    }
}