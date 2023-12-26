namespace UhKnow
{
    public class ConfirmationForm : Form
    {
        public bool UserConfirmed { get; private set; }
        public Button noButton = new Button();
        public Button yesButton = new Button();

        public ConfirmationForm(string message)
        {
            InitializeComponent(message);
            AcceptButton = yesButton;
            Load += (sender, e) => ActiveControl = noButton;
        }

        private void InitializeComponent(string message)
        {
            Text = "Confirmation";
            Size = new Size(400, 200);
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
                Padding = new Padding(30, 20, 0, 20)

            };
            Controls.Add(messageLabel);

            yesButton = new Button
            {
                Text = "Yes",
                DialogResult = DialogResult.Yes,
                Size = new Size(80, 40),
                Location = new Point((ClientSize.Width - 180) / 2, messageLabel.Bottom + 20)
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
                Size = new Size(80, 40),
                Location = Location = new Point(yesButton.Right + 20, yesButton.Top)
            };
            noButton.Click += (sender, e) => Close();
            Controls.Add(noButton);
        }
    }
}