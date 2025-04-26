using UnityEngine;

namespace ETD.Scripts.InGame.View.ViewBullet
{
    public class ViewBullet : View
    {
        public GameObject Model => _modelSpriteRenderer.gameObject;
        
        [SerializeField] protected SpriteRenderer _modelSpriteRenderer;

        public float radius;
        
        public void UpdateSize(float size)
        {
            transform.localScale = Vector3.one * size;
        }
        
        public void UpdatePosition(Vector2 position)
        {
            transform.position = position;
        }

        public void UpdateRotate(Vector3 rotation)
        {
            _modelSpriteRenderer.transform.rotation = Quaternion.Euler(rotation);
        }
    }
}