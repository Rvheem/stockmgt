using System;
using System.Windows.Forms;

namespace StockManagementApp.Helpers
{
    public static class MessageHelper
    {
        public static void ShowError(string message, Exception ex)
        {
            MessageBox.Show($"{message}: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        public static DialogResult ShowQuestion(string message)
        {
            return MessageBox.Show(message, "Question", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}
