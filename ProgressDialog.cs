using System.Windows.Forms;

namespace WoWSimsApp
{
	public class ProgressDialog : Form
	{
		private ProgressBar progressBar;

		public ProgressDialog()
		{
			this.Text = "Downloading...";
			this.Width = 400;
			this.Height = 80;
			this.CenterToScreen();

			// Set the border style to FixedSingle to prevent resizing
			this.FormBorderStyle = FormBorderStyle.FixedSingle;

			// Disable Minimize and Maximize buttons
			this.MaximizeBox = false;
			this.MinimizeBox = false;

			// Disable the Close button by overriding the ControlBox property
			this.ControlBox = false;

			progressBar = new ProgressBar
			{
				Dock = DockStyle.Fill,
				Minimum = 0,
				Maximum = 100
			};

			this.Controls.Add(progressBar);
		}

		public void UpdateProgress(int percentage)
		{
			progressBar.Value = percentage;
		}
	}
}