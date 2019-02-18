using System;
using System.Media;
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

        private TableLayout controlsLayout;
        private TableLayout fieldLayout;
        private TableLayout formLayout;

        private ObjectManager objectManager => this.gameLogic.objectManager;
        private readonly GameLogic gameLogic = new GameLogic(8, 6);
        private readonly GameSettings gameSettings = new GameSettings();
        private readonly FieldPainter fieldPainter;
        private readonly SoundPlayer soundPlayer = new SoundPlayer();

        public MainForm()
        {
            Title = "QWERTY Combat";
            ClientSize = new Size(400, 350);
            MinimumSize = new Size(400, 350);

            // Controls initialization

            this.pictureMap = new ImageView { BackgroundColor = Colors.LightBlue };
            this.pictureMap.MouseDown += this.pictureMap_MouseClick;
            this.pictureMap.MouseMove += this.pictureMap_MouseMove;

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

            this.checkBoxAudio = new CheckBox { Checked = false, Text = "Audio" };

            // Layouting

            this.controlsLayout = new TableLayout
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


            this.fieldLayout = new TableLayout
            {
                Rows =
                {
                    TableLayout.AutoSized(this.pictureMap, centered: true),
                    TableLayout.AutoSized(this.buttonEndTurn, centered: true),
                    null
                },
                Spacing = new Size(10, 10)
            };

            this.formLayout = new TableLayout(new TableRow(controlsLayout, fieldLayout));

            Content = this.formLayout;

            // constructor code from previous solution            
            this.fieldPainter = new FieldPainter(this.gameLogic.BitmapWidth, this.gameLogic.BitmapHeight, this.objectManager, this.gameSettings);
            ObjectManager.ObjectAnimated += this.fieldPainter.OnAnimationPending;
            ObjectManager.SoundPlayed += this.OnSoundEffect;
            this.fieldPainter.BitmapUpdated += this.OnBitmapUpdated;
            this.fieldPainter.DrawGameScene();
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.labelPlayerTurn.Text = this.gameLogic.ActivePlayerDescription + "'s turn";
            this.UpdateShipCount();
            this.Shown += (sender, e) => {
                //this.fieldLayout.Update();
                //this.formLayout.Update();
                //this.ClientSize = this.formLayout.ClientSize;
                this.ClientSize = new Size(this.pictureMap.Bounds.Right + 250, this.pictureMap.Bounds.Bottom + 100);
            };
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
            FieldPainter.BitmapElement elementClicked = this.fieldPainter.GetUiElementAtLocation((Point) e.Location);
            switch (elementClicked)
            {
                case FieldPainter.BitmapElement.GameField:
                    this.gameLogic.HandleFieldClick(this.fieldPainter.GetGameFieldCoordinates(e.Location));
                    break;
                case FieldPainter.BitmapElement.EndTurnButton:
                    this.btnEndTurn_Click(sender, EventArgs.Empty);
                    break;
                case FieldPainter.BitmapElement.SoundButton:
                    this.gameSettings.SoundEnabled = !this.gameSettings.SoundEnabled;
                    break;
                default:
                    break;
            }
            this.fieldPainter.UpdateBitmap(); // causes blinking of an animated object, bitmap won't be updated if this will be disabled and no animation happening
            // unfortunately there is no Refresh method in Eto.Forms and Invalidate doesn't work as intended, but reassigning image to updated bitmap does the trick :)
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.UpdateShipCount();
        }

        private void pictureMap_MouseMove(object sender, MouseEventArgs e)
        {
            var objectDescription = this.gameLogic.HandleFieldHover(this.fieldPainter.GetGameFieldCoordinates(e.Location));
            if (objectDescription != null)
            {
                this.fieldPainter.ActivateObjectTooltip((Point)e.Location, objectDescription);
            }
            else
            {
                this.fieldPainter.DeactivateTooltip();
            }
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            this.gameLogic.EndTurn();
            this.fieldPainter.UpdateBitmap(); // causes blinking of an animated object, bitmap won't be updated if this will be disabled and no animation happening
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.labelPlayerTurn.Text = this.gameLogic.ActivePlayerDescription + "'s turn";
            this.UpdateShipCount();
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hello from debug!");
        }

        private void OnBitmapUpdated(object sender, EventArgs e)
        {
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.pictureMap.Invalidate();
            this.Invalidate();
        }

        private void OnSoundEffect(object sender, SoundEventArgs e)
        {
            if (!this.gameSettings.SoundEnabled)
            {
                return;
            }
            this.soundPlayer.Stream = e.AudioStream;
            this.soundPlayer.Play();
        }
    }
}
