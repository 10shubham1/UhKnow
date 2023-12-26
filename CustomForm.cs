namespace UhKnow
{
    public class ConfirmationForm : Form
    {
        public bool UserConfirmed { get; private set; }
        public Button noButton = new Button();
        public Button yesButton = new Button();
        public int ScreenWidth;
        public int ScreenHeight;

        public ConfirmationForm(string message, int screenWidth, int screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            InitializeComponent(message);
            AcceptButton = yesButton;
            Load += (sender, e) => ActiveControl = noButton;
        }

        private void InitializeComponent(string message)
        {
            Text = "Confirmation";
            Size = new Size(ScreenWidth / 5, ScreenHeight / 6);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            Label messageLabel = new Label
            {
                Text = message,
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = true,
                Padding = new Padding(ScreenWidth / 50, ScreenHeight / 50, 0, 0)

            };
            Controls.Add(messageLabel);

            yesButton = new Button
            {
                Text = "Yes",
                DialogResult = DialogResult.Yes,
                Size = new Size(ScreenWidth / 25, ScreenHeight / 35),
                Location = new Point((ClientSize.Width - ScreenWidth / 10) / 2, messageLabel.Bottom + ScreenHeight / 30)
            };
            yesButton.Click += (sender, e) =>
            {
                UserConfirmed = true;
                Close();
            };
            Controls.Add(yesButton);

            noButton = new Button
            {
                Text = "No",
                DialogResult = DialogResult.No,
                Size = new Size(ScreenWidth / 25, ScreenHeight / 35),
                Location = Location = new Point(yesButton.Right + ScreenWidth / 100, yesButton.Top)
            };
            noButton.Click += (sender, e) => Close();
            Controls.Add(noButton);
        }
    }
}