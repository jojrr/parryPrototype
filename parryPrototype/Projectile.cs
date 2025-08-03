using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parryPrototype
{
    internal class Projectile : Entity
    {
        private int xVelocity;
        private int yVelocity;
        private int Velocity;

        public static List<Projectile> ProjectileList = new List<Projectile>();

        public Projectile (Point origin, int width, int height, int velocity, Point target)
            : base(origin: origin, width: width, height: height)
        {
            Velocity = velocity;
            int[] velocities = CalculateVelocities(target);
            xVelocity = velocities[0]; 
            yVelocity = velocities[1]; 
            ProjectileList.Add(this);
        }

        public void moveProjectile()
        {
           this.updateLocation( Location.X + xVelocity, Location.Y + yVelocity);
        }

        public int[] CalculateVelocities(Point target)
        {
            int xDiff = target.X - Location.X;
            int yDiff = Location.Y - target.Y;

            double velocityAngle = Math.Atan(yDiff/xDiff);

            xVelocity = (int)(Velocity * Math.Cos(velocityAngle)) * Math.Sign(xDiff);
            yVelocity = (int)(Velocity * Math.Sin(velocityAngle)) * Math.Sign(yDiff);

            return [xVelocity, yVelocity];
        }
    }
}
