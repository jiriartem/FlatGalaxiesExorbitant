using System;
using System.Drawing;

namespace GalaxiaplanismoDesorbitante
{
    public static class SimulationState
    {
        // posición normalizada del sistema [0..1]
        public static PointF SystemPosition { get; private set; } = new PointF(0.5f, 0.5f);

        // achatamiento vertical (1 = normal, <1 más aplanado verticalmente)
        public static float Flatten { get; private set; } = 1.0f;

        // inclinación del eje en grados (-90..90)
        public static float AxisTiltDeg { get; private set; } = 0.0f;

        // distancia/cámara que controla apertura de espirales
        public static float CameraDistance { get; private set; } = 1.0f;

        public static float SystemScale { get; private set; } = 1.0f;

        public static event Action? StateChanged;

        public static void MoveNormalized(float dx, float dy)
        {
            var nx = Clamp(SystemPosition.X + dx, 0f, 1f);
            var ny = Clamp(SystemPosition.Y + dy, 0f, 1f);
            SystemPosition = new PointF(nx, ny);
            RaiseChanged();
        }

        public static void AdjustFlatten(float delta) => SetFlatten(Flatten + delta);
        public static void SetFlatten(float f)
        {
            Flatten = MathF.Max(0.2f, MathF.Min(2.5f, f));
            RaiseChanged();
        }

        public static void AdjustAxisTilt(float delta) => SetAxisTilt(AxisTiltDeg + delta);
        public static void SetAxisTilt(float deg)
        {
            AxisTiltDeg = Clamp(deg, -80f, 80f);
            RaiseChanged();
        }

        public static void AdjustCameraDistance(float delta) => SetCameraDistance(CameraDistance + delta);
        public static void SetCameraDistance(float d)
        {
            CameraDistance = MathF.Max(0.3f, MathF.Min(3.5f, d));
            RaiseChanged();
        }

        public static void AdjustSystemScale(float delta) => SetSystemScale(SystemScale + delta);
        public static void SetSystemScale(float s)
        {
            SystemScale = MathF.Max(0.2f, MathF.Min(3.0f, s));
            RaiseChanged();
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }


        private static void RaiseChanged() => StateChanged?.Invoke();
    }
}

   

