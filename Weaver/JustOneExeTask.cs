using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Community.Packer;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.TypeSignatures;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Configuration;
using PostSharp.Sdk.Extensibility.Tasks;

namespace PostSharp.Community.Packer.Weaver
{
    [ExportTask(Phase = TaskPhase.CustomTransform, TaskName = nameof(Packer.Weaver.JustOneExeTask))] 
     public partial class JustOneExeTask : Task
     {
         [ImportService]
         private IAnnotationRepositoryService annotationsService;

         public override bool Execute()
        {
            // Find configuration:
            var annotations =
                annotationsService.GetAnnotationsOfType(typeof(Packer.PackerAttribute), false, true);
            Packer.PackerAttribute config = new Packer.PackerAttribute();
            if (annotations.MoveNext())
            {
                config = Configuration.Read(annotations.Current);
            }

            // Find gatherable assemblies:
            string[] paths = this.Project.Properties["ReferenceCopyLocalPaths"]?.Split('|') ?? new string[0];
            
            this.manifest = this.Project.Module.AssemblyManifest;
            
            FindMsCoreReferences();
            FixResourceCase(manifest);
            ProcessNativeResources(manifest, !config.DisableCompression);
            EmbedResources(config, paths);

            // CalculateHash();
            ImportAssemblyLoader(config.CreateTemporaryAssemblies);
            CallAttach(config);
            //
            // AddChecksumsToTemplate();
            // BuildUpNameDictionary(config.CreateTemporaryAssemblies, config.PreloadOrder);
            
            return true;
        }
     }
}