using System;
using System.Drawing;
using System.Windows.Forms;

namespace EposBridge;

public class SplashForm : Form
{
    public SplashForm()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Size = new Size(400, 250);
        this.BackColor = Color.White;
        this.ShowInTaskbar = false;
        
        // Use embedded icon if available, otherwise catch exception
        try {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("EposBridge.ico.ico");
            if (stream != null) this.Icon = new Icon(stream);
        } catch {}

        var panel = new Panel { Dock = DockStyle.Fill,  BorderStyle = BorderStyle.FixedSingle };
        
        var title = new Label
        {
            Text = "EPOS Bridge",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 120, 212),
            AutoSize = true,
            Location = new Point(50, 80)
        };
        
        var subtitle = new Label
        {
            Text = "Iniciando servicios...",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(50, 130)
        };

        var progressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            Width = 300,
            Height = 4,
            Location = new Point(50, 170)
        };

        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        panel.Controls.Add(progressBar);
        this.Controls.Add(panel);
    }
}
