/** @addtogroup Tracking
 * @{
 */

/**
 *	@file SetupDialogForm.cs
 *
 *	@brief Dialog zum Einstellen der Eigenschaften
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.Utilities;
using ASCOM.DeltaCodeV3;

namespace ASCOM.DeltaCodeV3
{
    [ComVisible(false)]					// Form not registered for COM!

    /**	--------------------------------------------------------------------------------
     *	@brief Konstruktor
     *	
     *  Es werden alle verfügbaren Com-Ports angezeigt und der zuletzt gewählte ist
     *  in der Liste selektiert
     *	--------------------------------------------------------------------------------
     */
    public partial class SetupDialogForm : Form
    {
        public SetupDialogForm()
        {
            InitializeComponent();

            //  Alle verfügbaren Com-Ports anzeigen
            //
            boxSerialComPort.Items.Clear();
            using (ASCOM.Utilities.Serial serial = new Utilities.Serial())
            {
                foreach (var item in serial.AvailableCOMPorts)
                {
                    boxSerialComPort.Items.Add(item);
                }
            }
            //  Show previously selected Comm Port
            //  (taken from Properties.Settings.Default.CommPort)
            //
            if (!String.IsNullOrEmpty(Server.comPort))
            {
                boxSerialComPort.SelectedItem = Server.comPort;
            }

            //  Show previously selected Comm Port Speed
            //  (taken from Properties.Settings.Default.CommPortSpeed)
            //  Show default speed if not explicitly selected
            //
            if (String.IsNullOrEmpty(Server.comPortSpeed))
            {
                Server.comPortSpeed = Server.comPortSpeedDefault;
            }
            switch (Server.comPortSpeed)
            {
                case "9600":
                    radioBaudrate9600 .Checked = true;
                    radioBaudrate19200.Checked = false;
                    radioBaudrate38400.Checked = false;
                    break;
                case "19200":
                    radioBaudrate9600 .Checked = false;
                    radioBaudrate19200.Checked = true;
                    radioBaudrate38400.Checked = false;
                    break;
                case "38400":
                    radioBaudrate9600 .Checked = false;
                    radioBaudrate19200.Checked = false;
                    radioBaudrate38400.Checked = true;
                    break;
                default:
                    radioBaudrate9600 .Checked = false;
                    radioBaudrate19200.Checked = false;
                    radioBaudrate38400.Checked = false;
                    break;
            }

            if (SharedResources.SharedSerial.Connected)
            {
                boxSerialComPort  .Enabled = false;
                radioBaudrate9600 .Enabled = false;
                radioBaudrate19200.Enabled = false;
                radioBaudrate38400.Enabled = false;
            }

            //  Show Trace ON/OFF state
            //
            chkTrace.Checked = Server.traceState;

            //  Show Timeout Handler ON/OFF state
            //
            chkTimeoutHandling.Checked = Server.timeoutHandlingState;
        }

        /**	--------------------------------------------------------------------------------
         *	@brief Klick auf OK, Abspeichern der Einstellungen und Dialog beenden
         *	--------------------------------------------------------------------------------
         */
        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            //  Einstellungen für den laufenden Treiber übernehmen
            //
            if (boxSerialComPort.SelectedItem != null)
            {
                Server.comPort = boxSerialComPort.SelectedItem.ToString();
            }
            else
            {
                Server.comPort = "";
            }

            if (radioBaudrate9600.Checked)
            {
                Server.comPortSpeed = "9600";
            }
            else if (radioBaudrate19200.Checked)
            {
                Server.comPortSpeed = "19200";
            }
            else if (radioBaudrate38400.Checked)
            {
                Server.comPortSpeed = "38400";
            }
            Server.traceState = chkTrace.Checked;
        }

        /**	--------------------------------------------------------------------------------
         *	@brief Klick auf Cancel, Dialog ohne Abspeichern beenden
         *	--------------------------------------------------------------------------------
         */
        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        /**	--------------------------------------------------------------------------------
         *	@brief Klick auf das ASCOM-Logo, Öffnen eines Browserfensters mit der
         *	ASCOM-WebSite
         *	--------------------------------------------------------------------------------
         */
        private void BrowseToAscom(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {
                    MessageBox.Show(noBrowser.Message);
                }
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

    }
}