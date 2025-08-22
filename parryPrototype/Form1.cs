using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;

namespace parryPrototype
{
    public partial class Form1 : Form
    {

        readonly new Entity defendBox = new 
            (origin: new Point(50, 250),
             width: 50,
             height: 50);
        Brush playerBrush;


        bool 
            movingUp = false,
            movingDown = false,
            movingLeft = false,
            movingRight = false,
            setFreeze,
            isParrying = false;
            

        const int 
            parryDuration = 30,
            perfectParryWindow = 8,
            playerVelocity = 50,
            slowFrameDuration = 35,
            freezeFrameDurationMS = 1500;

        int 
            targetFrameRate = 144,
            refreshRate;

        double
            parryWindow,
            slowFrame = 0,
            freezeFrameMS = 0;

        const float 
            zoomFactor = 3.35F, 
            slowFactor = 5;

        float currentSlowFactor = 1;
        
        double
            prevTime = 0,
            deltaTime = 0;
        
        Stopwatch stopWatch = new Stopwatch();


        int 
            bulletCooldown = 50,
            bulletInterval;

        //Point mousePos;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            // set height and width of window
            Width = 1460;
            Height = 770;

            refreshRate = (int)(1000 / targetFrameRate); 

            timer1.Enabled = true;
            timer1.Interval = refreshRate;
            System.Windows.Forms.Timer deltaTimer = new System.Windows.Forms.Timer(); 
            // deltaTimer.Interval = 1;
            // deltaTimer.Tick += getDeltaTime;
            // deltaTimer.Enabled = true;
            
            bulletInterval = bulletCooldown;

            playerBrush = Brushes.Blue;

            parryWindow = parryDuration;
            stopWatch.Start();

            Thread collisionThread = new Thread(() =>{
                    while(true)
                    {
                        getDeltaTime();
                        collisionHandler();
                        this.Invoke(() => this.Invalidate());
                        Thread.Sleep(1);
                    }
            });

            collisionThread.Start();

        }

        private void getDeltaTime(/* object sender, EventArgs e */)
        {
            if (freezeFrameMS > 0)
            {
                deltaTime = 0;
                return;
            }

            double currentTime = stopWatch.Elapsed.TotalSeconds*10;
            deltaTime = currentTime - prevTime;
            prevTime = currentTime;
        }

        
        private void collisionHandler()
        {
            if (slowFrame > 0)
                deltaTime /= slowFactor;

            foreach (Projectile bullet in Projectile.ProjectileList)
            {
                bullet.moveProjectile(deltaTime);

                if (defendBox.getHitbox().IntersectsWith(bullet.getHitbox()))
                {

                    if (isParrying)
                    {
                        bullet.rebound(defendBox.getCenter()); // required to prevent getting hit anyway when parrying

                        // if the current parry has lasted for at most the perfectParryWindow
                        if (parryWindow >= parryDuration - perfectParryWindow)
                        {
                            setFreeze = true;
                            //slowFrame = slowFrameDuration;
                            //slowTick =  (int)currentSlowFactor;
                            zoomScreen(zoomFactor);
                            continue; // so that the projectile is not disposed of when rebounded
                        }
                    }

                    else
                    {
                        playerBrush = Brushes.Red; // visual hit indicator
                        setFreeze = true;
                        //slowFrame = slowFrameDuration;
                        //slowTick =  (int)currentSlowFactor;
                    }

                    disposedProjectiles.Add(bullet);

                }
            }

            foreach (Projectile p in disposedProjectiles)
                Projectile.ProjectileList.Remove(p);

            disposedProjectiles.Clear();

            if (movingUp)
                playerMove(y: -playerVelocity*deltaTime);
            if (movingDown)
                playerMove(y: playerVelocity*deltaTime);
            if (movingRight)
                playerMove(x: playerVelocity*deltaTime);
            if (movingLeft)
                playerMove(x: -playerVelocity*deltaTime);
        }


