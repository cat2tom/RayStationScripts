﻿using System.Windows;

namespace ClinicalGoal.Views
{
    /// <summary>
    /// MultiplePlansWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MultiplePlansWindow : Window
    {
        public MultiplePlansWindow()
        {
            InitializeComponent();

            var mainWindowViewModel = new ViewModels.MainWindowViewModel();

            DataContext = mainWindowViewModel;
        }

        public MultiplePlansWindow(ViewModels.ClinicalGoalViewModel clinicalGoalViewModel)
        {
            InitializeComponent();

            var mainWindowViewModel = new ViewModels.MainWindowViewModel();
            mainWindowViewModel.ClinicalGoalViewModel = clinicalGoalViewModel;

            DataContext = mainWindowViewModel;
        }
    }
}
