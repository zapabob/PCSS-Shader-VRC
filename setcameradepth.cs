// SetCameraDepthMode.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraDepthMode : MonoBehaviour
{
    public bool enabled;
    public DepthTextureMode depthMode = DepthTextureMode.Depth;
    public DepthTextureMode depthMode2 = DepthTextureMode.None;
    public enum antiAliasing
    {
        None = 0,
        Two = 2,
        Four = 4,
        Eight = 8,
    }
    public antiAliasing MSAA = antiAliasing.None;
    private Camera _camera;

    private void Start()
    {
        SetDepthMode();
    }

    private void SetDepthMode()
    {
        if (!enabled)
            return;

        if (!_camera)
            _camera = GetComponent<Camera>();
        if (!_camera)
            return;

        _camera.depthTextureMode = depthMode | depthMode2;
        QualitySettings.antiAliasing = (int)MSAA;
        _camera.allowMSAA = (MSAA != antiAliasing.None);
    }
}