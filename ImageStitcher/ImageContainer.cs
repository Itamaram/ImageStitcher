using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ImageStitcher
{
    public class ImageContainer : Panel
    {
        private static ImageContainer drag;

        public ImageContainer()
        {
            Controls.Add(remove, image, add);
            remove.Location = new Point(Size.Width - remove.Width - 3, 3);
            add.Click += AddClick;
            add.DragEnter += (_, e) => e.Effect = DragDropEffects.Move;
            add.DragDrop += (sender, args) =>
            {
                SetImage(drag.image.ImageLocation);
                drag.RemoveImage();

            };
            image.DragEnter += (_, e) => e.Effect = DragDropEffects.Move;
            image.DragDrop += (sender, args) =>
            {
                drag.image.ImageLocation = image.ImageLocation;
                image.ImageLocation = args.Data.GetData(DataFormats.Text).ToString();
            };
            remove.Click += RemoveClick;
            image.MouseDown += (_, __) =>
            {
                drag = this;
                DoDragDrop(image.ImageLocation, DragDropEffects.All);
            };
        }

        private readonly Button add = new Button
        {
            Dock = DockStyle.Fill,
            Location = new Point(0, 0),
            Text = "+",
            AllowDrop = true
        };

        private readonly Button remove = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(18, 23),
            Text = "X",
            Visible = false,
        };

        private readonly PictureBox image = new PictureBox
        {
            Dock = DockStyle.Fill,
            Location = new Point(0, 0),
            SizeMode = PictureBoxSizeMode.StretchImage,
            Visible = false,
            AllowDrop = true
        };

        private void RemoveClick(object sender, EventArgs e)
        {
            RemoveImage();
        }

        private void AddClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                SetImage(dialog.FileName);
        }

        private void RemoveImage()
        {
            image.Hide();
            remove.Hide();
        }

        public ImageContainer SetImage(string path)
        {
            image.ImageLocation = path;

            //this show ordering is important
            remove.Show();
            image.Show();

            return this;
        }

        public string ImageLocation => image.ImageLocation;
    }

    public static class Extensions
    {
        public static Control.ControlCollection Add(this Control.ControlCollection collection, params Control[] items)
        {
            collection.AddRange(items);
            return collection;
        }

        public static void ForceEnumeration<A>(this IEnumerable<A> items)
        {
            foreach (var item in items) ;
        }

        public static void ForEach<A>(this IEnumerable<A> items, Action<A> action)
        {
            foreach (var item in items)
                action(item);
        }

        public static Rectangle ToRectangle(this Point p, int width, int height)
        {
            return new Rectangle(p.X, p.Y, width, height);
        }

        private static readonly TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));

        public static Font ToFont(this string s)
        {
            return (Font) converter.ConvertFromString(s);
        }
    }
}