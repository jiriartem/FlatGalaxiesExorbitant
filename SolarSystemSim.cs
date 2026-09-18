using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GalaxiaplanismoDesorbitante;
using GalaxiaplanismoDesorbitante.Models;
using SolarSystemSim.Models;

namespace SolarSystemSim
{
    public class FormMultiGalaxies : Form
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly List<GalaxiaplanismoDesorbitante.Models.Galaxy> _galaxies = new();
        private float _flatten = 1.0f;
        private float _time = 0f;
        private readonly Random _rng = new();

        public FormMultiGalaxies()
        {
            Text = "Galerías - Multi tipo (Flechas ← → achatar)";
            ClientSize = new Size(1200, 800);
            DoubleBuffered = true;

            GenerateInitialGalaxies(60);

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += (s, e) => { _time += 0.03f; Invalidate(); };
            _timer.Start();

            KeyPreview = true;
            KeyDown += FormMultiGalaxies_KeyDown;
        }

        private void FormMultiGalaxies_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) { _flatten = Math.Max(0.2f, _flatten - 0.03f); }
            else if (e.KeyCode == Keys.Right) { _flatten = Math.Min(2.0f, _flatten + 0.03f); }
            else if (e.KeyCode == Keys.Space) { GenerateInitialGalaxies(120); }
        }

        private void GenerateInitialGalaxies(int count)
        {
            _galaxies.Clear();
            for (int i = 0; i < count; i++)
            {
                _galaxies.Add(GalaxiaplanismoDesorbitante.GalaxyFactory.RandomGalaxy(ClientSize.Width, ClientSize.Height));
            }

            // Ensure at least two visible examples (as in your request)
            _galaxies[0] = new Galaxy(GalaxyType.Spiral, new PointF(200, 150), 120, Color.LightBlue);
            _galaxies[1] = new Galaxy(GalaxyType.Elliptical, new PointF(1000, 200), 140, Color.LightGoldenrodYellow);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            // Draw all galaxies; many small ones + variety
            foreach (var gal in _galaxies)
            {
                gal.Draw(g, _flatten, _time);
            }

            using var font = new Font("Consolas", 12, FontStyle.Bold);
            g.DrawString($"Galaxias: {_galaxies.Count}    Achatar (Y): {_flatten:0.00}", font, Brushes.LightGreen, 8, 8);
            g.DrawString("← →: achatar | Barra espaciadora: regenerar muchas galaxias", font, Brushes.LightGray, 8, 28);
        }
    }
}