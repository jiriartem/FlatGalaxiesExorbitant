using GalaxiaplanismoDesorbitante.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxiaplanismoDesorbitante
{
    public static class GalaxyFactory
    {
        private static readonly Random R = new();
        public static Galaxy RandomGalaxy(int w, int h)
        {
            var types = (GalaxyType[])Enum.GetValues(typeof(GalaxyType));
            var type = types[R.Next(types.Length)];
            var center = new PointF(R.Next(50, w - 50), R.Next(50, h - 50));
            float size = R.Next(30, 120);
            Color baseColor = Color.FromArgb(R.Next(120, 255), R.Next(80, 255), R.Next(80, 255), R.Next(255));
            return new Galaxy(type, center, size, baseColor);
        }
    }
}
