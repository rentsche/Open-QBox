using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VirtualQuizBox
{
    
    public partial class Form1 : Form
    {
        // This delegate enables asynchronous calls for updating the form's controls
        delegate void UpdatePadStateCallback(InterfaceBoxState newState);

        int time = 0;
        bool lockedOut = false;

        InterfaceBox interfaceBox;

        System.Media.SoundPlayer player;
        String soundFilePath = @"../../beep.wav";
        bool soundFileLoaded = false;

        // Temp system for organizing UIControls
        List<System.Windows.Forms.RadioButton> radioButtonList;
        List<System.Windows.Forms.CheckBox> CheckBoxList;


        public Form1()
        {
            InitializeComponent();
            interfaceBox = new InterfaceBox();
            interfaceBox.StateUpdated += new InterfaceBox.StateChangeHandler(interfaceBox_StateUpdated);
            InitializeSoundPlayer();

            CommTimer.Interval = 10;
            UITimer.Interval = 1000;

            // Temp system for organizing UIControls
            radioButtonList = new List<RadioButton>();
            radioButtonList.Add(radioButton1);
            radioButtonList.Add(radioButton2);
            radioButtonList.Add(radioButton3);
            radioButtonList.Add(radioButton4);
            radioButtonList.Add(radioButton5);
            radioButtonList.Add(radioButton6);
            radioButtonList.Add(radioButton7);
            radioButtonList.Add(radioButton8);
            radioButtonList.Add(radioButton9);
            radioButtonList.Add(radioButton10);
            radioButtonList.Add(radioButton11);
            radioButtonList.Add(radioButton12);
            radioButtonList.Add(radioButton13);
            radioButtonList.Add(radioButton14);
            radioButtonList.Add(radioButton15);

            CheckBoxList = new List<CheckBox>();
            CheckBoxList.Add(checkBox1);
            CheckBoxList.Add(checkBox2);
            CheckBoxList.Add(checkBox3);
            CheckBoxList.Add(checkBox4);
            CheckBoxList.Add(checkBox5);
            CheckBoxList.Add(checkBox6);
            CheckBoxList.Add(checkBox7);
            CheckBoxList.Add(checkBox8);
            CheckBoxList.Add(checkBox9);
            CheckBoxList.Add(checkBox10);
            CheckBoxList.Add(checkBox11);
            CheckBoxList.Add(checkBox12);
            CheckBoxList.Add(checkBox13);
            CheckBoxList.Add(checkBox14);
            CheckBoxList.Add(checkBox15);


            try
            {
                //serialPort.Open();
                interfaceBox.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem with the serial port: \n\n" + ex.GetType().ToString() + ex.StackTrace.ToString());
            }

        }

        private void InitializeSoundPlayer()
        {
            player = new System.Media.SoundPlayer();
            player.SoundLocation = soundFilePath;
            soundFileLoaded = true;

            try
            {
                player.Load();
            }
            catch
            {
                soundFileLoaded = false;
            }
            
        }

        private void interfaceBox_StateUpdated(InterfaceBoxState newState)
        {
            UpdateUI(newState);
        }

        // This method demonstrates a pattern for making thread-safe
        // calls on a Windows Forms control. 
        //
        // If the calling thread is different from the thread that
        // created the TextBox control, this method creates a
        // SetTextCallback and calls itself asynchronously using the
        // Invoke method.
        //
        // If the calling thread is the same as the thread that created
        // the TextBox control, the Text property is set directly. 

        private void UpdateUI(InterfaceBoxState newState)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                UpdatePadStateCallback d = new UpdatePadStateCallback(UpdateUI);
                this.Invoke(d, new object[] { newState });
            }
            else
            {
                // Check if this update is following the interface box being triggered from the 'armed' state               
                if (interfaceBox.State.ArmTriggered)
                {                 
                    

                    if (anyPadOn(newState))
                    {
                        lockedOut = true;
                        UITimer.Stop();                     
                        playBeepSound();
                    }                    
                    
                }

                int index = 0;
                foreach (Port currentPort in newState.Ports)
                {
                    foreach (Pad currentPad in currentPort.Pads)
                    {
                        if (CheckBoxList[index].Checked)
                        {
                            radioButtonList[index].Checked = currentPad.IsClosed;
                        }
                        else
                        {
                            radioButtonList[index].Checked = false;
                        }

                        index++;
                    }
                }

                //// Update the pad states on the UI
                //radioButton1.Checked = ! newState.Ports[0].Pads[0].IsClosed;
                //radioButton2.Checked = ! newState.Ports[0].Pads[1].IsClosed;
                //radioButton3.Checked = ! newState.Ports[0].Pads[2].IsClosed;
                //radioButton4.Checked = ! newState.Ports[0].Pads[3].IsClosed;
                //radioButton5.Checked = ! newState.Ports[0].Pads[4].IsClosed;
                //radioButton6.Checked = ! newState.Ports[1].Pads[0].IsClosed;
                //radioButton7.Checked = ! newState.Ports[1].Pads[1].IsClosed;
                //radioButton8.Checked = ! newState.Ports[1].Pads[2].IsClosed;
                //radioButton9.Checked = ! newState.Ports[1].Pads[3].IsClosed;
                //radioButton10.Checked = ! newState.Ports[1].Pads[4].IsClosed;

            }
        }

        private bool anyPadOn(InterfaceBoxState currentState)
        {
            bool anyPadOn = false;
            int index = 0;
            // Check if any seatpads on
            foreach (Port currentPort in currentState.Ports)
            {
                foreach (Pad currentPad in currentPort.Pads)
                {
                    if (currentPad.IsClosed && CheckBoxList[index].Checked)
                    {
                        anyPadOn = true;
                    }

                    index++;
                }
            }

            return anyPadOn;
        }

        private void playBeepSound()
        {
            if (soundFileLoaded)
            {
                player.Play();
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }
           
        private void button1_Click(object sender, EventArgs e)
        {
            //interfaceBox.Arm();
        }

        private void btnTimerStart_Click(object sender, EventArgs e)
        {
            if (lockedOut || anyPadOn(interfaceBox.State))
            {
                time = 30;
            }
            else
            {
                time = 5;
            }
            lblTimerDisplay.Text = time.ToString();
            UITimer.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            interfaceBox.Refresh();
            //UpdateUI();          
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            interfaceBox.dispose();
            player.Dispose();
        }

        private void UITimer_Tick(object sender, EventArgs e)
        {
            time--;
            lblTimerDisplay.Text = time.ToString();            
            if (time <= 0)
            {
                UITimer.Stop();
                playBeepSound();
            }            
            
        }

        private void btnReset_MouseDown(object sender, MouseEventArgs e)
        {
            lockedOut = false;
            CommTimer.Start();           
        }

        private void btnReset_MouseUp(object sender, MouseEventArgs e)
        {
            CommTimer.Stop();
            if (anyPadOn(interfaceBox.State))
            {
                UITimer.Stop();
                playBeepSound();
            }
            else
            {
                interfaceBox.Arm();
            }
        }

        private void btmTimerReset_Click(object sender, EventArgs e)
        {
            UITimer.Stop();
            time = 0;
            lblTimerDisplay.Text = time.ToString();
        }
    }
}
