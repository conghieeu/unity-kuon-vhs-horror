using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu ("Image Effects/VHS Pro")]
public class postVHSPro : MonoBehaviour {

	//main props
#pragma warning disable CS0414
	float time_ = 0.0f;
#pragma warning restore CS0414

	//properties will be assigned automatically 
	public Shader shader1 = null; //1st pass  
	public Shader shader2 = null; //2nd pass  
	public Shader shader3 = null; //3rd pass  
	public Shader shader4 = null; //4th pass  
	public Shader shader_clear = null; //clear
	public Shader shader_tape = null; //tape noise


	//groups/sections folding save
	public bool g_showCRT = true;
	public bool g_showNoise = true;
	public bool g_showJitter = true;
	public bool g_showSignal = true;
	public bool g_showFeedback = true;
	public bool g_showExtra = false;
	public bool g_showBypass = false;


	//CRT
	public bool bleedOn = true; 
	public int crtMode = 0; 
	public int crtLinesMode = 0;
	public float screenLinesNum = 240f;
	public float bleedAmount = 1f; //default 1.-2.
	public bool bleedDebugOn = false;

	public AnimationCurve bleedCurveY = AnimationCurve.Linear(0,1,1,0);
	public AnimationCurve bleedCurveI = AnimationCurve.Linear(0,0.5f,1,0);
	public AnimationCurve bleedCurveQ = AnimationCurve.Linear(0,0.5f,1,0);

	public int bleedLength = 21; 
	public bool bleedCurveEditModeOn = false;
	public bool bleedCurveIQSyncOn = true;

	//curves
	int max_curve_length = 50;
	Texture2D texCurves = null; //curves tex
	Vector4 curvesOffset = new Vector4(0, 0, 0, 0);
	float[,] curvesData = new float[50,3];

	// Public properties for RenderPass access
	public Texture2D TexCurves { get { return texCurves; } }
	public Vector4 CurvesOffset { get { return curvesOffset; } }

	public bool fisheyeOn = true;
	public float fisheyeBend = 2.0f; 
	public int 	 fisheyeType = 0; 
	public float fisheyeSize = 1.2f; 	
	public float cutoffX = 2.0f;
	public float cutoffY = 3.0f;		   		
	public float cutoffFadeX = 25.0f;
	public float cutoffFadeY = 25.0f;


	public bool vignetteOn = false;
	public float vignetteAmount = 1.0f; 
	public float vignetteSpeed = 1.0f; 


	//NOISE
	public int noiseLinesMode = 1;
	public float noiseLinesNum = 240f; //noise quantation for Y
	public float noiseQuantizeX = 0.0f; 

	public bool filmgrainOn = false;
	public float filmGrainAmount = 0.016f; 
	// public float filmGrainPower = 1.0f; //not using atm

	public bool signalNoiseOn = true; 
	public float signalNoiseAmount = 0.15f; 
	public float signalNoisePower = 0.83f; 


	public bool tapeNoiseOn = true;
	public float tapeNoiseTH = 0.63f; 
	public float tapeNoiseAmount = 1.0f; 
	public float tapeNoiseSpeed = 1.0f; 
	public bool lineNoiseOn = true;
	public float lineNoiseAmount = 1.0f; 
	public float lineNoiseSpeed = 5.0f; 


	//JITTER
	public bool scanLinesOn = false;
	public float scanLineWidth = 10.0f;
	
	public bool linesFloatOn = false; 
	public float linesFloatSpeed = 1.0f; 
	public bool stretchOn = true;

	public bool twitchHOn = false; 
	public float twitchHFreq = 1.0f; //default .5-1.
	public bool twitchVOn = false; 
	public float twitchVFreq = 1.0f; //default .5-1.

	public bool jitterHOn = true; 
	public float jitterHAmount = 0.5f; //default .5-1.
	public bool jitterVOn = false; 
	public float jitterVAmount = 1.0f; //default 1.
	public float jitterVSpeed = 1.0f; //default 1.
	

	//SIGNAL TWEAK
	public bool signalTweakOn = false; 
	public float signalAdjustY = 0f; //Luma
	public float signalAdjustI = 0f; //Chrominance
	public float signalAdjustQ = 0f; //Chrominance

	public float signalShiftY = 1f; //Luma
	public float signalShiftI = 1f; //Chrominance
	public float signalShiftQ = 1f; //Chrominance


	public float gammaCorection = 1f; 

	//FEEDBACK
	public bool feedbackOn = false; 
	public int feedbackMode = 0; 
	public float feedbackThresh = 0.1f; 
	public float feedbackAmount = 2.0f; 	
	public float feedbackFade = 0.82f; 
	public Color feedbackColor = new Color(1.0f,0.5f,0.0f); 
	public bool feedbackDebugOn = false; 


	//TOOLS (ADDITIONAL SECTION)
	public bool independentTimeOn = false; 


	//BYPASS
	public Texture bypassTex;
	public Sprite spriteTex;


	void Awake(){		
		if(crtMode==3) BuildCurves(); 
	}

	// Update is called once per frame
	void Update () {
	}


	//sample the curve
	public void BuildCurves(){ //AnimationCurve curve, int Length

		if(texCurves==null) texCurves  = new Texture2D(max_curve_length, 1, TextureFormat.RGBA32, false);

		//curves negative offsets
		curvesOffset[0] = 0.0f;
		curvesOffset[1] = 0.0f;
		curvesOffset[2] = 0.0f;
	
		//sample curves
		float t = 0.0f;
		for(int i=0; i<bleedLength; i++){

			t =  ((float)i)/((float)bleedLength);
			curvesData[i,0] = bleedCurveY.Evaluate( t );
			curvesData[i,1] = bleedCurveI.Evaluate( t );
			curvesData[i,2] = bleedCurveQ.Evaluate( t );
			if(bleedCurveIQSyncOn) curvesData[i,2] = curvesData[i,1]; //IQ sunc			

			if(curvesOffset[0]>curvesData[i,0]) curvesOffset[0] = curvesData[i,0];			
			if(curvesOffset[1]>curvesData[i,1]) curvesOffset[1] = curvesData[i,1];			
			if(curvesOffset[2]>curvesData[i,2]) curvesOffset[2] = curvesData[i,2];			
		};

		//offset is negative -> lets make it possitive
		curvesOffset[0] = Mathf.Abs(curvesOffset[0]); 		
		curvesOffset[1] = Mathf.Abs(curvesOffset[1]); 		
		curvesOffset[2] = Mathf.Abs(curvesOffset[2]);		
		
		for(int i=0; i<bleedLength; i++){

			curvesData[i,0] += curvesOffset[0]; 		
			curvesData[i,1] += curvesOffset[1]; 		
			curvesData[i,2] += curvesOffset[2]; 				

			//also -2 is super weird)
			texCurves.SetPixel(-2+bleedLength-i, 0, new Color(curvesData[i,0],curvesData[i,1],curvesData[i,2]));				
			
		};

		texCurves.Apply();			

	}

}
