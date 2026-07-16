using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HVACDesigner.Data.Models.Duct;
using DuctFlowDirection =
    HVACDesigner.Data.Models.Duct.FlowDirection;

namespace HVACDesigner.Data.Providers
{
    public class XmlDuctDataProvider : IDuctDataProvider
    {
        private readonly XDocument _doc;


        public XmlDuctDataProvider(string xmlPath)
        {
            if (!File.Exists(xmlPath))
            {
                throw new FileNotFoundException(
                    "Nem található a gépészeti XML adatbázis fájl.",
                    xmlPath);
            }

            _doc = XDocument.Load(xmlPath);
        }



        //========================================================
        // MÉRETEK
        //========================================================

        public IReadOnlyList<CircularDuctSize> GetCircularDuctSizes()
        {
            var section = _doc.Root?
                .Element("CircularSizes");

            if (section == null)
                return new List<CircularDuctSize>();

            return section.Elements("Size")
                .Select(x =>
                    new CircularDuctSize(
                        int.Parse(
                            (string?)x.Attribute("Diameter") ?? "0")))
                .ToList();
        }



        public IReadOnlyList<RectangularDuctSize> GetRectangularDuctSizes()
        {
            var section = _doc.Root?
                .Element("RectangularSizes");

            if (section == null)
                return new List<RectangularDuctSize>();

            return section.Elements("Size")
                .Select(x =>
                    new RectangularDuctSize
                    {
                        Width =
                            int.Parse(
                                (string?)x.Attribute("Width") ?? "0"),

                        Height =
                            int.Parse(
                                (string?)x.Attribute("Height") ?? "0")
                    })
                .ToList();
        }



        //========================================================
        // ANYAGOK
        //========================================================

        public IReadOnlyList<DuctMaterial> GetMaterials()
        {
            var section = _doc.Root?
                .Element("Materials");

            if (section == null)
                return new List<DuctMaterial>();

            return section.Elements("Material")
                .Select(x =>
                    new DuctMaterial
                    {
                        Name =
                            (string?)x.Attribute("Name") ?? "",

                        Roughness =
                            (double?)x.Attribute("Roughness")
                            ?? 0.15,

                        IsFlexible =
                            (bool?)x.Attribute("Flexible")
                            ?? false
                    })
                .ToList();
        }



        //========================================================
        // KÖR IDOMOK
        //========================================================

        public IReadOnlyList<CircularFittingDefinition> GetCircularFittings()
        {
            var section = _doc.Root?
                .Element("Fittings")?
                .Element("CircularFittings");


            if (section == null)
                return new List<CircularFittingDefinition>();


            return section.Descendants("Fitting")
                .Select(x =>
                    new CircularFittingDefinition
                    {
                        Id =
                            (string?)x.Attribute("Id")
                            ?? Guid.NewGuid().ToString(),

                        Name =
                            (string?)x.Attribute("Name")
                            ?? "",

                        ElementType =
                            ParseElementType(x),

                        Angle =
                            ParseAngle(x),

                        ElbowType =
                            ParseElbowType(x),

                        AllowShankLengths =
                            (bool?)x.Attribute("AllowLength")
                            ?? false,

                        DefaultZeta =
                            (double?)x.Attribute("DefaultZeta")
                            ?? 0,

                        FlowDirection =
                            ParseFlowDirection(x),

                        GeometryType =
                            ParseGeometryType(x)
                    })
                .ToList();
        }



        //========================================================
        // NÉGYSZÖG IDOMOK
        //========================================================

        public IReadOnlyList<RectangularFittingDefinition> GetRectangularFittings()
        {
            var section = _doc.Root?
                .Element("Fittings")?
                .Element("RectangularFittings");


            if (section == null)
                return new List<RectangularFittingDefinition>();


            return section.Descendants("Fitting")
                .Select(x =>
                    new RectangularFittingDefinition
                    {
                        Id =
                            (string?)x.Attribute("Id")
                            ?? Guid.NewGuid().ToString(),

                        Name =
                            (string?)x.Attribute("Name")
                            ?? "",

                        ElementType =
                            ParseElementType(x),

                        Angle =
                            ParseAngle(x),

                        ElbowType =
                            ParseElbowType(x),

                        AllowShankLengths =
                            (bool?)x.Attribute("AllowLength")
                            ?? false,

                        DefaultZeta =
                            (double?)x.Attribute("DefaultZeta")
                            ?? 0,

                        FlowDirection =
                            ParseFlowDirection(x),

                        GeometryType =
                            ParseGeometryType(x),

                        AllowOutletSizeChange =
                            (bool?)x.Attribute("AllowOutletSizeChange")
                            ?? false
                    })
                .ToList();
        }
        //========================================================
        // ÁTMENETI IDOMOK
        //========================================================

