using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
using System.Text;

namespace GraduationPhotosManagement.Captcha
{
    /// <summary>
    /// 验证码配置和绘制逻辑
    /// </summary>
    public class SecurityCodeHelper
    {
        /// <summary>
        /// 验证码文本池
        /// </summary>
        private static readonly string[] _enTextArr = new string[] { "a", "b", "c", "d", "e", "f", "h", "k", "m", "n", "r", "s", "t", "u", "v", "w", "x", "z", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private static readonly Rgba32 _rgba32White = new Rgba32(255, 255, 255);
        /// <summary>
        /// 验证码图片宽高
        /// </summary>
        private static readonly int _imageWidth = 120;

        private static readonly int _imageHeight = 50;

        private int _img2Size = Math.Min(_imageWidth / 4, _imageHeight);
        /// <summary>
        /// 颜色池,较深的颜色
        /// https://tool.oschina.net/commons?type=3
        /// </summary>
        private static readonly Rgba32[] _colorHexArr = 
            new Rgba32[] { 
                Rgba32.ParseHex("00E5EE"), Rgba32.ParseHex("CD5B45"), Rgba32.ParseHex("191970"),
                Rgba32.ParseHex("2F4F4F"), Rgba32.ParseHex("000000"), Rgba32.ParseHex("43CD80"),
                Rgba32.ParseHex("191970"), Rgba32.ParseHex("006400"), Rgba32.ParseHex("458B00"),
                Rgba32.ParseHex("8B7765"), 
            };

        ///较浅的颜色
        private static readonly Rgba32[] _lightColorHexArr = 
            new Rgba32[] {
                Rgba32.ParseHex("FFFACD"), Rgba32.ParseHex("FDF5E6"), Rgba32.ParseHex("F0FFFF"),
                Rgba32.ParseHex("BBFFFF"), Rgba32.ParseHex("FAFAD2"), Rgba32.ParseHex("FFE4E1"),
                Rgba32.ParseHex("DCDCDC"), Rgba32.ParseHex("F0E68C"),
            };
        private readonly Random _random = new();
        /// <summary>
        /// 字体池
        /// </summary>
        private List<Font> _fontList = new();
        public SecurityCodeHelper()
        {
            var font = SystemFonts.CreateFont("Arial", 12);
            Array styles = Enum.GetValues(typeof(FontStyle));
            foreach (var s in styles)
            {
                var scaledFont = new Font(font, _img2Size, (FontStyle)s);
                _fontList.Add(scaledFont);
            }
        }
        /// <summary>
        /// 生成随机英文字母/数字组合字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GetRandomEnDigitalText(int length)
        {
            StringBuilder sb = new StringBuilder();
            if (length > 0)
            {
                do
                {
                    if (_random.Next(0, 2) > 0)
                    {
                        sb.Append(_random.Next(2, 9));
                    }
                    else
                    {
                        sb.Append(_enTextArr[_random.Next(0, _enTextArr.Length)]);
                    }
                }
                while (--length > 0);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 英文字母+数字组合验证码
        /// </summary>
        /// <param name="text"></param>
        /// <returns>验证码图片字节数组</returns>
        public byte[] GetEnDigitalCodeByte(string text)
        {
            using Image<Rgba32> img = getEnDigitalCodeImage(text);
            using var ms = new MemoryStream();
            img.SaveAsJpeg(ms);
            return ms.ToArray();
        }
        private Image<Rgba32> getEnDigitalCodeImage(string text)
        {
            Image<Rgba32> img = new Image<Rgba32>(_imageWidth, _imageHeight);
            var colorTextHex = _colorHexArr[_random.Next(0, _colorHexArr.Length)];
            var lignthColorHex = _lightColorHexArr[_random.Next(0, _lightColorHexArr.Length)];

            img.Mutate(ctx => {
                ctx.Fill(_lightColorHexArr[_random.Next(0, _lightColorHexArr.Length)]);
                ctx.Glow(lignthColorHex);
                DrawingGrid(ctx, _imageWidth, _imageHeight, lignthColorHex, 8, 1);
                DrawingEnText(ctx, _imageHeight, text);
                ctx.GaussianBlur(0.4f);
                // DrawingCircles(ctx, _imageWidth, _imageHeight, 15, _miniCircleR, _maxCircleR, _rgba32White);
            });
            return img;
        }
        public void DrawingEnText(IImageProcessingContext processingContext, int containerHeight, string text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                var textWidth = (_imageWidth / text.Length);
                var img2Size = Math.Min(textWidth, _imageHeight);
                var fontStyleArr = Enum.GetValues(typeof(FontStyle));
                for (int i = 0; i < text.Length; i++)
                {
                    using Image<Rgba32> img2 = new(img2Size, img2Size);
                    var scaledFont = _fontList[_random.Next(0, _fontList.Count)];
                    var colorHex = _colorHexArr[_random.Next(0, _colorHexArr.Length)];
                    img2.Mutate(ctx => {
                        ctx.DrawText(text[i].ToString(), scaledFont, colorHex, new Point(0, 0));
                        DrawingGrid(ctx, _imageWidth, _imageHeight, colorHex, 6, 1);
                        ctx.Rotate(_random.Next(-45, 45));
                    });
                    var point = new Point(i * textWidth, (containerHeight - img2Size) / 2);
                    processingContext.DrawImage(img2, point, 1);
                }
            }
        }
        public void DrawingGrid(IImageProcessingContext processingContext, int containerWidth, int containerHeight, Rgba32 color, int count, float thickness)
        {
            var points = new List<PointF> { new PointF(0, 0) };
            for (int i = 0; i < count; i++)
            {
                getCirclePoginF(containerWidth, containerHeight, 9, ref points);
            }
            points.Add(new PointF(containerWidth, containerHeight));
            processingContext.DrawLines(color, thickness, points.ToArray());
        }
        private PointF getCirclePoginF(int containerWidth, int containerHeight, double lapR, ref List<PointF> list)
        {
            Random random = _random;
            PointF newPoint = new PointF();
            int retryTimes = 10;
            double tempDistance = 0;

            do
            {
                newPoint.X = random.Next(0, containerWidth);
                newPoint.Y = random.Next(0, containerHeight);
                bool tooClose = false;
                foreach (var p in list)
                {
                    tooClose = false;
                    tempDistance = Math.Sqrt((Math.Pow((p.X - newPoint.X), 2) + Math.Pow((p.Y - newPoint.Y), 2)));
                    if (tempDistance < lapR)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose == false)
                {
                    list.Add(newPoint);
                    break;
                }
            }
            while (retryTimes-- > 0);

            if (retryTimes <= 0)
            {
                list.Add(newPoint);
            }
            return newPoint;
        }
    }
}