using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace parryPrototype
{
    /// <summary>
    /// produces the hitbox and responsible for collision detection
    /// </summary>
    internal class Entity
    {
        protected PointF Location;
        protected PointF Center;
        protected SizeF Size;
        protected int
            Width = 0,
            Height = 0;
        protected RectangleF Hitbox;

        protected const int TotalLevels = 1;
        protected static readonly int[] ChunksInLvl = new int[TotalLevels] { 3 };

        /// <summary>
        /// returns the hitbox as a rectangle
        /// </summary>
        /// <returns>hitbox of type rectangle</returns>
        public RectangleF getHitbox()
        {
            return Hitbox;
        }


        /// <summary>
        /// Creates a hitbox at specified paramters
        /// </summary>
        /// <param name="origin">the point of the top-left of the rectangle</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="height">height of the rectangle</param>
        public Entity(PointF origin, int width, int height)
        {
            Location = origin;
            Width = width;
            Height = height;
            Size = new Size( width, height);
            Hitbox = new RectangleF(origin, Size); 
            Center = new PointF (Hitbox.X + Width/2, Hitbox.Y + Height/2);
        }

        /// <summary>
        /// Assigns a new position to the top-left of the hitbox
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void updateLocation(float x, float y)
        {
            Location = new PointF(x, y);
            Hitbox = new RectangleF(Location, Size);
            Center = new PointF (Location.X + Width/2, Location.Y + Height/2);
        }

        public void updateCenter(float x, float y)
        {
            Center = new PointF(x, y);
            Hitbox = new RectangleF(Location, Size);
            Location = new PointF (Location.X - Width/2, Location.Y - Height/2);
        }

        /// <summary>
        /// returns the point of the center
        /// </summary>
        /// <returns></returns>
        public PointF getCenter()
        {
            return Center;
        }

        /// <summary>
        /// returns the point of the top-left 
        /// </summary>
        /// <returns></returns>
        public PointF getLocation()
        {
            return Location;
        }

        public void scaleHitbox(float scaleF)
        {
            Hitbox = new RectangleF(
                    Center, 
                    new SizeF (Size.Width*scaleF, Size.Height*scaleF) );
        }
    }
}
