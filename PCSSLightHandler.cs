using UnityEngine;
using VRC.SDKBase;

public class PCSSLightInstaller : MonoBehaviour
{
    private void Start()
    {
        SetupPCSSLight();
    }

    private void SetupPCSSLight()
    {
        try
        {
            var pcssLight = GetComponent<PCSSLight>();
            if (pcssLight)
            {
                pcssLight.Setup();
                var plugin = gameObject.AddComponent<PCSSLightPlugin>();
                VRCObjectSync.AddNativePlugin(plugin);
            }
            else
            {
                Debug.LogError("PCSSLight component not found on the game object.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error setting up PCSS Light: {ex.Message}");
        }
    }
}public class PCSSLightPlugin : SDKUnityNativePluginBase
{
    private PCSSLight pcssLight;

    private void Start()
    {
        pcssLight = GetComponent<PCSSLight>();
    }

    public override void OnDestroy()
    {
        try
        {
            pcssLight.DestroyShadowRenderTexture();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error destroying PCSS Light shadow render texture: {ex.Message}");
        }
    }

    public override void OnUpdate()
    {
        try
        {
            pcssLight.UpdateShaderValues();
            pcssLight.UpdateCommandBuffer();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error updating PCSS Light: {ex.Message}");
        }
    }
}
