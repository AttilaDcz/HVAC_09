using System;
using System.Collections.Generic;
using System.Linq;

namespace HVACDesigner.Data.Models.Duct
{
    public class DuctNetwork
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public FlowDirection Direction { get; set; } = FlowDirection.Supply;

        public List<DuctElement> Elements { get; set; } = new();

        public DuctMaterial? GlobalMaterial { get; set; }

        public double AirDensity { get; set; } = 1.2;


        public void AddElement(DuctElement element)
        {
            if (element == null)
                return;

            element.Index = Elements.Count + 1;

            Elements.Add(element);
        }


        public void RemoveElement(string id)
        {
            var element = Elements.FirstOrDefault(x => x.Id == id);

            if (element != null)
                Elements.Remove(element);

            RefreshIndexes();
        }


        public void MoveElement(int oldIndex, int newIndex)
        {
            if (oldIndex < 1 || oldIndex > Elements.Count)
                return;

            if (newIndex < 1 || newIndex > Elements.Count)
                return;

            var item = Elements[oldIndex - 1];

            Elements.RemoveAt(oldIndex - 1);

            Elements.Insert(newIndex - 1, item);

            RefreshIndexes();
        }


        public void RefreshIndexes()
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Index = i + 1;
            }
        }


        public DuctElement? GetElement(int index)
        {
            return Elements.FirstOrDefault(x => x.Index == index);
        }


        public double GetTotalPressureDrop()
        {
            return Elements.Sum(x => x.PressureDrop);
        }


        public double GetTotalLength()
        {
            return Elements
                .OfType<DuctSegment>()
                .Sum(x => x.Length);
        }
        public List<DuctBranch> Branches { get; set; } = new();


        public void AddBranch(DuctBranch branch)
        {
            if (branch == null)
                return;

            Branches.Add(branch);
        }
        public DuctBranch AddBranch(string name, FlowDirection direction)
        {
            var branch = new DuctBranch
            {
                Name = name,
                BranchDirection = direction
            };
            Branches.Add(branch);
            return branch;
        }

        public double TotalPressureLossPa
        {
            get
            {
                if (Branches == null || Branches.Count == 0) return GetTotalPressureDrop();
                return Branches.Max(b => b.PressureLossPa);
            }
        }

        public void Clear()
        {
            Elements.Clear();
        }
    }
}
