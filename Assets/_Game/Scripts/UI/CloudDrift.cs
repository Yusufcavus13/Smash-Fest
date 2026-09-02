using System;
using UnityEngine;

namespace SmashFest.UI
{

    public class CloudDrift : MonoBehaviour
    {


        [Serializable]
        private class Cloud
        {
            public RectTransform transform;

            [Tooltip("Canvas units per second. Negative moves left.")]
            public float speed = 14f;
        }



        [Header("Clouds")]
        [SerializeField] private Cloud[] clouds;

        [Tooltip("Extra distance past the edge before a cloud is wrapped back.")]
        [SerializeField] private float wrapMargin = 260f;


        private RectTransform selfTransform;



        private void Awake()
        {
            selfTransform = (RectTransform)transform;
        }

        private void Update()
        {
            float halfWidth = selfTransform.rect.width * 0.5f;
            float right = halfWidth + wrapMargin;
            float left = -right;

            for (int i = 0; i < clouds.Length; i++)
            {
                Cloud cloud = clouds[i];
                Vector2 position = cloud.transform.anchoredPosition;

                position.x += cloud.speed * Time.deltaTime;

                if (position.x > right)
                {
                    position.x = left;
                }
                else if (position.x < left)
                {
                    position.x = right;
                }

                cloud.transform.anchoredPosition = position;
            }
        }
    }
}
