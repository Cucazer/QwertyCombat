using System;
using Eto.Forms;
using Eto.Drawing;

namespace QwertyCombat
{
    public class MainForm : Form
    {
        public ImageView pictureMap;

        private Label labelObjectDescription;
        private Button buttonEndTurn;
        private Label labelPlayerTurn;
        private Label labelShipsAlive;
        private Label labelBlueShipsAlive;
        private Label labelRedShipsAlive;
        private Button buttonDebug;
        private CheckBox checkBoxAudio;

        private ObjectManager objectManager => this.gameLogic.objectManager;
        private readonly GameLogic gameLogic = new GameLogic(8, 6);
        private readonly FieldPainter fieldPainter;
        //private readonly SoundPlayer soundPlayer = new SoundPlayer();

        public MainForm()
        {
            Title = "QWERTY Combat";
            ClientSize = new Size(400, 350);
            MinimumSize = new Size(400, 350);

            // Controls initialization

            this.pictureMap = new ImageView { Height = 300, Width = 400, BackgroundColor = Colors.LightBlue };
            this.pictureMap.MouseUp += this.pictureMap_MouseClick;

            this.labelObjectDescription = new Label();

            this.buttonEndTurn = new Button { Text = "End turn" };
            this.buttonEndTurn.Click += this.btnEndTurn_Click;

            this.labelPlayerTurn = new Label { Text = "First player's turn" };

            this.labelShipsAlive = new Label { Text = "Ships alive:" };

            this.labelBlueShipsAlive = new Label { TextColor = Colors.Blue };

            this.labelRedShipsAlive = new Label { TextColor = Colors.Red };

            this.buttonDebug = new Button { Text = "DEBUG" };
            this.buttonDebug.Click += this.buttonDebug_Click;
#if !DEBUG
            this.buttonDebug.Visible = false;
#endif

            this.checkBoxAudio = new CheckBox { Checked = true, Text = "Audio" };

            // Layouting

            var controlsLayout = new TableLayout
            {
                Rows =
                {
                    this.buttonDebug,
                    this.labelObjectDescription,
                    this.labelPlayerTurn,
                    this.labelShipsAlive,
                    new TableRow(this.labelBlueShipsAlive, this.labelRedShipsAlive),
                    this.checkBoxAudio,
                    null
                },
                Spacing = new Size(10, 10),
                Padding = new Padding(10),
                Width = 200
            };


            var fieldLayout = new TableLayout
            {
                Rows =
                {
                    TableLayout.AutoSized(this.pictureMap, centered: true),
                    TableLayout.AutoSized(this.buttonEndTurn, centered: true),
                    null
                },
                Spacing = new Size(10, 10)
            };

            var layout = new TableLayout(new TableRow(controlsLayout, fieldLayout));

            Content = layout;

            // constructor code from previous solution

            this.pictureMap.Width = this.gameLogic.BitmapWidth;
            this.pictureMap.Height = this.gameLogic.BitmapHeight;
            // i'll leave this as constants -> calculation from window size or placing in container later
            this.Width = this.pictureMap.Bounds.Right + 250;
            this.Height = this.pictureMap.Bounds.Bottom + 100;
            this.fieldPainter = new FieldPainter(this.gameLogic.BitmapWidth, this.gameLogic.BitmapHeight, this.objectManager);
            ObjectManager.ObjectAnimated += this.fieldPainter.OnAnimationPending;
            //ObjectManager.SoundPlayed += this.OnSoundEffect;
            this.fieldPainter.BitmapUpdated += this.OnBitmapUpdated;
            this.fieldPainter.DrawField();
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.labelPlayerTurn.Text = this.gameLogic.ActivePlayerDescription + "'s turn";
            this.UpdateShipCount();
        }

        public void UpdateShipCount()
        {
            int blueShipsCount = this.gameLogic.FirstPlayerShipCount;
            int redShipsCount = this.gameLogic.SecondPlayerShipCount;

            if (blueShipsCount == 0 || redShipsCount == 0)
            {
                this.labelBlueShipsAlive.Text = "";
                this.labelRedShipsAlive.Text = "";
                this.labelObjectDescription.Text = "GAME OVER!";
                return;
            }
            this.labelBlueShipsAlive.Text = $"{blueShipsCount}";
            this.labelRedShipsAlive.Text = $"{redShipsCount}";
        }

        private void pictureMap_MouseClick(object sender, MouseEventArgs e)
        {
            this.gameLogic.HandleFieldClick((Point)e.Location);
            this.fieldPainter.UpdateBitmap(); // causes blinking of an animated object, bitmap won't be updated if this will be disabled and no animation happening
            // unfortunately there is no Refresh method in Eto.Forms and Invalidate doesn't work as intended, but reassigning image to updated bitmap does the trick :)
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.labelObjectDescription.Text = this.gameLogic.ActiveShipDescription;
            this.UpdateShipCount();
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            this.gameLogic.EndTurn();
            this.fieldPainter.UpdateBitmap(); // causes blinking of an animated object, bitmap won't be updated if this will be disabled and no animation happening
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.labelObjectDescription.Text = this.gameLogic.ActiveShipDescription;
            this.labelPlayerTurn.Text = this.gameLogic.ActivePlayerDescription + "'s turn";
            this.UpdateShipCount();
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hello from debug!");
        }

        int step = 0;
        private void OnBitmapUpdated(object sender, EventArgs e)
        {
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.pictureMap.Invalidate();
            this.labelObjectDescription.Text = $"{step++}";
            this.labelObjectDescription.Invalidate();
            this.Invalidate();
            //var bit = (Bitmap)this.pictureMap.Image;
            //bit.Save($"step{step++}.jpg", ImageFormat.Jpeg);
        }

        //private void OnSoundEffect(object sender, SoundEventArgs e)
        //{
        //    if (!this.checkBoxAudio.Checked)
        //    {
        //        return;
        //    }
        //    this.soundPlayer.Stream = e.AudioStream;
        //    this.soundPlayer.Play();
        //}
    }
}