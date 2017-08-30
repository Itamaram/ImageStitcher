using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImageStitcher
{
    public class Progress : Form
    {
        private readonly ProgressBar progress;

        public Progress(int max)
        {
            FormBorderStyle = FormBorderStyle.None;

            Size = new Size(102, 25);



            progress = new ProgressBar
            {
                Maximum = max,
                Location = new Point(1, 1)
            };

            Controls.Add(progress);
        }

        public void Increment(int value) => progress.Increment(value);

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CenterToParent();
        }
    }

    public partial class Form1 : Form
    {
        private readonly IReadOnlyList<ImageContainer> containers;

        public Form1()
        {
            InitializeComponent();

            containers = Enumerable.Range(0, 8).Select(i => new ImageContainer { Dock = DockStyle.Fill })
                .Zip(CellCoordinates(), (c, p) =>
                {
                    tableLayoutPanel1.Controls.Add(c, p.X, p.Y);
                    return c;
                }).ToList();
        }

        private static IEnumerable<Point> CellCoordinates()
        {
            return Enumerable.Range(0, 9)
                .Where(i => i != 4)
                .Select(i => new Point(i % 3, i / 3));
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog { Multiselect = true };
            if (dialog.ShowDialog() == DialogResult.OK)
                dialog.FileNames.Zip(containers, (f, c) => c.SetImage(f)).ForceEnumeration();
        }

        private void stitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var progress = new Progress(100))
            {
                progress.Show(this);

                var config = Configuration.Get;
                int width = config.Image.Width,
                    height = config.Image.Height,
                    padding = config.Image.Padding,
                    border = config.Image.Border;

                var canvas = new Bitmap(2 * (border + padding) + 3 * width, 2 * (border + padding) + 3 * height);
                using (var g = Graphics.FromImage(canvas))
                {
                    g.FillRectangle(Brushes.White, 0, 0, canvas.Width, canvas.Height);

                    foreach (var t in CellCoordinates()
                        .Select(p => new Point(p.X * (width + padding) + border, p.Y * (height + padding) + border))
                        .Zip(containers, (p, c) => (Point: p, Path: c.ImageLocation))
                        .Where(t => t.Path != null && File.Exists(t.Path)))
                    {
                        g.DrawImage(Image.FromFile(t.Path), t.Point.ToRectangle(width, height));
                        progress.Increment(10);
                    }

                    new[] { textBox1, textBox2, textBox3, textBox4 }
                        .Select((box, i) => (Text: box.Text, Index: i))
                        .ForEach(t => g.DrawString(t.Text, "Arial, 50pt".ToFont(), Brushes.Black,
                            new Rectangle(width + padding + border, height + padding + border + t.Index * height / 4, width, height / 4), new StringFormat
                            {
                                LineAlignment = StringAlignment.Center,
                                Alignment = StringAlignment.Center
                            }));
                    progress.Increment(10);
                }

                var dir = Path.Combine(Application.StartupPath, "Output");
                var filename = DateTime.Now.ToString("ddMMyyyy-HHmmss") + ".jpg";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var path = Path.Combine(dir, filename);

                CompressAndSsave(canvas, 80, path);
                
                Process.Start(path);
                progress.Increment(10);
            }
        }

        private static void CompressAndSsave(Image source, int quality, string path)
        {
            var encoder = ImageCodecInfo.GetImageDecoders().First(f => f.FormatID == ImageFormat.Jpeg.Guid);

            var @params = new EncoderParameters
            {
                Param = new[] { new EncoderParameter(Encoder.Quality, quality) }
            };

            source.Save(path, encoder, @params);  
        }
    }
}
