using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using Timer = System.Threading.Timer;

namespace SP_3
{
    public partial class Form1 : Form
    {
        private Random ran = new Random();
        private Thread thread;
        private Timer timer;
        private int star_destroyer = 2;
        private int score = 0;
        List<Thread> threads = new List<Thread>();

        // sound effects
        SoundPlayer sound = new SoundPlayer(@"..\..\images\imperial_march.wav");
        SoundPlayer explosion = new SoundPlayer(@"..\..\images\Explosion.wav");
        
        public Form1()
        {
            InitializeComponent();
            //set default form size to fixed
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            //buttons
            button1_New.Click += Button1_New_Click;
            button1_Stop.Click += Button1_Stop_Click;
            //form, keys
            Load += Form1_Load;
            KeyUp += Form1_KeyUp;
        }

        //key move - left- right
        private void Form1_KeyUp(object sender, KeyEventArgs e)  
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                {
                    Previous();
                    star_destroyer--;
                    if (star_destroyer == 0)
                    {
                        star_destroyer = 1;
                    }
                    Now();
                    break;
                }
                case Keys.Right:
                {
                    Previous();
                    star_destroyer++;
                    if (star_destroyer == 5)
                    {
                        star_destroyer = 4;
                    }
                    Now();
                    break;
                }
            }
        }

        // starship previous position
        private void Previous() 
        {
            foreach (var VARIABLE in Game.Controls)
            {
                if (VARIABLE is Panel && (VARIABLE as Panel).Name == $"panel{star_destroyer}_8")
                {
                    foreach (var VAR in (VARIABLE as Panel).Controls)
                    {
                        if (VAR is PictureBox)
                        {
                            Bitmap btm = new Bitmap(@"..\..\images\asteroid1.png");
                            (VAR as PictureBox).Image = btm;
                        }
                    }

                    (VARIABLE as Panel).Visible = false;
                }
            }
        }

        // starship current position
        private void Now() 
        {
            foreach (var VARIABLE in Game.Controls)
            {
                if (VARIABLE is Panel && (VARIABLE as Panel).Name == $"panel{star_destroyer}_8")
                {
                    foreach (var VAR in (VARIABLE as Panel).Controls)
                    {
                        if (VAR is PictureBox)
                        {
                            Bitmap btm = new Bitmap(@"..\..\images\SD.png");
                            (VAR as PictureBox).Image = btm;
                        }
                    }

                    (VARIABLE as Panel).Visible = true;
                }
            }
        }

        // stop game
        private void Button1_Stop_Click(object sender, EventArgs e)  
        {
            sound.Stop();
            star_destroyer = 2;
            button1_New.Enabled = true;
            button1_Stop.Enabled = false;
            Thread_Abort();
            timer.Dispose();
        }

        // clear all threads
        private void Thread_Abort() 
        {
            KeyPreview = false;

            foreach (var VARIABLE in threads)
            {
                VARIABLE.Abort();
            }
            threads.Clear();
        }

        // load form
        private void Form1_Load(object sender, EventArgs e) 
        {
            button1_New.Enabled = true;
            button1_Stop.Enabled = false;
            New_Game();
        }

        private void New_Game()  
        {
            score = 0;
            label_Summ.Text = score.ToString();
            foreach (var VARIABLE in Game.Controls)
            {
                if (VARIABLE is Panel)
                {
                    (VARIABLE as Panel).Visible = false;
                }
            }
            Now();
        }

        // new game
        private void Button1_New_Click(object sender, EventArgs e) 
        {
            sound.Play();
            button1_New.Enabled = false;
            button1_Stop.Enabled = true;
            KeyPreview = true;
            New_Game();
            TimerCallback startCallback = new TimerCallback(Job);
            timer = new Timer(startCallback);
            timer.Change(0, 2000);
        }

        // 2 second timer
        private void Job(object state) 
        {
            int poz = ran.Next(1, 5); 
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(Position);
            thread = new Thread(threadStart);
            thread.Start((object)poz); 
            threads.Add(thread);
            thread.Join();
        }

        //show position
        private void Position(object obj) 
        {
            string NamePanel = null; 
            for (int i = 0; i < 9; i++)
            {
                if (NamePanel != null)
                {
                    foreach (var VARIABLE in Game.Controls)
                    {
                        if (VARIABLE is Panel)
                        {
                            if ((VARIABLE as Panel).Name == NamePanel)
                            {
                                (VARIABLE as Panel).Invoke(
                                    new Action(() => { (VARIABLE as Panel).Visible = false; }));
                            }
                        }
                    }
                }

                foreach (var VARIABLE in Game.Controls)
                {
                    if (VARIABLE is Panel)
                    {
                        if ((VARIABLE as Panel).Name == $"panel{obj}_{i}") 
                        {
                            (VARIABLE as Panel).Invoke(new Action(() => { (VARIABLE as Panel).Visible = true; }));
                            NamePanel = $"panel{obj}_{i}";
                        }
                    }
                }

                if (i == 8)
                {
                    // is it crash?
                    if ((int)obj == star_destroyer) 
                    {
                        foreach (var VARIABLE in Game.Controls) 
                        {
                            if (VARIABLE is Panel)
                            {
                                if ((VARIABLE as Panel).Name == $"panel{star_destroyer}_8")
                                {
                                    foreach (var VAR in (VARIABLE as Panel).Controls)
                                    {
                                        if (VAR is PictureBox)
                                        {
                                            Bitmap btm = new Bitmap(@"..\..\images\explosion.png");
                                            (VAR as PictureBox).Image = btm;
                                            // sound
                                            sound.Stop();
                                            explosion.Play();
                                            Thread.Sleep(1100);
                                            explosion.Stop();
                                        }
                                    }

                                    (VARIABLE as Panel).Invoke(new Action(() => { (VARIABLE as Panel).Visible = true; }));
                                    // delete threads
                                    Task task = Task.Factory.StartNew(() => Thread_Abort());
                                    timer.Dispose();
                                    button1_New.Invoke(new Action(() =>
                                    {
                                        button1_New.Enabled = true;
                                    }));
                                    button1_Stop.Invoke(new Action(() =>
                                    {
                                        button1_Stop.Enabled = false;
                                    }));
                                    task.Wait();

                                    break;
                                }
                            }
                        }
                    }

                }
                Thread.Sleep(500);
                if (i == 8)
                {
                    // is it crash from sideway?
                    if ((int)obj == star_destroyer)
                    {
                        foreach (var VARIABLE in Game.Controls)
                        {
                            if (VARIABLE is Panel)
                            {
                                if ((VARIABLE as Panel).Name == $"panel{star_destroyer}_8")
                                {
                                    foreach (var VAR in (VARIABLE as Panel).Controls)
                                    {
                                        if (VAR is PictureBox)
                                        {
                                            Bitmap btm = new Bitmap(@"..\..\images\explosion.png");
                                            (VAR as PictureBox).Image = btm;
                                            //sound
                                            sound.Stop();
                                            explosion.Play();
                                            Thread.Sleep(1100);
                                            explosion.Stop();
                                        }
                                    }

                                    (VARIABLE as Panel).Invoke(new Action(() => { (VARIABLE as Panel).Visible = true; }));
                                    Task task = Task.Factory.StartNew(() => Thread_Abort());
                                    timer.Dispose();
                                    button1_New.Invoke(new Action(() =>
                                    {
                                        button1_New.Enabled = true;
                                    }));
                                    button1_Stop.Invoke(new Action(() =>
                                    {
                                        button1_Stop.Enabled = false;
                                    }));
                                    task.Wait();
                                    break;
                                }
                            }
                        }
                    }

                }
                // score adding
                if (i == 8)
                {
                    score += 100;
                    label_Summ.Invoke(new Action(() => { label_Summ.Text = score.ToString(); }));
                    foreach (var VARIABLE in Game.Controls) 
                    {
                        if (VARIABLE is Panel)
                        {
                            if ((VARIABLE as Panel).Name == $"panel{obj}_{i}")
                            {
                                (VARIABLE as Panel).Invoke(new Action(() => { (VARIABLE as Panel).Visible = false; }));
                                NamePanel = $"panel{obj}_{i}";
                            }
                        }
                    }
                }
            }
        }
    }
}
