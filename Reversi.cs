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

        int breedte, hoogte, hokjesgrootte, beurt, score;
        int mx, my, klikbreedte, klikhoogte, winnaar, schermbreedte, schermhoogte;
        bool help;

        string spelstatus;

        public Reversi()
        {
            InitializeComponent();
            this.Text = "Reversi";
            help = false;
            breedte = 6;
            hoogte = 6;
            beurt = 0;
            stenen = new Piece[breedte, hoogte];

            Initialiseer();

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
            for (int rows = 0; rows < breedte; rows++)
                Rev.DrawLine(Pens.Black, rows * hokjesgrootte, 0, rows * hokjesgrootte, hoogte * hokjesgrootte - 1);
            Rev.DrawLine(Pens.Black, breedte * hokjesgrootte - 1, 0, breedte * hokjesgrootte - 1, hoogte * hokjesgrootte - 1);
            for (int columns = 0; columns < hoogte; columns++)
                Rev.DrawLine(Pens.Black, 0, columns * hokjesgrootte, breedte * hokjesgrootte - 1, columns * hokjesgrootte);
            Rev.DrawLine(Pens.Black, 0, hoogte * hokjesgrootte - 1, breedte * hokjesgrootte - 1, hoogte * hokjesgrootte - 1);

            // teken startpositie
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

            // tekstboxen; scores & labels
            InitializeMyControl();

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
        }
        // teken steen
        private void tekenSteen(object obj, PaintEventArgs ReversiBord, Color kleur, int posx, int posy)
        {
            Graphics Rev = ReversiBord.Graphics;

            SolidBrush kleur1 = new SolidBrush(kleur);
            Rectangle cirkel = new Rectangle(posx * hokjesgrootte - 1, posy * hokjesgrootte - 1, hokjesgrootte, hokjesgrootte);
            Rev.FillEllipse(kleur1, cirkel);
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
            int hulpcirkelsize = hokjesgrootte / 2;

            for (int y = 0; y < hoogte; y++)
            {
                for (int x = 0; x < breedte; x++)
                {
                    Piece kleuraanbeurt;
                    if (beurt % 2 == 0)
                        kleuraanbeurt = Piece.red;
                    else
                        kleuraanbeurt = Piece.blue;

                    if (stenen[x, y] == Piece.none && LegaleZet(x, y, kleuraanbeurt))
                    {
                        Rectangle opencirkeltje = new Rectangle(x * hokjesgrootte, y * hokjesgrootte, hulpcirkelsize, hulpcirkelsize);
                        Rev.DrawEllipse(Pens.Black, opencirkeltje);
                    }
                }
            }
        }

        // mouse-handler methode. Controleert of een klik (zet) legaal is en voert de zet uit als dat zo is
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // dan tekenen we op dit vakje een steen
            mx = e.X; // gewoon xcoords
            my = e.Y; // gewoon ycoords
            klikbreedte = mx / hokjesgrootte; //xcoords omgezet naar 6 hokjes
            klikhoogte = my / hokjesgrootte; // ycoords omgezet naar 6 hokjes

            // checken wie aan de beurt is en legale zetten voor deze speler bekijken
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

            // opnieuw scherm tekenen
            spelbord.Invalidate();
        }

        // bool-methode die kijkt of een zet legaal is
        public bool LegaleZet(int row, int column, Piece aandebeurt)
        {
            int teller = 1;
            if (stenen[row, column] == Piece.none) // is dit vakje leeg?
            {
                for (int dy = -1; dy <= 1; dy++) // kijken wat er omheen ligt
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        try
                        {
                            int huidigex = row + dx, huidigey = column + dy; // huidige x en y, die we op dat moment bekijken
                            if (stenen[huidigex, huidigey] != aandebeurt) //kleur van tegenstander & niet leeg
                            {
                                while (stenen[huidigex, huidigey] != aandebeurt && stenen[huidigex, huidigey] != Piece.none) // als aan if-voorwaarde wordt voldaan, doorgaan in die richting
                                {
                                    huidigex += teller * dx;
                                    huidigey += teller * dy;
                                    teller++;
                                }
                                if (stenen[huidigex, huidigey] == aandebeurt) // als de while-loop klaar is, kijken of er een steen van jezelf ligt
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

        // test-methode
        public bool LegaleZetty(int row, int column, Piece aandebeurt)
        {
            if (stenen[row, column] == Piece.none) // is dit vakje leeg?
            {
                for (int dy = -1; dy <= 1; dy++) // kijken wat er omheen ligt
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        try
                        {
                            for (int stap = 0; stap < breedte; stap++)
                            {
                                int huidigex = row + stap * dx;
                                int huidigey = column + stap * dy;
                                if (stenen[huidigex, huidigey] == Piece.none)
                                    return false;
                                if (stenen[huidigex, huidigey] == aandebeurt)
                                    return true;
                            }
                        }
                        catch (IndexOutOfRangeException e) { }
                    }
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
                            for(int p = 0; p < teller; p++)
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
                for (int x = 0; x < breedte; x++)
                    if (stenen[x, y] == Piece.none)
                        return false;
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
                        spelstatus = "Wit heeft gewonnen";
                    else
                        spelstatus = "Blauw heeft gewonnen";
                }
            }
        }

        // nieuw-spel knop: reset het veld, schakelt hulp uit en reset de beurt
        private void NieuwSpelKnop(object sender, EventArgs e)
        {
            Initialiseer();
        }

        private void Initialiseer()
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
    }
}

