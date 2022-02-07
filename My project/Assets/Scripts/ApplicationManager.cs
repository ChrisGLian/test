using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.iOS;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;


public enum ControlMode
{
    View = 0,
    Setting
}

public enum AntialiasingOption
{
    OFF = 0,
    FXAA,
    SMAA
}

public class ApplicationManager : MonoBehaviour
{
    public EventSystem eventSystem;
    private ControlMode m_controlMode = ControlMode.View;
    public Transform targetModel;

    private Vector2 m_touchedPosition;
    private bool m_modelTouched;
    private float m_sqTwoPointsDistance;

    public float scaleMin;
    public float scaleMax;
    public float positionYMin;
    public float positionYMax;

    private float m_rotateSensitivity;
    private float m_scaleSensitivity;
    private float m_moveSensitivity;

    public GameObject buttonParent;
    private int m_currentMenuIndex;
    public GameObject[] allMenus;

    private UniversalAdditionalCameraData cameraData;
    public Volume globalVolume;
    private Tonemapping tonemapping;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private AntialiasingOption m_antialiasing = AntialiasingOption.SMAA;
    public TextMeshProUGUI antialiasingText;
    private bool m_tonemapping = true;
    public TextMeshProUGUI tonemappingText;
    private bool m_vignette = true;
    public TextMeshProUGUI vignetteText;
    private bool m_colorAdjustments = true;
    public TextMeshProUGUI colorAdjustmentsText;

    public Light sceneLight;
    public Slider lightAngleSlider;
    public Slider lightIntensitySlider;
    public AssetManager assetManager;

    public GameObject settingParent;
    public Slider rotateSensitivitySlider;
    public Slider scaleSensitivitySlider;
    public Slider moveSensitivitySlider;

    // Start is called before the first frame update
    void Start()
    {
        m_currentMenuIndex = 0;

        cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        globalVolume.profile.TryGet(out tonemapping);
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out colorAdjustments);

        ChangeRotateSensitivity();
        ChangeScaleSensitivity();
        ChangeMoveSensitivity();

#if UNITY_ANDROID || UNITY_IOS
        buttonParent.transform.localScale *= 2;
        settingParent.transform.localScale *= 2;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (m_controlMode == ControlMode.View)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    m_touchedPosition = touch.position;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(m_touchedPosition)))
                    {
                        m_modelTouched = true;
                    }
                    else
                    {
                        m_modelTouched = false;
                    }
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    if (m_modelTouched)
                    {
                        Vector2 movement = touch.position - m_touchedPosition;
                        MoveModel(movement.x * 20f / Screen.height, movement.y * 20f / Screen.height);
                    }
                    else if (!eventSystem.currentSelectedGameObject)
                    {
                        Vector2 movement = touch.position - m_touchedPosition;
                        RotateModel(movement.y * 0.1f, -movement.x * 0.1f);
                    }

                    m_touchedPosition = touch.position;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                if (touch2.phase == TouchPhase.Began)
                {
                    Vector2 displacement = touch2.position - touch1.position;
                    m_sqTwoPointsDistance = displacement.x * displacement.x + displacement.y * displacement.y;
                }
                else if (touch2.phase == TouchPhase.Moved)
                {
                    Vector2 displacement = touch2.position - touch1.position;
                    float newSqTwoPointsDistance = displacement.x * displacement.x + displacement.y * displacement.y;
                    float deltaTwoPointsDistance = newSqTwoPointsDistance - m_sqTwoPointsDistance;

                    if (deltaTwoPointsDistance > 10000f || deltaTwoPointsDistance < -10000f)
                    {
                        ScaleModel(deltaTwoPointsDistance * 0.00005f);
                    }

                    m_sqTwoPointsDistance = newSqTwoPointsDistance;
                }
            }

            if (Input.GetMouseButton(0) && !eventSystem.currentSelectedGameObject)
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
        targetModel.Rotate(_axisX * m_rotateSensitivity, _axisY * m_rotateSensitivity, 0f, Space.World);
    }

    public void ScaleModel(float _scaleChange)
    {
        float newScale = Mathf.Clamp(targetModel.localScale.x + _scaleChange * m_scaleSensitivity, scaleMin, scaleMax);
        targetModel.localScale = new Vector3(newScale, newScale, newScale);
    }

    public void MoveModel(float _axisX, float _axisY)
    {
        float newX = Mathf.Clamp(targetModel.position.x + _axisX * m_moveSensitivity, positionYMin * Screen.width / Screen.height, positionYMax * Screen.width / Screen.height);
        float newY = Mathf.Clamp(targetModel.position.y + _axisY * m_moveSensitivity, positionYMin, positionYMax);
        targetModel.position = new Vector3(newX, newY, 0f);
    }

    public void ChangeAntialiasing(bool _next)
    {
        int index = (int)m_antialiasing;

        if (_next)
        {
            index++;
            if (index >= 3)
                index = 0;
        }
        else
        {
            index--;
            if (index < 0)
                index = 2;
        }

        m_antialiasing = (AntialiasingOption)index;

        switch(m_antialiasing)
        {
            case AntialiasingOption.OFF:
                cameraData.antialiasing = AntialiasingMode.None;
                antialiasingText.text = "OFF";
                break;
            case AntialiasingOption.FXAA:
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                antialiasingText.text = "FXAA";
                break;
            case AntialiasingOption.SMAA:
                cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                antialiasingText.text = "SMAA";
                break;
        }
    }

    public void ChangeTonemapping()
    {
        m_tonemapping = !m_tonemapping;

        if (m_tonemapping)
        {
            tonemapping.active = true;
            tonemappingText.text = "ON";
        }
        else
        {
            tonemapping.active = false;
            tonemappingText.text = "OFF";
        }
    }

    public void ChangeVignette()
    {
        m_vignette = !m_vignette;

        if (m_vignette)
        {
            vignette.active = true;
            vignetteText.text = "ON";
        }
        else
        {
            vignette.active = false;
            vignetteText.text = "OFF";
        }
    }

    public void ChangeColorAdjustments()
    {
        m_colorAdjustments = !m_colorAdjustments;

        if (m_colorAdjustments)
        {
            colorAdjustments.active = true;
            colorAdjustmentsText.text = "ON";
        }
        else
        {
            colorAdjustments.active = false;
            colorAdjustmentsText.text = "OFF";
        }
    }

    public void ChangeLightAngle()
    {
        sceneLight.transform.rotation = Quaternion.Euler(lightAngleSlider.value, 0f, 0f);
    }

    public void ChangeLightIntensity()
    {
        sceneLight.intensity = lightIntensitySlider.value;
    }

    public void SwitchMenu(int _index)
    {
        allMenus[m_currentMenuIndex].SetActive(false);
        m_currentMenuIndex = _index;
        allMenus[m_currentMenuIndex].SetActive(true);

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
        switch(m_currentMenuIndex)
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
        switch (m_currentMenuIndex)
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
        switch (m_currentMenuIndex)
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
        m_controlMode = ControlMode.Setting;

        buttonParent.SetActive(false);
        settingParent.SetActive(true);
    }

    public void CloseSettingMenu()
    {
        m_controlMode = ControlMode.View;

        buttonParent.SetActive(true);
        settingParent.SetActive(false);
    }

    public void ChangeRotateSensitivity()
    {
        m_rotateSensitivity = rotateSensitivitySlider.value;
    }

    public void ChangeScaleSensitivity()
    {
        m_scaleSensitivity = scaleSensitivitySlider.value * 0.01f;
    }

    public void ChangeMoveSensitivity()
    {
        m_moveSensitivity = moveSensitivitySlider.value * 0.05f;
    }
}
