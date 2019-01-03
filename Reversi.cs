using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Reversi
{
    public partial class Reversi : Form
    {
        public enum Piece : byte
        { none = 0, red = 1, blue = 2 }
        Piece[,] stenen;

        int breedte, hoogte, hokjesgrootte, beurt, scoreRood, scoreBlauw, diameter;
        int mx, my, klikbreedte, klikhoogte, winnaar, schermbreedte, schermhoogte;
        bool help, toegestaan, beeindigd;
        string spelstatus;

        public Reversi()
        {
            InitializeComponent();
            this.Text = "Reversi";
            breedte = 6;
            hoogte = 6;

            stenen = new Piece[breedte, hoogte];

            NieuwSpelKnop(null, EventArgs.Empty);

            this.spelbord.Paint += this.tekenScherm;
        }

        public void tekenScherm(object obj, PaintEventArgs ReversiBord)
        {

            Graphics Rev = ReversiBord.Graphics;

            //Om de hokjes vierkant te houden
            hokjesgrootte = Math.Min(spelbord.Width / breedte, spelbord.Height / hoogte);
            schermbreedte = hokjesgrootte * breedte;
            schermhoogte = hokjesgrootte * hoogte;

            // teken het bord
            for (int t1 = 0; t1 < breedte; t1++)
                Rev.DrawLine(Pens.Black, t1 * hokjesgrootte, 0, t1 * hokjesgrootte, hoogte * hokjesgrootte - 1);
            Rev.DrawLine(Pens.Black, breedte * hokjesgrootte - 1, 0, breedte * hokjesgrootte - 1, hoogte * hokjesgrootte - 1);
            for (int t2 = 0; t2 < hoogte; t2++)
                Rev.DrawLine(Pens.Black, 0, t2 * hokjesgrootte, breedte * hokjesgrootte - 1, t2 * hokjesgrootte);
            Rev.DrawLine(Pens.Black, 0, hoogte * hokjesgrootte - 1, breedte * hokjesgrootte - 1, hoogte * hokjesgrootte - 1);

            // teken spelpositie
            for (int x = 0; x < breedte; x++)
            {
                for (int y = 0; y < hoogte; y++)
                {
                    if (stenen[x, y] == Piece.red)
                    {
                        tekenSteen(obj, ReversiBord, Color.Red, x, y);
                    }

                    if (stenen[x, y] == Piece.blue)
                    {
                        tekenSteen(obj, ReversiBord, Color.Blue, x, y);
                    }

                }
            }

            InitializeMyControl();

            // eindeteksten
            if (IsHetSpelAfgelopen() == true)
            {
                if (scoreRood == scoreBlauw)
                    spelstatus = "Remise!";
                else
                {
                    winnaar = Math.Max(scoreRood, scoreBlauw);
                    if (winnaar == scoreRood)
                        spelstatus = "Wit heeft gewonnen";
                    else
                        spelstatus = "Blauw heeft gewonnen";

                }
            }

            // hier staat wat de hulpmethode doet, true/false wordt ergens anders bepaald
            if (help == true)
            {
                Hulp(obj, ReversiBord);
            }
        }
        // teken steen
        private void tekenSteen(object obj, PaintEventArgs ReversiBord, Color kleur, int posx, int posy)
        {
            Graphics Rev = ReversiBord.Graphics;

            diameter = 10;
            SolidBrush kleur1 = new SolidBrush(kleur);
            Rectangle cirkel = new Rectangle(posx * hokjesgrootte + diameter/2, posy * hokjesgrootte + diameter/2, hokjesgrootte - diameter, hokjesgrootte - diameter);
            Rev.FillEllipse(kleur1, cirkel);
        }

        public void Hulp(object obj, PaintEventArgs ReversiBord)
        {
            Graphics Rev = ReversiBord.Graphics;
            Pen pen = new Pen(Brushes.Black, 3);
            pen.DashStyle = DashStyle.Dash;

            for (int y = 0; y < hoogte; y++)
            {
                for (int x = 0; x < breedte; x++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            try
                            {
                                Piece smallvar;
                                if (beurt % 2 == 0)
                                    smallvar = Piece.red;
                                else
                                    smallvar = Piece.blue;

                                if (stenen[x, y] == Piece.none & LegaleZet(x, y, dx, dy, smallvar) == true)
                                {
                                    Rectangle opencirkeltje = new Rectangle(x * hokjesgrootte + diameter / 2, y * hokjesgrootte + diameter / 2, hokjesgrootte - diameter, hokjesgrootte - diameter);
                                    Rev.DrawEllipse(pen, opencirkeltje);
                                }
                            }
                            catch (IndexOutOfRangeException e) {}
                        }
                    }
                }
            }
        }

        public int Score(Piece color)
        {
            int score = 0;
            for (int y = 0; y < breedte; y++)
            {
                for (int x = 0; x < hoogte; x++)
                {
                    if (stenen[y, x] == color)
                        score++;
                }
            }
            return score;
        }

        // nieuw-spel knop
        private void NieuwSpelKnop(object sender, EventArgs e)
        {
            for (int x = 0; x < breedte; x++)
            {
                for (int y = 0; y < hoogte; y++)
                    stenen[x, y] = Piece.none;
            }

            // herlaadt startpositie
            stenen[breedte / 2 - 1, hoogte / 2 - 1] = Piece.red;
            stenen[breedte / 2, hoogte / 2] = Piece.red;
            stenen[breedte / 2 - 1, hoogte / 2] = Piece.blue;
            stenen[breedte / 2, hoogte / 2 - 1] = Piece.blue;

            help = false;
            beurt = 0;
            spelstatus = "Rood is aan zet";
            scoreRood = 0;
            scoreBlauw = 0;
            spelbord.Invalidate();
        }

        // help knop
        private void HelpMethode(object sender, EventArgs e)
        {
            if (help == true)
                help = false;
            else
                help = true;
            spelbord.Invalidate();
        }

        // method controlling textboxes
        private void InitializeMyControl()
        {
            this.UpdateRood.Text = Convert.ToString(Score(Piece.red));
            this.UpdateBlauw.Text = Convert.ToString(Score(Piece.blue));
            this.AandeBeurt.Text = spelstatus;
        }

        public bool IsHetSpelAfgelopen()
        {
            for (int y = 0; y < hoogte; y++)
                for (int x = 0; x < breedte; x++)
                    if (stenen[x, y] == Piece.none)
                        return false;
            return true;
        }

        public bool LegaleZet(int x, int y, int dx, int dy, Piece color)
        {
            for (int teller = 1; teller < breedte; teller++)
            {
                try
                {
                    int huidigex = x + teller * dx;
                    int huidigey = y + teller * dy;
                    if (stenen[huidigex, huidigey] == Piece.none)
                    {
                        return false;
                    }
                    if (stenen[huidigex, huidigey] == color)
                    {
                        return teller > 1;
                    }
                }
                catch (IndexOutOfRangeException e) {}

            }
            return false;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs ev)
        {
            // dan tekenen we op dit vakje een steen
            mx = ev.X; // gewoon xcoords
            my = ev.Y; // gewoon ycoords
            klikbreedte = mx / hokjesgrootte; //xcoords omgezet naar 1-6 hokjes
            klikhoogte = my / hokjesgrootte; // ycoords omgezet naar 1-6 hokjes

            // waarde van array x,y veranderen op basis van wie zijn beurt het is
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    try
                    {
                        if (beurt % 2 == 0)
                        {
                            if (LegaleZet(klikbreedte, klikhoogte, dx, dy, Piece.red) == true)
                            {
                                VoerBeurtUit(klikbreedte, klikhoogte, Piece.red);
                                beurt++;
                            }
                        }
                        else if (beurt % 2 == 1)
                        {
                            if (LegaleZet(klikbreedte, klikhoogte, dx, dy, Piece.blue) == true)
                            {
                                VoerBeurtUit(klikbreedte, klikhoogte, Piece.blue);
                                beurt++;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException e) {}
                }
            }
            // opnieuw scherm tekenen
            spelbord.Invalidate();
        }

        public void VoerBeurtUit(int row, int col, Piece color)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    try
                    {
                        int teller = 1;
                        while (stenen[row + (dx * teller), col + (dy * teller)] != color && teller < breedte)
                        {
                            teller++;
                        }
                        if (stenen[row + (dx * teller), col + (dy * teller)] == color)
                        {
                            while (teller >= 0)
                            {
                                stenen[row + (dx * teller), col + (dy * teller)] = color;
                                teller--;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException e) { }
                }
            }
            spelstatus = spelstatus == "Rood is aan zet" ? "Blauw is aan zet" : "Rood is aan zet";
        }
    }
}
