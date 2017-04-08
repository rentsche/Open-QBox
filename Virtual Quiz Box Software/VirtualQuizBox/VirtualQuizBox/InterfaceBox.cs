using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualQuizBox
{
    public class Pad
    {
        private bool isConnected = false;
        private bool isClosed = false;

        /// <summary>
        /// IsClosed is true when a switch connected to the input corresponding with this pad on the interface box is closed
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return isClosed;
            }
            set
            {
                isClosed = value;
            }
        }

        /// <summary>
        /// isConnected is true if a switch connected to the input corresponding with this pad on the interface box is closed at any time during operation
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                isConnected = value;
            }
        }

        public Pad()
        {

        }
    }

    public class Port
    {
        private bool isConnected = false;
        private List<Pad> pads;

        /// <summary>
        /// IsConnected is true when a switch connected to the input corresponding with any pad connected to this port is closed
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                isConnected = value;
            }
        }

        /// <summary>
        /// Collection of Pad objects associated with this port
        /// </summary>
        public List<Pad> Pads
        {
            get
            {
                return pads;
            }

            set
            {
                pads = value;
            }
        }

        public Port()
        {
            // Initialize list of pads
            pads = new List<Pad>(5);
            for (int i = 0; i <= 4; i++)
            {
                pads.Add(new Pad());
            }
        }
    }

    public class InterfaceBoxState
    {
        private bool isConnected = false;
        private bool isArmed = false;
        private bool armTriggered = false;
        private List<Port> ports;

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                isConnected = value;
            }
        }

        public bool IsArmed
        {
            get
            {
                return isArmed;
            }

            set
            {
                isArmed = value;
            }
        }

        public bool ArmTriggered
        {
            get
            {
                return armTriggered;
            }

            set
            {
                armTriggered = value;
            }
        }

        public List<Port> Ports
        {
            get
            {
                return ports;
            }

            set
            {
                ports = value;
            }
        }

        public InterfaceBoxState()
        {
            // Initialize list of ports
            ports = new List<Port>();
            for (int i = 0; i <= 2; i++)
            {
                ports.Add(new Port());
            }
        }
    }

    public class InterfaceBox
    {
        public delegate void StateChangeHandler(InterfaceBoxState newState);
        public event StateChangeHandler StateUpdated;
        public event StateChangeHandler ArmTriggered;

        private InterfaceBoxState state;

        private System.IO.Ports.SerialPort serialPort;

        public bool IsArmed
        {
            get
            {
                return state.IsArmed;
            }
        }

        public bool IsConnected
        {
            get
            {
                return state.IsConnected;
            }
        }

        public InterfaceBoxState State
        {
            get
            {
                return state;
            }
        }

        public InterfaceBox()
        {
            // Initialize InterfaceBoxState object
            state = new InterfaceBoxState();
            
            // Create serialPort connection
            serialPort = new System.IO.Ports.SerialPort();
            serialPort.BaudRate = 250000;
            serialPort.PortName = "COM4";
            serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort_DataReceived);
        }
       
        /// <summary>
        /// Attempts to establish a connection with the hardware
        /// </summary>
        public void Connect()
        {
            try
            {
                serialPort.Open();
                state.IsConnected = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Attempts to send and 'arm' command to the interfacebox
        /// </summary>
        public void Arm()
        {
            State.IsArmed = true;

            try
            {
                serialPort.WriteLine("a");
            }
            catch (Exception ex)
            {
                state.IsConnected = false;
                throw ex;
            }
        }

        /// <summary>
        /// Attempts to send a refresh command to the interfacebox. Will result in a StateUpdated event if successful.
        /// </summary>
        public void Refresh()
        {
            try
            {
                serialPort.WriteLine("r");
            }
            catch (Exception ex)
            {
                state.IsConnected = false;
                throw ex;
            }
        }

        /// <summary>
        /// Closes the serial port and disposes of resources
        /// </summary>
        public void dispose()
        {
            serialPort.Close();
            serialPort.Dispose();
        }

        // Handles new data received from the serial port.
        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            String newText = serialPort.ReadLine();
            newText = newText.Trim();
            int inputNum = Convert.ToInt32(newText);
            updatePadStates(inputNum);           
        }

        private void updatePadStates(Int32 input)
        {
            // Bitwise complement the two bytes to compensate for pullup resistors
            Int32 inputComp = ~input;           

            int bitMaskPosition = 0;           
            foreach (Port currentPort in state.Ports) 
            {               
                foreach (Pad currentPad in currentPort.Pads) 
                {
                    if ((Convert.ToInt32(Math.Pow(2, bitMaskPosition)) & inputComp) > 0)
                    {
                        // Pad connected to this input on the interfaceBox is closed so update related state variables
                        currentPad.IsClosed = true;
                        currentPad.IsConnected = true;
                        currentPort.IsConnected = true;
                    }
                    else
                    {
                        currentPad.IsClosed = false;
                    }

                    bitMaskPosition++;
                }
            }

            // Mask off the 16th bit which designates whether the 'arm' flag has been tripped, if set
            if ((input & 0x8000) > 0)
            {
                // The 'arm' flag has been tripped on the box
                state.IsArmed = false;
                state.ArmTriggered = true;             
            }
            else
            {
                state.ArmTriggered = false;
            }

            // Raise a StateUpdated event
            StateUpdated(state);

        }   
     

    }
}
