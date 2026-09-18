#!/usr/bin/env python3
"""Generate Unity metas, scenes, and ProjectSettings for Colony Haul."""
from __future__ import annotations

import hashlib
import os
from pathlib import Path

ROOT = Path("/workspace")
ASSETS = ROOT / "Assets"
PS = ROOT / "ProjectSettings"
PKG = ROOT / "Packages"

# Stable GUIDs from path hashes so reruns stay deterministic.
def guid_for(rel: str) -> str:
    return hashlib.md5(rel.encode()).hexdigest()


DEFAULT_IMPORTER = """fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

SCRIPT_META = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

FOLDER_META = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

ASMDEF_META = """fileFormatVersion: 2
guid: {guid}
AssemblyDefinitionImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def write_meta(path: Path) -> str:
    rel = str(path.relative_to(ROOT))
    g = guid_for(rel)
    meta = path.with_suffix(path.suffix + ".meta") if path.is_file() else Path(str(path) + ".meta")
    if path.is_dir():
        text = FOLDER_META.format(guid=g)
    elif path.suffix == ".cs":
        text = SCRIPT_META.format(guid=g)
    elif path.suffix == ".asmdef":
        text = ASMDEF_META.format(guid=g)
    else:
        text = DEFAULT_IMPORTER.format(guid=g)
    meta.write_text(text)
    return g


def scene_yaml(name: str, script_guid: str, component: str) -> str:
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {{fileID: 0}}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 1
  m_FogColor: {{r: 0.1, g: 0.24, b: 0.27, a: 1}}
  m_FogMode: 3
  m_FogDensity: 0.02
  m_LinearFogStart: 18
  m_LinearFogEnd: 48
  m_AmbientSkyColor: {{r: 0.35, g: 0.45, b: 0.48, a: 1}}
  m_AmbientEquatorColor: {{r: 0.2, g: 0.2, b: 0.2, a: 1}}
  m_AmbientGroundColor: {{r: 0.15, g: 0.1, b: 0.08, a: 1}}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {{r: 0.42, g: 0.48, b: 0.63, a: 1}}
  m_SkyboxMaterial: {{fileID: 0}}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {{fileID: 0}}
  m_SpotCookie: {{fileID: 0}}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {{fileID: 0}}
  m_Sun: {{fileID: 0}}
  m_IndirectSpecularColor: {{r: 0, g: 0, b: 0, a: 1}}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {{fileID: 0}}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {{fileID: 0}}
  m_LightingSettings: {{fileID: 0}}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {{fileID: 0}}
--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 100001}}
  - component: {{fileID: 100002}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &100002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
--- !u!1 &200000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 200001}}
  - component: {{fileID: 200002}}
  - component: {{fileID: 200003}}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &200001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0.279, y: 0.364, z: -0.115, w: 0.88}}
  m_LocalPosition: {{x: 18, y: 20, z: 18}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 35, y: 45, z: 0}}
