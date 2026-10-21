using System.Drawing;
using System.Drawing.Drawing2D;

namespace StudentManager
{
    // Tự vẽ icon nhỏ bằng code để khỏi phải thêm file ảnh vào project
    public static class IconHelper
    {
        public static Bitmap Make(string letter, Color color, int size = 32)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, 1, 1, size - 2, size - 2);
            using var font = new Font("Segoe UI", size * 0.45f, FontStyle.Bold, GraphicsUnit.Pixel);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(letter, font, Brushes.White, new RectangleF(0, 0, size, size), sf);
            return bmp;
        }
    }
}
