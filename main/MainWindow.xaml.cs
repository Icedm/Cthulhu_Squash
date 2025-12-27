using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CthulhuSquish;

public partial class MainWindow : Window
{
    int score = 0;
    Random rand = new Random();
    double imageWidth = 75;
    double imageHeight = 75;
    DispatcherTimer movementTimer;
    DispatcherTimer countdownTimer;
    MediaPlayer player = new MediaPlayer();
    bool gameStarted = false;

    // continuous movement / bouncing
    double vx = 0; // pixels per second
    double vy = 0;
    double speed = 120; // base speed in px/s
    DateTime lastTick;
    int timeLeft = 30;
    int round = 1;
    double baseSpeed;
    DispatcherTimer? round2BannerTimer;
    bool timeWarningActive = false;

    public MainWindow()
    {
        InitializeComponent();

        // Load image if present
        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "cthulhu.png");
        if (File.Exists(imagePath))
        {
            CthulhuImage.Source = new BitmapImage(new Uri(imagePath));
        }

        // Load sound if present
        var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Raw", "Slime-squish-sound-effect.mp3");
        if (File.Exists(soundPath))
        {
            player.Open(new Uri(soundPath));
        }

        baseSpeed = speed;

        CthulhuImage.MouseLeftButtonDown += OnImageClicked;

        Loaded += OnLoaded;

        StartButton.Click += StartButton_Click;
        StartRound2Button.Click += StartRound2Button_Click;

        movementTimer = new DispatcherTimer();
        movementTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
        movementTimer.Tick += MovementTimer_Tick;
        // don't start until Start button clicked
        
        countdownTimer = new DispatcherTimer();
        countdownTimer.Interval = TimeSpan.FromSeconds(1);
        countdownTimer.Tick += CountdownTimer_Tick;

