//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Telescope driver for DeltaCodeV3
//
// Description:	
//
// Implements:	ASCOM Telescope interface version: V3
// Author:		Martin Cibulski <martin.cibulski@gmx.de>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Telescope

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;


using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.DeltaCodeV3
{
    //
    // Your driver's DeviceID is ASCOM.DeltaCodeV3.Telescope
    //
    // The Guid attribute sets the CLSID for ASCOM.DeltaCodeV3.Telescope
    // The ClassInterface/None addribute prevents an empty interface called
    // _DeltaCodeV3 from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Telescope Driver for DeltaCodeV3.
    /// </summary>
    /// 
    [Guid("6B16CEBB-6892-4C28-96C3-A202D1C2746B")]
    [ProgId("ASCOM.DeltaCodeV3.Telescope")]
    [ServedClassName("ASCOM Telescope Driver for DeltaCodeV3")]
    [ClassInterface(ClassInterfaceType.None)]
    //public class Telescope : ITelescopeV3
    public class Telescope : ReferenceCountedObjectBase, ITelescopeV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        /// 
        internal static string driverID; // = "ASCOM.DeltaCodeV3.Telescope";    //TODO take from ProdId

        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        /// 
        //private static string driverDescription = "ASCOM Telescope Driver for DeltaCodeV3.";

        internal static string  comPortProfileName      = "COM Port";
        internal static string  comPortProfileSpeed     = "COM Port Speed";
        internal static string  comPortDefault          = "COM1";
        internal static string  comPortSpeedDefault     = "38400";
        internal static string  traceStateProfileName   = "Trace Level";
        internal static string  traceStateDefault       = "true";

        internal static string comPort; // Variables to hold the currrent device configuration
        internal static string comPortSpeed;
        internal static bool traceState;

        private Serial m_oSerialPort;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        /// 
        private bool connectedState;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        /// 
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        /// 
        private AstroUtils astroUtilities;

        /// <summary>
        /// Private variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        /// 
        private TraceLogger tl;


        /// <summary>
        /// Private variables to hold the DelatCode Product and Firmware Version
        /// </summary>
        /// 
        string cProductName             = "DeltaCodeV3";
        string cFirmwareVersion         = "V3";
        string cFirmwareVersionNumber   = "01.1";
        string cFirmwareVersionDate     = "2019-xx-xx";
        string cFirmwareVersionTime     = "00:00:00";


        /// <summary>
        /// Target Rightascension and Declination
        /// </summary>
        /// 
        private double      m_fTargetRightascension     = 0.0;
        private double      m_fTargetDeclination        = 0.0;
        private bool        m_bTargetRightascensionSet  = false;
        private bool        m_bTargetDeclinationSet     = false;
        private bool        m_bIsSlewingRA              = false;
        private bool        m_bIsSlewingDec             = false;
        private DateTime    m_dtPulseGuideStartNS       = DateTime.Now;
        private DateTime    m_dtPulseGuideStartEW       = DateTime.Now;
        private int         m_nPulseGuideDurationNS     = 0;
        private int         m_nPulseGuideDurationEW     = 0;

        /// <summary>
        /// Rightascension und Declination Drift Rate
        /// </summary>
        /// 
        private double      m_fRightascensionRate       = 0.0;
        private double      m_fDeclinationRate          = 0.0;


        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaCodeV3"/> class.
        /// Must be public for COM registration.
        /// </summary>
        /// 
        public Telescope()
        {
            driverID = Marshal.GenerateProgIdForType(this.GetType());

            //  Read device configuration from the ASCOM Profile store
            //  
            ReadProfile();

            tl = new TraceLogger("", "DeltaCodeV3");
            tl.Enabled = traceState;
            tl.LogMessage("Telescope", "Starting initialisation");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro utilities object

            tl.LogMessage("Telescope", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ITelescopeV3 IMPLEMENTATION
        //

#region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        /// 
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected

            if (IsConnected)
            {
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");
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


        /// <summary>
        /// Get List of supported Actions for the AscomDriver.Action Method
        /// </summary>
        /// 
        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }


        /// <summary>
        /// Perform a device specific action
        /// </summary>
        /// <param name="actionName">Action/Command name</param>
        /// <param name="actionParameters">Action/Command parameters</param>
        /// <returns>Answer string for the calling application</returns>
        /// 
        public string Action(string actionName, string actionParameters)
        {
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }


        /// <summary>
        /// Send a command to the telescope without receiving an answer
        /// </summary>
        /// <param name="command">Command string sent to the telescope</param>
        /// <param name="raw">Flag for raw command, will not be extended by a '#'</param>
        /// 
        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");

            ASCOM.Utilities.Serial serial = SharedResources.SharedSerial;

            serial.ClearBuffers();
            if (raw)
            {
                serial.Transmit(command);
            }
            else
            {
                serial.Transmit(command + '#');
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
        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");

            string ret = CommandString(command, raw);

            return false;
        }


        /// <summary>
        /// Send a command to the telescope, receive an answer string.
        /// The received string is returned as the command's result.
        /// </summary>
        /// <param name="command">Command string sent to the telescope</param>
        /// <param name="raw">Flag for raw command, will not be extended by a '#'</param>
        /// <returns>Answer string from the telescope</returns>
        /// 
        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");

            ASCOM.Utilities.Serial serial = SharedResources.SharedSerial;

            serial.ClearBuffers();
            if (raw)
            {
                serial.Transmit(command);
            }
            else
            {
                serial.Transmit(command + '#');
            }

            string cResponse = serial.ReceiveTerminated("#");
            if (cResponse.EndsWith("#"))
            {
                cResponse = cResponse.TrimEnd(new char[] { '#' });
            }
            return cResponse;
        }


        /// <summary>
        /// Destructor, deletes helper objects
        /// </summary>
        /// 
        public void Dispose()
        {
            //  Clean up the tracelogger and util objects
            //  
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }


        /// <summary>
        /// Property:
        /// Connect/disconnect the telescope or ask if telescope is connected
        /// </summary>     
        /// 
        public bool Connected
        {
            get
            {
                tl.LogMessage("Connected Get", IsConnected.ToString());
                return IsConnected;
            }
            set
            {

                tl.LogMessage("Connected Set", value.ToString());
                if (value == IsConnected)
                {
                    return;
                }
                if (value)
                {
                    tl.LogMessage("Connected Set", "Connecting to port " + comPort);

                    if (string.IsNullOrEmpty(Telescope.comPort))
                    {
                        throw new ASCOM.NotConnectedException("No Serial Port selected");
                    }

                    try
                    {
                        if (String.IsNullOrEmpty(Telescope.comPortSpeed))
                        {
                            Telescope.comPortSpeed = Telescope.comPortSpeedDefault;
                        }

                        m_oSerialPort = SharedResources.SharedSerial;   // new Serial();
                        m_oSerialPort.PortName = Telescope.comPort;
                        switch (Telescope.comPortSpeed)
                        {
                            case "9600":
                                m_oSerialPort.Speed = SerialSpeed.ps9600;
                                break;
                            case "19200":
                                m_oSerialPort.Speed = SerialSpeed.ps19200;
                                break;
                            case "38400":
                                m_oSerialPort.Speed = SerialSpeed.ps38400;
                                break;
                        }
                        m_oSerialPort.Connected = true;
                        connectedState = true;
                    }
                    catch (Exception ex)
                    {
                        throw new ASCOM.NotConnectedException(ex.Message, ex);
                    }

                    //  Ask version number
                    //
                    cProductName = CommandString(":GVP#", true);
                    cProductName = CommandString(":GVP#", true);
                    cFirmwareVersion = CommandString(":GVF#", true);
                    cFirmwareVersionNumber = CommandString(":GVN#", true);
                    cFirmwareVersionDate = CommandString(":GVD#", true);
                    cFirmwareVersionTime = CommandString(":GVT#", true);

                    tl.LogMessage("Connected Set", "Product Name...........: " + cProductName);
                    tl.LogMessage("Connected Set", "Firmware Version.......: " + cFirmwareVersion);
                    tl.LogMessage("Connected Set", "Firmware Version Number: " + cFirmwareVersionNumber);
                    tl.LogMessage("Connected Set", "Firmware Version Date..: " + cFirmwareVersionDate);
                    tl.LogMessage("Connected Set", "Firmware Version Time..: " + cFirmwareVersionTime);

                    //  Set coordinates to high precision
                    //  First ask for right ascension (:GR#) then check the result.
                    //  If the result is short, switch to high precision coordinates by the :U# command
                    //  (toggle High Precision Flag)
                    //
                    string cRightAscension;
                    cRightAscension = CommandString(":GR#", true);
                    cRightAscension = CommandString(":GR#", true);
                    if (cRightAscension.Length <= 8)
                    {
                        CommandBlind(":U#", true);
                    }
                }
                else
                {
                    tl.LogMessage("Connected Set", "Disconnecting from port " + comPort);

                    if (m_oSerialPort != null)
                    {
                        if (m_oSerialPort.Connected)
                        {
                            m_oSerialPort.Connected = false;
                        }
                    }
                    connectedState = false;
                }
            }
        }


        /// <summary>
        /// Return the Driver Description
        /// </summary>
        /// 
        public string Description
        {
            get
            {
                string cDescription;

                if (IsConnected)
                {
                    cDescription    = "Product Name...........: " + cProductName + Environment.NewLine
                                    + "Firmware Version.......: " + cFirmwareVersion + Environment.NewLine
                                    + "Firmware Version Number: " + cFirmwareVersionNumber + Environment.NewLine
                                    + "Firmware Version Date..: " + cFirmwareVersionDate + Environment.NewLine
                                    + "Firmware Version Time..: " + cFirmwareVersionTime;
                    tl.LogMessage("Description Get", cDescription.Replace("\n", "\\n"));
                }
                else
                {
                    tl.LogMessage("Description Get", "NOT CONNECTED !");
                    throw new ASCOM.NotConnectedException();
                }
                return cDescription;
            }
        }


        /// <summary>
        /// Return the Driver Info
        /// </summary>
        /// 
        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverInfo = "ASCOM Driver for DeltaCodeV3. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);

                tl.LogMessage("DriverInfo Get", driverInfo);

                return driverInfo;
            }
        }


        /// <summary>
        /// Return the Driver's Version
        /// </summary>
        /// 
        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);

                tl.LogMessage("DriverVersion Get", driverVersion);

                return driverVersion;
            }
        }


        /// <summary>
        /// Return the Telescope Interface Version (V3)
        /// (set by the driver wizard)
        /// </summary>
        /// 
        public short InterfaceVersion
        {
            get
            {
                tl.LogMessage("InterfaceVersion Get", "3");
                return Convert.ToInt16("3");
            }
        }


        /// <summary>
        /// Return a short Driver Name
        /// </summary>
        /// 
        public string Name
        {
            get
            {
                string name = "ASCOM.DeltaCodeV3.Telescope";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

#endregion


#region ITelescope Implementation

        public void AbortSlew()
        {
            tl.LogMessage("AbortSlew", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("AbortSlew");
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                tl.LogMessage("AlignmentMode Get", "algGermanPolar");
                return AlignmentModes.algGermanPolar;
            }
        }

        public double Altitude
        {
            get
            {
                tl.LogMessage("Altitude", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public double ApertureArea
        {
            get
            {
                tl.LogMessage("ApertureArea Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                tl.LogMessage("ApertureDiameter Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                tl.LogMessage("AtHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool AtPark
        {
            get
            {
                CheckConnected("AtPark.get");

                string  cMountStatus = CommandString(":Gstat", false);
                bool    bAtPark = cMountStatus == "5" ? true : false;
                
                tl.LogMessage("AtPark", "Get - " + bAtPark.ToString());
                return bAtPark;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            tl.LogMessage("AxisRates", "Get - " + Axis.ToString());
            return new AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                tl.LogMessage("Azimuth Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
            }
        }

        public bool CanFindHome
        {
            get
            {
                tl.LogMessage("CanFindHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {

            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                    tl.LogMessage("CanMoveAxis", "Get[axisPrimary] - " + true.ToString());
                    return true;
                case TelescopeAxes.axisSecondary:
                    tl.LogMessage("CanMoveAxis", "Get[axisSecondary] - " + true.ToString());
                    return true;
                case TelescopeAxes.axisTertiary:
                    tl.LogMessage("CanMoveAxis", "Get[axisTertiary] - " + false.ToString());
                    return false;
                default:
                    throw new InvalidValueException("CanMoveAxis", Axis.ToString(), "0 to 2");
            }
        }

        public bool CanPark
        {
            get
            {
                tl.LogMessage("CanPark", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                tl.LogMessage("CanPulseGuide", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                tl.LogMessage("CanSetDeclinationRate", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                tl.LogMessage("CanSetGuideRates", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetPark
        {
            get
            {
                tl.LogMessage("CanSetPark", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                tl.LogMessage("CanSetPierSide", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                tl.LogMessage("CanSetRightAscensionRate", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                tl.LogMessage("CanSetTracking", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlew
        {
            get
            {
                tl.LogMessage("CanSlew", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                tl.LogMessage("CanSlewAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                tl.LogMessage("CanSlewAltAzAsync", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                tl.LogMessage("CanSlewAsync", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                tl.LogMessage("CanSync", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                tl.LogMessage("CanSyncAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanUnpark
        {
            get
            {
                tl.LogMessage("CanUnpark", "Get - " + true.ToString());
                return true;
            }
        }

        public double Declination
        {
            get
            {
                CheckConnected("Declination.get");

                string cDeclination = CommandString(":GD#", true);
                double fDeclination = utilities.DMSToDegrees(cDeclination);

                tl.LogMessage("Declination", "Get - " + utilities.HoursToHMS(fDeclination));
                return fDeclination;
            }
        }

        /**	--------------------------------------------------------------------------------
         *	@brief Drift-Rate in Deklination
         *	
         *  Die Drift-Rate wird angegeben in 'arcseconds per second'
         *  Für die DeltaCodeV3-Steuerung wird sie in Millibogensekunden pro Minute umgerechnet
         *  und über den neu definierten Befehl :Sdr (set declination rate) eingestellt
         *	--------------------------------------------------------------------------------
         */
        public double DeclinationRate
        {
            get
            {
                string cDecRate_MilliArcsecPerMinute = CommandString(":GDr#", true);
                cDecRate_MilliArcsecPerMinute = cDecRate_MilliArcsecPerMinute.TrimEnd(new char[] { '#' });
                int nDecRate_MilliArcsecPerMinute = Convert.ToInt32(cDecRate_MilliArcsecPerMinute);

                m_fDeclinationRate = (nDecRate_MilliArcsecPerMinute) / (1000.0 * 60.0);

                tl.LogMessage("DeclinationRate", "Get - " + m_fDeclinationRate.ToString());
                return m_fDeclinationRate;
            }
            set
            {

                int nDecRate_MilliArcsecPerMinute = (int)(value * 1000.0 * 60.0);
                string cDecRate = nDecRate_MilliArcsecPerMinute.ToString();
                CommandBlind(":Sdr" + cDecRate, false);

                tl.LogMessage("DeclinationRate Set", "Set + " + value.ToString());

                m_fDeclinationRate = value;
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            tl.LogMessage("DestinationSideOfPier Get", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("DestinationSideOfPier");
        }

        public bool DoesRefraction
        {
            get
            {
                tl.LogMessage("DoesRefraction", "Get - " + false.ToString());
                return false;
            }
            set
            {
                tl.LogMessage("DoesRefraction Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equJ2000;
                tl.LogMessage("EquatorialSystem", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            tl.LogMessage("FindHome", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("FindHome");
        }

        public double FocalLength
        {
            get
            {
                tl.LogMessage("FocalLength Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public double GuideRateDeclination
        {
            get
            {
                tl.LogMessage("GuideRateDeclination Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", false);
            }
            set
            {
                tl.LogMessage("GuideRateDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                tl.LogMessage("GuideRateRightAscension Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
            }
            set
            {
                tl.LogMessage("GuideRateRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
            }
        }

        /**	--------------------------------------------------------------------------------
         *	@brief IsPulseGuiding, Abfrage, ob ein Pulse-Guide-Befehl im Gange ist
         *	
         */
        public bool IsPulseGuiding
        {
            get
            {
                bool bIsPulseGuiding = false;

                if (DateTime.Now.Subtract(m_dtPulseGuideStartNS).TotalMilliseconds <= m_nPulseGuideDurationNS)
                {
                    bIsPulseGuiding = true;
                }

                if (DateTime.Now.Subtract(m_dtPulseGuideStartEW).TotalMilliseconds <= m_nPulseGuideDurationEW)
                {
                    bIsPulseGuiding = true;
                }

                tl.LogMessage("CanSlewAsync", "Get - " + bIsPulseGuiding.ToString());
                return bIsPulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            tl.LogMessage("MoveAxis", "OK");

            AxisRates oRates = new AxisRates(Axis);

            bool bNegative = (Rate < 0.0) ? true : false;
            Rate = Math.Abs(Rate);

            //throw new ASCOM.MethodNotImplementedException("MoveAxis");

            if (Axis == TelescopeAxes.axisPrimary)
            {
                if (Rate == 0.0)
                {
                    CommandBlind(":Qe", false);
                    CommandBlind(":Qw", false);
                    m_bIsSlewingRA = false;
                    return;
                }
                else if (Rate >= oRates[1].Minimum && Rate <= oRates[1].Maximum)
                {
                    CommandBlind(":RG", false);
                }
                else if (Rate >= oRates[2].Minimum && Rate <= oRates[2].Maximum)
                {
                    CommandBlind(":RC", false);
                }
                else if (Rate >= oRates[3].Minimum && Rate <= oRates[3].Maximum)
                {
                    CommandBlind(":RM", false);
                }
                else if (Rate >= oRates[4].Minimum && Rate <= oRates[4].Maximum)
                {
                    CommandBlind(":RS", false);
                }
                else
                {
                    throw new ASCOM.InvalidValueException ("Rate not allowed");
                }
                if (bNegative)
                {
                    CommandBlind(":Mw", false);
                }
                else
                {
                    CommandBlind(":Me", false);
                }
                m_bIsSlewingRA = true;
            }
            else if (Axis == TelescopeAxes.axisSecondary)
            {
                if (Rate == 0.0)
                {
                    CommandBlind(":Qn", false);
                    CommandBlind(":Qs", false);
                    m_bIsSlewingDec = false;
                    return;
                }
                else if (Rate >= oRates[1].Minimum && Rate <= oRates[1].Maximum)
                {
                    CommandBlind(":RG", false);
                }
                else if (Rate >= oRates[2].Minimum && Rate <= oRates[2].Maximum)
                {
                    CommandBlind(":RC", false);
                }
                else if (Rate >= oRates[3].Minimum && Rate <= oRates[3].Maximum)
                {
                    CommandBlind(":RM", false);
                }
                else if (Rate >= oRates[4].Minimum && Rate <= oRates[4].Maximum)
                {
                    CommandBlind(":RS", false);
                }
                else
                {
                    throw new ASCOM.InvalidValueException("Rate not allowed");
                }
                if (bNegative)
                {
                    CommandBlind(":Ms", false);
                }
                else
                {
                    CommandBlind(":Mn", false);
                }
                m_bIsSlewingDec = true;
            }
            else
            {
                throw new ASCOM.MethodNotImplementedException("MoveAxis");
            }
        }


        public void Park()
        {
            tl.LogMessage("Park", "OK");
            CommandBlind(":hP", false);
        }


        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            string cDurationMS = Duration.ToString();

            switch (Direction)
            {
                case GuideDirections.guideNorth:
                    CommandBlind (":Mgn" + cDurationMS, false);
                    m_dtPulseGuideStartNS = DateTime.Now;
                    m_nPulseGuideDurationNS = Duration;
                    break;
                case GuideDirections.guideSouth:
                    CommandBlind (":Mgs" + cDurationMS, false);
                    m_dtPulseGuideStartNS = DateTime.Now;
                    m_nPulseGuideDurationNS = Duration;
                    break;
                case GuideDirections.guideEast:
                    CommandBlind (":Mge" + cDurationMS, false);
                    m_dtPulseGuideStartEW = DateTime.Now;
                    m_nPulseGuideDurationEW = Duration;
                    break;
                case GuideDirections.guideWest:
                    CommandBlind (":Mgw" + cDurationMS, false);
                    m_dtPulseGuideStartEW = DateTime.Now;
                    m_nPulseGuideDurationEW = Duration;
                    break;
            }


            tl.LogMessage("PulseGuide", "OK");
        }

        public double RightAscension
        {
            get
            {
                CheckConnected("RightAscension.get");

                string cRightAscension = CommandString(":GR#", true);
                double fRightAscension = utilities.HMSToHours (cRightAscension);

                tl.LogMessage("RightAscension", "Get - " + utilities.HoursToHMS(fRightAscension));
                return fRightAscension;
            }
        }

        /**	--------------------------------------------------------------------------------
         *	@brief Drift-Rate in Rektaszension
         *	
         *  Die Drift-Rate wird angegeben in 'seconds of right ascension per sidereal second'
         *  Für die DeltaCodeV3-Steuerung wird sie in Millibogensekunden pro Minute umgerechnet
         *  und über die Befehle :Rte (ost) oder :Rtw (west) eingestellt
         *	--------------------------------------------------------------------------------
         */
        public double RightAscensionRate
        {
            get
            {
                CheckConnected("RightAscensionRate.get");

                string cRaRate_MilliArcsecPerMinute = CommandString(":GRr#", true);
                cRaRate_MilliArcsecPerMinute = cRaRate_MilliArcsecPerMinute.TrimEnd(new char[] { '#' });
                int nRaRate_MilliArcsecPerMinute = Convert.ToInt32(cRaRate_MilliArcsecPerMinute);

                m_fRightascensionRate = (nRaRate_MilliArcsecPerMinute) / (15.0 * 1000.0 * 60.0);

                tl.LogMessage("RightAscensionRate", "Get - " + m_fRightascensionRate.ToString());
                return m_fRightascensionRate;
            }
            set
            {

                int nRaRate_MilliArcsecPerMinute = (int)(value * 15.0 * 1000.0 * 60.0);
                string cRaRate = nRaRate_MilliArcsecPerMinute.ToString();
                CommandBlind(":Srr" + cRaRate, false);

                tl.LogMessage("RightAscensionRate", "Set + " + m_fRightascensionRate.ToString());

                m_fRightascensionRate = value;
            }
        }


        public void SetPark()
        {
            tl.LogMessage("SetPark", "OK");
            CommandBlind(":hS", false);
        }


        /// <summary>
        /// PierSide
        /// Taken from Astrophysics/10Micron Command Set
        /// </summary>
        /// 
        public PierSide SideOfPier
        {
            get
            {
                CheckConnected("SideOfPier.get");

                string      cPS;
                PierSide    ps;

                cPS = CommandString(":pS#", true);
                switch (cPS)
                {
                    case "East":
                        ps = PierSide.pierEast;
                        break;
                    case "West":
                        ps = PierSide.pierWest;
                        break;
                    default:
                        ps = PierSide.pierUnknown;
                        break;
                }
                tl.LogMessage("SideOfPier", "Get - " + ps.ToString());
                return ps;
                //Command: :pS#
                //Response: “East#” or      “West#”
            }
            set
            {
                tl.LogMessage("SideOfPier Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
            }
        }

        public double SiderealTime
        {
            get
            {
                double siderealTime = (18.697374558 + 24.065709824419081 * (utilities.DateLocalToJulian(DateTime.Now) - 2451545.0)) % 24.0;
                tl.LogMessage("SiderealTime", "Get - " + siderealTime.ToString());
                return siderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                tl.LogMessage("SiteElevation Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", false);
            }
            set
            {
                tl.LogMessage("SiteElevation Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", true);
            }
        }

        public double SiteLatitude
        {
            get
            {
                tl.LogMessage("SiteLatitude Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLatitude", false);
            }
            set
            {
                tl.LogMessage("SiteLatitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLatitude", true);
            }
        }

        public double SiteLongitude
        {
            get
            {
                tl.LogMessage("SiteLongitude Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLongitude", false);
            }
            set
            {
                tl.LogMessage("SiteLongitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLongitude", true);
            }
        }

        public short SlewSettleTime
        {
            get
            {
                tl.LogMessage("SlewSettleTime Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", false);
            }
            set
            {
                tl.LogMessage("SlewSettleTime Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", true);
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            tl.LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            tl.LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            tl.LogMessage("SlewToCoordinates", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToCoordinates");
        }

        public void SlewToCoordinatesAsync(double fRightAscension, double fDeclination)
        {
            tl.LogMessage("SlewToCoordinatesAsync", "OK");

            string cRightAscension;
            string cDeclination;

            if (fRightAscension < 0.0 || fRightAscension >= 24.0)
            {
                throw new ASCOM.InvalidValueException("Invalid RightAscension");
            }
            if (fDeclination < -90.0 || fDeclination > 90.0)
            {
                throw new ASCOM.InvalidValueException("Invalid Declination");
            }

            m_fTargetRightascension     = fRightAscension;
            m_bTargetRightascensionSet  = true;
            m_fTargetDeclination        = fDeclination;
            m_bTargetDeclinationSet     = true;

            cRightAscension = utilities.HoursToHMS(fRightAscension);
            cDeclination    = utilities.HoursToHMS(fDeclination);

            CommandString(":Sr" + cRightAscension, false);
            CommandString(":Sd" + cDeclination, false);
            CommandBlind(":MS", false);
        }

        public void SlewToTarget()
        {
            tl.LogMessage("SlewToTarget", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToTarget");
        }

        public void SlewToTargetAsync()
        {
            tl.LogMessage("SlewToTargetAsync", "OK");

            string cRightAscension;
            string cDeclination;

            cRightAscension = utilities.HoursToHMS (m_fTargetRightascension);
            cDeclination = utilities.HoursToHMS (m_fTargetDeclination);

            CommandString(":Sr" + cRightAscension, false);
            CommandString(":Sd" + cDeclination, false);
            CommandBlind(":MS", false);
        }

        public bool Slewing
        {
            get
            {
                bool bSlewing = m_bIsSlewingRA || m_bIsSlewingDec;

                tl.LogMessage("Slewing", "Get - " + bSlewing.ToString());
                return bSlewing;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            tl.LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double fRightAscension, double fDeclination)
        {
            tl.LogMessage("SyncToCoordinates", "OK");

            string cRightAscension;
            string cDeclination;

            if (fRightAscension < 0.0 || fRightAscension >= 24.0)
            {
                throw new ASCOM.InvalidValueException("Invalid RightAscension");
            }
            if (fDeclination < -90.0 || fDeclination > 90.0)
            {
                throw new ASCOM.InvalidValueException("Invalid Declination");
            }

            m_fTargetRightascension     = fRightAscension;
            m_bTargetRightascensionSet  = true;
            m_fTargetDeclination        = fDeclination;
            m_bTargetDeclinationSet     = true;

            cRightAscension = utilities.HoursToHMS(fRightAscension);
            cDeclination    = utilities.HoursToHMS(fDeclination);

            CommandString(":Sr" + cRightAscension, false);
            CommandString(":Sd" + cDeclination, false);
            CommandString(":CM", false);
        }

        public void SyncToTarget()
        {
            string cRightAscension;
            string cDeclination;

            cRightAscension = utilities.HoursToHMS(m_fTargetRightascension);
            cDeclination    = utilities.HoursToHMS(m_fTargetDeclination);

            tl.LogMessage("SyncToTarget", "RA=" + cRightAscension + " / Dec=" + cDeclination);

            CommandString(":Sr" + cRightAscension, false);
            CommandString(":Sd" + cDeclination, false);
            CommandString(":CM", false);
        }

        public double TargetDeclination
        {
            get
            {
                if (m_bTargetDeclinationSet)
                {
                    tl.LogMessage("TargetDeclination", "Get - " + utilities.HoursToHMS(m_fTargetDeclination));
                    return m_fTargetDeclination;
                }
                else
                {
                    tl.LogMessage("TargetDeclination", "Get - Value not set");
                    throw new ASCOM.ValueNotSetException ("TargetDeclination");
                }
            }
            set
            {
                if (value >= -90.0 && value <= 90.0)
                {
                    tl.LogMessage("TargetDeclination", "Set - " + utilities.HoursToHMS(value));
                    m_fTargetDeclination = value;
                    m_bTargetDeclinationSet = true;
                }
                else
                {
                    tl.LogMessage("TargetDeclination", "Set - Invalid value" + utilities.HoursToHMS(value));
                    throw new ASCOM.InvalidValueException("TargetDeclination", value.ToString(), "-90 to +90");
                }
            }
        }

        public double TargetRightAscension
        {
            get
            {
                if (m_bTargetRightascensionSet)
                {
                    tl.LogMessage("TargetRightAscension", "Get - " + utilities.HoursToHMS(m_fTargetRightascension));
                    return m_fTargetRightascension;
                }
                else
                {
                    tl.LogMessage("TargetRightAscension", "Get - Value not set");
                    throw new ASCOM.ValueNotSetException("TargetRightAscension");
                }
            }
            set
            {
                if (value >= 0.0 && value <= 24.0)
                {
                    tl.LogMessage("TargetRightAscension", "Set - " + utilities.HoursToHMS(value));
                    m_fTargetRightascension = value;
                    m_bTargetRightascensionSet = true;
                }
                else
                {
                    tl.LogMessage("TargetRightAscension", "Set - Invalid value" + utilities.HoursToHMS(value));
                    throw new ASCOM.InvalidValueException("TargetRightAscension", value.ToString(), "0 to 24");
                }
            }
        }

        public bool Tracking
        {
            get
            {
                bool tracking = true;
                tl.LogMessage("Tracking", "Get - " + tracking.ToString());
                return tracking;
            }
            set
            {
                tl.LogMessage("Tracking Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Tracking", true);
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                tl.LogMessage("TrackingRate", "Get - " + DriveRates.driveSidereal.ToString());
                return DriveRates.driveSidereal;
            }
            set
            {
                tl.LogMessage("TrackingRate Set", "OK");

                ITrackingRates trackingRates = new TrackingRates();
                foreach (DriveRates driveRate in trackingRates)
                {
                    if (value == driveRate)
                    {
                        return;
                    }
                }
                throw new ASCOM.InvalidValueException ("TrackingRate", value.ToString(), DriveRates.driveSidereal.ToString());
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                ITrackingRates trackingRates = new TrackingRates();

                foreach (DriveRates driveRate in trackingRates)
                {
                    tl.LogMessage("TrackingRates", "Get - " + driveRate.ToString());
                }
                return trackingRates;
            }
            set
            {
                tl.LogMessage("TrackingRates Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TrackingRates", true);
            }
        }

        public DateTime UTCDate
        {
            get
            {
                DateTime utcDate = DateTime.UtcNow;
                tl.LogMessage("UTCDate", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
                return utcDate;
            }
            set
            {
                tl.LogMessage("UTCDate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("UTCDate", true);
            }
        }

        public void Unpark()
        {
            tl.LogMessage("Unpark", "OK");
            CommandBlind(":PO", false);
        }

#endregion


#region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development
        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return SharedResources.SharedSerial.Connected;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                traceState = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
                comPortSpeed = driverProfile.GetValue(driverID, comPortProfileSpeed, string.Empty, comPortSpeedDefault);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                driverProfile.WriteValue(driverID, traceStateProfileName, traceState.ToString());
                driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
                driverProfile.WriteValue(driverID, comPortProfileSpeed, comPortSpeed.ToString());
            }
        }

#endregion


#region ASCOM Registration

        //// Register or unregister driver for ASCOM. This is harmless if already
        //// registered or unregistered. 
        ////
        ///// <summary>
        ///// Register or unregister the driver with the ASCOM Platform.
        ///// This is harmless if the driver is already registered/unregistered.
        ///// </summary>
        ///// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        //private static void RegUnregASCOM(bool bRegister)
        //{
        //    using (var P = new ASCOM.Utilities.Profile())
        //    {
        //        P.DeviceType = "Telescope";
        //        if (bRegister)
        //        {
        //            P.Register(driverID, driverDescription);
        //        }
        //        else
        //        {
        //            P.Unregister(driverID);
        //        }
        //    }
        //}

        ///// <summary>
        ///// This function registers the driver with the ASCOM Chooser and
        ///// is called automatically whenever this class is registered for COM Interop.
        ///// </summary>
        ///// <param name="t">Type of the class being registered, not used.</param>
        ///// <remarks>
        ///// This method typically runs in two distinct situations:
        ///// <list type="numbered">
        ///// <item>
        ///// In Visual Studio, when the project is successfully built.
        ///// For this to work correctly, the option <c>Register for COM Interop</c>
        ///// must be enabled in the project settings.
        ///// </item>
        ///// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        ///// </list>
        ///// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        ///// </remarks>
        //[ComRegisterFunction]
        //public static void RegisterASCOM(Type t)
        //{
        //    RegUnregASCOM(true);
        //}

        ///// <summary>
        ///// This function unregisters the driver from the ASCOM Chooser and
        ///// is called automatically whenever this class is unregistered from COM Interop.
        ///// </summary>
        ///// <param name="t">Type of the class being registered, not used.</param>
        ///// <remarks>
        ///// This method typically runs in two distinct situations:
        ///// <list type="numbered">
        ///// <item>
        ///// In Visual Studio, when the project is cleaned or prior to rebuilding.
        ///// For this to work correctly, the option <c>Register for COM Interop</c>
        ///// must be enabled in the project settings.
        ///// </item>
        ///// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        ///// </list>
        ///// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        ///// </remarks>
        //[ComUnregisterFunction]
        //public static void UnregisterASCOM(Type t)
        //{
        //    RegUnregASCOM(false);
        //}

#endregion

    }
}
