using Mp3Player.Core.DataClasses;
using Mp3Player.Core.Ui.Forms;

namespace Mp3Player.Core.Ui.Services
{
    /// <summary>
    /// The modal dialog service
    /// </summary>
    public class ModalDialogService
    {
        /// <summary>
        /// Shows the model dialog.
        /// </summary>
        /// <param name="modalDialogData">The modal dialog data.</param>
        /// <returns>The modal dialog result data</returns>
        public ModalDialogResultData ShowModelDialog(ModalDialogData modalDialogData)
        {
            var dialog = new ModalDialog();
            _ = dialog.ShowDialog();
            return new ModalDialogResultData();
        }
    }
}