        RestartButton.Click += RestartButton_Click;
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
        CenterSprite();
    }

    void MovementTimer_Tick(object? sender, EventArgs e)
    {
        if (!gameStarted) return;
        var now = DateTime.UtcNow;
        double dt = (now - lastTick).TotalSeconds;
        if (dt <= 0) return;

        double x = Canvas.GetLeft(CthulhuImage);
        double y = Canvas.GetTop(CthulhuImage);

        x += vx * dt;
        y += vy * dt;

        double maxX = Math.Max(0, GameCanvas.ActualWidth - imageWidth);
        double maxY = Math.Max(0, GameCanvas.ActualHeight - imageHeight);

        // bounce on X
        if (x <= 0)
        {
            x = 0;
            vx = Math.Abs(vx);
        }
        else if (x >= maxX)
        {
            x = maxX;
            vx = -Math.Abs(vx);
        }

        // bounce on Y
        if (y <= 0)
        {
            y = 0;
            vy = Math.Abs(vy);
        }
        else if (y >= maxY)
        {
            y = maxY;
            vy = -Math.Abs(vy);
        }

        Canvas.SetLeft(CthulhuImage, x);
        Canvas.SetTop(CthulhuImage, y);

        lastTick = now;
    }

    void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        if (!gameStarted) return;

        timeLeft--;
        TimeText.Text = timeLeft.ToString();

        if (timeLeft <= 0)
        {
            EndGame();
            return;
        }

        // advance to round 2 if player reached 20 within time
        if (round == 1 && score >= 20)
        {
            StartRound2();
        }

        if (timeLeft <= 5 && !timeWarningActive)
        {
            TriggerTimeWarning();
        }
    }

    void StartRound2()
    {
        // pause timers and show overlay to let player start round 2
        movementTimer.Stop();
        countdownTimer.Stop();
        Round2Overlay.Visibility = Visibility.Visible;
        CthulhuImage.Visibility = Visibility.Hidden;
    }

    void StartRound2Button_Click(object? sender, RoutedEventArgs e)
    {
        Round2Overlay.Visibility = Visibility.Collapsed;
        round = 2;
        // increase speed and switch image
        speed = Math.Round(speed * 1.6);
        var altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "cthulhu_alt.png");
        if (File.Exists(altPath))
        {
            CthulhuImage.Source = new BitmapImage(new Uri(altPath));
        }

        // reset timer for round 2 and resume
        timeLeft = 30;
        TimeText.Text = timeLeft.ToString();
        CthulhuImage.Visibility = Visibility.Visible;
        lastTick = DateTime.UtcNow;
        movementTimer.Start();
        countdownTimer.Start();
    }

    void EndGame()
    {
        gameStarted = false;
        movementTimer.Stop();
        countdownTimer.Stop();
        FinalScoreText.Text = $"Final Score: {score}";
        GameOverOverlay.Visibility = Visibility.Visible;
        CthulhuImage.IsEnabled = false;
    }

    void RestartButton_Click(object? sender, RoutedEventArgs e)
    {
        // reset everything and show Start overlay
        GameOverOverlay.Visibility = Visibility.Collapsed;
        StartOverlay.Visibility = Visibility.Visible;
        CthulhuImage.Visibility = Visibility.Hidden;
        CthulhuImage.IsEnabled = true;
        gameStarted = false;
        round = 1;
        speed = baseSpeed;
        var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "cthulhu.png");
        if (File.Exists(defaultPath)) CthulhuImage.Source = new BitmapImage(new Uri(defaultPath));
        timeLeft = 30;
        TimeText.Text = timeLeft.ToString();
        score = 0;
        ScoreText.Text = "0";
        countdownTimer.Stop();
    }

    void TriggerScorePulse()
    {
        // animate ScaleX/ScaleY on ScoreText
        var scale = ScoreText.RenderTransform as ScaleTransform;
        if (scale == null) return;

        var anim = new DoubleAnimation(1.0, 1.25, TimeSpan.FromMilliseconds(140)) { AutoReverse = true, EasingFunction = new QuadraticEase() };
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
    }

    void TriggerTimeWarning()
    {
        timeWarningActive = true;
        // ensure Foreground is a SolidColorBrush we can animate
        Color baseColor;
        if (TimeText.Foreground is SolidColorBrush existingBrush)
            baseColor = existingBrush.Color;
        else
        {
            var baseBrush = FindResource("AccentBrush") as SolidColorBrush;
            baseColor = baseBrush?.Color ?? Colors.LightGreen;
        }

        // create a fresh brush instance (unfrozen) so animations can run
        var animBrush = new SolidColorBrush(baseColor);
        TimeText.Foreground = animBrush;

        var colorAnim = new ColorAnimation(Colors.OrangeRed, TimeSpan.FromMilliseconds(300)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(6) };
        colorAnim.Completed += (s, e) => { timeWarningActive = false; };
        animBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

        // also a subtle scale pulse
        var scale = TimeText.RenderTransform as ScaleTransform;
        if (scale != null)
        {
            var anim = new DoubleAnimation(1.0, 1.15, TimeSpan.FromMilliseconds(200)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(6) };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }
    }

    void OnImageClicked(object sender, MouseButtonEventArgs e)
    {
        if (!gameStarted) return;

        score++;
        ScoreText.Text = score.ToString();
        PlaySquish();
        // jump to a new random location and change movement direction
        MoveSpriteRandomly();
        SetRandomDirection();
        // reset timestep to avoid large delta after teleport
        lastTick = DateTime.UtcNow;
        // pulse the score briefly
        TriggerScorePulse();
    }

    void SetRandomDirection()
    {
        double angle = rand.NextDouble() * Math.PI * 2;
        vx = Math.Cos(angle) * speed;
        vy = Math.Sin(angle) * speed;
    }

    void PlaySquish()
    {
        try
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }
        catch { }
    }

    void CenterSprite()
    {
        double x = Math.Max(0, (GameCanvas.ActualWidth - imageWidth) / 2);
        double y = Math.Max(0, (GameCanvas.ActualHeight - imageHeight) / 2);
        Canvas.SetLeft(CthulhuImage, x);
        Canvas.SetTop(CthulhuImage, y);
    }

    void MoveSpriteRandomly()
    {
        if (GameCanvas.ActualWidth <= 0 || GameCanvas.ActualHeight <= 0)
            return;

        double maxX = Math.Max(0, GameCanvas.ActualWidth - imageWidth);
        double maxY = Math.Max(0, GameCanvas.ActualHeight - imageHeight);
        double x = rand.NextDouble() * maxX;
        double y = rand.NextDouble() * maxY;
        Canvas.SetLeft(CthulhuImage, x);
        Canvas.SetTop(CthulhuImage, y);
    }

    void StartButton_Click(object sender, RoutedEventArgs e)
    {
        StartOverlay.Visibility = Visibility.Collapsed;
        CthulhuImage.Visibility = Visibility.Visible;
        gameStarted = true;
        score = 0;
        ScoreText.Text = "0";
        CenterSprite();

        // initialize random velocity
        double angle = rand.NextDouble() * Math.PI * 2;
        vx = Math.Cos(angle) * speed;
        vy = Math.Sin(angle) * speed;

        lastTick = DateTime.UtcNow;
        movementTimer.Start();
        // start countdown
        timeLeft = 30;
        TimeText.Text = timeLeft.ToString();
        countdownTimer.Start();
    }
}
