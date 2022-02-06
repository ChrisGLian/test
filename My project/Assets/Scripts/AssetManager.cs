using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct MeshOption
{
    public Mesh mesh;
    public Sprite image;
}

[System.Serializable]
public struct MaterialOption
{
    public Material material;
    public Sprite image;
}

[System.Serializable]
public struct TextureOption
{
    public Texture texture;
    public Sprite image;
}

public class AssetManager : MonoBehaviour
{
    public GameObject cube;
    public GameObject sphere;
    public GameObject capsule;

    public MeshOption[] meshes;
    private int meshCurrentPage = 0;

    public MaterialOption[] materials;
    private int materialCurrentPage = 0;

    public TextureOption[] textures;
    private int textureCurrentPage = 0;

    public GameObject[] assetButton;
    public GameObject previousButton;
    public GameObject nextButton;

    private void Start()
    {
        meshes[0].mesh = cube.GetComponent<MeshFilter>().sharedMesh;
        meshes[1].mesh = sphere.GetComponent<MeshFilter>().sharedMesh;
        meshes[2].mesh = capsule.GetComponent<MeshFilter>().sharedMesh;
    }

    public void UpdateMesh()
    {
        UpdateButton(meshCurrentPage, meshes.Length);

        for (int i = 0; i < assetButton.Length; i++)
        {
            if (meshCurrentPage * 4 + i < meshes.Length)
            {
                assetButton[i].SetActive(true);
                assetButton[i].GetComponent<Image>().sprite = meshes[meshCurrentPage * 4 + i].image;
            }
            else
            {
                assetButton[i].SetActive(false);
            }
        }
    }

    public void MeshPrevious()
    {
        meshCurrentPage--;
        UpdateMesh();
    }

    public void MeshNext()
    {
        meshCurrentPage++;
        UpdateMesh();
    }

    public void PickMesh(int _index, Transform _model)
    {
        _model.GetComponent<MeshFilter>().mesh = meshes[meshCurrentPage * 4 + _index].mesh;
    }

    public void UpdateMaterial()
    {
        UpdateButton(materialCurrentPage, materials.Length);

        for (int i = 0; i < assetButton.Length; i++)
        {
            if (materialCurrentPage * 4 + i < materials.Length)
            {
                assetButton[i].SetActive(true);
                assetButton[i].GetComponent<Image>().sprite = materials[materialCurrentPage * 4 + i].image;
            }
            else
            {
                assetButton[i].SetActive(false);
            }
        }
    }

    public void MaterialPrevious()
    {
        materialCurrentPage--;
        UpdateMaterial();
    }

    public void MaterialNext()
    {
        materialCurrentPage++;
        UpdateMaterial();
    }

    public void PickMaterial(int _index, Transform _model)
    {
        Texture currentTexture = _model.GetComponent<MeshRenderer>().material.GetTexture("_MainTex");
        _model.GetComponent<MeshRenderer>().material = materials[materialCurrentPage * 4 + _index].material;
        _model.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", currentTexture);
    }

    public void UpdateTexture()
    {
        UpdateButton(textureCurrentPage, textures.Length);

        for (int i = 0; i < assetButton.Length; i++)
        {
            if (textureCurrentPage * 4 + i < textures.Length)
            {
                assetButton[i].SetActive(true);
                assetButton[i].GetComponent<Image>().sprite = textures[textureCurrentPage * 4 + i].image;
            }
            else
            {
                assetButton[i].SetActive(false);
            }
        }
    }

    public void TexturePrevious()
    {
        textureCurrentPage--;
        UpdateTexture();
    }

    public void TextureNext()
    {
        textureCurrentPage++;
        UpdateTexture();
    }

    public void PickTexture(int _index, Transform _model)
    {
        _model.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", textures[textureCurrentPage * 4 + _index].texture);
    }

    private void UpdateButton(int _currentPage, int _assetLenth)
    {
        if (_currentPage > 0)
            previousButton.SetActive(true);
        else
            previousButton.SetActive(false);

        if (_assetLenth > (_currentPage + 1) * assetButton.Length)
            nextButton.SetActive(true);
        else
            nextButton.SetActive(false);
    }
}
