﻿using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using RoiFormulaMaker.Notifications;

namespace RoiFormulaMaker.ViewModels
{
    class MakeMarginAddedRoiViewModel : BindableBase, IInteractionRequestAware
    {
        public string SelectedStructureName { get; set; }
        public string SelectedStructureType { get; set; } = "Control";

        public DelegateCommand MakeMarginAddedRoiCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public MakeMarginAddedRoiViewModel()
        {
            MakeMarginAddedRoiCommand = new DelegateCommand(AcceptMakingMarginAddedRoi);
            CancelCommand = new DelegateCommand(CancelInteraction);
        }

        private void CancelInteraction()
        {
            notification.StructureName = null;
            notification.StructureType = null;
            notification.Margin = 0;
            notification.BaseStructureName = null;
            notification.Confirmed = false;
            FinishInteraction?.Invoke();
        }

        private void AcceptMakingMarginAddedRoi()
        {
            notification.BaseStructureName = SelectedStructureName;
            notification.StructureType = SelectedStructureType;
            if (string.IsNullOrEmpty(notification.StructureName))
            {
                notification.StructureName = $"z{notification.BaseStructureName}_{notification.Margin}";
            }

            notification.Confirmed = true;
            FinishInteraction?.Invoke();
        }

        public Action FinishInteraction { get; set; }

        private MakeMarginAddedRoiNotification notification;

        public INotification Notification
        {
            get { return notification; }
            set { SetProperty(ref notification, (MakeMarginAddedRoiNotification)value); }
        }
    }
}
