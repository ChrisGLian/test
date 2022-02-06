using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.iOS;


public enum ControlMode
{
    View = 0,
    Setting
}

public class ApplicationManager : MonoBehaviour
{
    private ControlMode controlMode = ControlMode.View;
    public Transform targetModel;

    private Vector2 touchedPosition;
    private bool modelTouched;
    private float sqTwoPointsDistance;

    public float scaleMin;
    public float scaleMax;
    public float positionYMin;
    public float positionYMax;

    public float rotateSensitivity;
    public float scaleSensitivity;
    public float moveSensitivity;

    public GameObject buttonParent;
    public int currentMenuIndex;
    public GameObject[] allMenus;
    public AssetManager assetManager;

    public GameObject settingParent;
    public Slider rotateSensitivitySlider;
    public Slider scaleSensitivitySlider;
    public Slider moveSensitivitySlider;

    // Start is called before the first frame update
    void Start()
    {
        currentMenuIndex = 0;

#if UNITY_ANDROID || UNITY_IOS
        buttonParent.transform.localScale *= 2;
        settingParent.transform.localScale *= 2;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (controlMode == ControlMode.View)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchedPosition = touch.position;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(touchedPosition)))
                    {
                        modelTouched = true;
                    }
                    else
                    {
                        modelTouched = false;
                    }
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    if (modelTouched)
                    {
                        Vector2 movement = touch.position - touchedPosition;
                        MoveModel(movement.x * 20f / Screen.height, movement.y * 20f / Screen.height);
                    }
                    else
                    {
                        Vector2 movement = touch.position - touchedPosition;
                        RotateModel(movement.y * 0.1f, -movement.x * 0.1f);
                    }

                    touchedPosition = touch.position;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                if (touch2.phase == TouchPhase.Began)
                {
                    Vector2 displacement = touch2.position - touch1.position;
                    sqTwoPointsDistance = displacement.x * displacement.x + displacement.y * displacement.y;
                }
                else if (touch2.phase == TouchPhase.Moved)
                {
                    Vector2 displacement = touch2.position - touch1.position;
                    float newSqTwoPointsDistance = displacement.x * displacement.x + displacement.y * displacement.y;
                    float deltaTwoPointsDistance = newSqTwoPointsDistance - sqTwoPointsDistance;

                    if (deltaTwoPointsDistance > 10000f || deltaTwoPointsDistance < -10000f)
                    {
                        ScaleModel(deltaTwoPointsDistance * 0.00005f);
                    }

                    sqTwoPointsDistance = newSqTwoPointsDistance;
                }
            }

            if (Input.GetMouseButton(0))
            {
                RotateModel(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"));
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                ScaleModel(Input.mouseScrollDelta.y);
            }

            if (Input.GetMouseButton(1))
            {
                MoveModel(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }
        }
    }

    public void BackToDefault()
    {
        targetModel.rotation = Quaternion.identity;
        targetModel.localScale = Vector3.one;
        targetModel.position = Vector3.zero;
    }

    public void RotateModel(float _axisX, float _axisY)
    {
        targetModel.Rotate(_axisX * rotateSensitivity, _axisY * rotateSensitivity, 0f, Space.World);
    }

    public void ScaleModel(float _scaleChange)
    {
        float newScale = Mathf.Clamp(targetModel.localScale.x + _scaleChange * scaleSensitivity, scaleMin, scaleMax);
        targetModel.localScale = new Vector3(newScale, newScale, newScale);
    }

    public void MoveModel(float _axisX, float _axisY)
    {
        float newX = Mathf.Clamp(targetModel.position.x + _axisX * moveSensitivity, positionYMin * Screen.width / Screen.height, positionYMax * Screen.width / Screen.height);
        float newY = Mathf.Clamp(targetModel.position.y + _axisY * moveSensitivity, positionYMin, positionYMax);
        targetModel.position = new Vector3(newX, newY, 0f);
    }

    public void SwitchMenu(int _index)
    {
        allMenus[currentMenuIndex].SetActive(false);
        currentMenuIndex = _index;
        allMenus[currentMenuIndex].SetActive(true);

        switch(_index)
        {
            case 1:
                assetManager.UpdateMesh();
                break;
            case 2:
                assetManager.UpdateMaterial();
                break;
            case 3:
                assetManager.UpdateTexture();
                break;
        }
    }

    public void AssetPrevious()
    {
        switch(currentMenuIndex)
        {
            case 1:
                assetManager.MeshPrevious();
                break;
            case 2:
                assetManager.MaterialPrevious();
                break;
            case 3:
                assetManager.TexturePrevious();
                break;
        }
    }

    public void AssetNext()
    {
        switch (currentMenuIndex)
        {
            case 1:
                assetManager.MeshNext();
                break;
            case 2:
                assetManager.MaterialNext();
                break;
            case 3:
                assetManager.TextureNext();
                break;
        }
    }

    public void PickAsset(int _index)
    {
        switch (currentMenuIndex)
        {
            case 1:
                assetManager.PickMesh(_index, targetModel);
                break;
            case 2:
                assetManager.PickMaterial(_index, targetModel);
                break;
            case 3:
                assetManager.PickTexture(_index, targetModel);
                break;
        }
    }

    public void OpenSettingMenu()
    {
        controlMode = ControlMode.Setting;

        buttonParent.SetActive(false);
        settingParent.SetActive(true);
    }

    public void CloseSettingMenu()
    {
        controlMode = ControlMode.View;

        buttonParent.SetActive(true);
        settingParent.SetActive(false);
    }

    public void ChangeRotateSensitivity()
    {
        rotateSensitivity = rotateSensitivitySlider.value * 0.5f;
    }

    public void ChangeScaleSensitivity()
    {
        scaleSensitivity = scaleSensitivitySlider.value * 0.01f;
    }

    public void ChangeMoveSensitivity()
    {
        moveSensitivity = moveSensitivitySlider.value * 0.05f;
    }
}
