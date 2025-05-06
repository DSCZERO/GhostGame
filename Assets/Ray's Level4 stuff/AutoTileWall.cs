using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AutoTileWall : MonoBehaviour
{
    public float tileSize = 1f; // How many units per texture tile

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        Vector3 scale = transform.lossyScale;
        Vector3 size = rend.bounds.size;

        Vector2 tiling = new Vector2(
            Mathf.Round(size.x / tileSize),
            Mathf.Round(size.y / tileSize)
        );

        rend.material.mainTextureScale = tiling;
    }
}
