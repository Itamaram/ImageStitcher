using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageStitcher
{
    public partial class Form1 : Form
    {
        private readonly ContainersCollection containers = new ContainersCollection();

        public Form1()
        {
            InitializeComponent();

            textBox1.Text = Configuration.Get.Output.TextboxDefaults.Line1;
            textBox2.Text = Configuration.Get.Output.TextboxDefaults.Line2;
            textBox3.Text = Configuration.Get.Output.TextboxDefaults.Line3;
            textBox4.Text = Configuration.Get.Output.TextboxDefaults.Line4;

            foreach (var (container, point) in containers.Zip(CellCoordinates()))
                tableLayoutPanel1.Controls.Add(container, point.X, point.Y);

            Shown += toolStripMenuItem1_Click;
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
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            foreach (var (f, c) in dialog.FileNames.Zip(containers))
                c.SetImage(f);

            GetParentDirectory()
                ?.Name.Do(name => textBox2.Text = name);
        }

        private void stitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var loading = new ProgressForm(10);
            var progress = new Progress<int>(loading.Increment);
            Task.Run(() => { Save(progress); });
            loading.ShowDialog();
        }

        private DirectoryInfo GetParentDirectory()
        {
            return containers.Select(c => c.ImageLocation)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => new DirectoryInfo(s).Parent)
                .FirstOrDefault();
        }

        private void Save(IProgress<int> progress)
        {
            var config = Configuration.Get;
            int width = config.Image.Width,
                height = config.Image.Height,
                padding = config.Image.Padding,
                border = config.Image.Border;

            var canvas = new Bitmap(2 * (border + padding) + 3 * width, 2 * (border + padding) + 3 * height);
            using (var g = Graphics.FromImage(canvas))
            {
                g.FillRectangle(Brushes.White, 0, 0, canvas.Width, canvas.Height);

                foreach (var (point, location) in CellCoordinates()
                    .Select(p => new Point(p.X * (width + padding) + border, p.Y * (height + padding) + border))
                    .Zip(containers, (p, c) => (Point: p, Path: c.ImageLocation))
                    .Where(t => t.Path != null && File.Exists(t.Path)))
                {
                    g.DrawImage(Image.FromFile(location), point.ToRectangle(width, height));
                    progress.Report(1);
                }

                Enumerable.Range(0, 4)
                    .Select(i => new Rectangle(width + padding + border, height + padding + border + i * height / 4, width, height / 4))
                    .Zip(new[] { textBox1, textBox2, textBox3, textBox4 }.Select(b => b.Text))
                    .ForEach((rect, text) => g.DrawString(text, config.Image.Font.ToFont(), Brushes.Black, rect,
                        new StringFormat
                        {
                            LineAlignment = StringAlignment.Center,
                            Alignment = StringAlignment.Center
                        }));

                progress.Report(1);
            }
            
            var path = GetParentDirectory()
                .Map(d => Path.Combine(d.FullName, $"{textBox2.Text}-landscape-{DateTime.Now:ddMMyyyy}.jpg"));

            CompressAndSave(canvas, config.Output.Compression, path);

            Process.Start(path);
            progress.Report(10);
        }

        private static void CompressAndSave(Image source, int quality, string path)
        {
            var encoder = ImageCodecInfo.GetImageDecoders().First(f => f.FormatID == ImageFormat.Jpeg.Guid);

            var @params = new EncoderParameters
            {
                Param = new[] { new EncoderParameter(Encoder.Quality, quality) }
            };

            source.Save(path, encoder, @params);
        }
    }

    public class ContainersCollection : IEnumerable<ImageContainer>
    {
        private readonly IReadOnlyList<ImageContainer> containers;

        public ContainersCollection()
        {
            containers = Enumerable.Range(0, 8)
                .Select(i => new ImageContainer(this, i) { Dock = DockStyle.Fill })
                .ToList();
        }

        public IEnumerator<ImageContainer> GetEnumerator() => containers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Swap(int a, int b)
        {
            var temp = containers[a].ImageLocation;
            containers[a].SetImage(containers[b].ImageLocation);
            containers[b].SetImage(temp);
        }
    }
}
