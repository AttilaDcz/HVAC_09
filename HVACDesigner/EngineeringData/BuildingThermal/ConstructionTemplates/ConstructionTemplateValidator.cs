using System;
using System.Collections.Generic;
using System.Linq;
using HVACDesigner.EngineeringData.BuildingThermal.AirLayers;
using HVACDesigner.EngineeringData.BuildingThermal.Materials;

namespace HVACDesigner.EngineeringData.BuildingThermal.ConstructionTemplates
{
    public sealed class ConstructionTemplateValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public IReadOnlyList<string> Errors { get; }
        public IReadOnlyList<string> Warnings { get; }

        public ConstructionTemplateValidationResult(
            IEnumerable<string> errors,
            IEnumerable<string> warnings)
        {
            Errors = (errors ?? Array.Empty<string>()).ToList();
            Warnings = (warnings ?? Array.Empty<string>()).ToList();
        }
    }

    public sealed class ConstructionTemplateValidator
    {
        public ConstructionTemplateValidationResult Validate(
            ConstructionTemplateDefinition template,
            BuildingMaterialCatalog materialCatalog,
            AirLayerCatalog airLayerCatalog)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var errors = new List<string>();
            var warnings = new List<string>();

            if (template.UsesParallelPaths)
            {
                double sum =
                    template.ParallelPaths.Sum(item => item.AreaFraction);

                if (Math.Abs(sum - 1.0) > 1e-6)
                {
                    errors.Add(
                        $"A párhuzamos hőáramú utak területhányada nem 1,0: {sum:0.######}.");
                }

                foreach (ConstructionPathDefinition path in
                    template.ParallelPaths)
                {
                    ValidateLayers(
                        path.Layers,
                        materialCatalog,
                        airLayerCatalog,
                        errors,
                        warnings,
                        $"Útvonal: {path.Id}");
                }
            }
            else
            {
                ValidateLayers(
                    template.Layers,
                    materialCatalog,
                    airLayerCatalog,
                    errors,
                    warnings,
                    "Fő rétegrend");
            }

            return new ConstructionTemplateValidationResult(
                errors,
                warnings);
        }

        private static void ValidateLayers(
            IEnumerable<ConstructionLayerDefinition> layers,
            BuildingMaterialCatalog materialCatalog,
            AirLayerCatalog airLayerCatalog,
            ICollection<string> errors,
            ICollection<string> warnings,
            string context)
        {
            var list = layers.ToList();

            if (list.Count == 0)
            {
                errors.Add($"{context}: üres rétegrend.");
                return;
            }

            if (list.Select(item => item.Order).Distinct().Count() !=
                list.Count)
            {
                errors.Add($"{context}: duplikált rétegsorrend.");
            }

            foreach (ConstructionLayerDefinition layer in list)
            {
                switch (layer.LayerKind)
                {
                    case ConstructionLayerKind.Material:
                        if (materialCatalog == null ||
                            !materialCatalog.TryGet(
                                layer.ReferenceId,
                                out _))
                        {
                            errors.Add(
                                $"{context}: ismeretlen anyag: {layer.ReferenceId}.");
                        }
                        break;

                    case ConstructionLayerKind.AirLayer:
                        if (airLayerCatalog == null ||
                            !airLayerCatalog.TryGet(
                                layer.ReferenceId,
                                out _))
                        {
                            errors.Add(
                                $"{context}: ismeretlen légréteg: {layer.ReferenceId}.");
                        }
                        break;

                    case ConstructionLayerKind.FixedResistance:
                        warnings.Add(
                            $"{context}: közvetlen R-értékű réteg szerepel; forrását dokumentálni kell.");
                        break;
                }
            }
        }
    }
}
