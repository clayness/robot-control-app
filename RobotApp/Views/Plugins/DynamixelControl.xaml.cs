﻿using System;
using System.IO.Ports;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using System.Timers;
using GalaSoft.MvvmLight.Messaging;
using ROBOTIS;

namespace RobotApp.Views.Plugins
{
    /// <summary>
    /// Interaction logic for DynamixelSlider.xaml
    /// </summary>
    public partial class DynamixelControl : PluginBase
    {
        // Control table address (for DXL Pro)
        public const int P_TORQUE_ENABLE = 562;
        public const int P_GOAL_POSITION_L = 596;
        public const int P_PRESENT_POSITION_L = 611;
        public const int P_GOAL_VELOCITY_L = 600;
        public const int P_PRESENT_VELOCITY_L = 615;
        public const int P_MOVING = 610;
        public const int P_GOAL_ACCEL_L = 606;
        public const int P_GOAL_TORQUE_L = 30;
        public const int P_LED_RED = 563;
        public const int P_LED_GREEN = 564;
        public const int P_LED_BLUE = 565;
        public int PortNumber = 0;
        public int oldmotor3Setpoint = 0;




        public SerialPort connectPort;
        public bool connected = false;


        int CommStatus;

        // Default settings
        //    public const int DEFAULT_PORT = 13;    // COM13
        public const int DEFAULT_BAUD = 57600;

        Timer errorTimer = new Timer(5000);

        public DynamixelControl()
        {
            this.DataContext = this;
            this.TypeName = "Dynamixel Control";
            InitializeComponent();
            errorTimer.Elapsed += ErrorTimer_Elapsed;           //constructor, starts timers, initializes ports and everything
            errorTimer.Start();
            string[] ports = SerialPort.GetPortNames();
            PortList = new List<string>(ports);

        }

        private void ErrorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ErrorText = "";
        }

        /// <summary>
        /// The <see cref="PortList" /> property's name.
        /// </summary>
        public const string PortListPropertyName = "PortList";

        private List<string> portList = null;

