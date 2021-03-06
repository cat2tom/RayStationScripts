from connect import *

import clr
clr.AddReference("PresentationFramework")
clr.AddReference("PresentationCore")

from System.Collections.Generic import List, Dictionary
from System.Windows import MessageBox

import sys, os, codecs
import json
from collections import OrderedDict

from Helpers import ClearObjectiveFunctions
from Helpers import GetObjectiveFunctionDescription
from Helpers import GetPlanLabelForConstituentFunction
from Helpers import IsCombinedConstituentFunction
from Helpers import SizeOfIterator

dllsPath = os.environ["USERPROFILE"]
dllsPath = dllsPath + r"\Desktop\RayStationScripts\Dlls"
print(dllsPath)
sys.path.append(dllsPath)

RayStationScriptsPath = os.environ["USERPROFILE"] + r"\Desktop\RayStationScripts" + "\\"
scriptsPath = RayStationScriptsPath + "Scripts"
print(scriptsPath)
sys.path.append(scriptsPath)

case = get_current("Case")
# Get handles to "Original plan" and "Copy plan".
original_plan = case.TreatmentPlans["test"]
original_beamSet = original_plan.BeamSets[0]
new_plan = case.TreatmentPlans["test3"]
new_beamSets = new_plan.BeamSets

ClearObjectiveFunctions(new_plan)

# Define a function that will retrieve the necessary information
# and put it in a dictionary.
def get_arguments_from_function(function):
    dfp = function.DoseFunctionParameters
    arg_dict = {}
    arg_dict['RoiName'] = function.ForRegionOfInterest.Name
    arg_dict['IsRobust'] = function.UseRobustness
    arg_dict['Weight'] = dfp.Weight
    if hasattr(dfp, "FunctionType"):
        arg_dict['FunctionType'] = dfp.FunctionType
        arg_dict['DoseLevel'] = dfp.DoseLevel
        if 'Eud' in dfp.FunctionType:
            arg_dict['EudParameterA'] = dfp.EudParameterA
        elif 'Dvh' in dfp.FunctionType:
            arg_dict['PercentVolume'] = dfp.PercentVolume
    elif hasattr(dfp, 'HighDoseLevel'):
        # Dose fall-off function does not have function type attribute.
        arg_dict['FunctionType'] = 'DoseFallOff'
        arg_dict['HighDoseLevel'] = dfp.HighDoseLevel
        arg_dict['LowDoseLevel'] = dfp.LowDoseLevel
        arg_dict['LowDoseDistance'] = dfp.LowDoseDistance
    elif hasattr(dfp, 'PercentStdDeviation'):
        # Uniformity constraint does not have function type.
        arg_dict['FunctionType'] = 'UniformityConstraint'
        arg_dict['PercentStdDeviation'] = dfp.PercentStdDeviation
    else:
        # Unknown function type, raise exception.
        raise ('Unknown function type')
    return arg_dict

# Define a function that will use information in arg_dict to set
# optimization function parameters.
def set_function_arguments(function, arg_dict):
    dfp = function.DoseFunctionParameters
    dfp.Weight = arg_dict['Weight']
    if arg_dict['FunctionType'] == 'DoseFallOff':
        dfp.HighDoseLevel = arg_dict['HighDoseLevel']
        dfp.LowDoseLevel = arg_dict['LowDoseLevel']
        dfp.LowDoseDistance = arg_dict['LowDoseDistance']
    elif arg_dict['FunctionType'] == 'UniformityConstraint':
        dfp.PercentStdDeviation = arg_dict['PercentStdDeviation']
    else:
        dfp.DoseLevel = arg_dict['DoseLevel']
        if 'Eud' in dfp.FunctionType:
            dfp.EudParameterA = arg_dict['EudParameterA']
        elif 'Dvh' in dfp.FunctionType:
            dfp.PercentVolume = arg_dict['PercentVolume']

