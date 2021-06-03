using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Bedna
{
    public partial class MainWindow : Window
    {
        public ImageData ImageSources { get; private set; }
        public MediaData MediaManager { get; private set; }
        private AppState CurrentValues { get; set; }

        public MainWindow(ImageData imageSources, MediaData mediaSources)
        {
            InitializeComponent();
            ImageSources = imageSources;
            MediaManager = mediaSources;
            MediaManager.Player = MediaElement;
            CurrentValues = new AppState();

            fe_DropDown.SelectionChanged += this.Dropdown_selcted;
        }

        private void hideHint()
        {
            fe_Hint.Visibility = Visibility.Hidden;
        }
        private void showHint()
        {
            fe_Hint.Visibility = Visibility.Visible;
        }

        #region Value formating
        private string PrevieHotovost(int _value)
        {
            string value = _value.ToString();

            string format = "";
            format = value.ToCharArray().Length switch
            {
                0 => format = "0000",
                1 => format = $"000{value}",
                2 => format = $"00{value}",
                3 => format = $"0{value}",
                4 => format = $"{value}",

                _ => format = "0000"
            };
            return format;
        }

        private string PreviewText(int unformated)
        {
            unformated = unformated >= 0 ? unformated : 0;
            return unformated + "  -Kc";
        }

        private int PreviewCost(string unformated)
        {
            int result = 0;
            string[] s = unformated.ToString().Split(" ");
            result = int.Parse(fe_DropDown.SelectedItem.ToString().Split(" ")[s.Length-2]);
            return result;
        }

        private void UpdateState()
        {
            CurrentValues.Cost = PreviewCost(fe_DropDown.SelectedItem.ToString());
            fe_Change.Text = PrevieHotovost(CurrentValues.Change);
            fe_Spending.Text = PreviewText(CurrentValues.Spending);
            fe_Credit.Text = PreviewText(CurrentValues.Credit);
        }
        #endregion

        #region dragAndDrop
        UIElement dragObject;
        Point offset;

        private void fe_coin_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dragObject = sender as UIElement;
            offset = e.GetPosition(this.Canvas);
            offset.Y -= Canvas.GetTop(dragObject);
            offset.X -= Canvas.GetLeft(dragObject);
            this.Canvas.CaptureMouse();
        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragObject == null)
            {
                return;
            }
            Point position = e.GetPosition(sender as IInputElement);
            Canvas.SetTop(dragObject, position.Y - offset.Y);
            Canvas.SetLeft(dragObject, position.X - offset.X);
        }

        private void Canvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(fe_CoinSlot);
            double boundsX = fe_CoinSlot.Width;
            double boundsY = fe_CoinSlot.Height;

            this.Canvas.ReleaseMouseCapture();

            if ((position.X - boundsX < 10) && (position.Y - boundsY < 0))
            {
                this.Dispatcher.Invoke(lightUp);
                this.Dispatcher.Invoke(resetcoin);
                MediaManager.PlayFile(MediaData.MediaType.CoinIn);

                CurrentValues.Change += 10;
                UpdateState();
            }
            else if (position.X < 0 || position.Y < 0 
                || position.X + boundsX/5 + 20 > this.Width || position.Y + boundsY/5 + 20> this.Height)
            {
                this.Dispatcher.Invoke(resetcoin);
                MediaManager.PlayFile(MediaData.MediaType.Fall);
            }
            dragObject = null;
        }

        private async void lightUp()
        {
            fe_CoinSlot_hue.Opacity = 0.7;
            await Task.Delay(700);
            fe_CoinSlot_hue.Opacity = 0.5;
            await Task.Delay(470);
            fe_CoinSlot_hue.Opacity = 0.7;
            await Task.Delay(470);
            fe_CoinSlot_hue.Opacity = 0.5;
            await Task.Delay(200);
            fe_CoinSlot_hue.Opacity = 0.7;
            await Task.Delay(600);
            fe_CoinSlot_hue.Opacity = 0.5;
        }
        private void resetcoin()
        {
            fe_coin.SetValue(Canvas.LeftProperty, this.Width - fe_coin.Width - 20);
            fe_coin.SetValue(Canvas.TopProperty, 0 + fe_coin.Height - 20);
        }
        #endregion

        #region Roll
        private void Button_Click(object sender, RoutedEventArgs e) //roll
        {
            if (CurrentValues.Credit != 0 && CurrentValues.Credit >= CurrentValues.Cost)
            {
                Gambling().Wait();
                Evaluate();
            }
            else
                Dispatcher.Invoke(flicker);
        }

        private async void flicker()
        {
            fe_Change.Foreground = Brushes.OrangeRed;
            fe_Change.BorderBrush = Brushes.OrangeRed;
            fe_Credit.BorderBrush = Brushes.OrangeRed;

            await Task.Delay(700);
            fe_Change.Foreground = Brushes.Black;
            fe_Change.BorderBrush = Brushes.Black;
            fe_Credit.BorderBrush = Brushes.Black;

            await Task.Delay(700);
            fe_Change.Foreground = Brushes.OrangeRed;
            fe_Change.BorderBrush = Brushes.OrangeRed;
            fe_Credit.BorderBrush = Brushes.OrangeRed;

            await Task.Delay(700);
            fe_Change.Foreground = Brushes.Black;
            fe_Change.BorderBrush = Brushes.Black;
            fe_Credit.BorderBrush = Brushes.Black;

        }

        private Random rnd = new Random();

        private async Task Gambling()
        {
            rnd = new Random();
            MediaManager.PlayFile(MediaData.MediaType.Pull);

            StartRoll();
        }

        private void StartRoll()
        {
                Dispatcher.Invoke(
                    () => gambler(fe_slot1));
                Dispatcher.Invoke(
                    () => gambler(fe_slot2));
                Dispatcher.Invoke(
                    () => gambler(fe_slot3));
        }

        private void Evaluate()
        {
            int s1 = CurrentValues.SlotResults[0];
            int s2 = CurrentValues.SlotResults[1];
            int s3 = CurrentValues.SlotResults[2];


            if (s1 == s2 && s2 == s3)
                JackPot();
            else if (CurrentValues.SlotResults.Distinct().Count() == 2)
                SmollPot();
            else 
            {
                Loss(TheftMode);
            }

            UpdateState();
        }

        private bool? TheftMode { get { return fe_theft.IsChecked; } }
        private void Loss(bool? theft)
        {
            double coef = theft == true ? 0.1 : 0;
            double loss = 0;
            double gamblingValue = CurrentValues.Cost;
            loss = gamblingValue + gamblingValue * coef;

            int balance = int.Parse(Math.Round(loss, 0).ToString());
            if (balance >= 0 && CurrentValues.Credit - balance >= 0)
                CurrentValues.Credit -= balance;
            else
                CurrentValues.Credit = 0;

        }

        private void SmollPot()
        {
            double coef = 0.5;

            double newCredit = 0;
            double gamblingValue = CurrentValues.Cost;
            newCredit = gamblingValue + gamblingValue * coef;
            CurrentValues.Credit += int.Parse(Math.Round(newCredit, 0).ToString());

        }

        private void JackPot()
        {
            double coef = 2;

            double newCredit = 0;
            double gamblingValue = CurrentValues.Cost;
            newCredit = gamblingValue +  gamblingValue * coef;
            CurrentValues.Credit += int.Parse(Math.Round(newCredit, 0).ToString());
        }

        private async void gambler(Image img) // trigger result + eye candy
        {
            int len = img.Name.ToCharArray().Length;
            int slotNum = int.Parse(img.Name.ToCharArray()[len - 1].ToString()) - 1;
            int result = gambler_LogResult(slotNum);

            int index = 0;
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(60);
                index = rnd.Next(0, ImageSources.Images.Count);
                img.Source = ImageSources.Images[index];
            }

            img.Source = ImageSources.Images[result];
        }

        private int gambler_LogResult(int slotNum)
        {
            int result = rnd.Next(0, ImageSources.Images.Count);
            CurrentValues.SlotResults[slotNum] = result;
            return result;
        }
        #endregion

        private void Button_Click_1(object sender, RoutedEventArgs e) // vlozit
        {
            CurrentValues.Credit += CurrentValues.Change;
            CurrentValues.Spending += CurrentValues.Change;

            CurrentValues.Change = 0;
            UpdateState();
        }

        private void Vybrat_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentValues.Change > 0 || CurrentValues.Credit > 0)
            {
                CurrentValues = new AppState();
                if(!playing)
                    Dispatcher.Invoke(() => cool("ty4playing<3"));
                MediaManager.PlayFile(MediaData.MediaType.Pull);


                ImageSource sameSource = fe_slot1.Source;
                fe_slot2.Source = sameSource;
                fe_slot3.Source = sameSource;

                ComboBoxItem def = (ComboBoxItem)fe_DropDown.Items.GetItemAt(0);
                fe_DropDown.SelectedItem = def;
                UpdateState();
            }
        }

        bool playing = false;
        private async void cool(string msg)
        {
            playing = true;

            char[] s = msg.ToCharArray();
            char[] mod = $"    {msg}".ToCharArray();

            Func<int, string> modify = (m) => { return m > s.Length - 1 ? " " : s[m].ToString(); };

            fe_Change.Foreground = Brushes.BlueViolet;

            for (int i = 0; i < 2; i++)
            {
                for (int l = 0; l < s.Length; l++)
                {
                    await Task.Delay(400);

                    if(fe_Change.Foreground != Brushes.BlueViolet)
                        fe_Change.Foreground = Brushes.BlueViolet;

                    string display = $"{s[l]}{modify(l + 1)}{modify(l + 2)}{modify(l + 3)}";
                    fe_Change.Text = display;
                }
                s = mod;
            }

            fe_Change.Foreground = Brushes.Black;

            fe_Change.Text = PrevieHotovost(CurrentValues.Change);
            playing = false;
        }

        private void Dropdown_selcted(object sender, SelectionChangedEventArgs e)
        {
            CurrentValues.Cost = PreviewCost(fe_DropDown.SelectedItem.ToString());
            UpdateState();
        }

        private void fe_coin_MouseEnter(object sender, MouseEventArgs e)
        {
            showHint();
        }

        private void fe_coin_MouseLeave(object sender, MouseEventArgs e)
        {
           hideHint();
        }
    }
}
