using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine.UIElements;

namespace Unity.Services.CloudBuild.Editor
{
    [CustomEditor(typeof(BuildAutomationSettings))]
    internal class BuildAutomationSettingsEditor : UnityEditor.Editor
    {
        private const string k_PlayerSettingsHelpBoxAdjustmentsClass = "custom-player-settings-info-helpbox";
        const string k_Uss = "Packages/com.unity.services.cloud-build/Editor/USS/BuildProfile.uss";

        private Foldout m_BuilderConfigurationFoldout;
        private Label m_BuildAutomationLoadingLabel;

        public override VisualElement CreateInspectorGUI()
        {
            var apiClient = new BuildAutomationApiClient();
            var root = new VisualElement();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_Uss);
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet);
            var canFetchConfig = true;

            if (serializedObject.targetObject is not BuildAutomationSettings buildAutomationSettings)
                throw new InvalidOperationException("Editor object is not of type BuildAutomationSettings.");

            if (!BuildAutomationUtilities.IsConnectedToUnityCloud())
            {
                DisplayConnectToCloudWarning(root);
                canFetchConfig = false;
            }

            if (!BuildAutomationUVCSConnector.IsUvcsConfigured())
            {
                DisplayConfigureUvcsWarning(root);
                canFetchConfig = false;
            }

            if (!canFetchConfig)
            {
                return root;
            }

            // Build the Loading Label, which will be hidden after async calls complete
            m_BuildAutomationLoadingLabel = new Label("Fetching Build Automation Settings...");
            root.Add(m_BuildAutomationLoadingLabel);

            // Build and populate the Config Foldout
            m_BuilderConfigurationFoldout = new Foldout
            {
                text = "Remote Builder Configuration",
                tooltip = "Settings for the remote machine used to build your project in the cloud",
                style =
                {
                    display =  DisplayStyle.None,
                },
            };
            var builderConfig = new Components.BuildAutomationBuilderConfig(apiClient, serializedObject, buildAutomationSettings.buildTarget);
            m_BuilderConfigurationFoldout.Add(builderConfig);
            root.Add(m_BuilderConfigurationFoldout);

            // Append a credentials section if the target requires them
            if (BuildAutomationUtilities.TargetRequiresCredentials(buildAutomationSettings.buildTarget))
            {
                var credentialsConfig = new Components.BuildAutomationCredentialsConfig(apiClient, serializedObject);
                m_BuilderConfigurationFoldout.Add(credentialsConfig);
            }

            // Check if the Unity version and Target are supported. Required async API calls
            ShowConfigIfValid(root, apiClient, buildAutomationSettings);

            return root;
        }

        async void ShowConfigIfValid(VisualElement root, BuildAutomationApiClient apiClient, BuildAutomationSettings buildAutomationSettings)
        {
            var unityVersionSupported = await IsUnityVersionSupported(apiClient);
            var platformSupported = await IsPlatformSupported(apiClient, buildAutomationSettings.buildTarget);

            if (!unityVersionSupported)
            {
                var helpBox = new HelpBox($"This version of Unity is not currently available for use with Build Automation. See the <a href=\"{BuildAutomationDashboardUrls.GetSupportedVersionsUrl()}\">Build Automation documentation</a> for more information", HelpBoxMessageType.Warning);
                helpBox.AddToClassList(k_PlayerSettingsHelpBoxAdjustmentsClass);
                root.Add(helpBox);
            }

            if (!platformSupported)
            {
                var helpBox = new HelpBox($"This Build Target is not currently available for use with Build Automation. See the <a href=\"{BuildAutomationDashboardUrls.GetSupportedPlatformsUrl()}\">Build Automation documentation</a> for more information", HelpBoxMessageType.Warning);
                helpBox.AddToClassList(k_PlayerSettingsHelpBoxAdjustmentsClass);
                root.Add(helpBox);
            }

            if (unityVersionSupported && platformSupported)
            {
                m_BuilderConfigurationFoldout.style.display = DisplayStyle.Flex;
            }
            m_BuildAutomationLoadingLabel.style.display = DisplayStyle.None;
        }

        async Task<bool> IsUnityVersionSupported(BuildAutomationApiClient apiClient)
        {
            var unityVersions = await apiClient.GetSupportedUnityVersions();
            var currentUnityVersion = BuildAutomationUtilities.GetUnityVersion();
            return unityVersions.Any(unityVersion => unityVersion.Value == currentUnityVersion);
        }

        async Task<bool> IsPlatformSupported(BuildAutomationApiClient apiClient, BuildTarget currentTarget)
        {
            var supportedPlatforms = await apiClient.GetSupportedPlatforms(BuildAutomationUtilities.GetUnityVersion());
            return supportedPlatforms.Any(platform => platform.Platform.Equals(currentTarget.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        private void DisplayConnectToCloudWarning(VisualElement root)
        {
            var helpBox = new HelpBox($"Build Automation requires your project to be configured with Unity Cloud.", HelpBoxMessageType.Warning);
            helpBox.AddToClassList(k_PlayerSettingsHelpBoxAdjustmentsClass);
            var button = new Button { text = "Configure Unity Cloud" };
            button.clicked += () =>
            {
                SettingsService.OpenProjectSettings("Project/Services");
            };
            helpBox.Add(button);
            root.Add(helpBox);
        }

        private void DisplayConfigureUvcsWarning(VisualElement root)
        {
            var helpBox = new HelpBox($"Build Automation requires your project to be configured with Unity Version Control.", HelpBoxMessageType.Warning);
            helpBox.AddToClassList(k_PlayerSettingsHelpBoxAdjustmentsClass);
            var button = new Button { text = "Configure Unity Version Control" };
            button.clicked += BuildAutomationUVCSConnector.ShowWindow;
            helpBox.Add(button);
            root.Add(helpBox);
        }
    }
}
