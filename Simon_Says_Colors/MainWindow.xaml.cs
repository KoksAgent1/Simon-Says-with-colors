using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Simon_Says_Colors
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<Button> colorButtons;
        private List<SolidColorBrush> originalColors;
        private List<SolidColorBrush> darkColors;
        private List<SolidColorBrush> lightColors;
        private List<int> sequence;
        private List<int> userSequence;
        private Random random;
        private DispatcherTimer timer;
        private int displayIndex;
        private int currentScore;
        private int highScore;
        private int round;
        private bool gamerunning;
        private bool gameover;

        public MainWindow()
        {
            InitializeComponent();
            colorButtons = new List<Button> { RedButton, YellowButton, GreenButton, BlueButton };
            originalColors = new List<SolidColorBrush>
            {
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000")), // Red
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF00")), // Yellow
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008000")), // Green
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0000FF"))  // Blue
            };
            darkColors = new List<SolidColorBrush>
            {
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B0000")), // Dark Red
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B870C")), // Dark Yellow
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#006400")), // Dark Green
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00008B"))  // Dark Blue
            };
            lightColors = new List<SolidColorBrush>
            {
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA07A")), // Light Red
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFE0")), // Light Yellow
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90EE90")), // Light Green
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ADD8E6"))  // Light Blue
            };
            sequence = new List<int>();
            userSequence = new List<int>();
            random = new Random();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += Timer_Tick;
            currentScore = 0;
            highScore = 0;
            round = 0;
            ApplyDarkMode();
            gamerunning = false;

        }

        private void DarkModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplyLightMode();
        }

        private void DarkModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyDarkMode();
        }

        private void ApplyLightMode()
        {
            var lightMode = new ResourceDictionary { Source = new Uri("pack://application:,,,/LightMode.xaml") };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(lightMode);
        }

        private void ApplyDarkMode()
        {
            var darkMode = new ResourceDictionary { Source = new Uri("pack://application:,,,/DarkMode.xaml") };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(darkMode);
        }

        private void SetDifficulty()
        {
            var selectedDifficulty = (DifficultyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            switch (selectedDifficulty)
            {
                case "Einfach":
                    timer.Interval = TimeSpan.FromSeconds(1);
                    break;
                case "Mittel":
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    break;
                case "Schwer":
                    timer.Interval = TimeSpan.FromSeconds(0.25);
                    break;
            }
        }



        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if(gamerunning) return;
            SetDifficulty();
            sequence.Clear();
            userSequence.Clear();
            currentScore = 0;
            round = 1;
            UpdateScores();
            AddToSequence();
            gamerunning = true;
            if ((bool)FullSequenceRadioButton.IsChecked)
            {
                await DisplayFullSequence();
            }
            else
            {
                await DisplayLatestOnly();
            }
        }


        private void AddToSequence()
        {
            int nextColorIndex = random.Next(colorButtons.Count);
            sequence.Add(nextColorIndex);
        }


        private async Task DisplayFullSequence()
        {
            if(round == 1)
            {
                StatusTextBlock.Text = "Schau dir die Sequenz an!";
                StatusTextBlockMaskup.Text = "Schau dir die Sequenz an!";
            }
            else
            {
                StatusTextBlock.Text = "Korrekt. Schau dir die Sequenz an!";
                StatusTextBlockMaskup.Text = "Korrekt. Schau dir die Sequenz an!";
            }
            displayIndex = 0;

            while (displayIndex < sequence.Count)
            {
                int colorIndex = sequence[displayIndex];
                Button colorButton = colorButtons[colorIndex];
                ChangeButtonColor(colorButton, colorIndex, true);
                await Task.Delay(timer.Interval);
                ChangeButtonColor(colorButton, colorIndex, false);
                await Task.Delay(timer.Interval);

                displayIndex++;
            }

            StatusTextBlock.Text = "Du bist dran";
            StatusTextBlockMaskup.Text = "Du bist dran";
        }

        private async Task DisplayLatestOnly()
        {
            StatusTextBlock.Text = "Dieses Feld wurde hinzugefügt";
            StatusTextBlockMaskup.Text = "Dieses Feld wurde hinzugefügt";
            int colorIndex = sequence[sequence.Count - 1];
            Button colorButton = colorButtons[colorIndex];
            ChangeButtonColor(colorButton, colorIndex, true);
            await Task.Delay(500);
            ChangeButtonColor(colorButton, colorIndex, false);
            await Task.Delay(500);

            StatusTextBlock.Text = "Du bist dran";
            StatusTextBlockMaskup.Text = "Du bist dran";
        }

        private void ChangeButtonColor(Button button, int index, bool isActive)
        {
            if (isActive)
            {
                if ((bool)LightRadioButton.IsChecked)
                {
                    button.Background = lightColors[index];
                }
                else
                {
                    button.Background = darkColors[index];
                }
            }
            else
            {
                button.Background = originalColors[index];
            }
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (displayIndex < sequence.Count)
            {
                int colorIndex = sequence[displayIndex];
                Button colorButton = colorButtons[colorIndex];
                colorButton.Background = darkColors[colorIndex];
                Dispatcher.InvokeAsync(() =>
                {
                    System.Threading.Thread.Sleep(300);
                    colorButton.Background = originalColors[colorIndex];
                });
                displayIndex++;
            }
            else
            {
                timer.Stop();
                if (gamerunning)
                {
                    StatusTextBlock.Text = "Du bist dran";
                    StatusTextBlockMaskup.Text = "Du bist dran";
                }
            }
        }


        private async void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (!gamerunning)
            {
                return;
            }
            Button clickedButton = sender as Button;
            int clickedIndex = colorButtons.IndexOf(clickedButton);
            userSequence.Add(clickedIndex);

            // Klick-Feedback-Animation
            var originalColor = ((SolidColorBrush)clickedButton.Background).Color;
            var highlightColor = ((bool)LightRadioButton.IsChecked) ? lightColors[clickedIndex].Color : darkColors[clickedIndex].Color;

            var animation = new ColorAnimation
            {
                From = originalColor,
                To = highlightColor,
                Duration = TimeSpan.FromSeconds(0.1),
                AutoReverse = true
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, clickedButton);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Button.Background).(SolidColorBrush.Color)"));
            storyboard.Begin();

            if (userSequence[userSequence.Count - 1] != sequence[userSequence.Count - 1])
            {
                gamerunning = false;
                await DisplayCompleteSequenceOnGameOver(); // Zeige die gesamte korrekte Sequenz
                if (currentScore > highScore)
                {
                    highScore = currentScore;
                    UpdateScores();
                }
                return;
            }

            if (userSequence.Count == sequence.Count)
            {
                userSequence.Clear();
                StatusTextBlock.Text = "";
                currentScore++;
                UpdateScores();
                AddToSequence();
                if ((bool)FullSequenceRadioButton.IsChecked)
                {
                    DisplayFullSequence();
                }
                else
                {
                    DisplayLatestOnly();
                }
            }
        }

        private async Task DisplayCompleteSequenceOnGameOver()
        {
            StatusTextBlock.Text = "Game Over!";
            StatusTextBlockMaskup.Text = "Game Over! Das war die Korrekte Abfolge";
            foreach (var colorIndex in sequence)
            {
                Button colorButton = colorButtons[colorIndex];
                ChangeButtonColor(colorButton, colorIndex, true);
                await Task.Delay(timer.Interval);
                ChangeButtonColor(colorButton, colorIndex, false);
                await Task.Delay(timer.Interval);
            }
            StatusTextBlock.Text = "Game Over!";
            StatusTextBlockMaskup.Text = "Game Over!";
        }

        private void UpdateScores()
        {
            CurrentScoreTextBlock.Text = currentScore.ToString();
            HighScoreTextBlock.Text = highScore.ToString();
        }

    }
}
