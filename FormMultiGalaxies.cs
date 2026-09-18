using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GalaxiaplanismoDesorbitante.Models;

namespace GalaxiaplanismoDesorbitante
{
    public class FormMultiGalaxies : Form
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly List<Galaxy> _galaxies = new();
        private float _time = 0f;
        private readonly Random _rng = new();

        // BUG state
        private bool _bugVisible = false;
        private float _bugPulse = 0f;

        public FormMultiGalaxies()
        {
            Text = "Galaxias - Galaxiaplanismo Desorbitante (Pixel style)";
            ClientSize = new Size(1200, 800);
            DoubleBuffered = true;

            GenerateInitialGalaxies(140);

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += (s, e) =>
            {
                _time += 0.03f;
                _bugPulse += 0.06f;
                Invalidate();
            };
            _timer.Start();

            KeyPreview = true;
            KeyDown += FormMultiGalaxies_KeyDown;

            // react to global changes
            SimulationState.StateChanged += OnSimulationStateChanged;
        }

        private void FormMultiGalaxies_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) SimulationState.AdjustAxisTilt(-2f);
            else if (e.KeyCode == Keys.Right) SimulationState.AdjustAxisTilt(2f);
            else if (e.KeyCode == Keys.Up) SimulationState.AdjustFlatten(-0.04f);
            else if (e.KeyCode == Keys.Down) SimulationState.AdjustFlatten(0.04f);
            else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus) SimulationState.AdjustCameraDistance(0.06f);
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) SimulationState.AdjustCameraDistance(-0.06f);
            else if (e.KeyCode == Keys.Space) GenerateInitialGalaxies(220);
            // mover sistema con WASD como alternativa
            else if (e.KeyCode == Keys.A) SimulationState.MoveNormalized(-0.02f, 0f);
            else if (e.KeyCode == Keys.D) SimulationState.MoveNormalized(0.02f, 0f);
            else if (e.KeyCode == Keys.W) SimulationState.MoveNormalized(0f, -0.02f);
            else if (e.KeyCode == Keys.S) SimulationState.MoveNormalized(0f, 0.02f);
        }

        private void OnSimulationStateChanged() => Invalidate();

        private void GenerateInitialGalaxies(int count)
        {
            _galaxies.Clear();
            for (int i = 0; i < count; i++)
            {
                _galaxies.Add(GalaxyFactory.RandomGalaxy(ClientSize.Width, ClientSize.Height));
            }

            if (_galaxies.Count >= 2)
            {
                _galaxies[0] = new Galaxy(GalaxyType.Spiral, new PointF(220, 160), 160, Color.LightBlue) { Arms = 3, RotationDegrees = -18f };
                _galaxies[1] = new Galaxy(GalaxyType.Spiral, new PointF(980, 220), 180, Color.LightGoldenrodYellow) { Arms = 4, RotationDegrees = 40f };
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            var camDist = SimulationState.CameraDistance;
            var systemPos = SimulationState.SystemPosition;
            var systemTilt = SimulationState.AxisTiltDeg;
            var systemFlatten = SimulationState.Flatten;

            // 1) Dibujar galaxias en modo pixelado
            foreach (var gal in _galaxies)
            {
                DrawPixelGalaxy(g, gal, camDist, _time, systemPos, ClientSize, systemTilt, systemFlatten);
            }

            // 2) Dibujar punto de referencia: telescopio + PC (esquina inferior izquierda)
            DrawReferenceTelescopeAndPc(g);

            // 3) Comprobar proximidad y mostrar BUG si corresponde
            _bugVisible = CheckProximityForBug(systemPos, ClientSize, out Galaxy nearGalaxy);

            if (_bugVisible && nearGalaxy != null)
            {
                DrawBigBugOverlay(g, nearGalaxy.Center);
            }

            // HUD
            using var font = new Font("Consolas", 12, FontStyle.Bold);
            g.DrawString($"Galaxias: {_galaxies.Count}    DistCam: {camDist:0.00}    Tilt: {systemTilt:0.0}°    Flatten: {systemFlatten:0.00}", font, Brushes.LightGreen, 8, 8);
            g.DrawString("← →: inclinar | ↑ ↓: aplanar | + / -: abrir/ciñar | WASD: mover sistema | Barra: regenerar", font, Brushes.LightGray, 8, 28);
        }

        private void DrawPixelGalaxy(Graphics g, Galaxy gal, float distance, float time, PointF systemPosNormalized, Size canvasSize, float systemTiltDeg, float systemFlatten)
        {
            // Pixel size variable según tamaño de la galaxia
            int pixelSize = Math.Max(1, (int)Math.Clamp(gal.Size / 24f, 1f, 6f));

            // calculamos proximidad como en Galaxy.Draw
            var sysPx = new PointF(systemPosNormalized.X * canvasSize.Width, systemPosNormalized.Y * canvasSize.Height);
            var dx = gal.Center.X - sysPx.X;
            var dy = gal.Center.Y - sysPx.Y;
            var distToSystem = MathF.Sqrt(dx * dx + dy * dy);
            var maxDiag = MathF.Sqrt(canvasSize.Width * canvasSize.Width + canvasSize.Height * canvasSize.Height);
            var proximity = 1f - MathF.Min(1f, distToSystem / (maxDiag * 0.5f));

            // patrón: simular una espiral pero con rects "pixel"
            int cols = (int)(gal.Size / pixelSize) + 6;
            int stars = (int)(gal.Size * 10);
            for (int i = 0; i < stars; i++)
            {
                double t = i / (double)stars;
                double turns = 2.0 + (gal.Arms * 0.5);
                double theta = t * turns * Math.PI * 2 + time * 0.2 + gal.RotationDegrees * Math.PI / 180.0;
                double baseRadius = t * gal.Size * (0.6 + 0.8 * (_rng.NextDouble()));
                double noise = baseRadius * (0.05 + 0.25 * _rng.NextDouble());
                float r = (float)(baseRadius + noise);

                // posicion local
                float x = (float)(Math.Cos(theta) * r);
                float y = (float)(Math.Sin(theta) * r);

                // aplicamos flatten / tilt del sistema como proyección
                y *= systemFlatten;
                // rotación por tilt aplicada de forma global en forma de desplazamiento en X/Y
                float tiltRad = systemTiltDeg * (float)(Math.PI / 180.0);
                float tiltEffect = proximity * 0.25f; // cuánto influye según proximidad
                float tx = x * (float)Math.Cos(tiltRad * tiltEffect) - y * (float)Math.Sin(tiltRad * tiltEffect);
                float ty = x * (float)Math.Sin(tiltRad * tiltEffect) + y * (float)Math.Cos(tiltRad * tiltEffect);

                // convertir a coords mundo
                float wx = gal.Center.X + tx;
                float wy = gal.Center.Y + ty;

                // snap a grid pixel
                int ix = (int)Math.Round(wx / pixelSize) * pixelSize;
                int iy = (int)Math.Round(wy / pixelSize) * pixelSize;

                // small orbital artifact: si la galaxia está cerca del sistema, desplazar en perpendicular
                if (proximity > 0.06f)
                {
                    var dirX = -dx; var dirY = -dy;
                    var len = MathF.Sqrt(dirX * dirX + dirY * dirY) + 0.0001f;
                    dirX /= len; dirY /= len;
                    var perpX = -dirY; var perpY = dirX;
                    float amplitude = 6f * proximity * (0.5f + (float)Math.Sin(time * 1.2 + i * 0.07));
                    ix += (int)(perpX * amplitude);
                    iy += (int)(perpY * amplitude);
                }

                // color pulsatil levemente
                int alpha = 80 + (int)(120 * (1 - t));
                var c = Color.FromArgb(Math.Min(255, Math.Max(20, alpha)), gal.BaseColor);
                using var b = new SolidBrush(c);
                g.FillRectangle(b, ix - pixelSize / 2, iy - pixelSize / 2, pixelSize, pixelSize);
            }

            // núcleo pixelado
            int coreSizePx = Math.Max(6, (int)(pixelSize * 4 * (1 + proximity)));
            using var coreBrush = new SolidBrush(Color.FromArgb(180, 255, 230, 140));
            g.FillRectangle(coreBrush, (int)gal.Center.X - coreSizePx / 2, (int)gal.Center.Y - (int)(coreSizePx / 2 * systemFlatten), coreSizePx, (int)(coreSizePx * systemFlatten));
        }

        private bool CheckProximityForBug(PointF systemPosNormalized, Size canvasSize, out Galaxy nearGalaxy)
        {
            nearGalaxy = null!;
            var sysPx = new PointF(systemPosNormalized.X * canvasSize.Width, systemPosNormalized.Y * canvasSize.Height);

            foreach (var gal in _galaxies)
            {
                var dx = gal.Center.X - sysPx.X;
                var dy = gal.Center.Y - sysPx.Y;
                var d = MathF.Sqrt(dx * dx + dy * dy);
                // umbral: si estamos muy cerca del núcleo o dentro de un radio reducido
                if (d < gal.Size * 0.35f)
                {
                    nearGalaxy = gal;
                    return true;
                }
            }
            return false;
        }

        private void DrawReferenceTelescopeAndPc(Graphics g)
        {
            // esquina inferior izquierda con margen
            int margin = 18;
            int baseX = margin + 40;
            int baseY = ClientSize.Height - margin - 40;

            // Dibujar PC (monitor + base)
            using var monitor = new SolidBrush(Color.FromArgb(200, 30, 30, 40));
            g.FillRectangle(monitor, baseX - 4, baseY - 34, 48, 30);
            using var screen = new SolidBrush(Color.FromArgb(220, 10, 80, 120));
            g.FillRectangle(screen, baseX, baseY - 30, 40, 22);
            using var stand = new SolidBrush(Color.FromArgb(180, 60, 60, 60));
            g.FillRectangle(stand, baseX + 14, baseY - 8, 12, 8);
            g.FillRectangle(stand, baseX + 8, baseY - 2, 28, 6);

            // Dibujar telescopio estilizado a la derecha del PC
            int tx = baseX + 90;
            int ty = baseY - 20;
            using var tube = new Pen(Color.Silver, 6);
            g.DrawLine(tube, tx, ty, tx + 28, ty - 18);
            g.DrawLine(tube, tx + 10, ty + 4, tx + 36, ty - 12);
            using var lens = new SolidBrush(Color.FromArgb(200, 120, 180, 255));
            g.FillEllipse(lens, tx + 26, ty - 28, 16, 16);
            using var tripod = new Pen(Color.FromArgb(160, 120, 80, 40), 4);
            g.DrawLine(tripod, tx + 4, ty + 8, tx - 6, ty + 34);
            g.DrawLine(tripod, tx + 8, ty + 8, tx + 34, ty + 34);
            g.DrawLine(tripod, tx + 26, ty + 6, tx + 60, ty + 26);

            // Etiqueta pequeña
            using var f = new Font("Consolas", 9, FontStyle.Regular);
            g.DrawString("Observatorio", f, Brushes.LightGray, baseX - 8, baseY + 6);
        }

        private void DrawBigBugOverlay(Graphics g, PointF center)
        {
            // overlay semi-transparente
            using var overlay = new SolidBrush(Color.FromArgb(140, 10, 10, 20));
            g.FillRectangle(overlay, 0, 0, ClientSize.Width, ClientSize.Height);

            // posición del insecto: sobre la galaxia (center)
            float cx = center.X;
            float cy = center.Y;

            // pulso para animar tamaño/color
            float pulse = 1f + 0.08f * (float)Math.Sin(_bugPulse * 3.0);
            float size = 160f * pulse;

            // cuerpo
            using var bodyBrush = new SolidBrush(Color.FromArgb(220, 40, 10, 10));
            g.FillEllipse(bodyBrush, cx - size / 3f, cy - size / 4f, size * 2f / 3f, size / 2f);

            // cabeza
            using var headBrush = new SolidBrush(Color.FromArgb(230, 60, 20, 20));
            g.FillEllipse(headBrush, cx - size / 2f, cy - size / 2f, size / 3f, size / 3f);

            // ojos
            using var eyeBrush = Brushes.White;
            g.FillEllipse(eyeBrush, cx - size / 2f + 6, cy - size / 2f + 10, size * 0.08f, size * 0.08f);
            g.FillEllipse(eyeBrush, cx - size / 3f + 8, cy - size / 2f + 10, size * 0.08f, size * 0.08f);

            // antenas
            using var penA = new Pen(Color.FromArgb(200, 220, 220, 220), 3);
            g.DrawLine(penA, cx - size / 2f + 10, cy - size / 2f + 8, cx - size / 2f - 24, cy - size / 2f - 36);
            g.DrawLine(penA, cx - size / 3f + 14, cy - size / 2f + 8, cx - size / 3f + 44, cy - size / 2f - 36);

            // patas (simples líneas)
            using var penLeg = new Pen(Color.FromArgb(200, 40, 10, 10), 4);
            for (int i = -2; i <= 2; i++)
            {
                float dx = i * 18f;
                g.DrawLine(penLeg, cx + dx, cy - size / 8f, cx + dx - 28f, cy + size / 3f);
                g.DrawLine(penLeg, cx + dx, cy - size / 8f, cx + dx + 28f, cy + size / 3f);
            }

            // Texto gigante "BIG BUG"
            float fontSize = 72f * (1f + 0.12f * (float)Math.Sin(_bugPulse * 2.0));
            using var bigFont = new Font("Consolas", (float)Math.Max(18, fontSize), FontStyle.Bold);
            var text = "BIG BUG";
            var sz = g.MeasureString(text, bigFont);
            var tx = (ClientSize.Width - sz.Width) / 2f;
            var ty = 60f;
            // sombra
            g.DrawString(text, bigFont, Brushes.Black, tx + 4, ty + 4);
            // color pulsante
            var pulsed = Color.FromArgb(255, (int)(200 + 55 * Math.Abs(Math.Sin(_bugPulse))), 20, 40);
            using var brushText = new SolidBrush(pulsed);
            g.DrawString(text, bigFont, brushText, tx, ty);
        }
    }
}