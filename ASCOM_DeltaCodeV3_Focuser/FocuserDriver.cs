//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Focuser driver for DeltaCodeV3
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Focuser interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
#define Focuser

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;

namespace ASCOM.DeltaCodeV3
{
    //
    // Your driver's DeviceID is ASCOM.DeltaCodeV3.Focuser
    //
    // The Guid attribute sets the CLSID for ASCOM.DeltaCodeV3.Focuser
    // The ClassInterface/None attribute prevents an empty interface called
    // _DeltaCodeV3 from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Focuser Driver for DeltaCodeV3
    /// </summary>
    [Guid("c139eb83-17c0-4ef6-bbd1-65e35928a9cd")]
    [ProgId("ASCOM.DeltaCodeV3.Focuser")]
    [ServedClassName("Focuser Driver for DeltaCodeV3")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Focuser : ReferenceCountedObjectBase, IFocuserV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID;

        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "Focuser Driver for DeltaCodeV3";

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaCodeV3"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Focuser()
        {
            driverID = Marshal.GenerateProgIdForType(this.GetType());

            SharedResources.ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl = SharedResources.TraceLogger;
            LogMessage("Focuser", "Starting initialisation");

            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro-utilities object

            LogMessage("Focuser", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE IFocuserV3 IMPLEMENTATION
        //

#region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            SharedResources.SetupDialog();
        }

        public ArrayList SupportedActions
        {
            get
            {
                LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("Action {0} not implemented", actionName);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            SharedResources.CommandBlind(command, raw);
        }

        public bool CommandBool(string command, bool raw)
        {
            return SharedResources.CommandBool(command, raw);
        }

        public string CommandString(string command, bool raw)
        {
            return SharedResources.CommandString(command, raw);
        }

        public void Dispose()
        {
            // Clean up the trace logger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }

        public bool Connected
        {
            get
            {
                bool bIsConnected = SharedResources.Connected;
                LogMessage("Connected Get", bIsConnected.ToString());
                return bIsConnected;
            }
            set
            {
                LogMessage("Connected", "Set {0}", value);
                SharedResources.Connected = value;
            }
        }

        public string Description
        {
            get
            {
                LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "ASCOM Driver for DeltaCodeV3. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "3");
                return Convert.ToInt16("3");
            }
        }

        public string Name
        {
            get
            {
                string name = "ASCOM.DeltaCodeV3.Focuser";
                LogMessage("Name Get", name);
                return name;
            }
        }

#endregion


#region IFocuser Implementation

        private const int focuserSteps = 10000;

        public bool Absolute
        {
            get
            {
                LogMessage("Absolute Get", false.ToString());
                return false;
            }
        }

        public void Halt()
        {
            LogMessage("Halt", "OK");
            CommandBlind(":FQ", false);
        }

        public bool IsMoving
        {
            get
            {
                if (Connected)
                {
                    string cFocuserBusy = CommandString(":FB", false);
                    bool bFocuserBusy = cFocuserBusy == "1" ? true : false;

                    LogMessage("IsMoving Get", bFocuserBusy.ToString());
                    return bFocuserBusy;
                }
                else
                {
                    LogMessage("IsMoving Get", false.ToString());
                    return false;
                }
            }
        }

        public bool Link
        {
            get
            {
                LogMessage("Link Get", this.Connected.ToString());
                return this.Connected; // Direct function to the connected method, the Link method is just here for backwards compatibility
            }
            set
            {
                LogMessage("Link Set", value.ToString());
                this.Connected = value; // Direct function to the connected method, the Link method is just here for backwards compatibility
            }
        }

        public int MaxIncrement
        {
            get
            {
                LogMessage("MaxIncrement Get", focuserSteps.ToString());
                return focuserSteps; // Maximum change in one move
            }
        }

        public int MaxStep
        {
            get
            {
                LogMessage("MaxStep Get", focuserSteps.ToString());
                return focuserSteps; // Maximum extent of the focuser, so position range is 0 to 10,000
            }
        }

        public void Move(int nDistanceToGo)
        {
            string cPositionSteps = nDistanceToGo.ToString();

            CommandBlind(":FP" + cPositionSteps, false);

            LogMessage("Move", nDistanceToGo.ToString());
        }

        public int Position
        {
            get
            {
                LogMessage("Position Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Position", false);
            }
        }

        public double StepSize
        {
            get
            {
                LogMessage("StepSize Get", 1.ToString());
                return 1.0;
            }
        }

        public bool TempComp
        {
            get
            {
                LogMessage("TempComp Get", false.ToString());
                return false;
            }
            set
            {
                LogMessage("TempComp Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TempComp", false);
            }
        }

        public bool TempCompAvailable
        {
            get
            {
                LogMessage("TempCompAvailable Get", false.ToString());
                return false; // Temperature compensation is not available in this driver
            }
        }

        public double Temperature
        {
            get
            {
                LogMessage("Temperature Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Temperature", false);
            }
        }

#endregion


        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            SharedResources.CheckConnected(message);
        }


        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private void LogMessage(string identifier, string message)
        {
            tl.LogMessage("Focuser." + identifier, message);
        }


        private void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage("Focuser." + identifier, msg);
        }
    }
}
