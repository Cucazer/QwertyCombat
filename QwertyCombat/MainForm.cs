using System;
using System.Media;
using Eto.Forms;
using Eto.Drawing;

namespace QwertyCombat
{
    public class MainForm : Form
    {
        public ImageView pictureMap;

        private Button buttonDebug;

        private TableLayout controlsLayout;
        private TableLayout fieldLayout;
        private TableLayout formLayout;

        private ObjectManager objectManager => this.gameLogic.objectManager;
        private GameLogic gameLogic = new GameLogic(8, 6);
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

            this.KeyDown += this.Form_KeyDown;

            this.buttonDebug = new Button { Text = "DEBUG" };
            this.buttonDebug.MouseDoubleClick += this.buttonDebug_Click;
#if !DEBUG
            this.buttonDebug.Visible = false;
#endif

            // Layouting

            this.controlsLayout = new TableLayout
            {
                Rows =
                {
                    this.buttonDebug,
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

            this.fieldPainter.UpdateBitmap();
            this.Shown += (sender, e) => {
                //this.fieldLayout.Update();
                //this.formLayout.Update();
                //this.ClientSize = this.formLayout.ClientSize;
                this.ClientSize = new Size(this.pictureMap.Bounds.Right + 250, this.pictureMap.Bounds.Bottom + 100);
            };
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
                    this.gameLogic.EndTurn();
                    break;
                case FieldPainter.BitmapElement.SoundButton:
                    this.gameSettings.SoundEnabled = !this.gameSettings.SoundEnabled;
                    break;
                case FieldPainter.BitmapElement.RestartButton:
                    this.gameLogic = new GameLogic(8, 6);
                    this.fieldPainter.ObjectManager = this.gameLogic.objectManager;
                    break;
                default:
                    break;
            }
            this.fieldPainter.UpdateBitmap();
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

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Space:
                    this.gameLogic.EndTurn();
                    break;
                case Keys.M:
                    this.gameSettings.SoundEnabled = !this.gameSettings.SoundEnabled;
                    break;
            }
            this.fieldPainter.UpdateBitmap();
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
