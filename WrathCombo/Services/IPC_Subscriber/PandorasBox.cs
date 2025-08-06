#region

using System;
using Dalamud.Plugin.Ipc;
using ECommons.DalamudServices;
using ECommons.Logging;

#endregion

namespace WrathCombo.Services.IPC_Subscriber;

internal sealed class PandorasBoxIPC(
    string? pluginName = null,
    Version? validVersion = null)
    : ReusableIPC(pluginName ?? "PandorasBox", validVersion ?? new Version(1, 0, 0, 0))
{
    private ICallGateSubscriber<string, bool>? _getFeatureEnabled;
    private ICallGateSubscriber<string, string, bool>? _getConfigEnabled;

    public bool GetFeatureEnabled(string featureName)
    {
        if (!IsEnabled)
        {
            PluginLog.Debug($"[ConflictingPlugins] [{PluginName}] is not enabled.");
            return false;
        }

        try
        {
            _getFeatureEnabled ??= Svc.PluginInterface.GetIpcSubscriber<string, bool>("PandorasBox.GetFeatureEnabled");
            var enabled = _getFeatureEnabled.InvokeFunc(featureName);
            PluginLog.Verbose($"[ConflictingPlugins] [{PluginName}] Feature '{featureName}' enabled: {enabled}");
            return enabled;
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[ConflictingPlugins] [{PluginName}] GetFeatureEnabled failed: {e.Message}");
            return false;
        }
    }

    public bool GetConfigEnabled(string featureName, string configProperty)
    {
        if (!IsEnabled)
        {
            PluginLog.Debug($"[ConflictingPlugins] [{PluginName}] is not enabled.");
            return false;
        }

        try
        {
            _getConfigEnabled ??= Svc.PluginInterface.GetIpcSubscriber<string, string, bool>("PandorasBox.GetConfigEnabled");
            var enabled = _getConfigEnabled.InvokeFunc(featureName, configProperty);
            PluginLog.Verbose($"[ConflictingPlugins] [{PluginName}] Config '{featureName}.{configProperty}' enabled: {enabled}");
            return enabled;
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[ConflictingPlugins] [{PluginName}] GetConfigEnabled failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if any action-related features that could conflict with WrathCombo are enabled
    /// </summary>
    public bool HasActionConflicts()
    {
        if (!IsEnabled) return false;

        try
        {
            // Check for features that directly use ActionManager->UseAction() which could interfere
            return GetFeatureEnabled("Auto Tank Stance") ||
                   GetFeatureEnabled("Auto Peloton") ||
                   GetFeatureEnabled("Auto Sprint") ||
                   GetFeatureEnabled("Auto Fairy") ||
                   GetFeatureEnabled("Auto Mount Combat") ||
                   GetFeatureEnabled("Auto Cordial") ||
                   GetFeatureEnabled("Auto Collect");
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[ConflictingPlugins] [{PluginName}] HasActionConflicts failed: {e.Message}");
            return false;
        }
    }

    public override void Dispose()
    {
        _getFeatureEnabled = null;
        _getConfigEnabled = null;
        base.Dispose();
    }
}