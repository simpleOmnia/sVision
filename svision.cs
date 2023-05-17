using svision_internal;
using UnityEngine;
using ShaderHandler = svision_internal.ShaderHandler;

public class svision : MonoBehaviour
{

    public int postprocBlurAmount = 21;
    public int preprocBlurAmount = 21; 
    public float electrodesToPixelsGauss;
    public ComputeBuffer ElectrodeToNeuronGauss;
    public ComputeBuffer PixelsToElectrodesGauss;


    private int DownscalescaleFactor;
    private DeviceHandler dh;
    private ShaderHandler sh;

    public void Update()
    {
        if (sxr.InitialKeyPress(KeyCode.Alpha1)){
        
            Debug.Log("apply blurs");
            sh.SetPreprocessors(ShaderHandler.ShaderName.HorizontalBlur, ShaderHandler.ShaderName.VerticalBlur);
            sh.SetPreprocessorBlurAmount(preprocBlurAmount);
        }
        if (sxr.InitialKeyPress(KeyCode.Alpha2))
            sh.SetPreprocessors(ShaderHandler.ShaderName.Greyscale);
        if(sxr.InitialKeyPress(KeyCode.Alpha3))
            sh.SetPreprocessors(ShaderHandler.ShaderName.EdgeDetection);
        if (sxr.InitialKeyPress(KeyCode.Alpha4))
        {
            sh.SetPostProcessors(ShaderHandler.ShaderName.PostProcessingHorizontalBlur,
                ShaderHandler.ShaderName.PostProcessingVerticalBlur);
            sh.SetPostprocessorBlurAmount(postprocBlurAmount);
        }

    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
    }

    private void SetShaderVariables()
    {
        
    }

    private void Start()
    {
        dh = new DeviceHandler();
        sh = ShaderHandler.Instance;
        
        dh.LoadDevice(DeviceHandler.PrebuiltImplant.Prima); 
        dh.ListElectrodes();
        dh.MoveAndRotateElectrodeArray(20f, 10f, 10f, 10f);  
        dh.ListElectrodes();
        }

    public static svision Instance;
    private void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject.transform.root); }
        else  Destroy(gameObject); }
}
