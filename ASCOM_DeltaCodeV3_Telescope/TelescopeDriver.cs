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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;


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
    [ServedClassName("DeltaCode V3 Telescope")]
    [ClassInterface(ClassInterfaceType.None)]

    public class Telescope : ReferenceCountedObjectBase, ITelescopeV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        /// 
        internal static string driverID;

        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        /// 
        private static string driverDescription = "DeltaCode V3 Telescope";

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

        private static string m_cLogPrefix = "T:";


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
            //MessageBox.Show("Telescope Constructor");
            driverID = Marshal.GenerateProgIdForType(this.GetType());

            //  Read device configuration from the ASCOM Profile store
            //  
            SharedResources.ReadProfile();

            tl = SharedResources.TraceLogger;
            LogMessage("Telescope", "Starting initialisation");

            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro utilities object

            LogMessage("Telescope", "Completed initialisation");
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
            SharedResources.SetupDialog();
        }


        /// <summary>
        /// Get List of supported Actions for the AscomDriver.Action Method
        /// </summary>
        /// 
        public ArrayList SupportedActions
        {
            get
            {
                LogMessage("SupportedActions Get", "Returning empty arraylist");
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
            LogMessage("Action {0}, parameters {1} not implemented", actionName);
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
            SharedResources.CommandBlind(command, raw);
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
            return SharedResources.CommandBool (m_cLogPrefix, command, raw);
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
            return SharedResources.CommandString (m_cLogPrefix, command, raw);
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
                bool bIsConnected = SharedResources.Connected;
                LogMessage("Connected Get", bIsConnected.ToString());
                return bIsConnected;
            }
            set
            {
                LogMessage("Connected Set", value.ToString());
                SharedResources.Connected = value;

                if (value)
                {
                    //  Ask version number
                    //
                    string cProductName           = CommandString(":GVP", false);
                           cProductName           = CommandString(":GVP", false);
                    string cFirmwareVersion       = CommandString(":GVF", false);
                    string cFirmwareVersionNumber = CommandString(":GVN", false);
                    string cFirmwareVersionDate   = CommandString(":GVD", false);
                    string cFirmwareVersionTime   = CommandString(":GVT", false);

                    LogMessage("Connected Set", "Product Name...........: " + cProductName);
                    LogMessage("Connected Set", "Firmware Version.......: " + cFirmwareVersion);
                    LogMessage("Connected Set", "Firmware Version Number: " + cFirmwareVersionNumber);
                    LogMessage("Connected Set", "Firmware Version Date..: " + cFirmwareVersionDate);
                    LogMessage("Connected Set", "Firmware Version Time..: " + cFirmwareVersionTime);

                    //  Set coordinates to high precision
                    //  First ask for right ascension (:GR#) then check the result.
                    //  If the result is short, switch to high precision coordinates by the :U# command
                    //  (toggle High Precision Flag)
                    //
                    string cRightAscension;
                    cRightAscension = CommandString(":GR", false);
                    cRightAscension = CommandString(":GR", false);
                    if (cRightAscension.Length <= 8)
                    {
                        CommandBlind(":U", false);
                    }
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
                LogMessage("Description Get", driverDescription);
                return driverDescription;
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
                string driverInfo = "ASCOM Driver for DeltaCodeV3 Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", version.Major, version.Minor, version.Revision);

                LogMessage("DriverInfo Get", driverInfo);

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

                LogMessage("DriverVersion Get", driverVersion);

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
                LogMessage("InterfaceVersion Get", "3");
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
                LogMessage("Name Get", name);
                return name;
            }
        }

#endregion


