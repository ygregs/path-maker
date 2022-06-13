using UnityEngine;

public class SkinRenderer : MonoBehaviour
{
    [SerializeField] private MeshRenderer _gunMesh;
    [SerializeField] private SkinnedMeshRenderer[] _meshes;

    public void SetMeshesActive(bool state)
    {
        print($"set meshes to {state}");
        foreach (var mesh in _meshes)
        {
            mesh.enabled = state;
        }
        _gunMesh.enabled = state;
    }
}