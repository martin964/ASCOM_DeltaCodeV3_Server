//
// ================
// Shared Resources
// ================
//
// This class is a container for all shared resources that may be needed
// by the drivers served by the Local Server. 
//
// NOTES:
//
//	* ALL DECLARATIONS MUST BE STATIC HERE!! INSTANCES OF THIS CLASS MUST NEVER BE CREATED!
//
// Written by:	Bob Denny	29-May-2007
// Modified by Chris Rowland and Peter Simpson to hamdle multiple hardware devices March 2011
//
using System;
using System.Collections.Generic;
using System.Text;

using ASCOM;
using ASCOM.Utilities;

namespace ASCOM.DeltaCodeV3
{
    /// <summary>
    /// The resources shared by all drivers and devices, in this example it's a serial port with a shared SendMessage method
    /// an idea for locking the message and handling connecting is given.
    /// In reality extensive changes will probably be needed.
    /// Multiple drivers means that several applications connect to the same hardware device, aka a hub.
    /// Multiple devices means that there are more than one instance of the hardware, such as two focusers.
    /// In this case there needs to be multiple instances of the hardware connector, each with it's own connection count.
    /// </summary>
    public static class SharedResources
	{
        // object used for locking to prevent multiple drivers accessing common code at the same time
        private static readonly object lockObject = new object();

        // Shared serial port. This will allow multiple drivers to use one single serial port.
		private static ASCOM.Utilities.Serial   s_sharedSerial    = new ASCOM.Utilities.Serial();		// Shared serial port
		private static int                      s_nNoOfConnections = 0;     // counter for the number of connections to the serial port

        //
        // Public access to shared resources
        //
        public static string    traceStateProfileName = "Trace Level";
        public static string    traceStateDefault = "false";
        public static bool      traceState;

        public static string    comPortProfileName = "COM Port";
        public static string    comPortProfileSpeed = "COM Port Speed";
        public static string    comPortDefault = "COM1";
        public static string    comPortSpeedDefault = "38400";
        public static string    comPort; // Variables to hold the currrent device configuration
        public static string    comPortSpeed;

        public static string    timeoutHandlingStateProfileName = "Timeout Handling";
        public static string    timeoutHandlingStateDefault = "false";
        internal static bool    timeoutHandlingState;


#region singe_setup_dialog

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        /// 
        public static void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected

            if (Connected)
            {
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");
                return;
            }

            using (SetupDialogForm frmSetup = new SetupDialogForm())
            {
                System.Windows.Forms.DialogResult result = frmSetup.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    //  Persist device configuration values to the ASCOM Profile store
                    //  
                    WriteProfile();
                }
            }
        }

#endregion

#region single_trace_object

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        private static TraceLogger tl;

        public static TraceLogger TraceLogger
        {
            get
            {
                if (tl == null)
                {
                    tl = new TraceLogger("DeltaCodeV3");
                    tl.Enabled = traceState;
                }
                return tl;
            }
        }

#endregion


