using System;
using System.Windows.Forms;
using System.Drawing;

//Speelveld klasse
class Veld : UserControl
{
    //Veld attributen
    public int veldBreedte, veldHoogte, vakGrootte;

    //Variabelen die de huidige spelsituatie onthouden
    int[,] geheugen;
    bool[,] geldig;
    public bool Hint = false;
    //Gestart geeft aan dat er een spel bezig is
    public bool Gestart = false;
    bool[] pas = new bool[2];
    public int Beurt = 0;

    //Waarden voor weergave van de spelsituatie
    //De status wordt o.a. weergegeven in de titelbalk
    public String Status;
    String[] spelerNamen;
    public static Color Speler1Kleur = Color.Black;
    public static Color Speler2Kleur = Color.White;
    public Brush Speler1Brush = new SolidBrush(Speler1Kleur);
    public Brush Speler2Brush = new SolidBrush(Speler2Kleur);

    //Aantal mogelijke zetten
    public int Mogelijkheden
    {
        get
        {
            int mogelijkheden = 0;
            for (int y = 0; y < this.veldHoogte; y++)
                for (int x = 0; x < this.veldBreedte; x++)
                    if (this.geldig[x, y]) mogelijkheden++;

            return mogelijkheden;
        }
    }

    //Controleer of elk vakje een steen bevat
    private bool veldVol
    {
        get
        {
            for (int y = 0; y < this.veldHoogte; y++)
                for (int x = 0; x < this.veldBreedte; x++)
                    if (this.geheugen[x, y] == 0) return false;

            return true;
        }
    }

