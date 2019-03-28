﻿using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;

using RoiFormulaMaker.Notifications;

namespace RoiFormulaMaker.ViewModels
{
    public class MakeRingRoiViewModel : BindableBase, IInteractionRequestAware
    {
        public string SelectedStructureName { get; set; }

        public string SelectedStructureType { get; set; } = "Control";

        public DelegateCommand MakeRingRoiCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public MakeRingRoiViewModel()
        {
            MakeRingRoiCommand = new DelegateCommand(AcceptMakingRingRoi);
            CancelCommand = new DelegateCommand(CancelInteraction);
        }

        private void CancelInteraction()
        {
            notification.StructureName = null;
            notification.StructureType = null;
            notification.OuterMargin = 0;
            notification.InnerMargin = 0;
            notification.Confirmed = false;
            FinishInteraction?.Invoke();
        }

        private void AcceptMakingRingRoi()
        {
            notification.BaseStructureName = SelectedStructureName;
            notification.StructureType = SelectedStructureType;
            if (string.IsNullOrEmpty(notification.StructureName))
            {
                if (notification.InnerMargin == 0)
                {
                    notification.StructureName = $"zRing_{notification.OuterMargin}";
                }
                else
                {
                    notification.StructureName = $"zRing_{notification.OuterMargin}_{notification.InnerMargin}";
                }
            }
            
            notification.Confirmed = true;
            FinishInteraction?.Invoke();
        }

        public Action FinishInteraction { get; set; }

        private MakeRingRoiNotification notification;

        public INotification Notification
        {
            get { return notification; }
            set { SetProperty(ref notification, (MakeRingRoiNotification)value); }
        }
    }
}   