#region single serial port connector

        /// <summary>
        /// Shared serial port
        /// </summary>
        /// 
        private static ASCOM.Utilities.Serial SharedSerial { get { return s_sharedSerial; } }

        /// <summary>
        /// Example of a shared SendMessage method, the lock
        /// prevents different drivers tripping over one another.
        /// It needs error handling and assumes that the message will be sent unchanged
        /// and that the reply will always be terminated by a "#" character.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string SendMessage(string message)
        {
            lock (lockObject)
            {
                SharedSerial.Transmit(message);
                // TODO replace this with your requirements
                return SharedSerial.ReceiveTerminated("#");
            }
        }

        public static void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");

            SharedSerial.ClearBuffers();
            if (raw)
            {
                tl.LogMessage("CommandBlind", String.Format("command=<{0}>, raw={1}", command, raw));
                SharedSerial.Transmit(command);
            }
            else
            {
                tl.LogMessage("CommandBlind", String.Format("command=<{0}>, raw={1}", command, raw));
                SharedSerial.Transmit(command + '#');
            }
        }


        /// <summary>
        /// Send a command to the telescope, receive an answer string.
        /// The received string is checked and converted into a boolean result.
        /// </summary>
        /// <param name="command">Command string sent to the telescope</param>
        /// <param name="raw">Flag for raw command, will not be extended by a '#'</param>
        /// <returns>Flag for success</returns>
        /// 
        public static bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");

            string ret = CommandString(command, raw).ToUpper();
            tl.LogMessage("CommandBool", String.Format("command=<{0}>, raw={1}, return=<{2}>", command, raw, ret));

            if (ret == "1" || ret == "TRUE" || ret == "YES")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static string CommandString(string command, bool raw)
        {
            string cResponse;
            string cEndChar;

            CheckConnected("CommandString");

            SharedSerial.ClearBuffers();

            if (raw)
            {
                cEndChar = command.Substring(command.Length - 1);
                SharedSerial.Transmit(command);
            }
            else
            {
                cEndChar = "#";
                SharedSerial.Transmit(command + cEndChar);
            }

            cResponse = SharedSerial.ReceiveTerminated(cEndChar);
            tl.LogMessage("CommandString", String.Format("command=<{0}>, raw={1}, return=<{2}>", command, raw, cResponse));

            if (cResponse.EndsWith(cEndChar))
            {
                cResponse = cResponse.TrimEnd(new char[] { cEndChar[0] });
            }
            return cResponse;
        }


        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        public static void CheckConnected(string message)
        {
            if (!SharedSerial.Connected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }


        /// <summary>
        /// Property:
        /// Connect/disconnect the controller or ask if the controller is connected
        /// </summary>     
        /// 
        public static bool Connected
        {
            get
            {
                return SharedSerial.Connected;
            }
            set
            {
                lock (lockObject)
                {
                    if (value)
                    {
                        if (s_nNoOfConnections == 0)
                        {
                            Connect();
                        }
                        s_nNoOfConnections++;
                    }
                    else
                    {
                        if (s_nNoOfConnections > 0)
                        {
                            s_nNoOfConnections--;

                            if (s_nNoOfConnections == 0)
                            {
                                Disconnect();
                            }
                        }
                    }
                }
            }
        }


        public static void Connect ()
        {
            ReadProfile();

            if (string.IsNullOrEmpty(comPort))
            {
                throw new ASCOM.NotConnectedException("No Serial Port selected");
            }

            try
            {
                if (String.IsNullOrEmpty(comPortSpeed))
                {
                    comPortSpeed = comPortSpeedDefault;
                }

                Serial oSerialPort = SharedSerial;
                oSerialPort.PortName = comPort;
                switch (comPortSpeed)
                {
                    case "9600":
                        oSerialPort.Speed = SerialSpeed.ps9600;
                        break;
                    case "19200":
                        oSerialPort.Speed = SerialSpeed.ps19200;
                        break;
                    case "38400":
                        oSerialPort.Speed = SerialSpeed.ps38400;
                        break;
                }
                oSerialPort.Connected = true;
            }
            catch (Exception ex)
            {
                throw new ASCOM.NotConnectedException(ex.Message, ex);
            }
        }

        public static void Disconnect()
        {
            if (SharedSerial != null)
            {
                if (SharedSerial.Connected)
                {
                    SharedSerial.Connected = false;
                }
            }
        }
#endregion

#region Profiles
        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        public static void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                timeoutHandlingState = Convert.ToBoolean(driverProfile.GetValue("ASCOM.DeltaCodeV3.Telescope", timeoutHandlingStateProfileName, string.Empty, timeoutHandlingStateDefault));
                comPort = driverProfile.GetValue("ASCOM.DeltaCodeV3.Telescope", comPortProfileName, string.Empty, comPortDefault);
                comPortSpeed = driverProfile.GetValue("ASCOM.DeltaCodeV3.Telescope", comPortProfileSpeed, string.Empty, comPortSpeedDefault);

                string cTraceLevel = driverProfile.GetValue("ASCOM.DeltaCodeV3.Telescope", traceStateProfileName, string.Empty, traceStateDefault);
                traceState = cTraceLevel.ToLower() == "true" ? true : false;
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        public static void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                driverProfile.WriteValue("ASCOM.DeltaCodeV3.Telescope", timeoutHandlingStateProfileName, timeoutHandlingState.ToString());
                driverProfile.WriteValue("ASCOM.DeltaCodeV3.Telescope", comPortProfileName, comPort.ToString());
                driverProfile.WriteValue("ASCOM.DeltaCodeV3.Telescope", comPortProfileSpeed, comPortSpeed.ToString());

                driverProfile.WriteValue("ASCOM.DeltaCodeV3.Telescope", traceStateProfileName, traceState.ToString());
            }
        }

#endregion

    }
}