    //Constructor methode
    public Veld(Point locatie, int veldBreedte, int veldHoogte, String[] spelerNamen)
    {
        //Minimum breedte en hoogte is 3
        if (veldBreedte < 3) veldBreedte = 3;
        if (veldHoogte < 3) veldHoogte = 3;

        this.veldBreedte = veldBreedte;
        this.veldHoogte = veldHoogte;
        this.vakGrootte = 50;
        this.spelerNamen = spelerNamen;

        //User control eigenschappen
        this.Height = this.vakGrootte * this.veldHoogte + 1;
        this.Width = this.vakGrootte * this.veldBreedte + 1;
        this.Location = locatie;
        this.BackColor = Color.Green;
        //Vaagheid uit de designer om een achtergrond in te stellen
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Veld));
        this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
        this.Name = "Veld";
        this.Paint += this.updateSpel;
        this.MouseClick += this.doeZet;
        this.MouseMove += this.beweegCursor;
        //Dubbele graphics buffering tegen flikkeren van de achtergrond
        this.DoubleBuffered = true;

        //Stel standaard waarden in
        this.InitVeld();
    }

    public void InitVeld()
    {
        //Reset het spelgeheugen
        this.geheugen = new int[this.veldBreedte, this.veldHoogte];
        this.Beurt = 0;
        this.Hint = false;
        this.Gestart = true;

        //Plaats 4 stenen in het midden van het veld
        int middenX = (int)Math.Ceiling((double)this.veldBreedte / 2);
        int middenY = (int)Math.Ceiling((double)this.veldHoogte / 2);
        this.geheugen[middenX - 1, middenY - 1] = 1;
        this.geheugen[middenX, middenY] = 1;
        this.geheugen[middenX, middenY - 1] = 2;
        this.geheugen[middenX - 1, middenY] = 2;

        this.Cursor = Cursors.Hand;
    }

    //Update alle belangrijke waarden voor het spel als de paint event van het speelveld wordt aangeroepen
    private void updateSpel(object obj, PaintEventArgs pea)
    {
        Graphics gr = pea.Graphics;
        gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        //Reset de array met geldige zetten
        this.geldig = new bool[this.veldBreedte, this.veldHoogte];

        if (this.Beurt % 2 == 0 && this.Gestart) this.Status = this.spelerNamen[0];
        else if (this.Beurt % 2 == 1 && this.Gestart) this.Status = this.spelerNamen[1];

        //Teken de stenen uit het geheugen op het bord
        this.tekenVeld(gr);

        if (Mogelijkheden == 0 && Gestart)
        {
            //Als er geen mogelijkheden zijn krijgt de speler die aan de beurt is een pas
            this.pas[this.Beurt % 2] = true;
            
            //Beindig het spel als beide spelers na elkaar hebben gepast of het veld vol ligt
            if ((this.pas[0] && this.pas[1]) || this.veldVol)
            {
                //Beurt stijgt niet meer als de laatste zet is gedaan
                this.Beurt--;
                this.eindeSpel();
            }
            //Als één van beide spelers afzonderlijk moet passen is de ander aan de beurt
            else
            {
                if (this.pas[0]) MessageBox.Show(this.spelerNamen[0] + " heeft geen mogelijke zetten. " + this.spelerNamen[1] + " is aan de beurt.", "Geen mogelijkheden");
                else if (this.pas[1]) MessageBox.Show(this.spelerNamen[1] + " heeft geen mogelijke zetten. " + this.spelerNamen[0] + " is aan de beurt.", "Geen mogelijkheden");
                this.Invalidate();
                this.Beurt++;
            }
        }
    }

    //Teken het veld vanuit het geheugen op het scherm
    private void tekenVeld(Graphics gr)
    {
        Brush kleur = default(Brush);

        for (int y = 0; y < this.veldHoogte; y++)
            for (int x = 0; x < this.veldBreedte; x++)
            {
                //Teken vakjes
                Pen lijn = new Pen(new SolidBrush(Color.FromArgb(100, 00, 00, 00)));
                gr.DrawRectangle(lijn, this.vakGrootte * x, this.vakGrootte * y, this.vakGrootte, this.vakGrootte);

                //Bepaal kleur van steen en teken steen
                if (this.geheugen[x, y] > 0)
                {
                    if (this.geheugen[x, y] == 1) kleur = this.Speler1Brush;
                    else if (this.geheugen[x, y] == 2) kleur = this.Speler2Brush;
                    gr.FillEllipse(kleur, 5 + x * this.vakGrootte, 5 + y * this.vakGrootte, this.vakGrootte - 10, this.vakGrootte - 10);
                }

                //Teken mogelijke zetten
                else if (this.valideerZet(x, y) && Hint)
                {
                    this.geldig[x, y] = true;
                    Pen pen = default(Pen);
                    if (this.Beurt % 2 == 0) pen = new Pen(this.Speler1Brush, 2);
                    else if (this.Beurt % 2 == 1) pen = new Pen(this.Speler2Brush, 2);
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    gr.DrawEllipse(pen, 5 + x * this.vakGrootte, 5 + y * this.vakGrootte, this.vakGrootte - 10, this.vakGrootte - 10);
                }

                else if (this.valideerZet(x, y))
                {
                    this.geldig[x, y] = true;
                }

                //Teken de cursor in het aangewezen vak
                if (x == this.muisVak.X && y == this.muisVak.Y)
                {
                    //Teken zonder hints (draw ellipse)
                    if (this.geldig[x, y] == false || (this.geldig[x, y] == true && !this.Hint))
                    {
                        Pen pen = default(Pen);
                        if (this.Beurt % 2 == 0) pen = new Pen(this.Speler1Brush, 2);
                        else if (this.Beurt % 2 == 1) pen = new Pen(this.Speler2Brush, 2);
                        gr.DrawEllipse(pen, 5 + x * this.vakGrootte, 5 + y * this.vakGrootte, this.vakGrootte - 10, this.vakGrootte - 10);
                    }
                    //Teken met hints (fill ellipse)
                    else if (this.geldig[x, y] == true && this.Hint)
                    {
                        Brush spelerBrush = default(Brush);
                        if (this.Beurt % 2 == 0) spelerBrush = this.Speler1Brush;
                        else if (this.Beurt % 2 == 1) spelerBrush = this.Speler2Brush;
                        gr.FillEllipse(spelerBrush, 5 + x * this.vakGrootte, 5 + y * this.vakGrootte, this.vakGrootte - 10, this.vakGrootte - 10);
                    }
                }
            }
    }

    //Feedback voor het bewegen van de muis op het speelbord
    Point muisVak = new Point(-1, -1);
    private void beweegCursor(object obj, MouseEventArgs m)
    {
        /*Verplaats de cursor alleen als de muis in een ander vak is
         * dan de vorige keer dat de muis bewoog
         */
        Point muisVak = new Point(m.X / this.vakGrootte, m.Y / this.vakGrootte);
        if (muisVak.X != this.muisVak.X || muisVak.Y != this.muisVak.Y)
        {
            this.Invalidate();
            this.muisVak = muisVak;
        }
    }

    //Geef de score van de in de parameter aangegeven speler aan de hand van het geheugen
    public int Score(int speler)
    {
        int score = 0;
        for (int y = 0; y < this.veldHoogte; y++)
            for (int x = 0; x < this.veldBreedte; x++)
                if (this.geheugen[x, y] == speler) score++;

        return score;
    }

    //Functie die wordt aangeroepen als het spel beeindigd wordt
    private void eindeSpel()
    {
        String message = "Er zijn geen mogelijke zetten meer.";
        String message2 = "";

        //De speler met de hoogste score wint
        if (Score(1) > Score(2)) message2 = " " + this.spelerNamen[0] + " wint!";
        else if (Score(1) < Score(2)) message2 = " " + this.spelerNamen[1] + " wint!";
        else message += " Remise!";

        this.Status = message2;

        this.Gestart = false;

        MessageBox.Show(message + message2, message2);
    }

    private void doeZet(object obj, MouseEventArgs m)
    {
        this.pas[this.Beurt % 2] = false;

        //Bepaal in welk vak is geklikt
        int vakX = m.X / this.vakGrootte;
        int vakY = m.Y / this.vakGrootte;

        if (this.valideerZet(vakX, vakY)) {
            //Hints worden na deze zet weer verborgen
            this.Hint = false;

            //Verander het geheugen op de plaatsen van de veroverde stenen
            this.verover(vakX, vakY);

            //Plaats nieuwe steen in het geheugen
            this.geheugen[vakX, vakY] = this.Beurt % 2 + 1;

            //Verander beurt
            this.Beurt++;

            //Teken veld opnieuw
            this.Invalidate();
        }
    }

    //Controleer of zet geldig is
    private bool valideerZet(int x, int y)
    {
        if (!isBinnenVeld(x, y)) return false;
        
        if (this.heeftSteen(x,y)) return false;

        if (!this.check(x, y)) return false;

        return true;
    }

    //Check of zet binnen het veld is
    private bool isBinnenVeld(int x, int y)
    {
        if (x < 0 || x >= this.veldBreedte || y < 0 || y >= this.veldHoogte) return false;
        if (nieuwex < 0 || x  veldBreedte || nieuwey )

        return true;
    }

    //Check of veld leeg is
    private bool heeftSteen(int x, int y)
    {
        if (this.geheugen[x, y] != 0) return true;

        return false;
    }

    //Check of de steen in het veld een andere is dan die van degene die aan de beurt is
    private bool isAndereSteen(int x, int y)
    {
        if ((this.geheugen[x, y] != this.Beurt % 2 + 1) && (this.geheugen[x, y] > 0)) return true;

        return false;
    }

    //Check of de steen in het veld dezelfde is als die van degene die aan de beurt is
    private bool isZelfdeSteen(int x, int y)
    {
        if (this.geheugen[x, y] == this.Beurt % 2 + 1) return true;
        return false;
    }

    //Check of de zet een tegel kan veroveren die ernaast zit
    private bool check(int x, int y)
    {
        if (this.checkRichting(x, y, 0, -1)) return true; //Boven
        if (this.checkRichting(x, y, 1, -1)) return true; //Rechtsboven
        if (this.checkRichting(x, y, 1, 0)) return true; //Rechts
        if (this.checkRichting(x, y, 1, 1)) return true; //Rechtsonder
        if (this.checkRichting(x, y, 0, 1)) return true; //Onder
        if (this.checkRichting(x, y, -1, 1)) return true; //Linksonder
        if (this.checkRichting(x, y, -1, 0)) return true; //Links
        if (this.checkRichting(x, y, -1, -1)) return true; //Linksboven

        else return false;
    }

    //Kijk welke stenen veroverd worden en verander het geheugen
    private void verover(int x, int y)
    {
        this.veroverRichting(x, y, 0, -1); //Boven
        this.veroverRichting(x, y, 1, -1); //Rechtsboven
        this.veroverRichting(x, y, 1, 0); //Rechts
        this.veroverRichting(x, y, 1, 1); //Rechtsonder
        this.veroverRichting(x, y, 0, 1); //Onder
        this.veroverRichting(x, y, -1, 1); //Linksonder
        this.veroverRichting(x, y, -1, 0); //Links
        this.veroverRichting(x, y, -1, -1); //Linksboven
    }

    //Check welke stenen in één van de acht richtingen veroverd mogen worden
    private void veroverRichting(int x, int y, int rx, int ry, bool volgende = false)
    {
        bool[,] veroverbaar = new bool[this.veldBreedte, this.veldHoogte];
        
        //Check of volgende stenen in deze richting hetzelfde is
        while (isBinnenVeld(x + rx, y + ry) && isAndereSteen(x + rx, y + ry))
        {
            x = x + rx;
            y = y + ry;
            veroverbaar[x, y] = true;
            volgende = true;
        }

        /*
         * Als er een andere steen is gevonden in deze richting (volgende == true)
         * en de laatst gecheckte steen was er één van degene die aan de beurt is,
         * wordt het geheugen veranderd op alle met 'veroverbaar' aangegeven plekken
         * aangepast
         */
        if (volgende && isBinnenVeld(x + rx, y + ry) && isZelfdeSteen(x + rx, y + ry))
        {
            for (int vy = 0; vy < this.veldHoogte; vy++)
                for (int vx = 0; vx < this.veldBreedte; vx++)
                    if (veroverbaar[vx, vy])
                        this.geheugen[vx, vy] = this.Beurt % 2 + 1;
        }
    }

    //Check of in één van de acht richtingen een steen te veroveren is
    private bool checkRichting(int x, int y, int rx, int ry, bool volgende = false)
    {
        if (isBinnenVeld(x + rx, y + ry))
            /*
             * Check of volgende steen in deze richting hetzelfde is
             * Als dat zo is wordt de methode opnieuw aangeroepen totdat
             * een steen van de speler die de zet doet gevonden is
             */
            if (isAndereSteen(x + rx, y + ry))
                return checkRichting(x + rx, y + ry, rx, ry, true);

            /*Als er al een andere steen in deze richting is gevonden,
             * mag gekeken worden of de volgende steen hetzelfde is
             * en dus een insluitmogelijkheid vormt
             */
            else if (volgende && isZelfdeSteen(x + rx, y + ry)) return true;
        
        return false;
    }
}