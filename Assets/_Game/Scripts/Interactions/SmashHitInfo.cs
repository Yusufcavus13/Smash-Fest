using UnityEngine;

namespace SmashFest.Interactions
{
    public readonly struct SmashHitInfo
    {

        public Vector3 Point { get; }
        public Vector3 Direction { get; }
        public float Impulse { get; }


        public SmashHitInfo(Vector3 point, Vector3 direction, float impulse)
        {
            Point = point;
            Direction = direction;
            Impulse = impulse;
        }
    }
}