with CompositeAction('Copy Objective Functions'):

    # Loop over all objectives and constraints in the original plan
    # to retrieve information. It is assumed that the original plan
    # only contains one beam set.
    po_original = original_plan.PlanOptimizations[0]
    arguments = [] # List to hold arg_dicts of all functions.
    # Get arguments from objective functions.
    if po_original.Objective != None:
        for cf in po_original.Objective.ConstituentFunctions:
            arg_dict = get_arguments_from_function(cf)
            arg_dict['IsConstraint'] = False
            # cast to python bool.
            # If no cast, bool is written as True or False in a JSON file.
            # This causes errors when reading the JSON file.
            arg_dict['IsRobust'] = bool(arg_dict['IsRobust'])
            arg_dict['PlanLabel'] =  GetPlanLabelForConstituentFunction(original_beamSet, po_original, cf)
            arg_dict['IsCombined'] = IsCombinedConstituentFunction(original_beamSet, po_original, cf)
            arguments.append(arg_dict)
    # Get arguments from constraint functions.
    for cf in po_original.Constraints:
        arg_dict = get_arguments_from_function(cf)
        arg_dict['IsConstraint'] = True
        # cast to python bool.
        # If no cast, bool is written as True or False in a JSON file.
        # This causes errors when reading the JSON file.
        arg_dict['IsRobust'] = bool(arg_dict['IsRobust']) 
        arg_dict['PlanLabel'] =  GetPlanLabelForConstituentFunction(original_beamSet, po_original, cf)
        arg_dict['IsCombined'] = IsCombinedConstituentFunction(original_beamSet, po_original, cf)
        arguments.append(arg_dict)

    prescribedDose = 7000.0
    objectiveFunctions = OrderedDict()
    objectiveFunctions['prescribedDose'] = prescribedDose
    objectiveFunctions['arguments'] = arguments

    # Write to a file as json
    objectiveFunctionsFilePath = RayStationScriptsPath + "ObjectiveFunctions"
    filePath = os.path.join(objectiveFunctionsFilePath,'tmp.json') 
    with codecs.open(filePath, 'w', 'utf-8') as fp:
        json.dump(objectiveFunctions,fp,indent=4)

    # Read from a json file
    with codecs.open(filePath, 'r', 'utf-8') as fp:
        objectiveFunctionsJson = fp.read()

    # Read from a json file
    filePath2 = os.path.join(objectiveFunctionsFilePath,'tmp2.json') 
    with codecs.open(filePath2, 'w', 'utf-8') as fp:
        fp.write(objectiveFunctionsJson)

    loadedObjectiveFunctions = json.loads(objectiveFunctionsJson)

    prescribedDose = loadedObjectiveFunctions['prescribedDose']
    arguments = loadedObjectiveFunctions['arguments']

    for arg_dict in arguments:
        description = GetObjectiveFunctionDescription(arg_dict)
        print arg_dict['IsCombined'], arg_dict['PlanLabel'], arg_dict['FunctionType'], description

    # Add optimization functions to the new plan.
    po_new = new_plan.PlanOptimizations[0]

    if SizeOfIterator(po_new.OptimizedBeamSets) == 2:
        hasMultipleBeamSets = True
    else:
        hasMultipleBeamSets = False 

    for arg_dict in arguments:
        #with CompositeAction('Add Optimization Function'):
        if arg_dict['FunctionType'] == 'UniformEud':
            arg_dict['FunctionType'] = 'TargetEud'
        if not hasMultipleBeamSets:
            f = po_new.AddOptimizationFunction(FunctionType=arg_dict['FunctionType'],
                RoiName=arg_dict['RoiName'], IsConstraint=arg_dict['IsConstraint'],
                IsRobust=arg_dict['IsRobust'])
        else:
            if arg_dict['IsCombined']:
                restrictToBeamSet = None
            else:
                restrictToBeamSet = arg_dict['PlanLabel']
            f = po_new.AddOptimizationFunction(FunctionType=arg_dict['FunctionType'],
                RoiName=arg_dict['RoiName'], IsConstraint=arg_dict['IsConstraint'],
                IsRobust=arg_dict['IsRobust'], RestrictToBeamSet=restrictToBeamSet)

        set_function_arguments(f, arg_dict)
