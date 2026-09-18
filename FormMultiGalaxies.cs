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
        private float _distance = 1.0f; // cámara/distancia: afecta crecimiento de espirales
        private float _time = 0f;
        private readonly Random _rng = new();

        public FormMultiGalaxies()
        {
            Text = "Galaxias - Galaxiaplanismo Desorbitante";
            ClientSize = new Size(1200, 800);
            DoubleBuffered = true;

            GenerateInitialGalaxies(120);

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += (s, e) => { _time += 0.03f; Invalidate(); };
            _timer.Start();

            KeyPreview = true;
            KeyDown += FormMultiGalaxies_KeyDown;
        }

        private void FormMultiGalaxies_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) { /* opcional: rotar vista general */ }
            else if (e.KeyCode == Keys.Right) { /* opcional: rotar vista general */ }
            else if (e.KeyCode == Keys.Up) { _distance = Math.Min(2.5f, _distance + 0.06f); }   // acercarse -> espirales se abren
            else if (e.KeyCode == Keys.Down) { _distance = Math.Max(0.3f, _distance - 0.06f); } // alejarse -> se ciñen
            else if (e.KeyCode == Keys.Space) { GenerateInitialGalaxies(200); }
        }

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

            foreach (var gal in _galaxies)
            {
                gal.Draw(g, _distance, _time);
            }

            using var font = new Font("Consolas", 12, FontStyle.Bold);
            g.DrawString($"Galaxias: {_galaxies.Count}    Distancia (cam): {_distance:0.00}", font, Brushes.LightGreen, 8, 8);
            g.DrawString("← →: (opcional) | ↑ ↓: acercar/alejar (afecta espirales) | Barra: regenerar", font, Brushes.LightGray, 8, 28);
        }
    }
}