        public IReadOnlyList<TransitionFittingDefinition> GetTransitionFittings()
        {
            var section = _doc.Root?
                .Element("Fittings")?
                .Element("Transitions");


            if (section == null)
                return new List<TransitionFittingDefinition>();


            return section.Descendants("Fitting")
                .Select(x =>
                    new TransitionFittingDefinition
                    {
                        Id =
                            (string?)x.Attribute("Id")
                            ?? Guid.NewGuid().ToString(),

                        Name =
                            (string?)x.Attribute("Name")
                            ?? "",

                        DefaultZeta =
                            (double?)x.Attribute("DefaultZeta")
                            ?? 0,

                        FlowDirection =
                            ParseFlowDirection(x),

                        GeometryType =
                            ParseGeometryType(x),

                        AllowLength =
                            (bool?)x.Attribute("AllowLength")
                            ?? true,

                        AllowSizeChange =
                            (bool?)x.Attribute("AllowSizeChange")
                            ?? true
                    })
                .ToList();
        }



        //========================================================
        // ELÁGAZÓ IDOMOK
        //========================================================

        public IReadOnlyList<BranchFittingDefinition> GetBranchFittings()
        {
            var section = _doc.Root?
                .Element("Fittings")?
                .Element("Branches");


            if (section == null)
                return new List<BranchFittingDefinition>();


            return section.Descendants("Fitting")
                .Select(x =>
                    new BranchFittingDefinition
                    {
                        Id =
                            (string?)x.Attribute("Id")
                            ?? Guid.NewGuid().ToString(),

                        Name =
                            (string?)x.Attribute("Name")
                            ?? "",

                        BranchType =
                            ParseBranchType(x),

                        DefaultZeta =
                            (double?)x.Attribute("DefaultZeta")
                            ?? 0,

                        FlowDirection =
                            ParseFlowDirection(x),

                        GeometryType =
                            ParseGeometryType(x),

                        AllowBranchAirflow =
                            (bool?)x.Attribute("AllowBranchAirflow")
                            ?? true
                    })
                .ToList();
        }



        //========================================================
        // EGYENES LÉGCSATORNÁK
        //========================================================

        public IReadOnlyList<DuctDefinition> GetStraightDucts()
        {
            var section = _doc.Root?
                .Element("StraightDucts");


            if (section == null)
                return new List<DuctDefinition>();


            return section.Descendants("Duct")
                .Select(x =>
                    new DuctDefinition
                    {
                        Id =
                            (string?)x.Attribute("Id")
                            ?? Guid.NewGuid().ToString(),

                        Name =
                            (string?)x.Attribute("Name")
                            ?? "",

                        Category =
                            (string?)x.Attribute("Category")
                            ?? "Csőszakasz",

                        Material =
                            (string?)x.Attribute("Material")
                            ?? "",

                        PressureModel =
                            (string?)x.Attribute("PressureModel")
                            ?? "Friction",

                        GeometryType =
                            ParseGeometryType(x),

                        FlowDirection =
                            ParseFlowDirection(x),

                        IsFlexible = false
                    })
                .ToList();
        }



        //========================================================
        // FLEXIBILIS LÉGCSATORNÁK
        //========================================================

        public IReadOnlyList<DuctDefinition> GetFlexibleDucts()
        {
            var section = _doc.Root?
                .Element("FlexibleDucts");


            if (section == null)
                return new List<DuctDefinition>();


            return section.Descendants("Duct")
                .Select(x =>
                    new DuctDefinition
                    {
                        Id =
                            (string?)x.Attribute("Id")
                            ?? Guid.NewGuid().ToString(),

                        Name =
                            (string?)x.Attribute("Name")
                            ?? "",

                        Category =
                            (string?)x.Attribute("Category")
                            ?? "Flexibilis cső",

                        Material =
                            (string?)x.Attribute("Material")
                            ?? "",

                        PressureModel =
                            (string?)x.Attribute("PressureModel")
                            ?? "Friction",

                        GeometryType =
                            ParseGeometryType(x),

                        FlowDirection =
                            ParseFlowDirection(x),

                        IsFlexible = true
                    })
                .ToList();
        }



