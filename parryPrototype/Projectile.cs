using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace parryPrototype
{
    internal class Projectile : Entity
    {
        private float 
            xVelocity, 
            yVelocity;

        // debugging
        public float 
            velocityAngle, 
            yDiff, 
            xDiff;

        public float Velocity;

        private bool isRebound = false;

        private PointF Target;

        public static List<Projectile> ProjectileList = new List<Projectile>();



        public Projectile (PointF origin, int width, int height, float velocity, PointF target)
            : base(origin: origin, width: width, height: height)
        {
            Velocity = velocity;
            float[] velocities = CalculateVelocities(target);

            target = Target;

            xVelocity = velocities[0]; 
            yVelocity = velocities[1]; 

            ProjectileList.Add(this);

            velocityAngle = velocities[2];
            yDiff = velocities[3];
            xDiff = velocities[4];
        }

        public void moveProjectile()
        {
            if (isRebound)
            {
                this.updateLocation( Location.X - xVelocity, Location.Y - yVelocity);
                return;
            }

            this.updateLocation( Location.X + xVelocity, Location.Y + yVelocity);
        }

        public float[] CalculateVelocities(PointF target)
        {
            float xDiff = target.X - Location.X;
            float yDiff = target.Y - Location.Y;

            double velocityAngle = Math.Atan(yDiff/xDiff);

            float xVelocity = (float)Math.Abs(Math.Cos(velocityAngle)) * Velocity * Math.Sign(xDiff);
            float yVelocity = (float)Math.Abs(Math.Sin(velocityAngle)) * Velocity * Math.Sign(yDiff);

               

            return [xVelocity, yVelocity, (float)velocityAngle, yDiff, xDiff];
        }

        public void rebound(PointF target)
        {
            isRebound = !isRebound;

            xDiff = Center.X - target.X;
            yDiff = Center.Y - target.Y;

            this.updateLocation( Location.X + 2*xDiff, Location.Y + 2*yDiff);
        }
    }
}
