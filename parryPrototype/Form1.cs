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
        
        PointF bulletOrigin = new Point (800, 250);

        CancellationTokenSource threadTokenSrc = new CancellationTokenSource();
        Thread collisionThread = new Thread(() => { });

        hpBarUI hpBar;

        bool
            movingUp = false,
            movingDown = false,
            movingLeft = false,
            movingRight = false,
            playerIsHit = false,
            isParrying = false,
            setFreeze = false,
            slowedMov = false;


        const int playerVelocity = 50;


        int
            maxHp = 6,
            currentHp,
            targetFrameRate = 144,
            refreshRate;

        const float
            zoomFactor = 3.35F,
            slowFactor = 2.5F,
            parryDurationS = 0.3F,
            perfectParryWindowS = 0.08F,
            slowDurationS = 0.35F,
            bulletCooldownS = 0.5F,
            freezeDuratonS = 0.15F;

        float
            bulletInterval,
            parryWindow,
            curZoom = 1,
            slowFrame = 0,
            freezeFrame = 0;

        double
            prevTime = 0,
            deltaTime = 0;

        Stopwatch stopWatch = new Stopwatch();


        //Point mousePos;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            // set height and width of window
            Width = 1460;
            Height = 770;

            refreshRate = (int)(1000 / targetFrameRate);
            timer1.Interval = refreshRate;
            timer1.Enabled = true;


            bulletInterval = bulletCooldownS;
            currentHp=maxHp;
            computeHP();


            playerBrush = Brushes.Blue;

            stopWatch.Start();

            CancellationToken threadCT = threadTokenSrc.Token;
            collisionThread = new Thread(() =>
            {
                while (true)
                {
                    if (threadCT.IsCancellationRequested)
                        return;
                    getDeltaTime();
                    collisionHandler();
                    //this.BeginInvoke(() => this.Invalidate());
                    Thread.Sleep(refreshRate/4);
                }
            });

            collisionThread.Start();

        }

        private void getDeltaTime(/* object sender, EventArgs e */)
        {
            double currentTime = stopWatch.Elapsed.TotalSeconds;
            deltaTime = (currentTime - prevTime) * 10;
            prevTime = currentTime;
        }


        private void collisionHandler()
        {

            // checks if frozen 
            if (freezeFrame > 0)
            {
                freezeFrame -= (float)deltaTime;
                return;
            }
            // if not frozen continue:


            playerIsHit = false;
            setFreeze = false;
            freezeFrame = 0;


            // checks if slowed
            if (slowFrame > 0) // todo: functionize all the slow logic
            {
                 slowedMov = true;

                if (zoomFactor <= 1)
                    throw new ArgumentException("zoomFactor must be bigger than 1");

                slowFrame -= (float)deltaTime;

                deltaTime /= (zoomFactor / slowFactor);
            }
            else if (slowedMov)
            {
                movingUp=false; 
                movingDown=false; 
                movingLeft=false; 
                movingRight=false; 
                slowedMov=false;
            }



            foreach (Projectile bullet in Projectile.ProjectileList)
            {
                bullet.moveProjectile(deltaTime);

                if (defendBox.getHitbox().IntersectsWith(bullet.getHitbox()))
                {

                    if (isParrying)
                    {
                        bullet.rebound(defendBox.getCenter()); // required to prevent getting hit anyway when parrying

                        // if the current parry has lasted for at most the perfectParryWindow
                        if (parryWindow >= parryDurationS - perfectParryWindowS * 10)
                        {
                            //setFreeze = true;
                            slowFrame = slowDurationS*10;
                            zoomScreen(zoomFactor);
                            continue; // so that the projectile is not disposed of when rebounded
                        }
                    }

                    else
                    {
                        playerIsHit = true;
                        setFreeze = true;
                    }

                    disposedProjectiles.Add(bullet);

                }
            }

            if (setFreeze)
                freezeFrame = freezeDuratonS * 10;

            foreach (Projectile p in disposedProjectiles)
                Projectile.ProjectileList.Remove(p);

            disposedProjectiles.Clear();


            if ((bulletInterval > 0) || (deltaTime == 0))
                bulletInterval -= (float)deltaTime;
            else
            {
                createBullet();
                bulletInterval = bulletCooldownS * 10;
            }


            // ticks down the parry window
            if (isParrying && parryWindow > 0)
                parryWindow -= (float)deltaTime;
            if (parryWindow < 1)
                isParrying = false;


            if (movingUp)
                playerMove(y: -playerVelocity * deltaTime);
            if (movingDown)
                playerMove(y: playerVelocity * deltaTime);
            if (movingRight)
                playerMove(x: playerVelocity * deltaTime);
            if (movingLeft)
                playerMove(x: -playerVelocity * deltaTime);

        }



        // spawn bullet about a point
        private void createBullet()
        {
            Projectile bullet = new Projectile
                (origin: bulletOrigin,
                  width: 30,
                  height: 10,
                  velocity: 50,
                  target: defendBox.getCenter());
            bullet.scaleHitbox(curZoom);
        }



        const int hpIconOffset = 50; 

        private void computeHP()
        {
            hpBar = new hpBarUI(
                    origin: new PointF(70,50),
                    barWidth: 20,
                    barHeight: 40,
                    iconCount: maxHp / 2
            );

            float xOffset = 0;
            int tempHpStore = currentHp;

            for (int i = 0; i < hpBar.IconCount; i++)
            {
                PointF rectangleOrigin = new PointF(hpBar.Origin.X + xOffset, hpBar.Origin.Y); 
                hpBar.HpRectangles[i] = new RectangleF(rectangleOrigin, hpBar.ElementSize);
                if (tempHpStore >= 2)
                {
                    hpBar.HpRecColours[i] = Brushes.Green;
                }
                else if (tempHpStore == 1)
                {
                    hpBar.HpRecColours[i] = Brushes.Orange;
                }
                else
                {
                    hpBar.HpRecColours[i] = Brushes.DimGray;
                }

                xOffset += hpIconOffset * hpBar.ScaleF; 
                tempHpStore -= 2;
            }
            // MessageBox.Show($"{hpBar.HpRectangles.Length}");
        }


        
        private void doPlayerDamage(int amt)
        {
            currentHp -= amt;
            computeHP();
        }



        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // if Not parrying then resets parrywindow and sets to parrying
            if ((e.Button == MouseButtons.Right) && (!isParrying))
            {
                parryWindow = (parryDurationS * 10);
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

            for (int i = 0; i < hpBar.IconCount; i++)
            {
                Brush colour = hpBar.HpRecColours[i];
                RectangleF rec = hpBar.HpRectangles[i];
                e.Graphics.FillRectangle(colour, rec);
            }
        }


        // stores projectiles to be disposed of (as list cannot be altered mid-loop)
        List<Projectile> disposedProjectiles = new List<Projectile>();
        int slowTick = 0;

        // rendering timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (slowFrame <= 0 && freezeFrame <= 0 && curZoom != 1)
            {
                unZoomScreen(zoomFactor);
                curZoom = 1;
            }


            label2.Text = ($"({this.Size.Width}, {this.Size.Height})"); // debugging


            // debugging/visual indicator for parry
            if (isParrying)
                playerBrush = Brushes.Gray;
            else if (playerIsHit)
            {
                playerBrush = Brushes.Red; // visual hit indicator
                doPlayerDamage(1);
                playerIsHit = false;
            }
            else
                playerBrush = Brushes.Blue;



            latestBulletInfo(); // debugging


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
                label4.Text = freezeFrame.ToString();
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
            curZoom = scaleF;

            // gets center of screen
            float midX = this.Width / 2;
            float midY = this.Height / 2;

            mcPrevCenter = defendBox.getCenter();

            // calculates new position for each projectile based on distance from defendBox center and adjusts for Scale and the "screen" shifting to the center
            foreach (Projectile p in Projectile.ProjectileList)
            {
                float XDiff = p.getCenter().X - mcPrevCenter.X;
                float YDiff = p.getCenter().Y - mcPrevCenter.Y;

                float newPrjX = midX + XDiff * scaleF;
                float newPrjY = midY + YDiff * scaleF;

                p.updateCenter(newPrjX, newPrjY);
                p.scaleHitbox(scaleF);
                this.Invalidate();
            }

            float pOriginXDif = bulletOrigin.X - mcPrevCenter.X;
            float pOriginYDif = bulletOrigin.Y - mcPrevCenter.Y;

            float newPOriginX = midX + pOriginXDif * scaleF;
            float newPOriginY = midY + pOriginYDif * scaleF;

            bulletOrigin = new PointF(newPOriginX, newPOriginY);

            defendBox.updateCenter(midX, midY);
            defendBox.scaleHitbox(scaleF);
        }



        private void unZoomScreen(float scaleF)
        {
            float midX = this.Width / 2;
            float midY = this.Height / 2;

            foreach (Projectile p in Projectile.ProjectileList)
            {
                float XDiff = p.getCenter().X - midX;
                float YDiff = p.getCenter().Y - midY;

                float oldPrjX = mcPrevCenter.X + XDiff / scaleF;
                float oldPrjY = mcPrevCenter.Y + YDiff / scaleF;

                p.updateCenter(oldPrjX, oldPrjY);
                p.resetScale();
            }

            float oXDiff = bulletOrigin.X - midX;
            float oYDiff = bulletOrigin.Y - midY;

            float oldOrigX = mcPrevCenter.X + oXDiff / scaleF;
            float oldOrigY = mcPrevCenter.Y + oYDiff / scaleF;

            bulletOrigin = new PointF(oldOrigX, oldOrigY);

            defendBox.updateCenter(mcPrevCenter.X, mcPrevCenter.Y);
            defendBox.resetScale();
            curZoom = 1; // screen is no longer scaled
        }



        // todo: use booleans so that the player cannot move during freezeFrame
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    movingUp = true;
                    break;
                case Keys.S:
                    movingDown = true;
                    break;
                case Keys.A:
                    movingLeft = true;
                    break;
                case Keys.D:
                    movingRight = true;
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (slowFrame > 0)
                return;

            switch (e.KeyCode)
            {
                case Keys.W:
                    movingUp = false;
                    break;
                case Keys.S:
                    movingDown = false;
                    break;
                case Keys.A:
                    movingLeft = false;
                    break;
                case Keys.D:
                    movingRight = false;
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadTokenSrc.Cancel();
        }
    }
}
