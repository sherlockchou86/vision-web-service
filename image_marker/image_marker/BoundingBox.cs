using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace image_marker
{
    /// <summary>
    /// a rectangle standing for an object in image
    /// </summary>
    public class BoundingBox
    {
        public static Color[] Colors = new Color[] { Color.Blue, Color.Coral, Color.DarkGreen, Color.Beige, Color.Aqua, Color.DarkOrange };
        /// <summary>
        /// bounding box location and size in the image, pixel unit
        /// </summary>
        public RectangleF Region 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// class id, line index from default_classes.txt
        /// </summary>
        public int Class_ID 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// class name, line from default_classes.txt
        /// </summary>
        public String Class_Name
        {
            get
            {
                return Utils.Classes[Class_ID];
            }
        }

        /// <summary>
        /// the image the box belong to
        /// </summary>
        public String Image_Path
        {
            get;
            set;
        }

        /// <summary>
        /// if the box is selected in image, for edit purpose
        /// </summary>
        public bool IsSelected
        {
            get;
            set;
        }


        /// <summary>
        /// draw it on image for edit purpose
        /// </summary>
        /// <param name="g"></param>
        /// <param name="scale"></param>
        public void Draw(Graphics g, double scale)
        {
            Color color = IsSelected ? Color.Red : Colors[Class_ID];
            float width = 2;
            using (Pen p = new Pen(color, width))
            {
                g.DrawRectangle(p, new Rectangle((int)(Region.Left / scale), (int)(Region.Top / scale), (int)(Region.Width / scale), (int)(Region.Height / scale)));
            }
        }


        public override string ToString()
        {
            return Region.Left + "," + Region.Top + "," + Region.Right + "," + Region.Bottom + "," + Class_ID + "-->" + Class_Name;
        }
    }
}
