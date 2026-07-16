namespace HVACDesigner.Data.Models.Duct
{
    public enum FlowDirection
    {
        Supply,
        Extract,
        Both,
        OutsideAir,
        ExhaustAir
    }

    public enum GeometryShape
    {
        Circular,
        Rectangular
    }

    public enum GeometryType
    {
        SingleSize,
        SizeChange,
        LengthOnly,
        Offset,
        Branch
    }

    public enum PressureLossType
    {
        Friction,
        Zeta,
        FixedPressure,
        FreeArea
    }

    public enum DuctElementType
    {
        StraightDuct,
        FlexibleDuct,
        Elbow,
        Transition,
        Offset,
        Branch,
        Damper,
        FireDamper,
        Silencer,
        Filter,
        VAV,
        CAV,
        Grille,
        Diffuser,
        Louver,
        RoofCap,
        Hood,
        Custom
    }

    public enum ConnectionType
    {
        Inlet,
        Outlet,
        Branch1,
        Branch2
    }

    public enum MaterialType
    {
        GalvanizedSteel,
        StainlessSteel,
        Aluminum,
        Plastic,
        Flexible,
        Textile,
        Custom
    }

    public enum TransitionType
    {
        Reduction,
        Expansion
    }

    public enum ElbowType
    {
        Radius,
        Segmented,
        Sharp
    }

    public enum BranchType
    {
        Tee,
        Wye,
        Pants,
        Cross
    }

    public enum TerminalType
    {
        Grille,
        Diffuser,
        SlotDiffuser,
        SwirlDiffuser,
        Nozzle,
        Hood,
        RoofCap
    }

    public enum AccessoryType
    {
        Damper,
        FireDamper,
        Silencer,
        Filter,
        VAV,
        CAV,
        MeasuringDevice,
        AccessDoor,
        FlexibleConnector,
        Custom
    }

    public enum DamperType
    {
        Volume,
        Fire,
        Smoke,
        FireSmoke,
        Backdraft
    }

    public enum FittingAngle
    {
        Deg15,
        Deg30,
        Deg45,
        Deg60,
        Deg90
    }

    public enum CalculationMethod
    {
        DarcyWeisbach,
        Zeta,
        FixedPressure
    }
}