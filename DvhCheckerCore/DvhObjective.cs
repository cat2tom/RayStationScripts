﻿using CsvHelper;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Juntendo.MedPhys
{
    public enum DvhObjectiveType { Max, Min, MeanUpper, MeanLower, Upper, Lower, Spare, Veff };
    public enum DvhTargetType { Dose, Volume };
    public enum DvhPresentationType { None, Abs, Rel };

    public enum DvhDoseUnit { None, Percent, Gy };

    public enum DvhVolumeUnit { None, Percent, cc };

    public enum DvhEvalResult { Na, Pass, Acceptable, Fail}

    public class DvhObjective : BindableBase
    {
        [JsonProperty]
        public string ProtocolId { get; private set; }

        [JsonProperty]
        public string OriginalProtocolId { get; private set; } = String.Empty;

        private string title;
        [JsonProperty]
        public string Title
        {
            get { return title; }
            set
            {
                this.SetProperty(ref this.title, value);
            }
        }

        private bool inUse;
        [JsonProperty]
        public bool InUse
        {
            get { return inUse; }
            set
            {
                this.SetProperty(ref this.inUse, value);
            }
        }

        private string structureName;
        [JsonProperty]
        public string StructureName
        {
            get { return structureName; }
            set
            {
                this.SetProperty(ref this.structureName, value);
            }
        }

        private string structureNameTps;
        [JsonProperty]
        public string StructureNameTps
        {
            get { return structureNameTps; }
            set
            {
                this.SetProperty(ref this.structureNameTps, value);
            }
        }

        private DvhObjectiveType objectiveType;
        [JsonProperty]
        public DvhObjectiveType ObjectiveType
        {
            get { return objectiveType; }
            set
            {
                this.SetProperty(ref this.objectiveType, value);
            }
        }

        private DvhTargetType targetType;
        [JsonProperty]
        public DvhTargetType TargetType
        {
            get { return targetType; }
            private set
            {
                this.SetProperty(ref this.targetType, value);
            }
        }

        private DvhPresentationType targetUnit;
        [JsonProperty]
        public DvhPresentationType TargetUnit
        {
            get { return targetUnit; }
            set
            {
                this.SetProperty(ref this.targetUnit, value);
                ActualTargetUnit = getTargetUnit(TargetType, TargetUnit);
            }
        }

        private double targetValue;
        [JsonProperty]
        public double TargetValue
        {
            get { return targetValue; }
            set
            {
                this.SetProperty(ref this.targetValue, value);
            }
        }

        private DvhPresentationType argumentUnit;
        [JsonProperty]
        public DvhPresentationType ArgumentUnit
        {
            get { return argumentUnit; }
            set
            {
                this.SetProperty(ref this.argumentUnit, value);
                ActualArgumentUnit = getArgumentUnit(TargetType, ArgumentUnit);
            }
        }

        private double argumentValue;
        [JsonProperty]
        public double ArgumentValue
        {
            get { return argumentValue; }
            set
            {
                this.SetProperty(ref this.argumentValue, value);
            }
        }

        private DvhDoseUnit doseUnit;
        [JsonProperty]
        public DvhDoseUnit DoseUnit
        {
            get { return doseUnit; }
            set
            {
                this.SetProperty(ref this.doseUnit, value);
            }
        }

        private double value;
        [JsonProperty]
        public double Value
        {
            get { return value; }
            set
            {
                this.SetProperty(ref this.value, value);
            }
        }

        private DvhVolumeUnit volumeUnit;
        [JsonProperty]
        public DvhVolumeUnit VolumeUnit
        {
            get { return volumeUnit; }
            set
            {
                this.SetProperty(ref this.volumeUnit, value);
            }
        }

        // If Acceptable Limit is not given, set to -1
        private double acceptableLimitValue;
        [JsonProperty]
        public double AcceptableLimitValue
        {
            get { return acceptableLimitValue; }
            set
            {
                this.SetProperty(ref this.acceptableLimitValue, value);
            }
        }

        private string remarks;
        [JsonProperty]
        public string Remarks
        {
            get { return remarks; }
            set
            {
                this.SetProperty(ref this.remarks, value);
            }
        }

        private bool isPassed;
        [JsonProperty]
        public bool IsPassed
        {
            get { return isPassed; }
            set
            {
                this.SetProperty(ref this.isPassed, value);
            }
        }

        private bool isAcceptable;
        [JsonProperty]
        public bool IsAcceptable
        {
            get { return isAcceptable; }
            set
            {
                this.SetProperty(ref this.isAcceptable, value);
            }
        }

        private double volume;
        [JsonProperty]
        public double Volume
        {
            get { return volume; }
            set
            {
                this.SetProperty(ref this.volume, value);
            }
        }

        [JsonIgnore]
        private string actualTargetUnit;
        public string ActualTargetUnit
        {
            get { return actualTargetUnit; }
            private set { SetProperty(ref actualTargetUnit, value); }
        }
        //public string ActualTargetUnit { get { return getTargetUnit(TargetType, TargetUnit); } }
        [JsonIgnore]
        private string actualArgumentUnit;
        public string ActualArgumentUnit
        {
            get { return actualArgumentUnit; }
            private set { SetProperty(ref actualArgumentUnit, value); }
        }
        //public string ActualArgumentUnit { get { return getArgumentUnit(TargetType, ArgumentUnit); } }

        private DvhEvalResult evalResult = DvhEvalResult.Na;
        [JsonProperty]
        public DvhEvalResult EvalResult
        {
            get { return evalResult; }
            set
            {
                this.SetProperty(ref this.evalResult, value);
            }
        }

        [JsonConstructor]
        public DvhObjective(){}

        public DvhObjective(ObjectiveCsv objectiveCsv)
        {
            ProtocolId = objectiveCsv.ProtocolId;
            OriginalProtocolId = objectiveCsv.OriginalProtocolId;
            Title = objectiveCsv.Title;
            StructureName = objectiveCsv.StructureName;
            ObjectiveType = (DvhObjectiveType)Enum.Parse(typeof(DvhObjectiveType), objectiveCsv.ObjectiveType);
            TargetType = (DvhTargetType)Enum.Parse(typeof(DvhTargetType), objectiveCsv.TargetType);
            ArgumentValue = string.IsNullOrEmpty(objectiveCsv.ArgumentValue) ? 0.0 : double.Parse(objectiveCsv.ArgumentValue);
            TargetValue = double.Parse(objectiveCsv.TargetValue);

            // If Acceptable Limit is not given, set to -1
            AcceptableLimitValue = string.IsNullOrEmpty(objectiveCsv.AcceptableLimitValue) ? -1 : double.Parse(objectiveCsv.AcceptableLimitValue);
            Remarks = objectiveCsv.Remarks;

            StructureNameTps = objectiveCsv.StructureNameTps;

            string argumentUnit = objectiveCsv.ArgumentUnit;
            if (argumentUnit == "%")
            {
                argumentUnit = "Percent";
            }
            string targetUnit = objectiveCsv.TargetUnit; ;
            if (targetUnit == "%")
            {
                targetUnit = "Percent";
            }

            switch (TargetType)
            {
                case DvhTargetType.Dose:
                    DoseUnit = (DvhDoseUnit)Enum.Parse(typeof(DvhDoseUnit), targetUnit);
                    VolumeUnit = string.IsNullOrEmpty(argumentUnit) ? DvhVolumeUnit.None : (DvhVolumeUnit)Enum.Parse(typeof(DvhVolumeUnit), argumentUnit);
                    if (DoseUnit == DvhDoseUnit.Percent)
                    {
                        TargetUnit = DvhPresentationType.Rel;
                    }
                    else
                    {
                        TargetUnit = DvhPresentationType.Abs;
                    }

                    if (VolumeUnit == DvhVolumeUnit.Percent)
                    {
                        ArgumentUnit = DvhPresentationType.Rel;
                    }
                    else if (VolumeUnit == DvhVolumeUnit.None)
                    {
                        ArgumentUnit = DvhPresentationType.None;
                    }
                    else
                    {
                        ArgumentUnit = DvhPresentationType.Abs;
                    }
                    break;

                case DvhTargetType.Volume:
                    DoseUnit = string.IsNullOrEmpty(argumentUnit) ? DvhDoseUnit.None : (DvhDoseUnit)Enum.Parse(typeof(DvhDoseUnit), argumentUnit);
                    VolumeUnit = (DvhVolumeUnit)Enum.Parse(typeof(DvhVolumeUnit), targetUnit);
                    if (VolumeUnit == DvhVolumeUnit.Percent)
                    {
                        TargetUnit = DvhPresentationType.Rel;
                    }
                    else
                    {
                        TargetUnit = DvhPresentationType.Abs;
                    }

                    if (DoseUnit == DvhDoseUnit.Percent)
                    {
                        ArgumentUnit = DvhPresentationType.Rel;
                    }
                    else if (DoseUnit == DvhDoseUnit.None)
                    {
                        ArgumentUnit = DvhPresentationType.None;
                    }
                    else
                    {
                        ArgumentUnit = DvhPresentationType.Abs;
                    }
                    break;
            }

        }

        public static List<DvhObjective> ReadObjectivesFromCsv(string filePath)
        {
            List<DvhObjective> objectives = new List<DvhObjective>();
            using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("shift_jis")))
            {
                var csv = new CsvReader(sr);
                csv.Read();
                csv.ReadHeader();
                csv.Read();
                csv.Configuration.MissingFieldFound = null;

                var protocolId = csv["Protocol ID"];
                var originalProtocolId = csv["Original Protocol ID"];

                if (string.IsNullOrEmpty(protocolId))
                {
                    throw new InvalidOperationException("Protocol ID is empty, file: " + filePath);
                }
                if (string.IsNullOrEmpty(originalProtocolId))
                {
                    originalProtocolId = protocolId;
                }

                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    string title = csv["Title"];
                    string structureName = csv["Structure Name"];
                    string objectiveType = csv["Objective Type"];
                    string targetType = csv["Target Type"];
                    string targetValue = csv["Target Value"];
                    string targetUnit = csv["Target Unit"];
                    string acceptableLimitValue = csv["Acceptable Limit Value"];
                    string argumentValue = csv["Argument Value"];
                    string argumentUnit = csv["Argument Unit"];
                    string remarks = csv["Remarks"];
                    string structureNameTps = csv["Structure Name TPS"];

                    var objectiveCsv = new ObjectiveCsv()
                    {
                        ProtocolId = protocolId,
                        OriginalProtocolId = originalProtocolId,
                        Title = title,
                        StructureName = structureName,
                        ObjectiveType = objectiveType,
                        TargetType = targetType,
                        TargetValue = targetValue,
                        TargetUnit = targetUnit,
                        AcceptableLimitValue = acceptableLimitValue,
                        ArgumentValue = argumentValue,
                        ArgumentUnit = argumentUnit,
                        Remarks = remarks,
                        StructureNameTps = structureNameTps
                    };

                    var objective = new DvhObjective(objectiveCsv);
                    objectives.Add(objective);
                }
            }
            return objectives;
        }

        public static void WriteObjectivesToFile(List<DvhObjective> objectives, string protocolName, string filePath, PlanInfo planInfo)
        {

            if(objectives.Count == 0)
            {
                throw new ArgumentException("No item in objectives");
            }
            var originalProtocolId = objectives[0].OriginalProtocolId;

            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.GetEncoding("shift_jis")))
            {
                sw.WriteLine("\"Protocol ID\",\"Plan ID\",\"Course ID\",\"Patient Name\",\"Patient ID\",\"Prescribed Dose [Gy]\",\"Max Does [Gy]\",\"Original Protocol ID\"");
                sw.WriteLine("\"" + protocolName + "\",\"" + planInfo.PlanId + "\",\"" + planInfo.CourseId + "\",\"" + planInfo.PatientName + "\",\"" + planInfo.PatientId + "\",\"" + planInfo.PrescribedDose + "\",\"" + planInfo.MaxDose + "\",\"" + originalProtocolId + "\"");
                sw.WriteLine("\"Title\",\"Structure Name\",\"Structure Name TPS\",\"Objective Type\",\"Target Type\",\"Target Value\",\"Target Unit\",\"Acceptable Limit Value\",\"Argument Value\",\"Argument Unit\",\"Remarks\",\"Value\",\"Volume\",\"IsPassed\",\"IsAcceptable\"");
                foreach (var o in objectives)
                {
                    var line = "\"" + o.Title + "\",";
                    line += "\"" + o.StructureName + "\",";
                    line += "\"" + o.StructureNameTps + "\",";
                    line += "\"" + o.ObjectiveType + "\",";
                    line += "\"" + o.TargetType + "\",";
                    line += "\"" + o.TargetValue + "\",";
                    line += "\"" + getTargetUnit(o.TargetType, o.TargetUnit) + "\",";
                    line += "\"" + o.AcceptableLimitValue + "\",";
                    line += "\"" + o.ArgumentValue + "\",";
                    line += "\"" + getArgumentUnit(o.TargetType, o.ArgumentUnit) + "\",";
                    line += "\"" + o.Remarks+ "\",";
                    line += "\"" + o.Value + "\",";
                    line += "\"" + o.Volume + "\",";
                    line += "\"" + o.IsPassed + "\",";
                    line += "\"" + o.IsAcceptable+"\"";

                    sw.WriteLine(line);
                }
            }
        }

        private static string getTargetUnit(DvhTargetType targetType, DvhPresentationType presentationType)
        {
            switch (targetType)
            {
                case DvhTargetType.Dose:
                    if(presentationType == DvhPresentationType.Abs)
                    {
                        return "Gy";
                    }
                    else if (presentationType == DvhPresentationType.Rel)
                    {
                        return "%";
                    }
                    else
                    {
                        return string.Empty;
                    }
                case DvhTargetType.Volume:
                    if (presentationType == DvhPresentationType.Abs)
                    {
                        return "cc";
                    }
                    else if (presentationType == DvhPresentationType.Rel)
                    {
                        return "%";
                    }
                    else
                    {
                        return string.Empty;
                    }
                default:
                    return string.Empty;
            }
        }

        private static string getArgumentUnit(DvhTargetType targetType, DvhPresentationType presentationType)
        {
            switch (targetType)
            {
                case DvhTargetType.Volume:
                    if (presentationType == DvhPresentationType.Abs)
                    {
                        return "Gy";
                    }
                    else if (presentationType == DvhPresentationType.Rel)
                    {
                        return "%";
                    }
                    else
                    {
                        return string.Empty;
                    }
                case DvhTargetType.Dose:
                    if (presentationType == DvhPresentationType.Abs)
                    {
                        return "cc";
                    }
                    else if (presentationType == DvhPresentationType.Rel)
                    {
                        return "%";
                    }
                    else
                    {
                        return string.Empty;
                    }
                default:
                    return string.Empty;
            }
        }

    }

    public struct ObjectiveCsv
    {
        public string ProtocolId;
        public string OriginalProtocolId;
        public string Title;
        public string StructureName;
        public string ObjectiveType;
        public string TargetType;
        public string ArgumentValue;
        public string ArgumentUnit;
        public string TargetValue;
        public string TargetUnit;
        public string AcceptableLimitValue;
        public string Remarks;
        public string StructureNameTps;
    }

    public struct PlanInfo
    {
        public string PatientId;
        public string PatientName;
        public string CourseId;
        public string PlanId;
        public double PrescribedDose;
        public double MaxDose;
    }
}
