using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapDisplay : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture; //apply texture to plane in scene view
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height); //make plane size scale to texture size
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        meshFilter.transform.localScale = Vector3.one * FindObjectOfType<mapGenerator>().terrainData.uniformScale;
    }
}
