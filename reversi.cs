using System;
using System.Drawing;
using System.Windows.Forms;

namespace Reversi
{
    public class Reversi : Form
    {
        int[,] board;
        int width, height;
        int stoneSize = 50;
        int stoneMargin = 10;
        int offset = 100;
        int possibleMoves = 0;

        /*
         * status 1 = player blue
         * status 2 = player red
         * status 3 = blue won
         * status 4 = red won
         * status 5 = tie
         */
        int status = 1;

        bool help = true;
        int playersPassed = 0;

        public Reversi()
        {
            board = new int[6, 6];
            this.width = 6;
            this.height = 6;
            this.InitialiseEventHandlers();
        }

		public Reversi(int width, int height)
		{
            if (width < 3) { width = 3; }
            if (height < 3) { height = 3; }
            this.width = width;
            this.height = height;
            board = new int[width, height];
            this.InitialiseEventHandlers();
		}

        void InitialiseEventHandlers()
        {
            this.ClientSize = new Size(width*stoneSize + offset*2, height*stoneSize + offset*2);
            this.BackColor = Color.White;
            this.addInitialStones();
            this.Paint += drawBoard;
            this.Paint += calculatePossibleMoves;
            this.Paint += drawStones;
            this.Paint += drawGameState;
            this.MouseClick += addStone;
        }

        public void DebugReversi()
        {
            System.Diagnostics.Debug.WriteLine("------------");
            System.Diagnostics.Debug.WriteLine("Possible moves: " + this.possibleMoves);
            System.Diagnostics.Debug.WriteLine("Players passed: " + this.playersPassed);
            System.Diagnostics.Debug.WriteLine("Currentplayer: " + this.status);

            for (int i = 0; i < this.width; i++)
            {
                for (int j = 0; j < this.height; j++)
                {
                    System.Diagnostics.Debug.Write(board[i, j]);
                }
                System.Diagnostics.Debug.Write('\n');
            }
            System.Diagnostics.Debug.WriteLine("------------");
            System.Diagnostics.Debug.Write('\n');
        }

