using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class GyroscopeCameraRotating : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Settings")]
    [SerializeField] private float _smoothing = 0.1f;
    [SerializeField] private float _speed = 60.0f;
    [SerializeField] private float _gyroUpdateInterval = 0.0167f;

    [Header("Gyro Settings")]
    [SerializeField] private float _waitGyroInitializationDuration = 1f;
    [SerializeField] private bool _waitGyroInitialization = true;

    private Quaternion _initialRotation;
    private Quaternion _gyroInitialRotation;
    private Quaternion _offsetRotation;

    private bool _gyroEnabled;
    private bool _isGyroInitialized = false;

    private void InitGyro()
    {
        if (!_isGyroInitialized)
        {
            Input.gyro.enabled = true;
            Input.gyro.updateInterval = _gyroUpdateInterval;
        }
        _isGyroInitialized = true;
    }

    private void Awake()
    {
        if (_waitGyroInitialization && _waitGyroInitializationDuration < 0f)
        {
            _waitGyroInitializationDuration = 1f;
            throw new System.ArgumentException("waitGyroInitializationDuration can't be negative, it was set to 1 second");
        }

        ClearRenderTexture();
        videoPlayer.Prepare();
    }

    private IEnumerator Start()
    {
        if (HasGyro())
        {
            InitGyro();
            _gyroEnabled = true;
        }
        else _gyroEnabled = false;

        if (_waitGyroInitialization)
            yield return new WaitForSeconds(_waitGyroInitializationDuration);
        else
            yield return null;

        // Get gyroscope initial rotation for calibration
        _initialRotation = transform.rotation;

        Recalibrate();

        if(videoPlayer.isPrepared)
        {
            videoPlayer.Play();
        }
    }

    private void Update()
    {
        if (_gyroEnabled)
        {
            ApplyGyroRotation();

            float rotationStep = _speed * Time.deltaTime;

            // Progressive rotation of the object
            transform.rotation = Quaternion.Slerp(transform.rotation, _initialRotation * _offsetRotation, _smoothing * rotationStep);
        }
    }

    private void ApplyGyroRotation()
    {
        // Apply initial offset for calibration
        _offsetRotation = Quaternion.Inverse(_gyroInitialRotation) * GyroToUnity(Input.gyro.attitude);
    }

    private Quaternion GyroToUnity(Quaternion gyro)
    {
        return new Quaternion(gyro.x, gyro.y, -gyro.z, -gyro.w);
    }

    private bool HasGyro()
    {
        return SystemInfo.supportsGyroscope;
    }

    public void Recalibrate()
    {
        Quaternion gyro = GyroToUnity(Input.gyro.attitude);
        _gyroInitialRotation = gyro;
    }

    private void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = videoPlayer.targetTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;  
    }
}