        /// <summary>
        /// Sets and gets the PortList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<string> PortList
        {
            get
            {
                return portList;
            }

            set
            {
                if (portList == value)
                {
                    return;
                }

                portList = value;
                RaisePropertyChanged(PortListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedPort" /> property's name.
        /// </summary>
        public const string SelectedPortPropertyName = "SelectedPort";

        private string selectedPort = null;

        /// <summary>
        /// Sets and gets the SelectedPort property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedPort
        {
            get
            {
                return selectedPort;
            }

            set
            {
                if (selectedPort == value)
                {
                    return;
                }

                selectedPort = value;
                RaisePropertyChanged(SelectedPortPropertyName);
                if (selectedPort != null)
                {
                    PortNumber = Convert.ToInt32(selectedPort.Remove(0, 3));

                }
                else
                {
                    PortNumber = 0;
                }

            }
        }

        private RelayCommand<string> detectCOMsCommand;

        /// <summary>
        /// Gets the DetectCOMsCommand.
        /// </summary>
        public RelayCommand<string> DetectCOMsCommand
        {
            get
            {
                return detectCOMsCommand
                    ?? (detectCOMsCommand = new RelayCommand<string>(
                    p =>
                    {
                        PortList = null;
                        string[] ports = SerialPort.GetPortNames();
                        PortList = new List<string>(ports);
                    }));
            }
        }

        private RelayCommand connectCommand;

        /// <summary>
        /// Gets the ResetHomePositionCommand.
        /// </summary>
        public RelayCommand ConnectCommand
        {
            get
            {
                return connectCommand
                    ?? (connectCommand = new RelayCommand(
                    () =>
                    {
                        if (!ConnectCommand.CanExecute(null))
                        {
                            return;
                        }
                        if (PortNumber == 0)
                        {
                            ErrorText = "No Port Selected";
                            ButtonText = "Connect to Dynamixel Pro";
                            return;
                        }
                        if (dynamixel.dxl_initialize(PortNumber, DEFAULT_BAUD) == 0)
                        {
                            ErrorText = "Failed to open USB2Dynamixel!";
                            return;
                        }
                        else
                        {
                            connected = true;
                            ButtonText = "Connected, Press to Home";
                        }
                        for (byte index = 1; index <= 3; index++)
                        {
                            dynamixel.dxl2_write_byte(index, P_TORQUE_ENABLE, 1);
                            CommStatus = dynamixel.dxl_get_comm_result();
                            if (CommStatus != dynamixel.COMM_RXSUCCESS)
                            {
                                ErrorText = "Failed to enable torque!";
                            }
                            dynamixel.dxl2_write_dword(index, P_GOAL_POSITION_L, 0);
                            dynamixel.dxl2_write_dword(1, P_GOAL_ACCEL_L, 1);
                            dynamixel.dxl2_write_dword(2, P_GOAL_ACCEL_L, 1);
                            dynamixel.dxl2_write_dword(3, P_GOAL_ACCEL_L, 10);
                            dynamixel.dxl2_write_byte(index, P_LED_BLUE, 255);
                            CommStatus = dynamixel.dxl_get_comm_result();
                            if (CommStatus != dynamixel.COMM_RXSUCCESS)
                            {
                                ErrorText = "Failed to connect to Dynamixel Pro";
                                ButtonText = "Connect to Dynamixel Pro";
                            }
                            Slider1Value = 0;
                            Slider2Value = 127;
                            Slider4Value = 0;

                        }
                    },
                    () => true));
            }
        }

        /// <summary>
        /// The <see cref="ButtonText" /> property's name.
        /// </summary>
        public const string ButtonTextPropertyName = "ButtonText";

        private string buttonText = "Connect to Dynamixel Pro";

        /// <summary>
        /// Sets and gets the ButtonText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ButtonText
        {
            get
            {
                return buttonText;
            }

            set
            {
                if (buttonText == value)
                {
                    return;
                }

                buttonText = value;
                RaisePropertyChanged(ButtonTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ErrorText" /> property's name.
        /// </summary>
        public const string ErrorTextPropertyName = "ErrorText";

        private string errorText = "";

        /// <summary>
        /// Sets and gets the ErrorText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ErrorText
        {
            get
            {
                return errorText;
            }

            set
            {
                if (errorText == value)
                {
                    return;
                }

                errorText = value;
                RaisePropertyChanged(ErrorTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Slider1Value" /> property's name.
        /// </summary>
        public const string Slider1ValuePropertyName = "Slider1Value";

        private double slider1Value = 0;

        /// <summary>
        /// Sets and gets the Slider1Value property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Slider1Value
        {
            get
            {
                return slider1Value;
            }

            set
            {
                if (slider1Value == value)
                {
                    return;
                }

                slider1Value = (value);
                RaisePropertyChanged(Slider1ValuePropertyName);

                if (connected)
                {
                    UpdateSetpoints();

                    CommStatus = dynamixel.dxl_get_comm_result();
                    if (CommStatus != dynamixel.COMM_RXSUCCESS)
                    {
                        ErrorText = "Failed to send Motor1 setpoint!";
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="Slider2Value" /> property's name.
        /// </summary>
        public const string Slider2ValuePropertyName = "Slider2Value";

        private double slider2Value = 127;

        /// <summary>
        /// Sets and gets the Slider1Value property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Slider2Value
        {
            get
            {
                return slider2Value;
            }

            set
            {
                if (slider2Value == value)
                {
                    return;
                }

                slider2Value = (value);
                RaisePropertyChanged(Slider2ValuePropertyName);

                if (connected)
                {
                    UpdateSetpoints();

                    CommStatus = dynamixel.dxl_get_comm_result();
                    if (CommStatus != dynamixel.COMM_RXSUCCESS)
                    {
                        ErrorText = "Failed to send Motor2 setpoint!";
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="Slider4Value" /> property's name.
        /// </summary>
        public const string Slider4ValuePropertyName = "Slider4Value";

        private double slider4Value = 0;

        /// <summary>
        /// Sets and gets the global::_SID_NAME_USE property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Slider4Value
        {
            get
            {
                return slider4Value;
            }

            set
            {
                if (slider4Value == value)
                {
                    return;
                }

                slider4Value = value;
                RaisePropertyChanged(Slider4ValuePropertyName);

                if (connected)
                {
                    UpdateSetpoints();

                    CommStatus = dynamixel.dxl_get_comm_result();
                    if (CommStatus != dynamixel.COMM_RXSUCCESS)
                    {
                        ErrorText = "Failed to send Motor3 setpoint!";
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="YawMinimum" /> property's name.
        /// </summary>
        public const string YawMinimumPropertyName = "YawMinimum";

        private double yawMinimum = -180;

        /// <summary>
        /// Sets and gets the YawMinimum property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double YawMinimum
        {
            get
            {
                return yawMinimum;
            }

            set
            {
                if (yawMinimum == value)
                {
                    return;
                }

                yawMinimum = value;
                RaisePropertyChanged(YawMinimumPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="YawMaximum" /> property's name.
        /// </summary>
        public const string YawMaximumPropertyName = "YawMaximum";

        private double yawMaximum = 180;

        /// <summary>
        /// Sets and gets the YawMaximum property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double YawMaximum
        {
            get
            {
                return yawMaximum;
            }

            set
            {
                if (yawMaximum == value)
                {
                    return;
                }

                yawMaximum = value;
                RaisePropertyChanged(YawMaximumPropertyName);
            }
        }

        private void UpdateSetpoints()
        {
            //global frame position
            double roll = (Slider1Value * (Math.PI / 180));
            double pitch = (Slider2Value * (Math.PI / 180));        
            double yaw = (Slider4Value * (Math.PI / 180));         

            //Euler Z-Y-Z Rotation matrix 
            double x = Math.Sin(pitch) * Math.Cos(roll);
            double y = Math.Sin(pitch) * Math.Sin(roll);
            double z = Math.Cos(pitch);

            double xx = Math.Cos(roll) * Math.Cos(pitch) * Math.Cos(yaw) - Math.Sin(roll) * Math.Sin(yaw);
            double xy = Math.Sin(roll) * Math.Cos(pitch) * Math.Cos(yaw) + Math.Cos(roll) * Math.Sin(yaw);
            double xz = -Math.Sin(pitch) * Math.Cos(yaw);

            double yx = -Math.Cos(roll) * Math.Cos(pitch) * Math.Sin(yaw) - Math.Sin(roll) * Math.Cos(yaw);
            double yy = -Math.Sin(roll) * Math.Cos(pitch) * Math.Sin(yaw) + Math.Cos(roll) * Math.Cos(yaw);
            double yz = Math.Sin(pitch) * Math.Sin(yaw);

            //angles between motor axes
            double alpha13 = 75 * (Math.PI / 180);
            double alpha35 = 52 * (Math.PI / 180);

            //Solve for 2nd Motor angle
            double cTheta3 = -(z - Math.Cos(alpha13) * Math.Cos(alpha35)) / (Math.Sin(alpha13) * Math.Sin(alpha35));
            double cTheta31 = Math.Sqrt(1 - Math.Pow(cTheta3, 2));
            double Theta3 = Math.Atan2(cTheta31, cTheta3);

            //Solve for 1st Motor angle
            double cTheta1 = -(Math.Cos(alpha35) * Math.Sin(alpha13) * y - Math.Sin(Theta3) * Math.Sin(alpha35) * x + Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Sin(alpha35) * y) / (Math.Pow(Math.Cos(Theta3) , 2) * Math.Pow(Math.Cos(alpha13) , 2) * Math.Pow(Math.Sin(alpha35) , 2) + 2 * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(alpha35) , 2) * Math.Pow(Math.Sin(alpha13) , 2) + Math.Pow(Math.Sin(Theta3) , 2) * Math.Pow(Math.Sin(alpha35) , 2));
            double sTheta1 = (Math.Cos(alpha35) * Math.Sin(alpha13) * x + Math.Sin(Theta3) * Math.Sin(alpha35) * y + Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Sin(alpha35) * x) / (Math.Pow(Math.Cos(Theta3), 2) * Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Sin(alpha35), 2) + 2 * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(alpha35), 2) * Math.Pow(Math.Sin(alpha13), 2) + Math.Pow(Math.Sin(Theta3), 2) * Math.Pow(Math.Sin(alpha35), 2));
            double Theta1 = Math.Atan2(sTheta1, cTheta1);

            //Solve for yaw=0, This allows correction for the 3rd motors max and min position, so it can reach a full 360 degree range
            double yawFactor = 0;
            double xx0 = Math.Cos(roll) * Math.Cos(pitch) * Math.Cos(yawFactor) - Math.Sin(roll) * Math.Sin(yawFactor);
            double yx0 = -Math.Cos(roll) * Math.Cos(pitch) * Math.Sin(yawFactor) - Math.Sin(roll) * Math.Cos(yawFactor);
            double cTheta50 = -(Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta3) * yx0 - Math.Cos(Theta1) * Math.Cos(Theta3) * xx0 + Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) * xx0 - Math.Sin(Theta1) * Math.Sin(alpha13) * Math.Sin(alpha35) * yx0 + Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Sin(Theta1) * yx0) / ((Math.Pow(Math.Cos(Theta1), 2)) * (Math.Pow(Math.Cos(Theta3), 2)) + Math.Pow(Math.Cos(Theta1), 2) * (Math.Pow(Math.Cos(alpha35), 2)) * Math.Pow(Math.Sin(Theta3), 2) + 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Pow(Math.Cos(alpha35), 2) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta1) * Math.Sin(Theta3) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(Theta3), 2) * Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Cos(alpha35), 2) * Math.Pow(Math.Sin(Theta1), 2) - 2 * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Pow(Math.Sin(Theta1), 2) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(Theta3), 2) + Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(alpha13), 2) * Math.Pow(Math.Sin(alpha35), 2));
            double sTheta50 = -(Math.Cos(Theta1) * Math.Cos(Theta3) * yx0 + Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta3) * xx0 - Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) * yx0 - Math.Sin(Theta1) * Math.Sin(alpha13) * Math.Sin(alpha35) * xx0 + Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Sin(Theta1) * xx0) / ((Math.Pow(Math.Cos(Theta1), 2)) * (Math.Pow(Math.Cos(Theta3), 2)) + Math.Pow(Math.Cos(Theta1), 2) * (Math.Pow(Math.Cos(alpha35), 2)) * Math.Pow(Math.Sin(Theta3), 2) + 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Pow(Math.Cos(alpha35), 2) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta1) * Math.Sin(Theta3) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(Theta3), 2) * Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Cos(alpha35), 2) * Math.Pow(Math.Sin(Theta1), 2) - 2 * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Pow(Math.Sin(Theta1), 2) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(Theta3), 2) + Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(alpha13), 2) * Math.Pow(Math.Sin(alpha35), 2));
            double Theta50 = Math.Atan2(cTheta50, sTheta50);

            double yawOffset = (Theta50 * 180 / Math.PI) + 180;
            YawMinimum = -180 + yawOffset + 1;
            YawMaximum = 180 + yawOffset - 1;

            //Solve for 3rd Motor angle
            double cTheta5 = -(Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta3) * yx - Math.Cos(Theta1) * Math.Cos(Theta3) * xx + Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) * xx - Math.Sin(Theta1) * Math.Sin(alpha13) * Math.Sin(alpha35) * yx + Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Sin(Theta1) * yx) / ((Math.Pow(Math.Cos(Theta1), 2)) * (Math.Pow(Math.Cos(Theta3), 2)) + Math.Pow(Math.Cos(Theta1), 2) * (Math.Pow(Math.Cos(alpha35), 2)) * Math.Pow(Math.Sin(Theta3), 2) + 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Pow(Math.Cos(alpha35), 2) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta1) * Math.Sin(Theta3) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(Theta3), 2) * Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Cos(alpha35), 2) * Math.Pow(Math.Sin(Theta1), 2) - 2 * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Pow(Math.Sin(Theta1), 2) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(Theta3), 2) + Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(alpha13), 2) * Math.Pow(Math.Sin(alpha35), 2));
            double sTheta5 = -(Math.Cos(Theta1) * Math.Cos(Theta3) * yx + Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta3) * xx - Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) * yx - Math.Sin(Theta1) * Math.Sin(alpha13) * Math.Sin(alpha35) * xx + Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Sin(Theta1) * xx) / ((Math.Pow(Math.Cos(Theta1), 2)) * (Math.Pow(Math.Cos(Theta3), 2)) + Math.Pow(Math.Cos(Theta1), 2) * (Math.Pow(Math.Cos(alpha35), 2)) * Math.Pow(Math.Sin(Theta3), 2) + 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Pow(Math.Cos(alpha35), 2) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Sin(Theta1) * Math.Sin(Theta3) - 2 * Math.Cos(Theta1) * Math.Cos(alpha35) * Math.Sin(Theta1) * Math.Sin(Theta3) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(Theta3), 2) * Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Cos(alpha35), 2) * Math.Pow(Math.Sin(Theta1), 2) - 2 * Math.Cos(Theta3) * Math.Cos(alpha13) * Math.Cos(alpha35) * Math.Pow(Math.Sin(Theta1), 2) * Math.Sin(alpha13) * Math.Sin(alpha35) + Math.Pow(Math.Cos(alpha13), 2) * Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(Theta3), 2) + Math.Pow(Math.Sin(Theta1), 2) * Math.Pow(Math.Sin(alpha13), 2) * Math.Pow(Math.Sin(alpha35), 2));
            double Theta5 = Math.Atan2(cTheta5, sTheta5);

            //convert to motor setpoints
            int motor1Setpoint = -(int)Math.Round((((Theta1*180/Math.PI)-90) * 251000 / 180));
            int motor2Setpoint = (int)Math.Round((Theta3*180/Math.PI) * 151875 / 180);
            int motor3Setpoint = -(int)Math.Round(((Theta5 * 180 / Math.PI) + 180) * 151875 / 180);

            //correction if math sends it to a position out of range
            if (motor3Setpoint < -151875)
                motor3Setpoint += 151875 * 2;
            else if (motor3Setpoint > 151875)
                motor3Setpoint -= 151875 * 2;

            //If motor 3 setpoint is 180 degrees from the last point (flipping around) then it holds it's old position.
            int SetDif = 0;
            if (oldmotor3Setpoint != motor3Setpoint)
            {
                SetDif = Math.Abs(oldmotor3Setpoint - motor3Setpoint);
                if (SetDif > 150000)
                    motor3Setpoint = oldmotor3Setpoint;
                else
                    oldmotor3Setpoint = motor3Setpoint;
            }

            //Write
            dynamixel.dxl2_write_dword(1, P_GOAL_POSITION_L, (UInt32)motor1Setpoint);
            dynamixel.dxl2_write_dword(2, P_GOAL_POSITION_L, (UInt32)motor2Setpoint);
            dynamixel.dxl2_write_dword(3, P_GOAL_POSITION_L, (UInt32)motor3Setpoint);
        }



        /// <summary>
        /// The <see cref="Torque1" /> property's name.
        /// </summary>
        public const string Torque1PropertyName = "Torque1";

        private bool torque1 = true;

        /// <summary>
        /// Sets and gets the Torque1 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Torque1
        {
            get
            {
                return torque1;
            }

            set
            {
                if (torque1 == value)
                {
                    return;
                }

                torque1 = value;
                RaisePropertyChanged(Torque1PropertyName);
                dynamixel.dxl2_write_byte(1, P_TORQUE_ENABLE, Convert.ToByte(torque1));
                CommStatus = dynamixel.dxl_get_comm_result();
                if (CommStatus != dynamixel.COMM_RXSUCCESS)
                {
                    ErrorText = "Failed to disable torque1!";
                    Torque1 = true;
                }
            }
        }
        /// <summary>
        /// The <see cref="Torque2" /> property's name.
        /// </summary>
        public const string Torque2PropertyName = "Torque2";

        private bool torque2 = true;

        /// <summary>
        /// Sets and gets the Torque2 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Torque2
        {
            get
            {
                return torque2;
            }

            set
            {
                if (torque2 == value)
                {
                    return;
                }

                torque2 = value;
                RaisePropertyChanged(Torque2PropertyName);
                dynamixel.dxl2_write_byte(2, P_TORQUE_ENABLE, Convert.ToByte(torque2));
                CommStatus = dynamixel.dxl_get_comm_result();
                if (CommStatus != dynamixel.COMM_RXSUCCESS)
                {
                    ErrorText = "Failed to disable torque2!";
                    Torque2 = true;
                }
            }
        }
        /// <summary>
        /// The <see cref="Torque3" /> property's name.
        /// </summary>
        public const string Torque3PropertyName = "Torque3";

        private bool torque3 = true;

        /// <summary>
        /// Sets and gets the Torque3 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Torque3
        {
            get
            {
                return torque3;
            }

            set
            {
                if (torque3 == value)
                {
                    return;
                }

                torque3 = value;
                RaisePropertyChanged(Torque3PropertyName);
                dynamixel.dxl2_write_byte(3, P_TORQUE_ENABLE, Convert.ToByte(torque3));
                CommStatus = dynamixel.dxl_get_comm_result();
                if (CommStatus != dynamixel.COMM_RXSUCCESS)
                {
                    ErrorText = "Failed to disable torque3!";
                    Torque3 = true;
                }
            }
        }

    }
}
