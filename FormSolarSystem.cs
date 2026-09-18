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
        private float _time = 0f;

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

            SimulationState.StateChanged += OnSimulationStateChanged;
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
            _galaxies.Add(new Galaxy(GalaxyType.Spiral, new PointF(150, 120), 100, Color.FromArgb(200, 230, 200)) { Arms = 3, RotationDegrees = -12f });
            _galaxies.Add(new Galaxy(GalaxyType.Elliptical, new PointF(1000, 100), 120, Color.FromArgb(200, 200, 255)) { RotationDegrees = 20f });
            _galaxies.Add(new Galaxy(GalaxyType.Ring, new PointF(900, 650), 90, Color.FromArgb(255, 200, 200)) { RotationDegrees = -30f });
            _galaxies.Add(new Galaxy(GalaxyType.Irregular, new PointF(200, 700), 70, Color.FromArgb(220, 200, 255)) { RotationDegrees = 45f });
        }

        private void FormSolarSystem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) SimulationState.AdjustAxisTilt(-2f);
            else if (e.KeyCode == Keys.Right) SimulationState.AdjustAxisTilt(2f);
            else if (e.KeyCode == Keys.Up) SimulationState.AdjustFlatten(-0.04f);
            else if (e.KeyCode == Keys.Down) SimulationState.AdjustFlatten(0.04f);
            else if (e.KeyCode == Keys.PageUp) SimulationState.AdjustSystemScale(0.05f);
            else if (e.KeyCode == Keys.PageDown) SimulationState.AdjustSystemScale(-0.05f);
            else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus) SimulationState.AdjustCameraDistance(0.06f);
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) SimulationState.AdjustCameraDistance(-0.06f);
        }

        private void OnSimulationStateChanged() => Invalidate();

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

            var camDist = SimulationState.CameraDistance;
            var systemPos = SimulationState.SystemPosition;
            var flatten = SimulationState.Flatten;
            var tilt = SimulationState.AxisTiltDeg;
            var scale = SimulationState.SystemScale;

            // Galaxias de fondo reciben la info de proyección
            foreach (var gal in _galaxies)
            {
                gal.Draw(g, camDist, _time, systemPos, ClientSize, tilt, flatten);
            }

            // centro del sistema en píxeles
            var sysCenter = new PointF(systemPos.X * ClientSize.Width, systemPos.Y * ClientSize.Height);

            // Dibujamos órbitas y planetas rotando por tilt (transform)
            var save = g.Save();
            g.TranslateTransform(sysCenter.X, sysCenter.Y);
            g.RotateTransform(tilt); // inclinar eje
            // dibujar sol con scale/flatten aplicados localmente: usar coordenadas locales (0,0)
            float sunW = 48f * scale;
            float sunH = sunW * MathF.Max(0.2f, flatten);
            g.FillEllipse(Brushes.Yellow, -sunW / 2f, -sunH / 2f, sunW, sunH);

            using var orbitPen = new Pen(Color.FromArgb(60, 200, 200, 200));
            foreach (var p in _planets)
            {
                float rx = p.OrbitRadius * scale;
                float ry = p.OrbitRadius * scale * flatten;
                g.DrawEllipse(orbitPen, -rx, -ry, rx * 2, ry * 2);

                var pos = p.GetPosition(new PointF(0, 0), flatten);
                using var b = new SolidBrush(p.Color);
                g.FillEllipse(b, pos.X - p.Size / 2f, pos.Y - p.Size / 2f, p.Size, p.Size);
            }
            g.Restore(save);

            using var font = new Font("Consolas", 12, FontStyle.Bold);
            g.DrawString($"Pos: ({systemPos.X:0.00},{systemPos.Y:0.00})  Tilt: {tilt:0.0}°  Flatten: {flatten:0.00}  Dist: {camDist:0.00}", font, Brushes.LightGreen, 10, 10);
            g.DrawString("← →: inclinar eje | ↑ ↓: aplanar/levantar | + / -: abrir/ciñar espirales | PgUp/PgDn: escala sistema", font, Brushes.LightGray, 10, 30);
        }
    }
}