using UnityEngine;

namespace SmashFest.Interactions
{

    public interface ISmashable
    {
        bool IsSmashable { get; }
        Rigidbody Body { get; }

        void TakeHit(in SmashHitInfo hitInfo);
    }
}
