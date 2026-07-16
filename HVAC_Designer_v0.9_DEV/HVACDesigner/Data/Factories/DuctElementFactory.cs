using HVACDesigner.Data.Models.Duct;

using DuctFlowDirection =
    HVACDesigner.Data.Models.Duct.FlowDirection;

namespace HVACDesigner.Data.Factories
{
    public static class DuctElementFactory
    {
        public static DuctSegment CreateSegment(
            DuctDefinition definition,
            ElementCreationParameters parameters)
        {
            return new DuctSegment
            {
                Name = definition.Name,
                Category = definition.Category,
                ElementType = definition.IsFlexible
                    ? DuctElementType.FlexibleDuct
                    : DuctElementType.StraightDuct,

                GeometryType = GeometryType.LengthOnly,
                Geometry = parameters.Geometry,

                Airflow = parameters.Airflow,

                MaterialOverride = parameters.MaterialOverride,

                Length = parameters.Length,

                Direction = definition.FlowDirection,

                PressureLossType = PressureLossType.Friction
            };
        }


        public static DuctFitting CreateCircularFitting(
            CircularFittingDefinition definition,
            ElementCreationParameters parameters)
        {
            return CreateFitting(
                definition.Name,
                "Kör idom",
                definition.GeometryType,
                definition.FlowDirection,
                definition.DefaultZeta,
                parameters);
        }


        public static DuctFitting CreateRectangularFitting(
            RectangularFittingDefinition definition,
            ElementCreationParameters parameters)
        {
            return CreateFitting(
                definition.Name,
                "Négyszög idom",
                definition.GeometryType,
                definition.FlowDirection,
                definition.DefaultZeta,
                parameters);
        }


        private static DuctFitting CreateFitting(
            string name,
            string category,
            GeometryType geometryType,
            DuctFlowDirection direction,
            double zeta,
            ElementCreationParameters parameters)
        {
            return new DuctFitting
            {
                Name = name,
                Category = category,

                ElementType = DuctElementType.Elbow,

                GeometryType = geometryType,
                Geometry = parameters.Geometry,

                Airflow = parameters.Airflow,

                Direction = direction,

                Zeta = zeta,

                PressureLossType = PressureLossType.Zeta,

                ShankLength1 = parameters.ShankLength1,
                ShankLength2 = parameters.ShankLength2
            };
        }


        public static DuctTransition CreateTransition(
            TransitionFittingDefinition definition,
            ElementCreationParameters parameters)
        {
            return new DuctTransition
            {
                Name = definition.Name,
                Category = "Átmenet",

                ElementType = DuctElementType.Transition,

                GeometryType = definition.GeometryType,
                Geometry = parameters.Geometry,

                Airflow = parameters.Airflow,

                Direction = definition.FlowDirection,

                Zeta = definition.DefaultZeta,

                PressureLossType = PressureLossType.Zeta
            };
        }


        public static BranchFitting CreateBranch(
            BranchFittingDefinition definition,
            ElementCreationParameters parameters)
        {
            return new BranchFitting
            {
                Name = definition.Name,
                Category = "Elágazás",

                ElementType = DuctElementType.Branch,

                GeometryType = definition.GeometryType,
                Geometry = parameters.Geometry,

                Airflow = parameters.Airflow,

                Direction = definition.FlowDirection,

                Zeta = definition.DefaultZeta,

                PressureLossType = PressureLossType.Zeta
            };
        }


        public static DuctLouver CreateAccessory(
    DuctAccessoryDefinition definition,
    ElementCreationParameters parameters)
        {
            return new DuctLouver
            {
                Name = definition.Name,
                Category = definition.Category,

                ElementType = definition.ElementType,

                GeometryType = definition.GeometryType,
                Geometry = parameters.Geometry,

                Airflow = parameters.Airflow,

                Direction = definition.FlowDirection,

                Zeta = definition.DefaultZeta,

                FixedPressureDrop =
                    definition.FixedPressureDrop ?? 0,

                FreeAreaPercent =
                    parameters.FreeAreaPercent > 0
                    ? parameters.FreeAreaPercent
                    : definition.DefaultFreeArea,

                PressureLossType =
                    definition.FixedPressureDrop.HasValue
                    ? PressureLossType.FixedPressure
                    : definition.PressureLossType
            };
        }
    }
    
}