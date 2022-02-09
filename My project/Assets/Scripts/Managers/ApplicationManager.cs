using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif
#if UNITY_IOS
using UnityEngine.iOS;
#endif

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
    [Header("General")]
    public EventSystem eventSystem;
    public AssetManager assetManager;
    public Transform targetModel;
    private ControlMode m_controlMode = ControlMode.View;

    private bool m_onePointTouchBegin;   // make sure certain behaviour only happens during one point touch
    private bool m_twoPointsTouchBegin;  // make sure certain behaviour only happens during two points touch
    private Vector2 m_touchedPosition;   // record touched postion from previous frame
    private bool m_modelTouched;         // if we hit model when the touch starts
    private float m_twoPointsDistance;   // record distance between two touched points from previous frame

    [Space]
    [Header("Transform Limit")]
    public float scaleMin;      // minimum scale
    public float scaleMax;      // maximum scale
    public float positionYMin;  // border to keep the model inside the screen
    public float positionYMax;  // border to keep the model inside the screen

    [Space]
    [Header("Control Sensitivity")]
    private float m_rotateSensitivity;  // sensitivity of rotating model
    private float m_scaleSensitivity;   // sensitivity of scaling model
    private float m_moveSensitivity;    // sensitivity of moving model

    [Space]
    [Header("Main UI Reference")]
    public GameObject UIBottomLeft;
    public GameObject UITopRight;
    private int m_currentMenuIndex = 0;
    public GameObject[] allMenus;

    [Space]
    [Header("Post Processing")]
    public Volume globalVolume;
    private UniversalAdditionalCameraData cameraData;
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

    [Space]
    [Header("Light")]
    public Light sceneLight;
    public Slider lightAngleSlider;
    public Slider lightIntensitySlider;

    [Space]
    [Header("Setting")]
    public GameObject settingParent;
    public GameObject instructionForMouse;
    public GameObject instructionForTouchScreen;
    public Slider rotateSensitivitySlider;
    public Slider scaleSensitivitySlider;
    public Slider moveSensitivitySlider;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize post processing reference
        globalVolume.profile.TryGet(out tonemapping);
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out colorAdjustments);
        cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();

        // Initialize control sensitivities based on slider value
        ChangeRotateSensitivity();
        ChangeScaleSensitivity();
        ChangeMoveSensitivity();

#if UNITY_ANDROID || UNITY_IOS

        // Scale up UI for mobile device
        UIBottomLeft.transform.localScale *= 2.5f;
        UITopRight.transform.localScale *= 2.5f;
        settingParent.transform.localScale *= 2.5f;

        // Turn some raycast on for easier mobile device interaction
        lightAngleSlider.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;
        lightIntensitySlider.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;
        rotateSensitivitySlider.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;
        scaleSensitivitySlider.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;
        moveSensitivitySlider.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;

        instructionForTouchScreen.SetActive(true);
#else
        instructionForMouse.SetActive(true);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // take input when it is in view mode
        if (m_controlMode == ControlMode.View)
        {
            // one point touch
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    m_onePointTouchBegin = true;
                    m_touchedPosition = touch.position;

                    // check if user touch the model
                    m_modelTouched = false;
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(m_touchedPosition), out hit))
                    {
                        if (hit.collider.transform == targetModel)
                            m_modelTouched = true;
                    }
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    if (m_onePointTouchBegin)
                    {
                        // move the model when user start by touching on the model
                        if (m_modelTouched)
                        {
                            Vector2 movement = touch.position - m_touchedPosition;
                            MoveModel(movement.x * 20f / Screen.height, movement.y * 20f / Screen.height);
                        }
                        // rotate the model when user start by touching somewhere else
                        else if (!eventSystem.currentSelectedGameObject)
                        {
                            Vector2 movement = touch.position - m_touchedPosition;
                            RotateModel(movement.y * 0.05f, -movement.x * 0.05f);
                        }

                        // set position for next frame
                        m_touchedPosition = touch.position;
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    m_onePointTouchBegin = false;
                }
            }
            else if (Input.touchCount == 2)
            {
                // end one point input
                m_onePointTouchBegin = false;

                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                if (touch2.phase == TouchPhase.Began)
                {
                    m_twoPointsTouchBegin = true;

                    m_twoPointsDistance = (touch2.position - touch1.position).magnitude;
                }
                else if (touch2.phase == TouchPhase.Moved)
                {
                    if (m_twoPointsTouchBegin)
                    {
                        // calculate distance difference between two frames
                        float newtwoPointsDistance = (touch2.position - touch1.position).magnitude;
                        float deltatwoPointsDistance = newtwoPointsDistance - m_twoPointsDistance;

                        if (deltatwoPointsDistance > 0.5f || deltatwoPointsDistance < -0.5f)
                        {
                            ScaleModel(deltatwoPointsDistance * 0.05f);
                        }

                        // set distance for next frame
                        m_twoPointsDistance = newtwoPointsDistance;
                    }
                }
                else if (touch2.phase == TouchPhase.Ended)
                {
                    m_twoPointsTouchBegin = false;
                }
            }
            else if (Input.touchCount > 2)
            {
                m_twoPointsTouchBegin = false;
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

    public void ResetModel()
    {
        targetModel.rotation = Quaternion.identity;
        targetModel.localScale = Vector3.one;
        targetModel.position = Vector3.zero;

        // abandoned
        //assetManager.ResetModelAssets(targetModel);
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

        // change anti-aliasing
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
        // hide previous menu and show new menu
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

        UIBottomLeft.SetActive(false);
        UITopRight.SetActive(false);
        settingParent.SetActive(true);
    }

    public void CloseSettingMenu()
    {
        m_controlMode = ControlMode.View;

        UIBottomLeft.SetActive(true);
        UITopRight.SetActive(true);
        settingParent.SetActive(false);
    }

    public void ChangeRotateSensitivity()
    {
        m_rotateSensitivity = rotateSensitivitySlider.value;
    }

    public void ChangeScaleSensitivity()
    {
        m_scaleSensitivity = scaleSensitivitySlider.value;
    }

    public void ChangeMoveSensitivity()
    {
        m_moveSensitivity = moveSensitivitySlider.value;
    }
}
