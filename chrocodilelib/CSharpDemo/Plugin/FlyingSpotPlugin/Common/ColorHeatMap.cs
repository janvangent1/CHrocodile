using System;
using System.Drawing;
using System.Numerics;

namespace FSSCommon
{
    public class ColorHeatMap 
    {
        public byte Alpha = 0xFF;
        private double m_minVal;
        private double m_maxVal;
        private double m_invRange;

        public ColorHeatMap(double minVal, double maxVal)
        {
            m_minVal = minVal;
            m_maxVal = Math.Max(minVal, maxVal);
            m_invRange = m_maxVal - m_minVal;
            if (m_invRange < 1e-7)
            {
                Console.WriteLine("ColorHeatMap: min/max range [{0}; {1}] is too small!",
                    m_minVal, m_maxVal);
                m_invRange = -1;
            } else
                m_invRange = 1.0 / m_invRange;
            Console.WriteLine("ColorHeatMap: min/max range [{0}; {1}]",
                    m_minVal, m_maxVal); 
        }

        public Color GetColorForValue(double val)
        {
            if (m_invRange < 0) // no range is available
                return Color.Gray;

            if (double.IsNaN(val) || double.IsNegativeInfinity(val))
                val = m_minVal;

            if (double.IsPositiveInfinity(val))
                val = m_maxVal;

            byte r = 0, g = 0, b = 0;

            var gray = Math.Min(val, m_maxVal);
            gray = 8.0 * (Math.Max(gray, m_minVal) - m_minVal) * m_invRange;
            const double s = 255.0 / 2.0;

            if (gray <= 1)
            {
                b = (byte)((gray + 1) * s);
            }
            else if (gray <= 3)
            {
                g = (byte)((gray - 1) * s);
                b = 255;
            }
            else if (gray <= 5)
            {
                r = (byte)((gray - 3) * s);
                g = 255;
                b = (byte)((5 - gray) * s);
            }
            else if (gray <= 7)
            {
                r = 255;
                g = (byte)((7 - gray) * s);
            }
            else
            {
                r = (byte)((9 - gray) * s);
            }

           // Console.WriteLine("Range: {0} - {1}; val: {2}; rgb: {3},{4},{5}",
             //   m_minVal, m_maxVal, val, r, g, b);
            return Color.FromArgb(Alpha, r, g, b);
        }
    }
}
