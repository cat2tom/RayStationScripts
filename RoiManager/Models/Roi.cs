﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoiManager.Models
{
    public class Roi : BindableBase
    { 
        public string CaseName { get; set; }
        public string ExaminationName { get; set; }
        public string Name { get; set; }
        public bool IsDerived { get; set; }
        public bool HasGeometry { get; set; }
        public List<string> DependentRois { get; set; }
        public bool CanUnderive { get; set; }
        public bool CanDeleteGeometry { get; set; }

        public override string ToString()
        {
            return $"ROI Name: {Name}, HasGeometry: {HasGeometry}, CanUnderive: {CanUnderive}, CanDeleteGeometry: {CanDeleteGeometry}"
                + $" IsDerved: {IsDerived}, DependentRois: [{string.Join(",",DependentRois)}], Case: {CaseName}, Examination: {ExaminationName}";
        }
    }
}
