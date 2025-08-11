using System.Runtime.CompilerServices;

namespace parryPrototype
{
    public partial class Form1 : Form
    {

        readonly new Entity defendBox = new 
            (origin: new Point(50, 250),
             width: 50,
             height: 50);

        int playerVelocity = 8;

        bool isParrying = false;
        int parryDuration = 30;
        int parryWindow;
        int perfectParryWindow = 8;
        int freezeFrame = 0;

        int bulletCooldown = 50;
        int bulletInterval;

        Point mousePos;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            Width = 1460;
            Height = 770;

            timer1.Enabled = true;
            timer1.Interval = 16;

            bulletInterval = bulletCooldown;

            playerPen = Pens.Blue;

            parryWindow = parryDuration;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        private void createBullet()
        {
            Projectile bullet = new Projectile
                (origin: new Point(800, 250),
                  width: 30,
                  height: 10,
                  velocity: 8,
                  target: defendBox.getCenter());
        }




        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (!isParrying))
            {
                parryWindow = parryDuration;
                isParrying = true;
            }

        }



        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isParrying = false;
            }
        }



        Pen playerPen;

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(playerPen, defendBox.getHitbox());

            foreach (Projectile bullet in Projectile.ProjectileList)
                e.Graphics.DrawRectangle(Pens.Red, bullet.getHitbox());
        }


        List<Projectile> disposedProjectiles = new List<Projectile>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool setFreeze = false;

            if (freezeFrame > 0)
            {
                freezeFrame -= 1;
                return;
            }

            label2.Text = ($"({this.Size.Width}, {this.Size.Height})");
            // mousePos = System.Windows.Forms.Cursor.Position;
            
            if (bulletInterval > 0)
                bulletInterval -= 1;
            else
            {
                createBullet();
                bulletInterval = bulletCooldown;
            }


            if (isParrying && parryWindow > 0)
                parryWindow -= 1;
            if (parryWindow < 1)
                isParrying = false;

            if (isParrying)
                playerPen = Pens.Gray;
            else
                playerPen = Pens.Blue;



            foreach (Projectile bullet in Projectile.ProjectileList)
            {
                bullet.moveProjectile();
                if (defendBox.getHitbox().IntersectsWith(bullet.getHitbox()))
                {
                    if (isParrying)
                    {
                        bullet.rebound(defendBox.getCenter());
                        if (parryWindow >= parryDuration - perfectParryWindow)
                            setFreeze = true;
                    }
                    else
                    {
                        playerPen = Pens.Red;
                        disposedProjectiles.Add(bullet);
                    }
                }
            }

            foreach (Projectile p in disposedProjectiles)
                Projectile.ProjectileList.Remove(p);

            disposedProjectiles.Clear();

            latestBulletInfo();

            if (setFreeze)
            {
                freezeFrame = 10;
                zoomScreen(defendBox, 1.5F);
            }

            this.Refresh();
            GC.Collect();
        }

        // for debugging
        private void latestBulletInfo()
        {
            if (Projectile.ProjectileList.Count > 0)
            {
                Projectile p = Projectile.ProjectileList.Last();
                label1.Text = p.yDiff.ToString();
                label3.Text = p.xDiff.ToString();
                label4.Text = p.velocityAngle.ToString();
            }
        }




        private void playerMove(float x, float y)
        {
            defendBox.updateLocation(defendBox.getLocation().X + x, defendBox.getLocation().Y + y);
        }

        private void zoomScreen(Entity focus, float scale)
        {
            focus.updateCenter(this.Width / 2, this.Height / 2);
            PointF center = new PointF (this.Width /2, this.Height /2);

            float XdiffFocus;
            float YdiffFocus;


            foreach (Projectile p in Projectile.ProjectileList)
            {
                zoomEntity(
                        center: center,
                        scale: scale,
                        target: p);
                XdiffFocus = this.Width - p.getCenter().X;
                YdiffFocus = this.Height - p.getCenter().Y;
                p.updateLocation(p.getLocation().X + XdiffFocus, p.getLocation().Y + YdiffFocus);
            }
            
           
            XdiffFocus = this.Width - defendBox.getCenter().X;
            YdiffFocus = this.Height - defendBox.getCenter().Y;
            defendBox.updateLocation(defendBox.getLocation().X + XdiffFocus, defendBox.getLocation().Y + YdiffFocus);
            zoomEntity(
                    center: center,
                    scale: scale,
                    target: defendBox);
        }

        private void zoomEntity(PointF center, float scale, Entity target)
        {
            target.scaleHitbox(scale);
            PointF pLoc = target.getLocation();
            float pXdiff = pLoc.X - center.X;
            float pYdiff = pLoc.Y - center.Y;
            target.updateLocation(pLoc.X + pXdiff, pLoc.Y + pYdiff);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                playerMove(0, -playerVelocity);

            if (e.KeyCode == Keys.S)
                playerMove(0, playerVelocity);

            if (e.KeyCode == Keys.A)
                playerMove(-playerVelocity, 0);

            if (e.KeyCode == Keys.D)
                playerMove(playerVelocity, 0);

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