--- !u!20 &200002
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 1
  m_BackGroundColor: {{r: 0.08, g: 0.21, b: 0.25, a: 1}}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {{x: 2, y: 11}}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {{x: 36, y: 24}}
  m_LensShift: {{x: 0, y: 0}}
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 80
  field of view: 60
  orthographic: 1
  orthographic size: 14
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {{fileID: 0}}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 1
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
--- !u!81 &200003
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  m_Enabled: 1
"""


def main() -> None:
    PS.mkdir(parents=True, exist_ok=True)
    PKG.mkdir(parents=True, exist_ok=True)

    (PS / "ProjectVersion.txt").write_text(
        "m_EditorVersion: 2022.3.50f1\nm_EditorVersionWithRevision: 2022.3.50f1 (c3db7f8c9b10)\n"
    )

    (PKG / "manifest.json").write_text(
        """{
  "dependencies": {
    "com.unity.ide.rider": "3.0.31",
    "com.unity.ide.visualstudio": "2.0.22",
    "com.unity.nuget.newtonsoft-json": "3.2.2",
    "com.unity.render-pipelines.universal": "14.0.11",
    "com.unity.ugui": "1.0.0",
    "com.gamenami.unity-scemantic-bridge": "file:../ThirdParty/unity-semantic-bridge/com.gamenami.unity-semantic-bridge",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.particlesystem": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.umbra": "1.0.0",
    "com.unity.modules.unitywebrequest": "1.0.0"
  }
}
"""
    )

    boot_cs = ROOT / "Assets/_ColonyHaul/Scripts/Boot/BootLoader.cs"
    slice_cs = ROOT / "Assets/_ColonyHaul/Scripts/Boot/VerticalSliceBootstrap.cs"
    boot_guid = guid_for(str(boot_cs.relative_to(ROOT)))
    slice_guid = guid_for(str(slice_cs.relative_to(ROOT)))

    scenes = ROOT / "Assets/_ColonyHaul/Scenes"
    scenes.mkdir(parents=True, exist_ok=True)
    (scenes / "Boot.unity").write_text(scene_yaml("BootLoader", boot_guid, "BootLoader"))
    (scenes / "Game_VerticalSlice.unity").write_text(
        scene_yaml("VerticalSlice", slice_guid, "VerticalSliceBootstrap")
    )

    boot_scene_guid = guid_for("Assets/_ColonyHaul/Scenes/Boot.unity")
    slice_scene_guid = guid_for("Assets/_ColonyHaul/Scenes/Game_VerticalSlice.unity")

    (PS / "EditorBuildSettings.asset").write_text(
        f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1045 &1
EditorBuildSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Scenes:
  - enabled: 1
    path: Assets/_ColonyHaul/Scenes/Boot.unity
    guid: {boot_scene_guid}
  - enabled: 1
    path: Assets/_ColonyHaul/Scenes/Game_VerticalSlice.unity
    guid: {slice_scene_guid}
  m_configObjects: {{}}
"""
    )

    (PS / "ProjectSettings.asset").write_text(
        """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!129 &1
PlayerSettings:
  productGUID: 7c0a1e2b3d4f5a697887766554433221
  companyName: Gamenami
  productName: Colony Haul
  defaultScreenWidth: 1920
  defaultScreenHeight: 1080
  defaultScreenWidthWeb: 960
  defaultScreenHeightWeb: 600
  m_ShowUnitySplashScreen: 0
  androidRenderOutsideSafeArea: 1
  androidEnableTango: 0
  m_ActiveColorSpace: 1
  m_SpriteBatchVertexThreshold: 300
  m_MTRendering: 1
  mipStripping: 0
  numberOfMipsStripped: 0
  m_StackTraceTypes: 010000000100000001000000010000000100000001000000
  iosShowActivityIndicatorOnLoading: -1
  androidShowActivityIndicatorOnLoading: -1
  iosUseCustomAppBackgroundBehavior: 0
  allowedAutorotateToPortrait: 1
  allowedAutorotateToPortraitUpsideDown: 1
  allowedAutorotateToLandscapeRight: 1
  allowedAutorotateToLandscapeLeft: 1
  useOSAutorotation: 1
  use32BitDisplayBuffer: 1
  preserveFramebufferAlpha: 0
  disableDepthAndStencilBuffers: 0
  androidStartInFullscreen: 1
  androidRenderOutsideSafeArea: 1
  androidUseSwappy: 1
  androidBlitType: 0
  androidResizableWindow: 0
  androidDefaultWindowWidth: 1920
  androidDefaultWindowHeight: 1080
  androidMinimumWindowWidth: 400
  androidMinimumWindowHeight: 300
  bakeCollisionMeshes: 0
  forceSingleInstance: 0
  useFlipModelSwapchain: 1
  resizableWindow: 1
  useMacAppStoreValidation: 0
  macAppStoreCategory: public.app-category.games
  gpuSkinning: 1
  xboxPIXTextureCapture: 0
  xboxEnableAvatar: 0
  xboxEnableKinect: 0
  xboxEnableKinectAutoTracking: 0
  xboxEnableFitness: 0
  visibleInBackground: 1
  allowFullscreenSwitch: 1
  fullscreenMode: 1
  xboxSpeechDB: 0
  xboxEnableHeadOrientation: 0
  xboxEnableGuest: 0
  xboxEnablePIXSampling: 0
  metalFramebufferOnly: 0
  xboxOneResolution: 0
  xboxOneSResolution: 0
  xboxOneXResolution: 3
  xboxOneMonoLoggingLevel: 0
  xboxOneLoggingLevel: 1
  xboxOneDisableEsram: 0
  xboxOneEnableTypeOptimization: 0
  xboxOnePresentImmediateThreshold: 0
  switchQueueCommandMemory: 0
  switchQueueControlMemory: 16384
  switchQueueComputeMemory: 262144
  vulkanNumSwapchainBuffers: 3
  vulkanEnableSetSRGBWrite: 0
  vulkanEnablePreTransform: 1
  vulkanEnableLateAcquireNextImage: 0
  vulkanEnableCommandBufferRecycling: 1
  m_SupportedAspectRatios:
    4:3: 1
    5:4: 1
    16:10: 1
    16:9: 1
    Others: 1
  bundleVersion: 0.1.0
  preloadedAssets: []
  metroInputSource: 0
  wsaTransparentSwapchain: 0
  xboxOneDisableKinectGpuReservation: 1
  xboxOneEnable7thCore: 1
  vrSettings:
    enable360StereoCapture: 0
  isWsaHolographicRemotingEnabled: 0
  enableFrameTimingStats: 0
  enableOpenGLProfilerGPURecorders: 1
  useHDRDisplay: 0
  hdrBitDepth: 0
  m_ColorGamuts: 00000000
  targetPixelDensity: 30
  resolutionScalingMode: 0
  resetResolutionOnWindowResize: 0
  androidSupportedAspectRatio: 1
  androidMaxAspectRatio: 2.1
  applicationIdentifier:
    Standalone: com.gamenami.colonyhaul
  buildNumber:
    Standalone: 0
    iPhone: 0
    tvOS: 0
  overrideDefaultApplicationIdentifier: 1
  AndroidBundleVersionCode: 1
  AndroidMinSdkVersion: 22
  AndroidTargetSdkVersion: 0
  AndroidPreferredInstallLocation: 1
  aotOptions: 
  stripEngineCode: 1
  iPhoneStrippingLevel: 0
  iPhoneScriptCallOptimization: 0
  ForceInternetPermission: 0
  ForceSDCardPermission: 0
  CreateWallpaper: 0
  APKExpansionFiles: 0
  keepLoadedShadersAlive: 0
  StripUnusedMeshComponents: 1
  strictShaderVariantMatching: 0
  VertexChannelCompressionMask: 4054
  iPhoneSdkVersion: 988
  macOSXTargetOSVersion: 101400
  tvOSSdkVersion: 0
  tvOSRequireExtendedGameController: 0
  tvOSTargetOSVersionString: 12.0
  uIPrerenderedIcon: 0
  uIRequiresPersistentWiFi: 0
  uIRequiresFullScreen: 1
  uIStatusBarHidden: 1
  uIExitOnSuspend: 0
  uIStatusBarStyle: 0
  appleTVSplashScreen: {fileID: 0}
  appleTVSplashScreen2x: {fileID: 0}
  tvOSSmallIconLayers: []
  tvOSSmallIconLayers2x: []
  tvOSLargeIconLayers: []
  tvOSLargeIconLayers2x: []
  tvOSTopShelfImageLayers: []
  tvOSTopShelfImageLayers2x: []
  tvOSTopShelfImageWideLayers: []
  tvOSTopShelfImageWideLayers2x: []
  iOSLaunchScreenType: 0
  iOSLaunchScreenPortrait: {fileID: 0}
  iOSLaunchScreenLandscape: {fileID: 0}
  iOSLaunchScreenBackgroundColor:
    serializedVersion: 2
    rgba: 0
  iOSLaunchScreenFillPct: 100
  iOSLaunchScreenSize: 100
  iOSLaunchScreenCustomXibPath: 
  iOSLaunchScreeniPadType: 0
  iOSLaunchScreeniPadImage: {fileID: 0}
  iOSLaunchScreeniPadBackgroundColor:
    serializedVersion: 2
    rgba: 0
  iOSLaunchScreeniPadFillPct: 100
  iOSLaunchScreeniPadSize: 100
  iOSLaunchScreeniPadCustomXibPath: 
  iOSLaunchScreenCustomStoryboardPath: 
  iOSLaunchScreeniPadCustomStoryboardPath: 
  iOSDeviceRequirements: []
  iOSURLSchemes: []
  macOSURLSchemes: []
  iOSBackgroundModes: 0
  iOSMetalForceHardShadows: 0
  metalEditorSupport: 1
  metalAPIValidation: 1
  iOSRenderExtraFrameOnPause: 0
  iosCopyPluginsCodeInsteadOfLinking: 0
  appleDeveloperTeamID: 
  iOSManualSigningProvisioningProfileID: 
  tvOSManualSigningProvisioningProfileID: 
  iOSManualSigningProvisioningProfileType: 0
  tvOSManualSigningProvisioningProfileType: 0
  appleEnableAutomaticSigning: 0
  iOSRequireARKit: 0
  iOSAutomaticallyDetectAndAddCapabilities: 1
  appleEnableProMotion: 0
  shaderPrecisionModel: 0
  clonedFromGUID: 00000000000000000000000000000000
  templatePackageId: 
  templateDefaultScene: 
  useCustomMainManifest: 0
  useCustomLauncherManifest: 0
  useCustomMainGradleTemplate: 0
  useCustomLauncherGradleManifest: 0
  useCustomBaseGradleTemplate: 0
  useCustomGradlePropertiesTemplate: 0
  useCustomGradleSettingsTemplate: 0
  useCustomProguardFile: 0
  AndroidTargetArchitectures: 1
  AndroidSplashScreenScale: 0
  androidSplashScreen: {fileID: 0}
  AndroidKeystoreName: 
  AndroidKeyaliasName: 
  AndroidEnableArmv9SecurityFeatures: 0
  AndroidBuildApkPerCpuArchitecture: 0
  AndroidTVCompatibility: 0
  AndroidIsGame: 1
  AndroidEnableTango: 0
  androidEnableBanner: 1
  androidUseLowAccuracyLocation: 0
  androidUseCustomKeystore: 0
  m_AndroidBanners:
  - width: 320
    height: 180
    banner: {fileID: 0}
  androidGamepadSupportLevel: 0
  chromeosInputEmulation: 1
  AndroidMinifyWithR8: 0
  AndroidMinifyRelease: 0
  AndroidMinifyDebug: 0
  AndroidValidateAppBundleSize: 1
  AndroidAppBundleSizeToValidate: 150
  m_BuildTargetIcons: []
  m_BuildTargetPlatformIcons: []
  m_BuildTargetBatching: []
  m_BuildTargetShaderSettings: []
  m_BuildTargetGraphicsJobs: []
  m_BuildTargetGraphicsJobMode: []
  m_BuildTargetGraphicsAPIs: []
  m_BuildTargetVRSettings: []
  m_DefaultShaderChunkSizeInMB: 16
  m_DefaultShaderChunkCount: 0
  openGLRequireES31: 0
  openGLRequireES31AEP: 0
  openGLRequireES32: 0
  m_TemplateCustomTags: {}
  mobileMTRendering:
    Android: 1
    iPhone: 1
    tvOS: 1
  m_BuildTargetGroupLightmapEncodingQuality: []
  m_BuildTargetGroupHDRCubemapEncodingQuality: []
  m_BuildTargetGroupLightmapSettings: []
  m_BuildTargetGroupLoadStoreDebugModeSettings: []
  m_BuildTargetNormalMapEncoding: []
  m_BuildTargetDefaultTextureCompressionFormat: []
  playModeTestRunnerEnabled: 0
  runPlayModeTestAsEditModeTest: 0
  actionOnDotNetUnhandledException: 1
  enableInternalProfiler: 0
  logObjCUncaughtExceptions: 1
  enableCrashReportAPI: 0
  cameraUsageDescription: 
  locationUsageDescription: 
  microphoneUsageDescription: 
  bluetoothUsageDescription: 
  macOSTargetOSVersion: 10.13.0
  switchNMETAOverride: 
  switchNetLibKey: 
  switchSocketMemoryPoolSize: 6144
  switchSocketAllocatorPoolSize: 128
  switchSocketConcurrencyLimit: 14
  switchScreenResolutionBehavior: 2
  switchUseNewStyleFilepaths: 1
  switchUseLegacyFmodPriorities: 0
  switchUseMicroSleepForYield: 1
  switchEnableRamDiskSupport: 0
  switchMicroSleepForYieldTime: 25
  switchRamDiskSpaceSize: 12
  ps4NPAgeRating: 12
  ps4NPTitleSecret: 
  ps4NPTrophyPackPath: 
  ps4ParentalLevel: 11
  ps4ContentID: ED1633-NPXX51362_00-0000000000000000
  ps4Category: 0
  ps4MasterVersion: 01.00
  ps4AppVersion: 01.00
  ps4AppType: 0
  ps4ParamSfxPath: 
  ps4VideoOutPixelFormat: 0
  ps4VideoOutInitialWidth: 1920
  ps4VideoOutBaseModeInitialWidth: 1920
  ps4VideoOutReprojectionRate: 60
  ps4PronunciationXMLPath: 
  ps4PronunciationSIGPath: 
  ps4BackgroundImagePath: 
  ps4StartupImagePath: 
  ps4StartupImagesFolder: 
  ps4IconImagesFolder: 
  ps4SaveDataImagePath: 
  ps4SdkOverride: 
  ps4BGMPath: 
  ps4ShareFilePath: 
  ps4ShareOverlayImagePath: 
  ps4PrivacyGuardImagePath: 
  ps4ExtraSceSysFile: 
  ps4NPtitleDatPath: 
  ps4RemotePlayKeyAssignment: -1
  ps4RemotePlayKeyMappingDir: 
  ps4PlayTogetherPlayerCount: 0
  ps4EnterButtonAssignment: 1
  ps4ApplicationParam1: 0
  ps4ApplicationParam2: 0
  ps4ApplicationParam3: 0
  ps4ApplicationParam4: 0
  ps4DownloadDataSize: 0
  ps4GarlicHeapSize: 2048
  ps4ProGarlicHeapSize: 2560
  playerPrefsMaxSize: 32768
  ps4Passcode: frAQBc8Wsa1xVPf6iCR6ax7YE8
  ps4pnSessions: 1
  ps4pnPresence: 1
  ps4pnFriends: 1
  ps4pnGameCustomData: 1
  playerPrefsSupport: 0
  enableApplicationExit: 0
  resetTempFolder: 1
  restrictedAudioUsageRights: 0
  ps4UseResolutionFallback: 0
  ps4ReprojectionSupport: 0
  ps4UseAudio3dBackend: 0
  ps4UseLowGarlicFragmentationMode: 1
  ps4SocialScreenEnabled: 0
  ps4ScriptOptimizationLevel: 0
  ps4Audio3dVirtualSpeakerCount: 14
  ps4attribCpuUsage: 0
  ps4PatchPkgPath: 
  ps4PatchLatestPkgPath: 
  ps4PatchChangeinfoPath: 
  ps4PatchDayOne: 0
  ps4attribUserManagement: 0
  ps4attribMoveSupport: 0
  ps4attrib3DSupport: 0
  ps4attribShareSupport: 0
  ps4attribExclusiveVR: 0
  ps4disableAutoHideSplash: 0
  ps4videoRecordingFeaturesUsed: 0
  ps4contentSearchFeaturesUsed: 0
  ps4CompatibilityPS5: 0
  ps4AllowPS5Detection: 0
  ps4GPU800MHz: 1
  ps4attribEyeToEyeDistanceSettingVR: 0
  ps4IncludedModules: []
  ps4attribVROutputEnabled: 0
  monoEnv: 
  splashScreenBackgroundSourceLandscape: {fileID: 0}
  splashScreenBackgroundSourcePortrait: {fileID: 0}
  blurSplashScreenBackground: 1
  spritePackerPolicy: 
  webGLMemorySize: 32
  webGLExceptionSupport: 1
  webGLNameFilesAsHashes: 0
  webGLShowDiagnostics: 0
  webGLDataCaching: 1
  webGLDebugSymbols: 0
  webGLEmscriptenArgs: 
  webGLModulesDirectory: 
  webGLTemplate: APPLICATION:Default
  webGLAnalyzeBuildSize: 0
  webGLUseEmbeddedResources: 0
  webGLCompressionFormat: 1
  webGLWasmArithmeticExceptions: 0
  webGLLinkerTarget: 1
  webGLThreadsSupport: 0
  webGLDecompressionFallback: 0
  webGLInitialMemorySize: 32
  webGLMaximumMemorySize: 2048
  webGLMemoryGrowthMode: 2
  webGLMemoryLinearGrowthStep: 16
  webGLMemoryGeometricGrowthStep: 0.2
  webGLMemoryGeometricGrowthCap: 96
  webGLPowerPreference: 2
  scriptingDefineSymbols: {}
  additionalCompilerArguments: {}
  platformArchitecture: {}
  scriptingBackend: {}
  il2cppCompilerConfiguration: {}
  il2cppCodeGeneration: {}
  managedStrippingLevel: {}
  incrementalIl2cppBuild: {}
  suppressCommonWarnings: 1
  allowUnsafeCode: 0
  useDeterministicCompilation: 1
  additionalIl2CppArgs: 
  scriptingRuntimeVersion: 1
  gcIncremental: 1
  gcWBarrierValidation: 0
  apiCompatibilityLevelPerPlatform: {}
  m_RenderingPath: 1
  m_MobileRenderingPath: 1
  metroPackageName: ColonyHaul
  metroPackageVersion: 
  metroCertificatePath: 
  metroCertificatePassword: 
  metroCertificateSubject: 
  metroCertificateIssuer: 
  metroCertificateNotAfter: 0000000000000000
  metroApplicationDescription: Colony Haul
  wsaImages: {}
  metroTileShortName: 
  metroTileShowName: 0
  metroMediumTileShowName: 0
  metroLargeTileShowName: 0
  metroWideTileShowName: 0
  metroSupportStreamingInstall: 0
  metroLastRequiredScene: 0
  metroDefaultTileSize: 1
  metroTileForegroundText: 2
  metroTileBackgroundColor: {r: 0.08, g: 0.21, b: 0.25, a: 1}
  metroSplashScreenBackgroundColor: {r: 0.08, g: 0.21, b: 0.25, a: 1}
  metroSplashScreenUseBackgroundColor: 0
  platformCapabilities: {}
  metroTargetDeviceFamilies: {}
  metroFTAName: 
  metroFTAFileTypes: []
  metroProtocolName: 
  vcxProjDefaultLanguage: 
  XboxOneProductId: 
  XboxOneUpdateKey: 
  XboxOneSandboxId: 
  XboxOneContentId: 
  XboxOneTitleId: 
  XboxOneSCId: 
  XboxOneGameOsOverridePath: 
  XboxOnePackagingOverridePath: 
  XboxOneAppManifestOverridePath: 
  XboxOneVersion: 1.0.0.0
  XboxOnePackageEncryption: 0
  XboxOnePackageUpdateGranularity: 2
  XboxOneDescription: 
  XboxOneLanguage:
  - enus
  XboxOneCapability: []
  XboxOneGameRating: {}
  XboxOneIsContentPackage: 0
  XboxOneEnhancedXboxCompatibilityMode: 0
  XboxOneEnableGPUVariability: 1
  XboxOneSockets: {}
  XboxOneSplashScreen: {fileID: 0}
  XboxOneAllowedProductIds: []
  XboxOnePersistentLocalStorageSize: 0
  XboxOneXTitleMemory: 8
  XboxOneOverrideIdentityName: 
  XboxOneOverrideIdentityPublisher: 
  vrEditorSettings: {}
  cloudServicesEnabled: {}
  luminIcon:
    m_Name: 
    m_ModelFolderPath: 
    m_PortalFolderPath: 
  luminCert:
    m_CertPath: 
    m_SignPackage: 1
  luminIsChannelApp: 0
  luminVersion:
    m_VersionTitle: 
    m_VersionCode: 1
  hmiPlayerDataPath: 
  hmiForceSRGBBlit: 1
  embeddedLinuxEnableGamepadInput: 1
  hmiLogStartupTiming: 0
  hmiCpuConfiguration: 
  apiCompatibilityLevel: 6
  activeInputHandler: 0
  windowsGamepadBackendBehavior: 0
  cloudProjectId: 
  framebufferDepthMemorylessMode: 0
  qualitySettingsNames: []
  projectName: Colony Haul
  organizationId: 
  cloudEnabled: 0
  legacyClampBlendShapeWeights: 0
  hmiLoadingImage: {fileID: 0}
  platformRequiresReadableAssets: 0
  virtualTexturingSupportEnabled: 0
  insecureHttpOption: 0
"""
    )

    (PS / "EditorSettings.asset").write_text(
        """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!159 &1
EditorSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_SerializationMode: 2
  m_LineEndingsForNewScripts: 1
  m_DefaultBehaviorMode: 0
  m_PrefabRegularEnvironment: {fileID: 0}
  m_PrefabUIEnvironment: {fileID: 0}
  m_SpritePackerMode: 0
  m_SpritePackerCacheSize: 10
  m_SpritePackerPaddingPower: 1
  m_EtcTextureCompressorBehavior: 1
  m_EtcTextureFastCompressor: 1
  m_EtcTextureNormalCompressor: 2
  m_EtcTextureBestCompressor: 4
  m_ProjectGenerationIncludedExtensions: txt;xml;fnt;cd;asmdef;asmref;rsp
  m_ProjectGenerationRootNamespace: ColonyHaul
  m_EnableTextureStreamingInEditMode: 1
  m_EnableTextureStreamingInPlayMode: 1
  m_EnableEditorAsyncCPUTextureLoading: 0
  m_AsyncShaderCompilation: 1
  m_DisableCookiesInLightmapper: 1
  m_BlockInspectorUpdatesUntilFinalized: 1
"""
    )

    (PS / "TagManager.asset").write_text(
        """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!78 &1
TagManager:
  serializedVersion: 2
  tags: []
  layers:
  - Default
  - TransparentFX
  - Ignore Raycast
  - 
  - Water
  - UI
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  - 
  m_SortingLayers:
  - name: Default
    uniqueID: 0
    locked: 0
"""
    )

    (PS / "TimeManager.asset").write_text(
        """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!5 &1
TimeManager:
  m_ObjectHideFlags: 0
  Fixed Timestep: 0.02
  Maximum Allowed Timestep: 0.33333334
  m_TimeScale: 1
  Maximum Particle Timestep: 0.03
"""
    )

    urp_guid = guid_for("Assets/Settings/URP-ColonyHaul.asset")
    renderer_guid = guid_for("Assets/Settings/URP-ColonyHaul-Renderer.asset")
    settings = ROOT / "Assets/Settings"
    settings.mkdir(parents=True, exist_ok=True)
    (settings / "URP-ColonyHaul-Renderer.asset").write_text(
        f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: de640fe3d0db1804a85f9fc8f5cadabf, type: 3}}
  m_Name: URP-ColonyHaul-Renderer
  m_EditorClassIdentifier: 
  m_RendererFeatures: []
  m_RendererFeatureMap: 
  postProcessData: {{fileID: 0}}
  xrSystemData: {{fileID: 0}}
  shaders: {{fileID: 0}}
  m_OpaqueLayerMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_TransparentLayerMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_DefaultStencilState:
    overrideStencilState: 0
    stencilReference: 0
    stencilCompareFunction: 8
    passOperation: 2
    failOperation: 0
    zFailOperation: 0
  m_ShadowTransparentReceive: 1
  m_RenderingMode: 0
  m_DepthPrimingMode: 0
  m_CopyDepthMode: 1
  m_AccurateGbufferNormals: 0
  m_ClusteredRendering: 0
  m_TileSize: 32
