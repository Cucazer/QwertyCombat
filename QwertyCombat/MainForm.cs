using System;
using Eto.Forms;
using Eto.Drawing;

namespace QwertyCombat
{
	public class MainForm : Form
	{
        public Drawable pictureMap;

        private Label labelObjectDescription;
        private Button buttonEndTurn;
        private Label labelPlayerTurn;
        private Label labelShipsAlive;
        private Label labelBlueShipsAlive;
        private Label labelRedShipsAlive;
        private Button buttonDebug;
        private CheckBox checkBoxAudio;

        public MainForm()
		{
            Title = "QWERTY Combat";
            ClientSize = new Size(400, 350);
            MinimumSize = new Size(400, 350);

            this.pictureMap = new Drawable { Height = 300, Width = 400, BackgroundColor = Colors.LightBlue };
            this.pictureMap.MouseUp += (sender, e) => { MessageBox.Show($"{e.Location}"); };

            this.labelPlayerTurn = new Label { Text = "First player's turn" };

            var controlsLayout = new TableLayout
            {
                Rows =
                {
                    new Button { Text = "DEBUG", Visible = false},
                    new Label { Text = "Description" },
                    this.labelPlayerTurn,
                    new Label { Text = "Ships alive:" },
                    new CheckBox { Text = "Audio" },
                    null
                },
                Spacing = new Size(10, 10),
                Padding = new Padding(10),
                Width = 200
            };

            var endTurnBtn = new Button { Text = "End turn" };
            endTurnBtn.Click += (sender, e) => this.labelPlayerTurn.Text = this.labelPlayerTurn.Text.Contains("First") ? "Second player's turn" : "First player's turn";

            var fieldLayout = new TableLayout { Rows = { TableLayout.AutoSized(this.pictureMap, centered: true), TableLayout.AutoSized(endTurnBtn, centered: true), null }, Spacing = new Size(10, 10) };

            var layout = new TableLayout(new TableRow(controlsLayout, fieldLayout));

            Content = layout;
        }
	}
}