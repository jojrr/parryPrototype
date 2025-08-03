using System.Runtime.CompilerServices;

namespace parryPrototype
{
    public partial class Form1 : Form
    {

        readonly new Entity defendBox = new Entity
            (origin: new Point(50, 250),
             width: 50,
             height: 50);


        bool isParrying = false;
        int parryWindow = 20;

        Point mousePos;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            Width = 1460;
            Height = 770;
            timer1.Enabled = true;
            timer1.Interval = 10;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (!isParrying))
            {
                parryWindow = 10;
                isParrying = true;
                createBullet();
            }

        }

        private void createBullet()
            {
                Projectile bullet = new Projectile
                    ( origin: new Point (800, 250),
                      width: 30,
                      height: 10,
                      velocity: 4,
                      target: mousePos);
            }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isParrying = false;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Blue, defendBox.getHitbox());

            foreach (Projectile bullet in Projectile.ProjectileList)
                e.Graphics.DrawRectangle(Pens.Red, bullet.getHitbox());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = ($"({this.Size.Width}, {this.Size.Height})");
            mousePos = System.Windows.Forms.Cursor.Position;

            if (isParrying && parryWindow > 0)
                parryWindow -= 0;
            if (parryWindow < 1)
                isParrying = false;
            
            foreach (Projectile bullet in Projectile.ProjectileList)
                bullet.moveProjectile();

            this.Refresh();
            GC.Collect();
        }

    }
}
