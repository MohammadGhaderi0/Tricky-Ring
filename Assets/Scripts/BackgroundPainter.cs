using UnityEngine;

public class BackgroundPainter : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        // Changed from MeshRenderer to SpriteRenderer
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Directly setting the color property is the most efficient method for 2D Sprites
        _spriteRenderer.color = ColorManager.Instance.CurrentColor;
    }
}