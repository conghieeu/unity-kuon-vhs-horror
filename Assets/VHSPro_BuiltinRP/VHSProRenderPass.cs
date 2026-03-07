using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VHSProRenderPass : ScriptableRenderPass
{
    private postVHSPro settings;
    private string profilerTag = "VHS Pro Effect";

    // Materials
    private Material mat1;      // 1st pass
    private Material mat2;      // 2nd pass
    private Material mat3;      // 3rd pass (feedback)
    private Material mat4;      // 4th pass (feedback blend)
    private Material mat_clear; // clear
    private Material mat_tape;  // tape noise

    // Persistent Render Textures (feedback/tape must persist across frames)
    private RenderTexture texPass12;
    private RenderTexture texPass23;
    private RenderTexture texLast;
    private RenderTexture texFeedback;
    private RenderTexture texFeedback2;
    private RenderTexture texClear;
    private RenderTexture texTape;

    // Time
    private float time_;

    // Temp RT for source copy
    private int srcCopyId;

    public VHSProRenderPass()
    {
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        srcCopyId = Shader.PropertyToID("_VHSProSrcCopy");
    }

    public void Setup(postVHSPro vhsSettings)
    {
        settings = vhsSettings;
    }

    private void CreateMaterials()
    {
        if (settings.shader1 != null && mat1 == null) mat1 = CoreUtils.CreateEngineMaterial(settings.shader1);
        if (settings.shader2 != null && mat2 == null) mat2 = CoreUtils.CreateEngineMaterial(settings.shader2);
        if (settings.shader3 != null && mat3 == null) mat3 = CoreUtils.CreateEngineMaterial(settings.shader3);
        if (settings.shader4 != null && mat4 == null) mat4 = CoreUtils.CreateEngineMaterial(settings.shader4);
        if (settings.shader_clear != null && mat_clear == null) mat_clear = CoreUtils.CreateEngineMaterial(settings.shader_clear);
        if (settings.shader_tape != null && mat_tape == null) mat_tape = CoreUtils.CreateEngineMaterial(settings.shader_tape);
    }

    private void CreateTextures(RenderTextureDescriptor desc)
    {
        int w = desc.width;
        int h = desc.height;

        ReleaseTexture(ref texClear);
        texClear = new RenderTexture(w, h, 0);
        texClear.filterMode = FilterMode.Point;
        texClear.Create();

        ReleaseTexture(ref texPass12);
        texPass12 = new RenderTexture(w, h, 0);
        texPass12.filterMode = FilterMode.Point;
        texPass12.Create();

        ReleaseTexture(ref texPass23);
        texPass23 = new RenderTexture(w, h, 0);
        texPass23.filterMode = FilterMode.Point;
        texPass23.Create();

        ReleaseTexture(ref texFeedback);
        texFeedback = new RenderTexture(w, h, 0);
        texFeedback.hideFlags = HideFlags.HideAndDontSave;
        texFeedback.filterMode = FilterMode.Point;
        texFeedback.Create();

        ReleaseTexture(ref texFeedback2);
        texFeedback2 = new RenderTexture(w, h, 0);
        texFeedback2.hideFlags = HideFlags.HideAndDontSave;
        texFeedback2.filterMode = FilterMode.Point;
        texFeedback2.Create();

        ReleaseTexture(ref texLast);
        texLast = new RenderTexture(w, h, 0);
        texLast.hideFlags = HideFlags.HideAndDontSave;
        texLast.filterMode = FilterMode.Point;
        texLast.Create();

        // Clear persistent textures via CommandBuffer
        var cmd = CommandBufferPool.Get("VHS Clear Init");
        cmd.Blit(texClear, texFeedback, mat_clear);
        cmd.Blit(texClear, texFeedback2, mat_clear);
        cmd.Blit(texClear, texLast, mat_clear);
        Graphics.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void ReleaseTexture(ref RenderTexture tex)
    {
        if (tex != null)
        {
            tex.Release();
            CoreUtils.Destroy(tex);
            tex = null;
        }
    }

    private void FeatureToggle(Material mat, bool propVal, string featureName)
    {
        if (propVal) mat.EnableKeyword(featureName);
        else mat.DisableKeyword(featureName);
    }

#pragma warning disable CS0618 // Suppress obsolete warning for Execute (needed for compatibility mode)
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings == null || !settings.enabled || !settings.gameObject.activeInHierarchy)
            return;

        var cameraData = renderingData.cameraData;

        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

        try
        {
            // Create materials if needed
            if (mat1 == null) CreateMaterials();
            if (mat1 == null) return; // shaders not assigned

            // Get camera target
            var renderer = cameraData.renderer;
#pragma warning disable CS0618
            var cameraColorTarget = renderer.cameraColorTargetHandle;
#pragma warning restore CS0618
            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            // Create intermediate textures if needed
            if (texPass12 == null || (desc.width != texPass12.width || desc.height != texPass12.height))
            {
                CreateTextures(desc);
            }

            // Noise tex
            float screenLinesNum_ = settings.screenLinesNum;
            if (screenLinesNum_ <= 0) screenLinesNum_ = desc.height;

            if (settings.tapeNoiseOn || settings.filmgrainOn || settings.lineNoiseOn)
            {
                if (texTape == null || (texTape.height != Mathf.Min(settings.noiseLinesNum, screenLinesNum_)))
                {
                    int texHeight = (int)Mathf.Min(settings.noiseLinesNum, screenLinesNum_);
                    int texWidth = (int)((float)texHeight * (float)desc.width / (float)desc.height);

                    ReleaseTexture(ref texTape);
                    texTape = new RenderTexture(texWidth, texHeight, 0);
                    texTape.hideFlags = HideFlags.HideAndDontSave;
                    texTape.filterMode = FilterMode.Point;
                    texTape.Create();

                    cmd.Blit(texClear, texTape, mat_tape);
                }
            }

            // Time
            if (settings.independentTimeOn) time_ = Time.unscaledTime;
            else time_ = Time.time;

            // --- 1ST PASS ---
            mat1.SetFloat("time_", time_);
            mat1.SetFloat("screenLinesNum", screenLinesNum_);
            mat1.SetFloat("noiseLinesNum", settings.noiseLinesNum);
            mat1.SetFloat("noiseQuantizeX", settings.noiseQuantizeX);

            FeatureToggle(mat1, settings.filmgrainOn, "VHS_FILMGRAIN_ON");
            FeatureToggle(mat1, settings.tapeNoiseOn, "VHS_TAPENOISE_ON");
            FeatureToggle(mat1, settings.lineNoiseOn, "VHS_LINENOISE_ON");

            FeatureToggle(mat1, settings.jitterHOn, "VHS_JITTER_H_ON");
            mat1.SetFloat("jitterHAmount", settings.jitterHAmount);

            FeatureToggle(mat1, settings.jitterVOn, "VHS_JITTER_V_ON");
            mat1.SetFloat("jitterVAmount", settings.jitterVAmount);
            mat1.SetFloat("jitterVSpeed", settings.jitterVSpeed);

            FeatureToggle(mat1, settings.linesFloatOn, "VHS_LINESFLOAT_ON");
            mat1.SetFloat("linesFloatSpeed", settings.linesFloatSpeed);

            FeatureToggle(mat1, settings.twitchHOn, "VHS_TWITCH_H_ON");
            mat1.SetFloat("twitchHFreq", settings.twitchHFreq);

            FeatureToggle(mat1, settings.twitchVOn, "VHS_TWITCH_V_ON");
            mat1.SetFloat("twitchVFreq", settings.twitchVFreq);

            FeatureToggle(mat1, settings.scanLinesOn, "VHS_SCANLINES_ON");
            mat1.SetFloat("scanLineWidth", settings.scanLineWidth);

            FeatureToggle(mat1, settings.signalNoiseOn, "VHS_YIQNOISE_ON");
            mat1.SetFloat("signalNoisePower", settings.signalNoisePower);
            mat1.SetFloat("signalNoiseAmount", settings.signalNoiseAmount);

            FeatureToggle(mat1, settings.stretchOn, "VHS_STRETCH_ON");

            FeatureToggle(mat1, settings.fisheyeOn, "VHS_FISHEYE_ON");
            mat1.SetFloat("cutoffX", settings.cutoffX);
            mat1.SetFloat("cutoffY", settings.cutoffY);
            mat1.SetFloat("cutoffFadeX", settings.cutoffFadeX);
            mat1.SetFloat("cutoffFadeY", settings.cutoffFadeY);

            // --- 2ND PASS ---
            mat2.SetFloat("time_", time_);
            mat2.SetFloat("screenLinesNum", screenLinesNum_);

            FeatureToggle(mat2, settings.bleedOn, "VHS_BLEED_ON");

            mat2.DisableKeyword("VHS_OLD_THREE_PHASE");
            mat2.DisableKeyword("VHS_THREE_PHASE");
            mat2.DisableKeyword("VHS_TWO_PHASE");

            if (settings.crtMode == 0) mat2.EnableKeyword("VHS_OLD_THREE_PHASE");
            else if (settings.crtMode == 1) mat2.EnableKeyword("VHS_THREE_PHASE");
            else if (settings.crtMode == 2) mat2.EnableKeyword("VHS_TWO_PHASE");
            else if (settings.crtMode == 3) { if (settings.bleedCurveEditModeOn) settings.BuildCurves(); }

            mat2.SetTexture("_CurvesTex", settings.TexCurves);
            mat2.SetVector("curvesOffest", settings.CurvesOffset);
            mat2.SetInteger("bleedLength", settings.bleedLength);
            FeatureToggle(mat2, (settings.crtMode == 3), "VHS_CUSTOM_BLEED_ON");
            FeatureToggle(mat2, settings.bleedDebugOn, "VHS_DEBUG_BLEEDING_ON");

            mat2.SetFloat("bleedAmount", settings.bleedAmount);

            FeatureToggle(mat2, settings.fisheyeOn, "VHS_FISHEYE_ON");
            FeatureToggle(mat2, settings.fisheyeType == 1, "VHS_FISHEYE_HYPERSPACE");
            mat2.SetFloat("fisheyeBend", settings.fisheyeBend);
            mat2.SetFloat("fisheyeSize", settings.fisheyeSize);

            FeatureToggle(mat2, settings.vignetteOn, "VHS_VIGNETTE_ON");
            mat2.SetFloat("vignetteAmount", settings.vignetteAmount);
            mat2.SetFloat("vignetteSpeed", settings.vignetteSpeed);

            FeatureToggle(mat2, settings.signalTweakOn, "VHS_SIGNAL_TWEAK_ON");
            mat2.SetFloat("signalAdjustY", settings.signalAdjustY);
            mat2.SetFloat("signalAdjustI", settings.signalAdjustI);
            mat2.SetFloat("signalAdjustQ", settings.signalAdjustQ);
            mat2.SetFloat("signalShiftY", settings.signalShiftY);
            mat2.SetFloat("signalShiftI", settings.signalShiftI);
            mat2.SetFloat("signalShiftQ", settings.signalShiftQ);
            mat2.SetFloat("gammaCorection", settings.gammaCorection);

            // --- TAPE NOISE ---
            if (settings.tapeNoiseOn || settings.filmgrainOn || settings.lineNoiseOn)
            {
                mat_tape.SetFloat("time_", time_);

                FeatureToggle(mat_tape, settings.filmgrainOn, "VHS_FILMGRAIN_ON");
                mat_tape.SetFloat("filmGrainAmount", settings.filmGrainAmount);

                FeatureToggle(mat_tape, settings.tapeNoiseOn, "VHS_TAPENOISE_ON");
                mat_tape.SetFloat("tapeNoiseTH", settings.tapeNoiseTH);
                mat_tape.SetFloat("tapeNoiseAmount", settings.tapeNoiseAmount);
                mat_tape.SetFloat("tapeNoiseSpeed", settings.tapeNoiseSpeed);

                FeatureToggle(mat_tape, settings.lineNoiseOn, "VHS_LINENOISE_ON");
                mat_tape.SetFloat("lineNoiseAmount", settings.lineNoiseAmount);
                mat_tape.SetFloat("lineNoiseSpeed", settings.lineNoiseSpeed);

                cmd.Blit(texTape, texTape, mat_tape);

                mat1.SetTexture("_TapeTex", texTape);
                mat1.SetFloat("tapeNoiseAmount", settings.tapeNoiseAmount);
            }

            // --- RENDER CHAIN ---

            // Get temp RT for source copy
            cmd.GetTemporaryRT(srcCopyId, desc);
            cmd.Blit(cameraColorTarget, srcCopyId);

            // Bypass texture or camera source
            if (settings.spriteTex != null) settings.bypassTex = (Texture)settings.spriteTex.texture;

            if (settings.bypassTex != null)
                cmd.Blit(settings.bypassTex, texPass12, mat1);
            else
                cmd.Blit(srcCopyId, texPass12, mat1);

            if (!settings.feedbackOn)
            {
                // No feedback — direct pass 2 to output
                cmd.Blit(texPass12, cameraColorTarget, mat2);
            }
            else
            {
                cmd.Blit(texPass12, texPass23, mat2);

                // Recalc feedback buffer
                mat3.SetTexture("_LastTex", texLast);
                mat3.SetTexture("_FeedbackTex", texFeedback);
                mat3.SetFloat("feedbackThresh", settings.feedbackThresh);
                mat3.SetFloat("feedbackAmount", settings.feedbackAmount);
                mat3.SetFloat("feedbackFade", settings.feedbackFade);
                mat3.SetColor("feedbackColor", settings.feedbackColor);
                cmd.Blit(texPass23, texFeedback2, mat3);

                cmd.Blit(texFeedback2, texFeedback);

                // Mix last frame and feedback buffer
                mat4.SetFloat("feedbackAmp", 1.0f);
                mat4.SetTexture("_FeedbackTex", texFeedback);
                cmd.Blit(texPass23, texLast, mat4);

                if (!settings.feedbackDebugOn)
                    cmd.Blit(texLast, cameraColorTarget);
                else
                    cmd.Blit(texFeedback, cameraColorTarget);
            }

            cmd.ReleaseTemporaryRT(srcCopyId);

            context.ExecuteCommandBuffer(cmd);
        }
        finally
        {
            CommandBufferPool.Release(cmd);
        }
    }
#pragma warning restore CS0618

    public void Cleanup()
    {
        ReleaseTexture(ref texPass12);
        ReleaseTexture(ref texPass23);
        ReleaseTexture(ref texLast);
        ReleaseTexture(ref texFeedback);
        ReleaseTexture(ref texFeedback2);
        ReleaseTexture(ref texClear);
        ReleaseTexture(ref texTape);

        CoreUtils.Destroy(mat1);
        CoreUtils.Destroy(mat2);
        CoreUtils.Destroy(mat3);
        CoreUtils.Destroy(mat4);
        CoreUtils.Destroy(mat_clear);
        CoreUtils.Destroy(mat_tape);
        mat1 = mat2 = mat3 = mat4 = mat_clear = mat_tape = null;
    }
}
