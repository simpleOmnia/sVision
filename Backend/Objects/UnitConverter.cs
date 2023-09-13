using System;

namespace svision_internal
{
    public static class UnitConverter
    {
        public static float degreeToScreenPos(float degree, float headset_fov)
        { return ((headset_fov / 2.0f) + degree) / headset_fov; }

        public static float degreeToScreenPos(float degree)
        { return degreeToScreenPos(degree, svision.Instance.headsetHorizontalFOV);}

        public static float degreeToScreenPos(float degree, float headset_fov, bool invert)
        { return ((headset_fov / 2.0f) - degree) / headset_fov; }

        public static float degreeToMicron(float degree) {
            float sign = degree >= 0 ? 1.0f : -1.0f;
            degree = Math.Abs(degree);
            float micron = 0.268f * degree + 3.427e-4f * (float) Math.Pow(degree, 2) -
                           8.3309e-6f * (float) Math.Pow(degree, 3);
            micron = 1e3f * micron;

            return micron * sign; }

        public static float micronToDegree(float micron) {
            float sign = micron >= 0 ? 1.0f : -1.0f;
            float micronMM = 1e-3f * Math.Abs(micron);
            float degree = (3.556f * micronMM) + (0.05993f * (float) Math.Pow(micronMM, 2)) -
                           (0.007358f * (float) Math.Pow(micronMM, 3));
            degree += 3.027e-4f * (float) Math.Pow(micronMM, 4);
            return sign * degree;
        }

        public static float pixelToDegree(int pixel, float headset_fov, int resolution)
        { return pixel * (headset_fov / resolution) - headset_fov / 2.0f; }


        public static float pixelToMicron(int pixel, float headset_fov, int resolution)
        { return degreeToMicron(pixelToDegree(pixel, headset_fov, resolution)); }

        public static float micronToScreenPos(float micron, float headset_fov)
        { return degreeToScreenPos(micronToDegree(micron), headset_fov); }
        
        public static float micronToScreenPos(float micron)
        { return degreeToScreenPos(micronToDegree(micron), svision.Instance.headsetHorizontalFOV); }


        public static float micronToScreenPos(float micron, float headset_fov, bool invert)
        { return degreeToScreenPos(micronToDegree(micron), headset_fov, invert); }

        public static float screenPosToDegree(float screenPos, float headset_fov)
        { return (screenPos - 0.5f) * headset_fov; }

        public static float screenPosToDegree(float screenPos, float headset_fov, bool invert)
        { return invert ? screenPosToDegree(1 - screenPos, headset_fov) : screenPosToDegree(screenPos, headset_fov); }

        public static float screenPosToMicron(float screenPos, float headset_fov)
        { return degreeToMicron(screenPosToDegree(screenPos, headset_fov)); }

        public static float screenPosToMicron(float screenPos, float headset_fov, bool invert)
        { return degreeToMicron(screenPosToDegree(screenPos, headset_fov, invert)); }
    }
}