        //========================================================
        // KIEGÉSZÍTŐ ELEMEK
        //========================================================

        public IReadOnlyList<DuctAccessoryDefinition> GetDuctAccessories()
        {
            var result =
                new List<DuctAccessoryDefinition>();


            var root = _doc.Root;

            if (root == null)
                return result;


            var sections = new[]
            {
                "Dampers",
                "Silencers",
                "Filters",
                "VAV",
                "CAV",
                "Louvers",
                "Grilles",
                "Diffusers",
                "RoofCaps",
                "Hoods"
            };


            foreach (var sectionName in sections)
            {
                var section =
                    root.Element(sectionName);


                if (section == null)
                    continue;


                foreach (var x in section.Elements())
                {
                    result.Add(
                        new DuctAccessoryDefinition
                        {
                            Id =
                                (string?)x.Attribute("Id")
                                ?? Guid.NewGuid().ToString(),

                            Name =
                                (string?)x.Attribute("Name")
                                ?? "",

                            Category =
                                (string?)x.Attribute("Category")
                                ?? sectionName,


                            ElementType =
                                ParseElementType(x),


                            GeometryType =
                                ParseGeometryType(x),


                            FlowDirection =
                                ParseFlowDirection(x),


                            DefaultZeta =
                                (double?)x.Attribute("DefaultZeta")
                                ?? 0,


                            FixedPressureDrop =
                                (double?)x.Attribute("FixedPressureDrop"),


                            DefaultFreeArea =
                                (double?)x.Attribute("FreeArea")
                                ?? 100,


                            AllowSizeChange =
                                (bool?)x.Attribute("AllowSizeChange")
                                ?? false,


                            AllowLength =
                                (bool?)x.Attribute("AllowLength")
                                ?? false,


                            AllowBranch =
                                (bool?)x.Attribute("AllowBranch")
                                ?? false
                        });
                }
            }


            return result;
        }



        //========================================================
        // EGYEDI ELEMEK
        //========================================================

        public IReadOnlyList<CustomElementDefinition> GetCustomElements()
        {
            var section = _doc.Root?
                .Element("CustomElements");


            if (section == null)
                return new List<CustomElementDefinition>();


            return section.Elements("Element")
                .Select(x =>
                    new CustomElementDefinition(
                        (string?)x.Attribute("Name")
                        ?? "",

                        (double?)x.Attribute("DefaultZeta")
                        ?? 0,

                        ParseFlowDirection(x),

                        ParseGeometryType(x),

                        (double?)x.Attribute("MinOperatingPressure")
                        ?? 0,

                        (double?)x.Attribute("InitialPressureDrop")
                        ?? 0,

                        (double?)x.Attribute("FinalPressureDrop")
                        ?? 0
                    ))
                .ToList();
        }
        //========================================================
        // SEGÉDFÜGGVÉNYEK
        //========================================================

        private static DuctFlowDirection ParseFlowDirection(XElement x)
        {
            return Enum.TryParse(
                (string?)x.Attribute("FlowDirection"),
                true,
                out DuctFlowDirection result)

                ? result

                : DuctFlowDirection.Both;
        }



        private static GeometryType ParseGeometryType(XElement x)
        {
            return Enum.TryParse(
                (string?)x.Attribute("GeometryType"),
                true,
                out GeometryType result)

                ? result

                : GeometryType.SingleSize;
        }



        private static DuctElementType ParseElementType(XElement x)
        {
            return Enum.TryParse(
                (string?)x.Attribute("ElementType"),
                true,
                out DuctElementType result)

                ? result

                : DuctElementType.Custom;
        }



        private static FittingAngle ParseAngle(XElement x)
        {
            return Enum.TryParse(
                (string?)x.Attribute("Angle"),
                true,
                out FittingAngle result)

                ? result

                : FittingAngle.Deg90;
        }



        private static ElbowType ParseElbowType(XElement x)
        {
            return Enum.TryParse(
                (string?)x.Attribute("ElbowType"),
                true,
                out ElbowType result)

                ? result

                : ElbowType.Radius;
        }



        private static BranchType ParseBranchType(XElement x)
        {
            return Enum.TryParse(
                (string?)x.Attribute("BranchType"),
                true,
                out BranchType result)

                ? result

                : BranchType.Tee;
        }
    }
}
