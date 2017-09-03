using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImageStitcher
{
    public class ProgressForm : Form
    {
        private readonly ProgressBar progress;

        public ProgressForm(int max)
        {
            FormBorderStyle = FormBorderStyle.None;

            progress = new ProgressBar
            {
                Maximum = max,
                Size = new Size(100, 50),
                Location = new Point(1, 1)
            };

            Controls.Add(progress);

            Size = new Size(progress.Width + 2, progress.Height + 2);
        }

        public void Increment(int value)
        {
            progress.Increment(value);
            if (progress.Value >= progress.Maximum)
                Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CenterToParent();
        }
    }
}