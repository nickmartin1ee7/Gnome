using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Gnome
{
    public partial class MainWindow : Window
    {
        private const double GNOMED_CHANCE = 1;

        private TaskbarIcon _tb;
        private SoundPlayer _soundPlayer;
        private Timer _gnomeTimer;
        private Random _rng = new();

        public MainWindow()
        {
            InitializeComponent();

            mainWindow.Visibility = Visibility.Hidden;

            SetupSoundPlayer();
            SetupGnomeTrayIcon();
            SetupGnomeTimer();

            _ = GetGnomedAsync().ConfigureAwait(false); // Get gnomed on
        }

        private async Task GetGnomedAsync()
        {
            var mousePos = Control.MousePosition;
            mainWindow.Left = mousePos.X - (mainWindow.Width / 2);
            mainWindow.Top = mousePos.Y - (mainWindow.Height / 2);
            mainWindow.Visibility = Visibility.Visible;

            _soundPlayer.Play();
            await Task.Delay(300);

            mainWindow.Visibility = Visibility.Hidden;
        }

        private Stream GetResourceStreamByName(string name)
        {
            var asm = GetType().Assembly;
            var resourceNames = asm.GetManifestResourceNames();
            var gnomeResName = resourceNames.First(r => r.Contains(name));
            return asm.GetManifestResourceStream(gnomeResName);
        }

        private void SetupGnomeTrayIcon()
        {
            _tb = new();
            _tb.Icon = new Icon(GetResourceStreamByName("gnome.ico"));
            _tb.ToolTipText = $"Status: Currently getting gnomed on{Environment.NewLine}Left Click the gnome for a surprise{Environment.NewLine}Right Click the gnome to stop";
            _tb.TrayLeftMouseDown += TrayIcon_LeftMouseDown;
            _tb.TrayRightMouseDown += TrayIcon_RightMouseDown;
        }

        private void SetupSoundPlayer()
        {
            _soundPlayer = new(GetResourceStreamByName("gnome.wav"));
        }

        private void SetupGnomeTimer()
        {
            _gnomeTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _gnomeTimer.Elapsed += Timer_RandomGnomeAsync;
            _gnomeTimer.Start();
        }

        private async void TrayIcon_LeftMouseDown(object sender, RoutedEventArgs e)
        {
            await GetGnomedAsync();
        }

        private void TrayIcon_RightMouseDown(object sender, RoutedEventArgs e)
        {
            _gnomeTimer.Stop();
            _soundPlayer.Stop();

            Environment.Exit(0);
        }

        private async void Timer_RandomGnomeAsync(object? sender, ElapsedEventArgs e)
        {
            if (_rng.NextDouble() > GNOMED_CHANCE)
                return;

            await Dispatcher.InvokeAsync(GetGnomedAsync);
        }
    }
}