        // spawn bullet about a point
        private void createBullet()
        {
            Projectile bullet = new Projectile
                (origin: new Point(800, 250),
                  width: 30,
                  height: 10,
                  velocity: 50,
                  target: defendBox.getCenter());
        }




        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // if Not parrying then resets parrywindow and sets to parrying
            if ((e.Button == MouseButtons.Right) && (!isParrying))
            {
                parryWindow = parryDuration;
                isParrying = true;
            }
        }



        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            // stops parrying when mouseup but doesnt reset timer > only on mouse down 
            if (e.Button == MouseButtons.Right)
            {
                isParrying = false;
            }
        }



        // draws player and all projetiles
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(playerBrush, defendBox.getHitbox());

            foreach (Projectile bullet in Projectile.ProjectileList)
                e.Graphics.FillRectangle(Brushes.Red, bullet.getHitbox());
        }


        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        List<Projectile> disposedProjectiles = new List<Projectile>(); 
        bool isScaled = false; // todo: maybe move into class
        int slowTick = 0;
                               
        // main game tick timer 
        private void timer1_Tick(object sender, EventArgs e)
        {
            setFreeze = false;

            // checks if frozen 
            if (freezeFrameMS > 0)
            {
                freezeFrameMS -= refreshRate;
                this.Refresh();
                return;
            }
            // if not frozen continue:


            currentSlowFactor = 1;
            // checks if slowed
            if (slowFrame > 0) // todo: functionize all the slow logic
            {

                if (zoomFactor <= 1)
                    throw new ArgumentException("zoomFactor must be bigger than 1");

                currentSlowFactor = 1/(zoomFactor); // accounts for scale when applying velocity changes

                slowFrame -= deltaTime;
            }

            if ((slowFrame <= 0 || freezeFrameMS <= 0) && isScaled)
            {
                unZoomScreen(zoomFactor);
                isScaled = false;
            }


            label2.Text = ($"({this.Size.Width}, {this.Size.Height})"); // debugging
            // mousePos = System.Windows.Forms.Cursor.Position;
            
            // creates bullets based on an interval
            if (slowTick == 0 || slowFrame == 0) 
            {
                if (bulletInterval > 0)
                    bulletInterval -= 1;
                else
                {
                    createBullet();
                    bulletInterval = bulletCooldown;
                }


                // ticks down the parry window
                if (isParrying && parryWindow > 0)
                    parryWindow -= 1;
                if (parryWindow < 1)
                    isParrying = false;

                // debugging/visual indicator for parry
                if (isParrying)
                    playerBrush = Brushes.Gray;
                else
                    playerBrush = Brushes.Blue;

            }

            latestBulletInfo(); // debugging

            if (setFreeze)
                freezeFrameMS = freezeFrameDurationMS;


            this.Refresh();
            GC.Collect();
        }




        // for debugging
        private void latestBulletInfo()
        {
            if (Projectile.ProjectileList.Count > 0)
            {
                Projectile p = Projectile.ProjectileList.Last();
                // p's y/x distance from defendBox
                label1.Text = p.yDiff.ToString();
                label3.Text = p.xDiff.ToString();
                label4.Text = p.velocityAngle.ToString();
            }
        }




        // functionised for... some reason
        private void playerMove(double x = 0, double y = 0)
        {
            defendBox.updateLocation(defendBox.getLocation().X + (float)x, defendBox.getLocation().Y + (float)y);
        }



        PointF mcPrevCenter; // previous center position of defendBox

        private void zoomScreen(float scaleF)
        {
            isScaled = true;

            // gets center of screen
            float midX = this.Width / 2;
            float midY = this.Height / 2;

            mcPrevCenter = defendBox.getCenter();

            // calculates new position for each projectile based on distance from defendBox center and adjusts for Scale and the "screen" shifting to the center
            foreach (Projectile p in Projectile.ProjectileList)
            {
                float XDiff = p.getCenter().X - mcPrevCenter.X;
                float YDiff = p.getCenter().Y - mcPrevCenter.Y;

                float newPrjX = midX + XDiff*scaleF;
                float newPrjY = midY + YDiff*scaleF;

                p.updateCenter(newPrjX, newPrjY);
                p.scaleHitbox(scaleF);
                this.Invalidate();
            }

            defendBox.updateCenter(midX, midY);
            defendBox.scaleHitbox(scaleF);
        }



        // reverse of zoomScreen()
        private void unZoomScreen(float scaleF)
        {
            float midX = this.Width / 2;
            float midY = this.Height / 2;

            foreach (Projectile p in Projectile.ProjectileList)
            {
                float XDiff = p.getCenter().X - midX;
                float YDiff = p.getCenter().Y - midY;

                float oldPrjX = mcPrevCenter.X + XDiff/scaleF;
                float oldPrjY = mcPrevCenter.Y + YDiff/scaleF;

                p.updateCenter(oldPrjX, oldPrjY);
                p.resetScale();
            }

            defendBox.updateCenter(mcPrevCenter.X, mcPrevCenter.Y);
            defendBox.resetScale();
            isScaled = false; // screen is no longer scaled
        }



        // todo: use booleans so that the player cannot move during freezeFrame
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case  Keys.W:
                    movingUp = true;
                    break;
                case  Keys.S:
                    movingDown = true;
                    break;
                case  Keys.A:
                    movingLeft = true;
                    break;
                case  Keys.D:
                    movingRight = true;
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        { 
            switch (e.KeyCode)
            {
                case  Keys.W:
                    movingUp = false;
                    break;
                case  Keys.S:
                    movingDown = false;
                    break;
                case  Keys.A:
                    movingLeft = false;
                    break;
                case  Keys.D:
                    movingRight = false;
                    break;
            }
        }



    }
}