        void drawBoard(Object obj, PaintEventArgs pea)
        {
            this.InitialiseMenuItems();
			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
                    pea.Graphics.DrawRectangle(Pens.Gray, i*stoneSize + offset, j*stoneSize + offset, stoneSize, stoneSize);
				}
			}
        }

        void drawStones(Object obj, PaintEventArgs pea)
        {
			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
                    // blue stones
                    if (board[i,j] == 1)
                    {
                        pea.Graphics.FillEllipse(Brushes.Blue, i * stoneSize + offset + stoneMargin / 2, j * stoneSize + offset + stoneMargin / 2, stoneSize - stoneMargin, stoneSize - stoneMargin);
                    }

                    // red stones
                    else if (board[i,j] == 2)
                    {
                        pea.Graphics.FillEllipse(Brushes.Red, i * stoneSize + offset + stoneMargin / 2, j * stoneSize + offset + stoneMargin /2, stoneSize - stoneMargin, stoneSize - stoneMargin);
                    }

                    // possible moves
                    else if (board[i,j] == -1 && help)
                    {
                        pea.Graphics.DrawEllipse(Pens.Black, i * stoneSize + offset + stoneMargin, j * stoneSize + offset + stoneMargin, stoneSize - stoneMargin * 2, stoneSize - stoneMargin * 2);
                    }

					// debug stone
					else if (board[i, j] == -2)
					{
                        pea.Graphics.FillEllipse(Brushes.Yellow, i * stoneSize + offset + stoneMargin / 2, j * stoneSize + offset + stoneMargin / 2, stoneSize - stoneMargin, stoneSize - stoneMargin);
					}
				}
			}
        }

        void calculatePossibleMoves(Object obj, PaintEventArgs pea)
        {
            this.cleanPossibleMoves();
            int oppositePlayer = getOppositePlayer();
            int possibleMovesCount = 0;

			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
                    // check each stone of the current player
                    if (board[i,j] == status)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            for (int l = -1; l <= 1; l++)
                            {
                                try {
                                    // check if there are stones of the opposite player around the stone
                                    if (board[i + k, j + l] == oppositePlayer)
                                    {
                                        int count = 1;
                                        while (board[i + (k * count), j + (l * count)] == oppositePlayer)
                                        {
                                            count++;
                                        }

                                        // check if the next stone is empty
                                        if (board[i + (k * count), j + (l * count)] == 0)
                                        {
                                            board[i + (k * count), j + (l * count)] = -1;
                                            possibleMovesCount++;
                                        }
                                    }
								}
								catch (IndexOutOfRangeException e) {}
                            }
                        }
                    }
				}
			}

			// check if there are no moves available
			if (!(possibleMoves > 0))
			{
				playersPassed++;
			}
			else
			{
				playersPassed = 0;
			}

            this.possibleMoves = possibleMovesCount;
        }

        void drawGameState(Object obj, PaintEventArgs pea)
        {
            string player = "";
            player = (status == 1) ? "blue" : "red";
            Brush b = (status == 1) ? Brushes.Blue : Brushes.Red;

            // Current player
            pea.Graphics.DrawString(String.Format("Current player is:         {0}", player), this.Font, Brushes.Black, offset, offset/2);
            pea.Graphics.FillEllipse(b, offset + 88, offset / 2 + 2, 10, 10);

            // Player counts
            int blueStoneCount = getStoneCount(1);
            int redStoneCount = getStoneCount(2);
            pea.Graphics.FillEllipse(Brushes.Blue, offset + 170, offset / 2 + 2 - 10, 10, 10);
            pea.Graphics.FillEllipse(Brushes.Red, offset + 170, offset / 2 + 2 + 10, 10, 10);
            pea.Graphics.DrawString(String.Format("Blue player: {0} stones", blueStoneCount), this.Font, Brushes.Black, offset + 190, offset / 2 - 10);
            pea.Graphics.DrawString(String.Format("Red player: {0} stones", redStoneCount), this.Font, Brushes.Black, offset + 190, offset / 2 + 10);

            if (!(possibleMoves > 0) && playersPassed < 2)
            {
                changePlayer();
                this.Invalidate();
            }

            // check if game ended
            if (!(possibleMoves > 0) && playersPassed == 2)
            {
                Brush ba = new SolidBrush(Color.FromArgb(235, 255, 255, 255));
                pea.Graphics.FillRectangle(ba, 0, 0, width * stoneSize + offset * 2, height * stoneSize + offset * 2);
                bool remise = (blueStoneCount == redStoneCount);

                if (!remise)
                {
                    string wonPlayer = "";
                    wonPlayer = (blueStoneCount > redStoneCount) ? "Blue" : "Red";
                    Brush wonColor = (blueStoneCount > redStoneCount) ? Brushes.Blue : Brushes.Red;

                    pea.Graphics.FillEllipse(wonColor, (width * stoneSize + offset * 2) / 2 - 20, (height * stoneSize + offset * 2) / 2 - 60, 40, 40);
                    pea.Graphics.DrawString(String.Format("{0} player won", wonPlayer), new Font("Tahoma", 24), Brushes.Black, (width * stoneSize + offset * 2) / 2 - 115, (height * stoneSize + offset * 2) / 2);
                }
                else 
                {
                    pea.Graphics.DrawString("Remise!", new Font("Tahoma", 24), Brushes.Black, (width * stoneSize + offset * 2) / 2 - 55, (height * stoneSize + offset * 2) / 2 - 40);
                }
            }
        }

        void addInitialStones()
        {
            int tempx = width / 2;
            int tempy = height / 2;

            board[tempx-1, tempy-1] = 1;
            board[tempx, tempy-1] = 2;
            board[tempx-1, tempy] = 2;
            board[tempx, tempy] = 1;
        }

        void addStone(Object obj, MouseEventArgs mea)
        {
            // check if within bounds
            if (
                mea.X > offset
                && mea.Y > offset
                && mea.X < offset + width * stoneSize
                && mea.Y < offset + height * stoneSize
            )
            {

                int x = mea.X - offset;
                int y = mea.Y - offset;
                int stoneX = x / stoneSize;
                int stoneY = y / stoneSize;

                // check if the move is possible
                if (board[stoneX, stoneY] == -1)
                {
					board[stoneX, stoneY] = status;
                    changeEnclosedStones(stoneX, stoneY);
					changePlayer();
					this.Invalidate();   
                }
            }
        }

        void changeEnclosedStones(int stoneX, int stoneY)
        {
            int oppositePlayer = getOppositePlayer();

			for (int i = -1; i <= 1; i++)
			{
                for (int j = -1; j <= 1; j++)
                {
                    try
                    {
                        int count = 1;
                        while (board[stoneX + (i * count), stoneY + (j * count)] == oppositePlayer)
                        {
                            count++;
                        }

                        // check if enclosed with own stone
                        if (board[stoneX + (i * count), stoneY + (j * count)] == status)
                        {
                            while (count >= 1)
                            {
                                board[stoneX + (i * count), stoneY + (j * count)] = status;
                                count--;
                            }
                        }
                    } catch (IndexOutOfRangeException e) {}
				}
			}
        }

        void cleanPossibleMoves()
        {
			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
                    if (board[i, j] == -1)
                    {
                        board[i, j] = 0;
                    }
				}
			}
        }

        int getStoneCount(int player)
        {
            int count = 0;
			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
                    if (board[i, j] == player)
					{
                        count++;
					}
				}
			}
            return count;
        }

        int getOppositePlayer()
        {
            if (status == 1)
            {
                return 2;
            } else
            {
                return 1;
            }
        }

        void changePlayer()
        {
            status = getOppositePlayer();
        }

        void toggleHelp(Object obj, EventArgs ea)
        {
            this.help = !this.help;
            this.Invalidate();
        }

        void resetGame(Object obj, EventArgs ea)
        {
			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
					board[i, j] = 0;
				}
			}
            this.addInitialStones();
            this.status = 1;
            this.playersPassed = 0;
            this.Invalidate();
        }

        void InitialiseMenuItems()
        {
			MainMenu mainMenu = new MainMenu();

			MenuItem menuNieuw = new MenuItem("Nieuw spel");
            menuNieuw.Click += this.resetGame;

            MenuItem menuHulp = new MenuItem("Schakel hulp in/uit");
            menuHulp.Click += this.toggleHelp;

			mainMenu.MenuItems.Add(menuNieuw);
            mainMenu.MenuItems.Add(menuHulp);

            this.Menu = mainMenu;
        }
    }
}