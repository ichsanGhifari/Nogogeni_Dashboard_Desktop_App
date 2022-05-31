using System;
using System.Windows;

namespace DashboardEV
{
    public static class Extensions
    {
        public static void Error(this Window window, Exception ex)
        {

            MessageBox.Show(window, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
