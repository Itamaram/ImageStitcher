using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ImageStitcher
{
    public interface DragDropHandler
    {
        bool CanHandle(IDataObject data);
        void Handle(IDataObject data);
    }

    public abstract class DragDropHandler<T> : DragDropHandler
    {
        public bool CanHandle(IDataObject data)
        {
            return data.GetDataPresent(typeof(T)) && CanHandle((T)data.GetData(typeof(T)));
        }

        protected abstract bool CanHandle(T data);

        public void Handle(IDataObject data) => Handle((T)data.GetData(typeof(T)));

        protected abstract void Handle(T item);
    }

    public abstract class DataFormatDragDropHandler<T> : DragDropHandler
    {
        protected abstract string DataFormat { get; }

        public bool CanHandle(IDataObject data)
        {
            return data.GetDataPresent(DataFormat) && CanHandle((T) data.GetData(DataFormat));
        }

        protected abstract bool CanHandle(T data);

        public void Handle(IDataObject data)
        {
            Handle((T)data.GetData(DataFormat));
        }

        protected abstract void Handle(T item);
    }

    public class InternalDragDrop : DragDropHandler<int>
    {
        private readonly ImageContainer container;

        public InternalDragDrop(ImageContainer container)
        {
            this.container = container;
        }

        protected override bool CanHandle(int data) => true;

        protected override void Handle(int item) => container.Swap(item);
    }

    public class ExternalDragDrop : DataFormatDragDropHandler<string[]>
    {
        private readonly ImageContainer container;

        public ExternalDragDrop(ImageContainer container)
        {
            this.container = container;
        }

        protected override string DataFormat { get; } = DataFormats.FileDrop;

        protected override bool CanHandle(string[] data) => true;

        protected override void Handle(string[] item) => container.SetImage(item[0]);
    }

    public class ImageContainer : Panel
    {
        private readonly ContainersCollection parent;
        private readonly int id;

        public ImageContainer(ContainersCollection parent, int id)
        {
            this.parent = parent;
            this.id = id;

            handlers = new DragDropHandler[]
            {
                new InternalDragDrop(this),
                new ExternalDragDrop(this),
            };

            Controls.Add(remove, image, add);
            remove.Location = new Point(Size.Width - remove.Width - 3, 3);

            add.Do(RegisterDragDrop);
            image.Do(RegisterDragDrop);

            remove.Click += RemoveClick;

            add.Click += AddClick;

            image.MouseDown += (_, __) => DoDragDrop(id, DragDropEffects.All);
        }

        private void RegisterDragDrop(Control control)
        {
            control.DragEnter += (_, e) => e.Effect = DragDropEffects.All;
            control.DragDrop += (sender, args) =>
                handlers.FirstOrDefault(h => h.CanHandle(args.Data))?.Handle(args.Data);
        }

        private readonly IEnumerable<DragDropHandler> handlers;

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
            image.ImageLocation = null;
            image.Hide();
            remove.Hide();
        }

        public ImageContainer SetImage(string path)
        {
            if (path == null)
            {
                RemoveImage();
                return this;
            }

            image.ImageLocation = path;

            //this show ordering is important
            remove.Show();
            image.Show();

            return this;
        }

        public string ImageLocation => image.ImageLocation;

        public void Swap(int i) => parent.Swap(id, i);
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
            foreach (var _ in items) ;
        }

        public static void ForEach<A>(this IEnumerable<A> items, Action<A> action)
        {
            foreach (var item in items)
                action(item);
        }

        public static void ForEach<A, B>(this IEnumerable<(A, B)> items, Action<A, B> action)
        {
            foreach (var item in items)
                action(item.Item1, item.Item2);
        }

        public static Rectangle ToRectangle(this Point p, int width, int height)
        {
            return new Rectangle(p.X, p.Y, width, height);
        }

        private static readonly TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));

        public static Font ToFont(this string s)
        {
            return (Font)converter.ConvertFromString(s);
        }

        public static IEnumerable<(A, B)> Zip<A, B>(this IEnumerable<A> first, IEnumerable<B> second)
        {
            return first.Zip(second, (f, s) => (f, s));
        }

        public static A Do<A>(this A a, Action<A> action)
        {
            action(a);
            return a;
        }

        public static (A, B) Do<A, B>(this (A, B) t, Action<A, B> action)
        {
            action(t.Item1, t.Item2);
            return t;
        }

        public static B Map<A, B>(this A a, Func<A, B> map) => map(a);

        public static C Map<A, B, C>(this (A, B) t, Func<A, B, C> map) => map(t.Item1, t.Item2);
    }
}