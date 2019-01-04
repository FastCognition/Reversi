using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        int breedte, hoogte, hokjesgrootte, beurt, score, diameter;
        int mx, my, klikbreedte, klikhoogte, winnaar, schermbreedte, schermhoogte;
        bool help;
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
            tekenBord(obj, ReversiBord);

            // teken stenen
            for (int y = 0; y < hoogte; y++)
            {
                for (int x = 0; x < breedte; x++)
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

            // tekst tijdens spel
            if (beurt % 2 == 0)
                spelstatus = "Rood is aan zet";
            else
                spelstatus = "Blauw is aan zet";

            // eindeteksten
            if (IsHetSpelAfgelopen() == true)
            {
                EindeTekst();
            }

            // hulpmethode aanroepen, als er op de hulpknop wordt geklikt
            if (help == true)
            {
                Hulp(obj, ReversiBord);
            }

            // tekstboxen; scores & labels
            InitializeMyControl();
        }
        // teken steen
        private void tekenSteen(object obj, PaintEventArgs ReversiBord, Color kleur, int posx, int posy)
        {
            Graphics Rev = ReversiBord.Graphics;

            diameter = 10;
            SolidBrush kleur1 = new SolidBrush(kleur);
            Rectangle cirkel = new Rectangle(posx * hokjesgrootte + diameter / 2, posy * hokjesgrootte + diameter / 2, hokjesgrootte - diameter, hokjesgrootte - diameter);
            Rev.FillEllipse(kleur1, cirkel);
        }

        // teken bord
        private void tekenBord(object obj, PaintEventArgs ReversiBord)
        {
            Graphics Rev = ReversiBord.Graphics;

            for (int rows = 0; rows < breedte; rows++)
                Rev.DrawLine(Pens.Black, rows * hokjesgrootte, 0, rows * hokjesgrootte, hoogte * hokjesgrootte - 1);
            Rev.DrawLine(Pens.Black, breedte * hokjesgrootte - 1, 0, breedte * hokjesgrootte - 1, hoogte * hokjesgrootte - 1);
            for (int columns = 0; columns < hoogte; columns++)
                Rev.DrawLine(Pens.Black, 0, columns * hokjesgrootte, breedte * hokjesgrootte - 1, columns * hokjesgrootte);
            Rev.DrawLine(Pens.Black, 0, hoogte * hokjesgrootte - 1, breedte * hokjesgrootte - 1, hoogte * hokjesgrootte - 1);
        }

        // help knop: wisselt waarde true/false voor het wel/niet aanstaan van de hulpmethode
        private void HelpMethode(object sender, EventArgs e)
        {
            if (help == true)
                help = false;
            else
                help = true;
            spelbord.Invalidate();
        }

        // tekent een cirkeltje waar legale zetten kunnen worden gedaan, voor de speler die aan de beurt is
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
                            catch (IndexOutOfRangeException e) { }
                        }
                    }
                }
            }
        }

        private void PasKnop_Click(object sender, EventArgs ev)
        {
            beurt++;
            spelbord.Invalidate();
        }

        // mouse-handler methode. Controleert of een klik (zet) legaal is en voert de zet uit als dat zo is
        private void pictureBox1_MouseClick(object sender, MouseEventArgs ev)
        {
            // dan tekenen we op dit vakje een steen
            mx = ev.X; // gewoon xcoords
            my = ev.Y; // gewoon ycoords
            klikbreedte = mx / hokjesgrootte; //xcoords omgezet naar 1-6 hokjes
            klikhoogte = my / hokjesgrootte; // ycoords omgezet naar 1-6 hokjes

            // waarde van array x,y veranderen op basis van wie zijn beurt het is
            if (stenen[klikbreedte, klikhoogte] == Piece.none)
            {
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
                        catch (IndexOutOfRangeException e) { }
                    }
                }
                // opnieuw scherm tekenen
                spelbord.Invalidate();
            }

        }

        // bool-methode die kijkt of een zet legaal is
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
                catch (IndexOutOfRangeException e) { }

            }
            return false;
        }

        // methode die zet uitvoert, inclusief het veranderen van kleuren
        public void VoerBeurtUit(int row, int col, Piece color)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    try
                    {
                        int teller = 1;
                        while (stenen[row + (dx * teller), col + (dy * teller)] != color && (stenen[row + (dx * teller), col + (dy * teller)] != Piece.none))
                        {
                            teller++;
                        }
                        if (stenen[row + (dx * teller), col + (dy * teller)] == color)
                        {
                            for (int p = 0; p < teller; p++)
                            {
                                stenen[row + p * dx, col + p * dy] = color;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException e) { }
                }
            }
        }

        // methode die score berekent voor de spelerskleur
        public int Score(Piece color)
        {
            score = 0;
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

        // method controlling textboxes
        private void InitializeMyControl()
        {
            this.UpdateRood.Text = Convert.ToString(Score(Piece.red));
            this.UpdateBlauw.Text = Convert.ToString(Score(Piece.blue));
            this.SpelStatusTekst.Text = spelstatus;
        }

        // bool-methode die checkt of het spel is afgelopen
        public bool IsHetSpelAfgelopen()
        {
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
                                // het spel is nog niet afgelopen als er nog lege vakjes zijn en rood OF blauw nog een legale zet kan doen
                                if (stenen[x, y] == Piece.none && (LegaleZet(x, y, dx, dy, Piece.blue) == true || LegaleZet(x, y, dx, dy, Piece.red) == true))
                                    return false;
                            }
                            catch (IndexOutOfRangeException e) { }
                        }
                    }
                }
            }
            return true;
        }

        // methode die eindeteksten op scherm zet
        public void EindeTekst()
        {
            {
                if (Score(Piece.red) == Score(Piece.blue))
                    spelstatus = "Remise!";
                else
                {
                    winnaar = Math.Max(Score(Piece.red), Score(Piece.blue));
                    if (winnaar == Score(Piece.red))
                        spelstatus = "Rood heeft gewonnen";
                    else
                        spelstatus = "Blauw heeft gewonnen";
                }
            }
        }

        // nieuw-spel knop: reset het veld, schakelt hulp uit en reset de beurt
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
            spelbord.Invalidate();
        }
    }
}

