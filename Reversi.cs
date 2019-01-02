void Plaatsstenen(int nieuwex, int nieuwey, Piece color)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    {
                        try
                        {
                            int teller = 1;
                            while (stenen[nieuwex + (dx  teller), nieuwey + (dy  teller)] != Piece.color)
                            {
                                teller++;
                            }
                            if (stenen[nieuwex + (dx  teller), nieuwey + (dy  teller)] == Piece.color)
                            {
                                while (teller >= 1)
                                {
                                    stenen[nieuwex + (dx  teller), nieuwey + (dy  teller)] = Piece.color;
                                    teller--;
                                }
                            }
                        }catch (IndexOutOfRangeException e) { }
                    }
                }
            }
            beurt++;
            //huidigespelstatus = huidigespelstatus == "Zwart" ? "Wit" : "Zwart";
        }