#region ITelescope Implementation

        public void AbortSlew()
        {
            LogMessage("AbortSlew", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("AbortSlew");
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                LogMessage("AlignmentMode Get", "algGermanPolar");
                return AlignmentModes.algGermanPolar;
            }
        }

        public double Altitude
        {
            get
            {
                LogMessage("Altitude", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public double ApertureArea
        {
            get
            {
                LogMessage("ApertureArea Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                LogMessage("ApertureDiameter Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                LogMessage("AtHome", "Get - " + false.ToString());
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
                
                LogMessage("AtPark", "Get - " + bAtPark.ToString());
                return bAtPark;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            LogMessage("AxisRates", "Get - " + Axis.ToString());
            return new AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                LogMessage("Azimuth Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
            }
        }

        public bool CanFindHome
        {
            get
            {
                LogMessage("CanFindHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {

            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                    LogMessage("CanMoveAxis", "Get[axisPrimary] - " + true.ToString());
                    return true;
                case TelescopeAxes.axisSecondary:
                    LogMessage("CanMoveAxis", "Get[axisSecondary] - " + true.ToString());
                    return true;
                case TelescopeAxes.axisTertiary:
                    LogMessage("CanMoveAxis", "Get[axisTertiary] - " + false.ToString());
                    return false;
                default:
                    throw new InvalidValueException("CanMoveAxis", Axis.ToString(), "0 to 2");
            }
        }

        public bool CanPark
        {
            get
            {
                CheckConnected("CanPark.get");

                string cCanPark = CommandString(":h?", false);

                if (cCanPark == "1")
                {
                    LogMessage("CanPark", "Get - " + true.ToString());
                    return true;
                }
                else { 
                    LogMessage("CanPark", "Get - " + false.ToString());
                    return false;
                }
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                LogMessage("CanPulseGuide", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                LogMessage("CanSetDeclinationRate", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                LogMessage("CanSetGuideRates", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetPark
        {
            get
            {
                LogMessage("CanSetPark", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                LogMessage("CanSetPierSide", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                LogMessage("CanSetRightAscensionRate", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                LogMessage("CanSetTracking", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlew
        {
            get
            {
                LogMessage("CanSlew", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                LogMessage("CanSlewAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                LogMessage("CanSlewAltAzAsync", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                LogMessage("CanSlewAsync", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                LogMessage("CanSync", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                LogMessage("CanSyncAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanUnpark
        {
            get
            {
                CheckConnected("CanUnpark.get");

                string cCanPark = CommandString(":h?", false);

                if (cCanPark == "1")
                {
                    LogMessage("CanUnpark", "Get - " + true.ToString());
                    return true;
                }
                else
                {
                    LogMessage("CanUnpark", "Get - " + false.ToString());
                    return false;
                }
            }
        }

        public double Declination
        {
            get
            {
                CheckConnected("Declination.get");

                string cDeclination = CommandString(":GD", false);
                double fDeclination = utilities.DMSToDegrees(cDeclination);

                LogMessage("Declination", "Get - " + utilities.HoursToHMS(fDeclination));
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
                string cDecRate_MilliArcsecPerMinute = CommandString(":GDr", false);
                cDecRate_MilliArcsecPerMinute = cDecRate_MilliArcsecPerMinute.TrimEnd(new char[] { '#' });
                int nDecRate_MilliArcsecPerMinute = Convert.ToInt32(cDecRate_MilliArcsecPerMinute);

                m_fDeclinationRate = (nDecRate_MilliArcsecPerMinute) / (1000.0 * 60.0);

                LogMessage("DeclinationRate", "Get - " + m_fDeclinationRate.ToString());
                return m_fDeclinationRate;
            }
            set
            {

                int nDecRate_MilliArcsecPerMinute = (int)(value * 1000.0 * 60.0);
                string cDecRate = nDecRate_MilliArcsecPerMinute.ToString();
                CommandBlind(":Sdr" + cDecRate, false);

                LogMessage("DeclinationRate Set", "Set + " + value.ToString());

                m_fDeclinationRate = value;
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            LogMessage("DestinationSideOfPier Get", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("DestinationSideOfPier");
        }

        public bool DoesRefraction
        {
            get
            {
                LogMessage("DoesRefraction", "Get - " + false.ToString());
                return false;
            }
            set
            {
                LogMessage("DoesRefraction Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equJ2000;
                LogMessage("EquatorialSystem", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            LogMessage("FindHome", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("FindHome");
        }

        public double FocalLength
        {
            get
            {
                LogMessage("FocalLength Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        //TODO
        public double GuideRateDeclination
        {
            get
            {
                CheckConnected("GuideRateDeclination.get");

                string cRate = CommandString(":Ggui", false);
                double fRate;

                if (Double.TryParse(cRate, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out fRate))
                {
                    LogMessage("GuideRateDeclination", "Get - " + fRate.ToString());
                    return fRate;
                }
                else
                {
                    LogMessage("GuideRateDeclination Get", "Not implemented");
                    throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", false);
                }
            }
            set
            {
                LogMessage("GuideRateDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                CheckConnected("GuideRateRightAscension.get");

                string cRate = CommandString(":Ggui", false);
                double fRate;

                if (Double.TryParse(cRate, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out fRate))
                {
                    LogMessage("GuideRateRightAscension", "Get - " + fRate.ToString());
                    return fRate;
                }
                else
                {
                    LogMessage("GuideRateRightAscension Get", "Not implemented");
                    throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
                }
            }
            set
            {
                LogMessage("GuideRateRightAscension Set", "Not implemented");
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

                LogMessage("CanSlewAsync", "Get - " + bIsPulseGuiding.ToString());
                return bIsPulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                throw new ASCOM.InvalidValueException("MoveAxis");
            }

            LogMessage("MoveAxis", "OK");

            AxisRates oRates = new AxisRates(Axis);

            bool bNegative = (Rate < 0.0) ? true : false;
            Rate = Math.Abs(Rate);

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
            string cCanPark = CommandString(":h?", false);

            if (cCanPark == "1")
            {
                LogMessage("Park", "OK");
                CommandBlind(":hP", false);

                //  give DeltaCode time to  calculate Parked status
                //
                Thread.Sleep(500);
            }
            else
            {
                LogMessage("Park", "Not implemented");
                throw new ASCOM.MethodNotImplementedException("Park");
            }
        }


        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                throw new ASCOM.InvalidValueException("MoveAxis");
            }

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


            LogMessage("PulseGuide", "OK");
        }


        public double RightAscension
        {
            get
            {
                CheckConnected("RightAscension.get");

                string cRightAscension = CommandString(":GR", false);
                double fRightAscension = utilities.HMSToHours (cRightAscension);

                LogMessage("RightAscension", "Get - " + utilities.HoursToHMS(fRightAscension));
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

                string cRaRate_MilliArcsecPerMinute = CommandString(":GRr", false);
                cRaRate_MilliArcsecPerMinute = cRaRate_MilliArcsecPerMinute.TrimEnd(new char[] { '#' });
                int nRaRate_MilliArcsecPerMinute = Convert.ToInt32(cRaRate_MilliArcsecPerMinute);

                m_fRightascensionRate = (nRaRate_MilliArcsecPerMinute) / (15.0 * 1000.0 * 60.0);

                LogMessage("RightAscensionRate", "Get - " + m_fRightascensionRate.ToString());
                return m_fRightascensionRate;
            }
            set
            {

                int nRaRate_MilliArcsecPerMinute = (int)(value * 15.0 * 1000.0 * 60.0);
                string cRaRate = nRaRate_MilliArcsecPerMinute.ToString();
                CommandBlind(":Srr" + cRaRate, false);

                LogMessage("RightAscensionRate", "Set + " + m_fRightascensionRate.ToString());

                m_fRightascensionRate = value;
            }
        }


        public void SetPark()
        {
            LogMessage("SetPark", "OK");
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

                string cPS;
                PierSide ps;

                cPS = CommandString(":pS", false);
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
                LogMessage("SideOfPier", "Get - " + ps.ToString());
                return ps;
            }
            set
            {
                LogMessage("SideOfPier Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
            }
        }

        public double SiderealTime
        {
            get
            {
                CheckConnected("SiderealTime.get");

                string cSiderealTime = CommandString(":GS", false);
                double fSiderealTime = utilities.HMSToHours(cSiderealTime);

                LogMessage("SiderealTime", "Get - " + utilities.HoursToHMS(fSiderealTime));
                return fSiderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                LogMessage("SiteElevation Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", false);
            }
            set
            {
                LogMessage("SiteElevation Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", true);
            }
        }


        //TODO
        public double SiteLatitude
        {
            get //  :Gt#
            {
                CheckConnected("SiteLatitude.get");

                string cLat = CommandString(":Gt", false);
                double fLat = utilities.HMSToHours(cLat);

                LogMessage("SiteLatitude", "Get - " + fLat.ToString());
                return fLat;
            }
            set //  :StsDD*MM:SS#
            {
                LogMessage("SiteLatitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLatitude", true);
            }
        }


        //TODO
        public double SiteLongitude //:Gg#
        {
            get //:Gg#
            {
                CheckConnected("SiteLongitude.get");

                string cLong = CommandString(":Gg", false);
                double fLong = utilities.HMSToHours(cLong);

                LogMessage("SiteLongitude", "Get - " + fLong.ToString());
                return fLong;
            }
            set //  :SgsDDD*MM:SS#
            {
                LogMessage("SiteLongitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLongitude", true);
            }
        }

        public short SlewSettleTime
        {
            get
            {
                LogMessage("SlewSettleTime Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", false);
            }
            set
            {
                LogMessage("SlewSettleTime Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", true);
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                LogMessage("SlewToCoordinates", "ERROR : Mount is parked");
                throw new ASCOM.InvalidOperationException("SlewToCoordinates");
            }

            LogMessage("SlewToCoordinates", "OK");
            SlewToCoordinatesAsync (RightAscension, Declination);

            while (true)
            {
                Thread.Sleep(500);
                if (!Slewing)
                {
                    break;
                }
            }
        }

        public void SlewToCoordinatesAsync(double fRightAscension, double fDeclination)
        {
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                LogMessage("SlewToCoordinatesAsync", "ERROR : Mount is parked");
                throw new ASCOM.InvalidOperationException("SlewToCoordinatesAsync");
            }

            LogMessage("SlewToCoordinatesAsync", "OK");

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
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                LogMessage("SlewToTarget", "ERROR : Mount is parked");
                throw new ASCOM.InvalidOperationException("SlewToTarget");
            }

            LogMessage("SlewToTarget", "OK");
            SlewToTargetAsync();

            while (true)
            {
                Thread.Sleep(500);
                if (!Slewing)
                {
                    break;
                }
            }
        }

        public void SlewToTargetAsync()
        {
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                LogMessage("SlewToTargetAsync", "ERROR : Mount is parked");
                throw new ASCOM.InvalidOperationException("SlewToTargetAsync");
            }

            LogMessage("SlewToTargetAsync", "OK");

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

                if (!bSlewing)
                {
                    string cResponse = CommandString(":D", false);
                    if (cResponse.Length > 1)
                    {
                        bSlewing = true;
                    }
                }
                LogMessage("Slewing", "Get - " + bSlewing.ToString());
                return bSlewing;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double fRightAscension, double fDeclination)
        {
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                LogMessage("SyncToCoordinates", "ERROR : Mount is parked");
                throw new ASCOM.InvalidOperationException("SyncToCoordinates");
            }

            LogMessage("SyncToCoordinates", "OK");

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
            // Check if mount is parked
            //
            string cMountStatus = CommandString(":Gstat", false);
            bool bAtPark = cMountStatus == "5" ? true : false;

            if (bAtPark)
            {
                LogMessage("SyncToTarget", "ERROR : Mount is parked");
                throw new ASCOM.InvalidOperationException("SyncToTarget");
            }

            string cRightAscension;
            string cDeclination;

            cRightAscension = utilities.HoursToHMS(m_fTargetRightascension);
            cDeclination    = utilities.HoursToHMS(m_fTargetDeclination);

            LogMessage("SyncToTarget", "RA=" + cRightAscension + " / Dec=" + cDeclination);

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
                    LogMessage("TargetDeclination", "Get - " + utilities.HoursToHMS(m_fTargetDeclination));
                    return m_fTargetDeclination;
                }
                else
                {
                    LogMessage("TargetDeclination", "Get - Value not set");
                    throw new ASCOM.ValueNotSetException ("TargetDeclination");
                }
            }
            set
            {
                if (value >= -90.0 && value <= 90.0)
                {
                    LogMessage("TargetDeclination", "Set - " + utilities.HoursToHMS(value));
                    m_fTargetDeclination = value;
                    m_bTargetDeclinationSet = true;
                }
                else
                {
                    LogMessage("TargetDeclination", "Set - Invalid value" + utilities.HoursToHMS(value));
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
                    LogMessage("TargetRightAscension", "Get - " + utilities.HoursToHMS(m_fTargetRightascension));
                    return m_fTargetRightascension;
                }
                else
                {
                    LogMessage("TargetRightAscension", "Get - Value not set");
                    throw new ASCOM.ValueNotSetException("TargetRightAscension");
                }
            }
            set
            {
                if (value >= 0.0 && value <= 24.0)
                {
                    LogMessage("TargetRightAscension", "Set - " + utilities.HoursToHMS(value));
                    m_fTargetRightascension = value;
                    m_bTargetRightascensionSet = true;
                }
                else
                {
                    LogMessage("TargetRightAscension", "Set - Invalid value" + utilities.HoursToHMS(value));
                    throw new ASCOM.InvalidValueException("TargetRightAscension", value.ToString(), "0 to 24");
                }
            }
        }

        public bool Tracking
        {
            get
            {
                bool tracking = true;
                LogMessage("Tracking", "Get - " + tracking.ToString());
                return tracking;
            }
            set
            {
                LogMessage("Tracking Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Tracking", true);
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                LogMessage("TrackingRate", "Get - " + DriveRates.driveSidereal.ToString());
                return DriveRates.driveSidereal;
            }
            set
            {
                LogMessage("TrackingRate Set", "OK");

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
                    LogMessage("TrackingRates", "Get - " + driveRate.ToString());
                }
                return trackingRates;
            }
            set
            {
                LogMessage("TrackingRates Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TrackingRates", true);
            }
        }

        public DateTime UTCDate
        {
            get
            {
                DateTime utcDate = DateTime.UtcNow;
                LogMessage("UTCDate", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
                return utcDate;
            }
            set
            {
                LogMessage("UTCDate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("UTCDate", true);
            }
        }

        public void Unpark()
        {
            string cCanPark = CommandString(":h?", false);

            if (cCanPark == "1")
            {
                LogMessage("Unpark", "OK");
                CommandBlind(":PO", false);

                //  give DeltaCode time to  calculate Parked status
                //
                Thread.Sleep(500);
            }
            else
            {
                LogMessage("Unpark", "Not implemented");
                throw new ASCOM.MethodNotImplementedException("Unpark");
            }
        }

#endregion


#region Private properties and methods

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
        /// 
        private void LogMessage(string identifier, string message)
        {
            tl.LogMessage(m_cLogPrefix + identifier, message);
        }


        private void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(m_cLogPrefix + identifier, msg);
        }

        #endregion



    }
}
