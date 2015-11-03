namespace SubSearch.WPF
{
    using System.Windows.Forms;

    /// <summary>
    /// The utilities.
    /// </summary>
    internal static class Utils
    {
        public static Screen GetActiveScreen()
        {
            var mousePosition = Control.MousePosition;
            return Screen.FromPoint(mousePosition);
        }
    }
}
