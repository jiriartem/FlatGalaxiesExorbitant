using GalaxiaplanismoDesorbitante;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SolarSystemSim
{
    public class LauncherForm : Form
    {
        public LauncherForm()
        {
            Text = "Simulaciones - Lanzador";
            Size = new Size(400, 200);
            StartPosition = FormStartPosition.CenterScreen;

            var btnSolar = new Button { Text = "Sistema Solar (Flechas: ← → achatar)", Dock = DockStyle.Top, Height = 50 };
            var btnGal = new Button { Text = "Galerías (Muchos tipos)", Dock = DockStyle.Top, Height = 50 };
            Controls.Add(btnGal);
            Controls.Add(btnSolar);

            btnSolar.Click += (s, e) => { new FormSolarSystem().Show(); };
            btnGal.Click += (s, e) => { new FormMultiGalaxies().Show(); };
        }
    }
}