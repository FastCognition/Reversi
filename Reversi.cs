using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reversi
{
    public partial class Reversi : Form
    {
        public enum Piece : byte
        { none = 0, red = 1, blue = 2 }
        Piece[,] stenen;

        int breedte, hoogte, hokjesgrootte, beurt, scoreRood, scoreBlauw;
        int mx, my, klikbreedte, klikhoogte, winnaar, schermbreedte, schermhoogte;
        bool help, toegestaan, beeindigd;

        string spelstatus;

        public Reversi()
        {
            InitializeComponent();
            this.Text = "Reversi";
            help = false;
            breedte = 6;
            hoogte = 6;
            beurt = 0;
            scoreRood = 0;
            scoreBlauw = 0;

            stenen = new Piece[breedte, hoogte];
            for (int x = 0; x < breedte; x++)
                for (int y = 0; y < hoogte; y++)
                    stenen[x, y] = Piece.none;

            // laadt startpositie
            stenen[breedte / 2 - 1, hoogte / 2 - 1] = Piece.red;
            stenen[breedte / 2, hoogte / 2] = Piece.red;
            stenen[breedte / 2 - 1, hoogte / 2] = Piece.blue;
            stenen[breedte / 2, hoogte / 2 - 1] = Piece.blue;

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

            SolidBrush kleur1 = new SolidBrush(kleur);
            Rectangle cirkel = new Rectangle(posx * hokjesgrootte - 1, posy * hokjesgrootte - 1, hokjesgrootte, hokjesgrootte);
            Rev.FillEllipse(kleur1, cirkel);
        }

        public void Hulp(object obj, PaintEventArgs ReversiBord)
        {
            Graphics Rev = ReversiBord.Graphics;
            int hulpcirkelsize = hokjesgrootte / 2;

            for (int y = 0; y < hoogte; y++)
            {
                for (int x = 0; x < breedte; x++)
                {
                    Piece smallvar;
                    if (beurt % 2 == 0)
                        smallvar = Piece.red;
                    else
                        smallvar = Piece.blue;

                    if (stenen[x, y] == Piece.none & LegaleZet(x, y, smallvar) == true)
                    {
                        Rectangle opencirkeltje = new Rectangle(x * hokjesgrootte, y * hokjesgrootte, hulpcirkelsize, hulpcirkelsize);
                        Rev.DrawEllipse(Pens.Black, opencirkeltje);
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
                    stenen[x, y] = 0;
            }

            // herlaadt startpositie
            stenen[breedte / 2 - 1, hoogte / 2 - 1] = Piece.red;
            stenen[breedte / 2, hoogte / 2] = Piece.red;
            stenen[breedte / 2 - 1, hoogte / 2] = Piece.blue;
            stenen[breedte / 2, hoogte / 2 - 1] = Piece.blue;

            help = false;
            beurt = 0;

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
        }

        public bool IsHetSpelAfgelopen()
        {
            for (int y = 0; y < hoogte; y++)
                for (int x = 0; x < breedte; x++)
                    if (stenen[x, y] == Piece.none)
                        return false;
            return true;
        }

        public bool LegaleZet(int row, int column, Piece color)
        {
            int teller = 1;
            if (stenen[row, column] == Piece.none) // is dit vakje leeg?
            {
                for (int dy = -1; dy <= 1; dy++) // kijken wat er omheen ligt
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        try
                        {
                            int huidigex = row + dx, huidigey = column + dy;
                            if (stenen[huidigex, huidigey] != color && stenen[huidigex, huidigey] != Piece.none)
                            {
                                while (stenen[huidigex, huidigey] != color)
                                {
                                    huidigex += teller * dx;
                                    huidigey += teller * dy;
                                    teller++;
                                }
                                if (stenen[huidigex, huidigey] == color)
                                {
                                    return true;
                                }
                            }
                        }
                        catch (IndexOutOfRangeException e) { }
                    }
            }
            return false;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // dan tekenen we op dit vakje een steen
            mx = e.X; // gewoon xcoords
            my = e.Y; // gewoon ycoords
            klikbreedte = mx / hokjesgrootte; //xcoords omgezet naar 1-6 hokjes
            klikhoogte = my / hokjesgrootte; // ycoords omgezet naar 1-6 hokjes

            // waarde van array x,y veranderen op basis van wie zijn beurt het is
            if (beurt % 2 == 0)
            {
                if (LegaleZet(klikbreedte, klikhoogte, Piece.red) == true)
                {
                    VoerBeurtUit(klikbreedte, klikhoogte, Piece.red);
                    beurt++;
                }
            }
            else if (beurt % 2 == 1)
            {
                if (LegaleZet(klikbreedte, klikhoogte, Piece.blue) == true)
                {
                    VoerBeurtUit(klikbreedte, klikhoogte, Piece.blue);
                    beurt++;
                }
            }

            //code om de kleur aan te passen op basis van huidige klikstatus en toegevoegde bolletje


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
        }
    }
}