"""
    )
    (settings / "URP-ColonyHaul.asset").write_text(
        f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 15c394f3fa53d6045a97997554027756, type: 3}}
  m_Name: URP-ColonyHaul
  m_EditorClassIdentifier: 
  k_AssetVersion: 11
  k_AssetPreviousVersion: 11
  m_RendererType: 1
  m_RendererData: {{fileID: 0}}
  m_RendererDataList:
  - {{fileID: 11400000, guid: {renderer_guid}, type: 2}}
  m_DefaultRendererIndex: 0
  m_RequireDepthTexture: 0
  m_RequireOpaqueTexture: 0
  m_OpaqueDownsampling: 1
  m_SupportsTerrainHoles: 1
  m_SupportsHDR: 1
  m_MSAA: 1
  m_RenderScale: 1
  m_UpscalingFilter: 0
  m_FsrOverrideSharpness: 0
  m_FsrSharpness: 0.92
  m_EnableLODCrossFade: 1
  m_LODCrossFadeDitheringType: 1
  m_MainLightRenderingMode: 1
  m_MainLightShadowsSupported: 1
  m_MainLightShadowmapResolution: 1024
  m_AdditionalLightsRenderingMode: 1
  m_AdditionalLightsPerObjectLimit: 4
  m_AdditionalLightShadowsSupported: 0
  m_AdditionalLightsShadowmapResolution: 512
  m_AdditionalLightsShadowResolutionTierLow: 256
  m_AdditionalLightsShadowResolutionTierMedium: 512
  m_AdditionalLightsShadowResolutionTierHigh: 1024
  m_ReflectionProbeBlending: 0
  m_ReflectionProbeBoxProjection: 0
  m_ShadowDistance: 42
  m_ShadowCascadeCount: 1
  m_Cascade2Split: 0.25
  m_Cascade3Split: {{x: 0.1, y: 0.3}}
  m_Cascade4Split: {{x: 0.067, y: 0.2, z: 0.467}}
  m_CascadeBorder: 0.1
  m_ShadowDepthBias: 1
  m_ShadowNormalBias: 1
  m_AnyShadowsSupported: 1
  m_SoftShadowsSupported: 1
  m_ConservativeEnclosingSphere: 0
  m_NumIterationsEnclosingSphere: 64
  m_SoftShadowQuality: 2
  m_AdditionalLightsCookieResolution: 512
  m_AdditionalLightsCookieFormat: 3
  m_UseSRPBatcher: 1
  m_SupportsDynamicBatching: 0
  m_MixedLightingSupported: 1
  m_SupportsLightCookies: 1
  m_SupportsLightLayers: 0
  m_DebugLevel: 0
  m_StoreActionsOptimization: 0
  m_UseAdaptivePerformance: 1
  m_ColorGradingMode: 0
  m_ColorGradingLutSize: 32
  m_UseFastSRGBLinearConversion: 0
  m_SupportDataDrivenLensFlare: 1
  m_ShadowType: 1
  m_LocalShadowsSupported: 0
  m_LocalShadowsAtlasResolution: 256
  m_MaxPixelLights: 0
  m_ShadowAtlasResolution: 256
  m_VolumeFrameworkUpdateMode: 0
  m_Textures:
    blueNoise64LTex: {{fileID: 0}}
    bayerMatrixTex: {{fileID: 0}}
  m_PrefilteringModeMainLightShadows: 1
  m_PrefilteringModeAdditionalLight: 4
  m_PrefilteringModeAdditionalLightShadows: 1
  m_PrefilterXRKeywords: 0
  m_PrefilteringModeForwardPlus: 0
  m_PrefilteringModeDeferredRendering: 0
  m_PrefilteringModeScreenSpaceOcclusion: 0
  m_PrefilterDebugKeywords: 0
  m_PrefilterWriteRenderingLayers: 0
  m_PrefilterHDROutput: 0
  m_PrefilterSSAODepthNormals: 0
  m_PrefilterSSAOSourceDepthLow: 0
  m_PrefilterSSAOSourceDepthMedium: 0
  m_PrefilterSSAOSourceDepthHigh: 0
  m_PrefilterSSAOInterleaved: 0
  m_PrefilterSSAOBlueNoise: 0
  m_PrefilterSSAOSampleCountLow: 0
  m_PrefilterSSAOSampleCountMedium: 0
  m_PrefilterSSAOSampleCountHigh: 0
  m_PrefilterDBufferMRT1: 0
  m_PrefilterDBufferMRT2: 0
  m_PrefilterDBufferMRT3: 0
  m_PrefilterSoftShadowsQualityLow: 0
  m_PrefilterSoftShadowsQualityMedium: 0
  m_PrefilterSoftShadowsQualityHigh: 0
  m_PrefilterSoftShadows: 0
  m_PrefilterScreenCoord: 0
  m_PrefilterNativeRenderPass: 0
  m_ShaderVariantLogLevel: 0
  m_ShadowCascades: 0
"""
    )

    (PS / "GraphicsSettings.asset").write_text(
        f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!30 &1
GraphicsSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 15
  m_Deferred:
    m_Mode: 1
    m_Shader: {{fileID: 0}}
  m_DeferredReflections:
    m_Mode: 1
    m_Shader: {{fileID: 0}}
  m_ScreenSpaceShadows:
    m_Mode: 1
    m_Shader: {{fileID: 0}}
  m_DepthNormals:
    m_Mode: 1
    m_Shader: {{fileID: 0}}
  m_MotionVectors:
    m_Mode: 1
    m_Shader: {{fileID: 0}}
  m_LightHalo:
    m_Mode: 1
    m_Shader: {{fileID: 0}}
  m_LensFlare:
    m_Mode: 1
    m_Shader: {{fileID: 0}}
  m_VideoShadersIncludeMode: 2
  m_AlwaysIncludedShaders: []
  m_PreloadedShaders: []
  m_PreloadShadersBatchTimeLimit: -1
  m_SpritesDefaultMaterial: {{fileID: 0}}
  m_CustomRenderPipeline: {{fileID: 11400000, guid: {urp_guid}, type: 2}}
  m_TransparencySortMode: 0
  m_TransparencySortAxis: {{x: 0, y: 0, z: 1}}
  m_DefaultRenderingPath: 1
  m_DefaultMobileRenderingPath: 1
  m_TierSettings: []
  m_LightmapStripping: 0
  m_FogStripping: 0
  m_InstancingStripping: 0
  m_BrgStripping: 0
  m_LightmapKeepPlain: 1
  m_LightmapKeepDirCombined: 1
  m_LightmapKeepDynamicPlain: 1
  m_LightmapKeepDynamicDirCombined: 1
  m_LightmapKeepShadowMask: 1
  m_LightmapKeepSubtractive: 1
  m_FogKeepLinear: 1
  m_FogKeepExp: 1
  m_FogKeepExp2: 1
  m_AlbedoSwatchInfos: []
  m_LightsUseLinearIntensity: 1
  m_LightsUseColorTemperature: 1
  m_DefaultRenderingLayerMask: 1
  m_LogWhenShaderIsCompiled: 0
  m_SRPDefaultSettings:
    UnityEngine.Rendering.Universal.UniversalRenderPipeline: {{fileID: 11400000, guid: {urp_guid}, type: 2}}
"""
    )

    for dirpath, dirnames, filenames in os.walk(ASSETS):
        d = Path(dirpath)
        write_meta(d)
        for fn in filenames:
            if fn.endswith(".meta"):
                continue
            write_meta(d / fn)

    print("boot", boot_guid)
    print("slice", slice_guid)
    print("urp", urp_guid)


if __name__ == "__main__":
    main()
