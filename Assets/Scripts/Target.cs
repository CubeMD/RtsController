using UnityEngine;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    [SerializeField] private Renderer _renderer;
    
    private bool clicked;
    private float halfSpawnableSize;
    
    public void Initialize(float halfGroundSize)
    {
        halfSpawnableSize = halfGroundSize;
        Reset();
    }

    public void Reset()
    {
        Vector3 position = new Vector3(Random.Range(-halfSpawnableSize, halfSpawnableSize), 0, Random.Range(-halfSpawnableSize, halfSpawnableSize));
        transform.localPosition = position;
        clicked = false;
        _collider.enabled = true;
        _renderer.enabled = true;
    }

    public void Clicked()
    {
        clicked = true;
        _collider.enabled = false;
        _renderer.enabled = false;
    }

    public bool IsClicked()
    {
        return clicked;
    }
}
