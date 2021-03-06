﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Shapes;

namespace FlipTile 
{
 
 
    class Tile
    {
        public static Color FaceDownColor = Color.FromArgb(255, 34, 49, 63);
        public static Color FaceUpColor = Color.FromArgb(255, 107, 185, 240);

        Rectangle rect;
        Boolean faceup = false;

        public Rectangle Rect
        {
            get
            {
                return rect;
            }

            set
            {
                rect = value;
            }
        }
        
        public bool Faceup
        {
            get
            {
                return faceup;
            }

            set
            {
                faceup = value;
            }
        }
    }
}
