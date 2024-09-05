using System;
using System.IO;

using UnityEngine;
using UnityEditor;

using YooAsset.Editor;
using YooAsset;

public static class YooAssetExtend
{
}

//自定义扩展范例
public class CollectLua : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        Debug.Log(string.Format("data.AssetPath: {0}", data.AssetPath));
        return data.AssetPath.EndsWith(".lua.bytes");
    }
}

abstract public class BuildPipelineBuilderBase
{
    protected string PackageName;
    protected EBuildPipeline BuildPipeline;

    public BuildPipelineBuilderBase(string packageName, EBuildPipeline buildPipeline)
    {
        PackageName = packageName;
        BuildPipeline = buildPipeline;
    }

    protected IEncryptionServices CreateEncryptionInstance()
    {
        var encyptionClassName = AssetBundleBuilderSetting.GetPackageEncyptionClassName(PackageName, BuildPipeline);
        var encryptionClassTypes = YooAsset.Editor.EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
        var classType = encryptionClassTypes.Find(x => x.FullName.Equals(encyptionClassName));
        if (classType != null)
            return (IEncryptionServices)Activator.CreateInstance(classType);
        else
            return null;
    }
    public string GetDefaultPackageVersion()
    {
        int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
        return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
    }

    abstract public BuildResult BuildInternal(BuildTarget buildTarget, string packageVersion);
}

public class BuiltinBuildPipelineBuilder : BuildPipelineBuilderBase
{

    public BuiltinBuildPipelineBuilder(string packageName)
        :base(packageName, EBuildPipeline.BuiltinBuildPipeline) { }

    public override BuildResult BuildInternal(BuildTarget buildTarget, string packageVersion)
    {
        Debug.Log($"开始构建 : {buildTarget}");

        // 构建参数
        BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = BuildPipeline.ToString();
        buildParameters.BuildTarget = buildTarget;
        buildParameters.BuildMode = EBuildMode.IncrementalBuild;
        buildParameters.PackageName = PackageName;
        buildParameters.PackageVersion = packageVersion;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.EnableSharePackRule = true; //启用共享资源构建模式，兼容1.5x版本
        buildParameters.FileNameStyle = EFileNameStyle.HashName;
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
        buildParameters.BuildinFileCopyParams = string.Empty;
        buildParameters.EncryptionServices = CreateEncryptionInstance();
        buildParameters.CompressOption = ECompressOption.LZ4;

        // 执行构建
        BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
        return pipeline.Run(buildParameters, true);
    }
}


public class ScriptableBuildPipelineBuilder : BuildPipelineBuilderBase
{

    public ScriptableBuildPipelineBuilder(string packageName)
        :base(packageName, EBuildPipeline.ScriptableBuildPipeline) {}

    public override BuildResult BuildInternal(BuildTarget buildTarget, string packageVersion)
    {
        ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = BuildPipeline.ToString();
        buildParameters.BuildTarget = buildTarget;
        buildParameters.BuildMode = EBuildMode.IncrementalBuild;
        buildParameters.PackageName = PackageName;
        buildParameters.PackageVersion = packageVersion;
        buildParameters.EnableSharePackRule = true;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.FileNameStyle = EFileNameStyle.HashName;
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
        buildParameters.BuildinFileCopyParams = string.Empty;
        buildParameters.EncryptionServices = CreateEncryptionInstance();
        buildParameters.CompressOption = ECompressOption.LZ4;

        ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
        return pipeline.Run(buildParameters, true);
    }
}
