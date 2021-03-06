from connect import *

import clr
import sys, math, wpf, os
from System.Collections.Generic import List;
from System.Windows import MessageBox

clr.AddReference("PresentationFramework")
clr.AddReference("PresentationCore")

#sys.path.append(r"C:\Users\satoru\Source\Repos\RayStationScripts\OptimizatoinSettings\bin\Debug")

RayStationScriptsPath = os.environ["USERPROFILE"] + r"\DeskTop\RayStationScripts" + "\\"
dllsPath = RayStationScriptsPath + "Dlls"
print(dllsPath)
sys.path.append(dllsPath)

scriptsPath = RayStationScriptsPath + "Scripts"
print(scriptsPath)
sys.path.append(scriptsPath)

clr.AddReference("OptimizationRepeater")

from OptimizationRepeater.Models import RepetitionParameters
from OptimizationRepeater.Models import OptimizationFunction
from OptimizationRepeater.Views import MainWindow

from Helpers import GetPlanOptimizationForBeamSet
from Helpers import SetOptimizationFunction
from Helpers import UpdateObjectiveConstituentFunctionWeights
from Helpers import BoostObjectiveConstituentFunctionWeights

numberOfRepetitionTimes = 2
scaleDoseAfterEachIteration = True
scaleDoseAfterLastIteration = True
resetBeforeStartingOptimization = False

repetitionParameters = RepetitionParameters()

repetitionParameters.NumberOfRepetitionTimes = numberOfRepetitionTimes
repetitionParameters.ScaleDoseAfterEachIteration = scaleDoseAfterEachIteration
repetitionParameters.ScaleDoseAfterLastIteration = scaleDoseAfterLastIteration
repetitionParameters.ResetBeforeStartingOptimization = resetBeforeStartingOptimization

plan = get_current("Plan")
beamSet = get_current("BeamSet")
optimizations = plan.PlanOptimizations
planOptimization = GetPlanOptimizationForBeamSet(optimizations, beamSet)
objectiveConstituentFunctions = planOptimization.Objective.ConstituentFunctions

optimizationFunctions = List[OptimizationFunction]();

for i, f in enumerate(objectiveConstituentFunctions):
    optimizationFunction = OptimizationFunction()
    SetOptimizationFunction(beamSet, planOptimization, f, i, optimizationFunction)
    optimizationFunctions.Add(optimizationFunction)

for optimizationFunction in optimizationFunctions:
    print optimizationFunction.Order, optimizationFunction, optimizationFunction.IsBoosted 

mainWindow = MainWindow(repetitionParameters, optimizationFunctions)
mainWindow.ShowDialog()

for optimizationFunction in optimizationFunctions:
    print optimizationFunction.Order, optimizationFunction.RoiName, optimizationFunction, optimizationFunction.Weight, optimizationFunction.IsBoosted, optimizationFunction.BoostedWeight

#print 'Exit'
#sys.exit()

numberOfRepetitionTimes = repetitionParameters.NumberOfRepetitionTimes
scaleDoseAfterEachIteration = repetitionParameters.ScaleDoseAfterEachIteration
scaleDoseAfterLastIteration = repetitionParameters.ScaleDoseAfterLastIteration
resetBeforeStartingOptimization = repetitionParameters.ResetBeforeStartingOptimization
canExecute = repetitionParameters.CanExecute 

print numberOfRepetitionTimes, scaleDoseAfterEachIteration, scaleDoseAfterLastIteration, resetBeforeStartingOptimization, canExecute

from ScaleDose import ScaleDoseBeamSets

#ScaleDoseBeamSets(plan, beamSet)
#canExecute = False

if (canExecute):
    if(resetBeforeStartingOptimization):
        MessageBox.Show("Reset Optimization")
        #Execute Reset Optimization
        planOptimization.ResetOptimization()
        
    UpdateObjectiveConstituentFunctionWeights(objectiveConstituentFunctions, optimizationFunctions)
                
    for i in xrange(numberOfRepetitionTimes):

        print "Start optimization {0}".format(i+1)

        # Weight boosting at the last iteration
        if i + 1 == numberOfRepetitionTimes:
            print "Boost weights at last iteration"
            BoostObjectiveConstituentFunctionWeights(objectiveConstituentFunctions, optimizationFunctions)
        

        # Execute Optimization
        planOptimization.RunOptimization()

        if(i+1 == numberOfRepetitionTimes and scaleDoseAfterLastIteration):
            print "Scale dose after last iteration {0}".format(i+1)
            #Execute Scale dose
            ScaleDoseBeamSets(plan, beamSet)

        if(i+1 < numberOfRepetitionTimes and scaleDoseAfterEachIteration):
            print "Scale dose after iteration {0}".format(i+1)
            #Execute Scale dose
            ScaleDoseBeamSets(plan, beamSet)
pass
