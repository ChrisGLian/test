using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AssetOption
{
    public Sprite image;
}

[System.Serializable]
public class MeshOption : AssetOption
{
    public Mesh mesh;
}

[System.Serializable]
public class MaterialOption : AssetOption
{
    public Material material;
}

[System.Serializable]
public class TextureOption : AssetOption
{
    public Texture texture;
}

public class AssetManager : MonoBehaviour
{
    [Header("Primitive Prefab")]
    public GameObject cube;
    public GameObject sphere;
    public GameObject capsule;

    [Space]
    [Header("Asset Data")]
    public MeshOption[] meshes;
    private int meshCurrentPage = 0;

    public MaterialOption[] materials;
    private int materialCurrentPage = 0;

    public TextureOption[] textures;
    private int textureCurrentPage = 0;

    private int m_baseMapID;

    [Space]
    [Header("Button Reference")]
    public GameObject[] assetButton;
    public GameObject previousButton;
    public GameObject nextButton;

    private void Start()
    {
        // Initialize primitive meshes
        meshes[0].mesh = cube.GetComponent<MeshFilter>().sharedMesh;
        meshes[1].mesh = sphere.GetComponent<MeshFilter>().sharedMesh;
        meshes[2].mesh = capsule.GetComponent<MeshFilter>().sharedMesh;

        m_baseMapID = Shader.PropertyToID("_BaseMap");
    }

    public void UpdateMesh()
    {
        UpdateButton(meshCurrentPage, meshes);
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
        UpdateButton(materialCurrentPage, materials);
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
        Texture currentTexture = _model.GetComponent<MeshRenderer>().material.GetTexture(m_baseMapID);
        _model.GetComponent<MeshRenderer>().material = materials[materialCurrentPage * 4 + _index].material;
        _model.GetComponent<MeshRenderer>().material.SetTexture(m_baseMapID, currentTexture);
    }

    public void UpdateTexture()
    {
        UpdateButton(textureCurrentPage, textures);
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
        _model.GetComponent<MeshRenderer>().material.SetTexture(m_baseMapID, textures[textureCurrentPage * 4 + _index].texture);
    }

    public void ResetModelAssets(Transform _model)
    {
        _model.GetComponent<MeshFilter>().mesh = meshes[0].mesh;
        _model.GetComponent<MeshRenderer>().material = materials[0].material;
        _model.GetComponent<MeshRenderer>().material.SetTexture(m_baseMapID, textures[0].texture);
    }

    private void UpdateButton(int _currentPage, AssetOption[] _assets)
    {
        // check if "previous page" button should be available
        if (_currentPage > 0)
            previousButton.SetActive(true);
        else
            previousButton.SetActive(false);

        // check if "next page" button should be available
        if (_assets.Length > (_currentPage + 1) * assetButton.Length)
            nextButton.SetActive(true);
        else
            nextButton.SetActive(false);

        // show available asset options in current page
        for (int i = 0; i < assetButton.Length; i++)
        {
            if (_currentPage * 4 + i < _assets.Length)
            {
                assetButton[i].SetActive(true);
                assetButton[i].GetComponent<Image>().sprite = _assets[_currentPage * 4 + i].image;
            }
            else
            {
                assetButton[i].SetActive(false);
            }
        }
    }
}
