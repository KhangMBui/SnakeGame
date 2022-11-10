using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging; //JPG Compressor
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media; 

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        SoundPlayer biteSound = new SoundPlayer("biteEffect.wav");
        SoundPlayer bonkSound = new SoundPlayer("bonkEffect.wav");
        SoundPlayer beanSound = new SoundPlayer("beanScreamEffect.wav");
        SoundPlayer magicSound = new SoundPlayer("magicEffect.wav");

        private List<Circle> Snake = new List<Circle>();
        private Circle food = new Circle();

        Label caption;

        int maxW;
        int maxH;

        int foodNumber = 1;
        bool Bonus = false;

        int score;
        int highScore;

        Random rand = new Random();

        bool goUp, goDown, goLeft, goRight;
        public Form1()
        {
            InitializeComponent();
            new Settings();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            cbGameMode.Items.Add("No Wall");
            cbGameMode.Items.Add("Wall");
            cbGameMode.DropDownStyle = ComboBoxStyle.DropDownList;
            
        }
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up && Settings.directions != "down")
                goUp = true;
            if (e.KeyCode == Keys.Down && Settings.directions != "up")
                goDown = true;
            if (e.KeyCode == Keys.Left && Settings.directions != "right")
                goLeft = true;
            if (e.KeyCode == Keys.Right && Settings.directions != "left")
                goRight = true;
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                goUp = false;
            if (e.KeyCode == Keys.Down)
                goDown = false;
            if (e.KeyCode == Keys.Left)
                goLeft = false;
            if (e.KeyCode == Keys.Right)
                goRight = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            RestartGame();
        }

        private void btnSnap_Click(object sender, EventArgs e)
        {
            pictureBox.Controls.Remove(caption);
            caption = new Label();
            if (score<=5)
                caption.Text = "Score: " + score + ". High score: " + highScore + ". I am just a baby snake.";
            else if (score <=10)
                caption.Text = "Score: " + score + ". High score: " + highScore + ". I am a teen snake.";
            else if (score <=15)
                caption.Text = "Score: " + score + ". High score: " + highScore + ". I am an adult snake.";
            else
                caption.Text = "Score: " + score + ". High score: " + highScore + ". I am the king of the snakes.";
            caption.Font = new Font("Aerial", 12, FontStyle.Bold);
            caption.ForeColor = Color.DarkRed;
            caption.AutoSize = false;
            caption.Width = pictureBox.Width;
            caption.Height = 30;
            caption.TextAlign = ContentAlignment.MiddleCenter;
            pictureBox.Controls.Add(caption);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "Snake game snapshot.";
            dialog.DefaultExt = "jpg";
            dialog.Filter = "JPG file | *.jpg";
            //Make name stable can't be wrong.
            dialog.ValidateNames = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int width = Convert.ToInt32(pictureBox.Width);
                int height = Convert.ToInt32(pictureBox.Height);
                Bitmap bmp = new Bitmap(width, height);
                pictureBox.DrawToBitmap(bmp, new Rectangle(0,0, width, height));
                bmp.Save(dialog.FileName, ImageFormat.Jpeg);
            }
        }

        private void TimerEvent(object sender, EventArgs e)
        {
            if (goUp)
                Settings.directions = "up";
            if (goDown)
                Settings.directions = "down";
            if (goLeft)
                Settings.directions = "left";
            if (goRight)
                Settings.directions = "right";

            for (int i = Snake.Count-1; i>= 0; i--)
            {
                if (i == 0)
                {
                    switch(Settings.directions)
                    {
                        case "up":
                            Snake[i].Y--;
                            break;
                        case "down":
                            Snake[i].Y++;
                            break;
                        case "left":
                            Snake[i].X--;
                            break;
                        case "right":
                            Snake[i].X++;
                            break;
                    }

                    if (cbGameMode.Text == "Wall")
                    {
                        if (Snake[i].X < 0 || Snake[i].X > maxW || Snake[i].Y > maxH || Snake[i].Y < 0)
                        {
                            bonkSound.Play();
                            GameOver();
                        }
                    }
                    else
                    {
                        if (Snake[i].X < 0)
                            Snake[i].X = maxW;

                        if (Snake[i].X > maxW)
                            Snake[i].X = 0;

                        if (Snake[i].Y > maxH)
                            Snake[i].Y = 0;

                        if (Snake[i].Y < 0)
                            Snake[i].Y = maxH;
                    }
                    

                    if (Snake[i].X == food.X && Snake[i].Y == food.Y)
                        EatFood();

                    for (int j = 1; j<Snake.Count-1; j++)
                    {
                        if (Snake[i].X == Snake[j].X && Snake[i].Y == Snake[j].Y)
                        {
                            beanSound.Play();
                            GameOver();
                        }
                    }
                }
                else
                {
                    Snake[i].X = Snake[i - 1].X;
                    Snake[i].Y = Snake[i - 1].Y;
                }

                pictureBox.Invalidate();
            }
        }

        private void UpdatePictureBox(object sender, PaintEventArgs e)
        {
            Graphics gameGraphics = e.Graphics;

            Brush snakeColor;

            for (int i = 0; i< Snake.Count; i++)
            {
                if (i == 0)
                    snakeColor = Brushes.DarkRed;
                else
                    snakeColor = Brushes.DarkGreen;

                gameGraphics.FillEllipse(snakeColor, new Rectangle
                    (
                    Snake[i].X * Settings.Width,
                    Snake[i].Y * Settings.Height,
                    Settings.Width, Settings.Height
                    )) ;
            }

            if ((foodNumber%10==5 || foodNumber%5==0))
            {
                //Create random colored brush
  
                Random rand = new Random();
                Type brushesType = typeof(Brushes);
                PropertyInfo[] properties = brushesType.GetProperties();
                int random = rand.Next(properties.Length);
                Brush result = (Brush)properties[random].GetValue(null, null); 

                gameGraphics.FillEllipse(result, new Rectangle
                    (
                    food.X * Settings.Width,
                    food.Y * Settings.Height,
                    Settings.Width, Settings.Height
                    ));
                Bonus = true;
            }
            else
            {
                gameGraphics.FillEllipse(Brushes.Green, new Rectangle
                    (
                    food.X * Settings.Width,
                    food.Y * Settings.Height,
                    Settings.Width, Settings.Height
                    ));
            }
            
        }
        private void RestartGame()
        {
            if (cbGameMode.SelectedIndex == -1)
            {
                MessageBox.Show("Please select your game mode.");
            }
            else
            {
                cbGameMode.Enabled = false;
                pictureBox.Controls.Remove(caption);
                maxW = pictureBox.Width / Settings.Width - 1;
                maxH = pictureBox.Height / Settings.Height - 1;

                Snake.Clear();

                btnStart.Enabled = false;
                btnSnap.Enabled = false;
                score = 0;
                lblScore.Text = "Score: " + score;

                Circle head = new Circle { X = 20, Y = 5 };
                Snake.Add(head);

                for (double i = 0; i < 4; i = i + 1)
                {
                    Circle body = new Circle();
                    Snake.Add(body);
                }
                food = new Circle { X = rand.Next(2, maxW), Y = rand.Next(2, maxH) };
                gameTimer.Start();
            }
            
        }

        private void EatFood()
        {
            foodNumber += 1;
            if (Bonus == true)
            {
                score += 3;
                Bonus = false;
                magicSound.Play();
            }

            else
            {
                score += 1;
                biteSound.Play();
            }
                
            lblScore.Text = "Score: " + score;
            Circle body = new Circle
            {
                X = Snake[Snake.Count - 1].X,
                Y = Snake[Snake.Count - 1].Y
            };
            Snake.Add(body);

            food = new Circle { X = rand.Next(2, maxW), Y = rand.Next(2, maxH) };
            
        }
        
        private void GameOver()
        {
            
            gameTimer.Stop();
            btnStart.Enabled = true;
            btnSnap.Enabled = true;
            caption = new Label();
            caption.Text = "GAME OVER!";
            caption.Font = new Font("Aerial", 16, FontStyle.Bold);
            caption.ForeColor = Color.DarkRed;
            caption.AutoSize = false;
            caption.Width = pictureBox.Width;
            caption.Height = 30;
            caption.TextAlign = ContentAlignment.MiddleCenter;
            pictureBox.Controls.Add(caption);
            foodNumber = 1;
            cbGameMode.Enabled = true;

            if (score> highScore)
            {
                highScore = score;
                lblhighScore.Text = "High Score: " + Environment.NewLine + highScore;
                lblhighScore.ForeColor = Color.Maroon;
                lblhighScore.TextAlign = ContentAlignment.MiddleCenter;
            }
            
        }
    }
}
