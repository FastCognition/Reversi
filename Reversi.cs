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
        int mx, my, klikbreedte, klikhoogte, winnaar;
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
            hokjesgrootte = spelbord.Width / breedte;

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

            // score bijhouden
            for (int t = 0; t < breedte; t++)
            {
                for (int r = 0; r < hoogte; r++)
                {
                    if (stenen[t, r] != Piece.none)
                        if (stenen[t, r] == Piece.red)
                            scoreRood++;
                        else
                            scoreBlauw++;
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
                int hulpcirkelsize = hokjesgrootte / 2;

                Rectangle opencirkeltje = new Rectangle(2 * hokjesgrootte, 3 * hokjesgrootte, hulpcirkelsize, hulpcirkelsize);
                Rev.DrawEllipse(Pens.Black, opencirkeltje);
            }
        }
        // teken steen
        private void tekenSteen(object obj, PaintEventArgs ReversiBord, Color kleur, int posx, int posy)
        {
            Graphics Rev = ReversiBord.Graphics;

            SolidBrush kleur1 = new SolidBrush(kleur);
            Rectangle cirkel = new Rectangle(posx * hokjesgrootte - 1, posy*hokjesgrootte - 1, hokjesgrootte, hokjesgrootte);
            Rev.FillEllipse(kleur1, cirkel);
        }

        // nieuw-spel knop
        private void NieuwSpelKnop(object sender, EventArgs e)
        {
            for(int x = 0; x < breedte; x++)
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
            this.UpdateRood.Text = Convert.ToString(scoreRood);
            this.UpdateBlauw.Text = Convert.ToString(scoreBlauw);
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
            toegestaan = false;
            if (stenen[klikbreedte, klikhoogte] == 0) // is dit vakje leeg?
            {
                for (int y = -1; y <= 1; y++) // kijken wat er omheen ligt
                    for (int x = -1; x <= 1; x++)
                    {
                        // check voor array-boundaries
                        if ((klikbreedte + x) <= breedte && (klikhoogte + y) <= hoogte && (klikhoogte + y) > 0 && (klikbreedte + x) > 0)
                        {
                            if (stenen[klikbreedte + x, klikhoogte + y] == Piece.red && beurt % 2 == 0)
                                toegestaan = true; // als het vakje eromheen van een andere kleur is, dan kan dit op een zet wijzen
                            if (stenen[klikbreedte + x, klikhoogte + y] == Piece.blue && beurt % 2 == 1)
                                toegestaan = true;
                        }
                    }
            }
            return toegestaan;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // dan tekenen we op dit vakje een steen
            mx = e.X; // gewoon xcoords
            my = e.Y; // gewoon ycoords
            klikbreedte = mx / hokjesgrootte; //xcoords omgezet naar 1-6 hokjes
            klikhoogte = my / hokjesgrootte; // ycoords omgezet naar 1-6 hokjes

            // waarde van array x,y veranderen op basis van wie zijn beurt het is
            if (LegaleZet(klikbreedte, klikhoogte, stenen[klikbreedte, klikhoogte]) == true)
            {
                if (beurt % 2 == 0)
                {
                    stenen[klikbreedte, klikhoogte] = Piece.red;
                    beurt++;
                }
                else
                {
                    stenen[klikbreedte, klikhoogte] = Piece.blue;
                    beurt++;
                }
            }
            
            //code om de kleur aan te passen op basis van huidige klikstatus en toegevoegde bolletje


            // opnieuw scherm tekenen
            spelbord.Invalidate();
        }
    }
}

