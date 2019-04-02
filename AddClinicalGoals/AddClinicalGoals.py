#from connect import *

import clr
clr.AddReference("PresentationFramework")
clr.AddReference("PresentationCore")
import sys, os

from System.Windows import MessageBox
from System.Collections.Generic import List, Dictionary
from System.Collections.ObjectModel import ObservableCollection

RayStationScriptsPath = os.environ["USERPROFILE"] + r"\DeskTop\RayStationScripts" + "\\"

dllsPath = RayStationScriptsPath + "Dlls"
print(dllsPath)
sys.path.append(dllsPath)

scriptsPath = RayStationScriptsPath + "Scripts"
print(scriptsPath)
sys.path.append(scriptsPath)

clr.AddReference("ClinicalGoal")
from ClinicalGoal.Views import MainWindow
from ClinicalGoal.ViewModels import ClinicalGoalViewModel
from ClinicalGoal.Models import ClinicalGoal

#case = get_current("Case")
#examination = get_current("Examination")
#structureSet = GetStructureSet(case, examination)
#roiDetails = GetRoiDetails(structureSet)

clinicalGoalViewModel = ClinicalGoalViewModel()

dvhCheckerDirectoryPath = r"C:\Users\satoru\Desktop\DvhChecker"
clinicalGoalViewModel.DvhCheckerDirectoryPath = dvhCheckerDirectoryPath

#structureNames = List[str]()
#for roiName in roiDetails.keys:
#    structureNames.Add(roiName)

structureNames = List[str]({'PTV', 'CTV', 'Rectal outline', 'Bladder outline'})

clinicalGoalViewModel.StructureNames = ObservableCollection[str](structureNames)

mainWindow = MainWindow(clinicalGoalViewModel)

mainWindow.ShowDialog()

if not clinicalGoalViewModel.CanExecute:
    print 'Canceled'
    sys.exit()

dvhObjectives = clinicalGoalViewModel.DvhObjectives;
prscrivedDose = clinicalGoalViewModel.PrescribedDose;

with CompositeAction('Add Clinical Goals'):
    for dvhObjective in dvhObjectives:
        if len(dvhObjective.StructureNameTps) > 0 and dvhObjective.InUse:
            clinicalGoal = ClinicalGoal(dvhObjective, prescribedDose=prscrivedDose)
            print clinicalGoal
            AddClinicalGoal(plan, clinicalGoal)
  # CompositeAction ends   
pass