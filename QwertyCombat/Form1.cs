using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using Hex = Barbar.HexGrid;

namespace qwerty
{
    public partial class Form1 : Form
    {
        private ObjectManager objectManager => this.gameLogic.objectManager;
        private readonly GameLogic gameLogic = new GameLogic(8,6);
        private readonly FieldPainter fieldPainter;
        private readonly SoundPlayer soundPlayer = new SoundPlayer();
        
        public Form1()
        {
            this.InitializeComponent();
            this.pictureMap.Width = this.gameLogic.BitmapWidth;
            this.pictureMap.Height = this.gameLogic.BitmapHeight;
            // i'll leave this as constants -> calculation from window size or placing in container later
            this.Width = this.pictureMap.Right + 25;
            this.Height = this.pictureMap.Bottom + 45;
            this.fieldPainter = new FieldPainter(this.gameLogic.BitmapWidth, this.gameLogic.BitmapHeight, this.objectManager);
            ObjectManager.ObjectAnimated += this.fieldPainter.OnAnimationPending;
            ObjectManager.SoundPlayed += this.OnSoundEffect;
            this.fieldPainter.BitmapUpdated += this.OnBitmapUpdated;
            this.fieldPainter.DrawField();
            this.pictureMap.Image = this.fieldPainter.CurrentBitmap;
            this.pictureMap.Refresh();
            this.lblTurn.Text = this.gameLogic.ActivePlayerDescription + "'s turn";

            this.UpdateShipCount();
#if !DEBUG
            buttonDebug.Visible = false;
#endif
        }

        public void UpdateShipCount()
        {
            int blueShipsCount = this.gameLogic.FirstPlayerShipCount;
            int redShipsCount = this.gameLogic.SecondPlayerShipCount;

            if (blueShipsCount == 0 || redShipsCount == 0)
            {
                this.txtBlueShips.Text = "";
                this.txtRedShips.Text = "";
                this.label1.Text = "GAME OVER!";
                return;
            }
            this.txtBlueShips.Text = $"{blueShipsCount}";
            this.txtRedShips.Text = $"{redShipsCount}";
        }

        private void pictureMap_MouseClick(object sender, MouseEventArgs e)
        {
            this.gameLogic.HandleFieldClick(e.Location);
            this.fieldPainter.UpdateBitmap();
            this.pictureMap.Refresh();
            this.boxDescription.Text = this.gameLogic.ActiveShipDescription;
            this.UpdateShipCount();
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            this.gameLogic.EndTurn();
            this.fieldPainter.UpdateBitmap();
            this.pictureMap.Refresh();
            this.boxDescription.Text = this.gameLogic.ActiveShipDescription;
            this.lblTurn.Text = this.gameLogic.ActivePlayerDescription + "'s turn";
            this.UpdateShipCount();
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hello from debug!");
        }
       
        private void OnBitmapUpdated(object sender, EventArgs e)
        {
            this.pictureMap.Refresh();
        }

        private void OnSoundEffect(object sender, SoundEventArgs e)
        {
            if (!this.checkBoxAudio.Checked)
            {
                return;
            }
            this.soundPlayer.Stream = e.AudioStream;
            this.soundPlayer.Play();
        }
    }
}
