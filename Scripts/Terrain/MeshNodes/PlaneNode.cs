using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public class PlaneNode : MeshNode
{
    public int width = 10;
    public int height = 10;
    public int subdivisions = 5;
    public Material defaultMaterial;

    private int lastWidth = -1, lastHeight = -1, lastSubdivisions = -1;

    public override ProBuilderMesh GenerateMesh(ProBuilderMesh existingMesh)
    {
        int optimizedSubdivisions = Mathf.Clamp(subdivisions, 1, 200); // Prevent extreme lag

        if (GeneratedMesh == null || width != lastWidth || height != lastHeight || subdivisions != lastSubdivisions)
        {
            lastWidth = width;
            lastHeight = height;
            lastSubdivisions = subdivisions;

            if (GeneratedMesh != null) Destroy(GeneratedMesh.gameObject);

            GeneratedMesh = ShapeGenerator.GeneratePlane(
                PivotLocation.Center,
                width,
                height,
                optimizedSubdivisions,
                optimizedSubdivisions,
                Axis.Up
            );

            ApplyMaterial();
            GeneratedMesh.ToMesh();
            GeneratedMesh.Refresh();
        }

        return GeneratedMesh;
    }

    private void ApplyMaterial()
    {
        if (GeneratedMesh == null) return;

        Renderer renderer = GeneratedMesh.gameObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = GeneratedMesh.gameObject.AddComponent<MeshRenderer>();
        }

        if (defaultMaterial != null)
        {
            renderer.sharedMaterial = defaultMaterial;
        }
        else
        {
            Debug.LogWarning("⚠ No default material assigned. Using Unity’s default material.");
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }
    }
}
