using UnityEngine;
using System.Collections;


namespace Endless2DTerrain
{
    //From http://answers.unity3d.com/questions/8220/rendering-order.html

    public static class RenderQueue
    {
        public const int FrontPlane = 1;
        public const int DetailPlane = 2;
        public const int Background = 0;
    }
}
