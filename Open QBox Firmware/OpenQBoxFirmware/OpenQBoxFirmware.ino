bool locked;
bool armed;
unsigned int pinState;
unsigned int lastPinState;
unsigned int savedPinState;
int inChar;

//Test Code
unsigned char portDState;
unsigned char portBState;
unsigned char portCState;

void setup() 
{
  // Initialize variables
  armed = false;
  locked = false;
  pinState = 0;
  lastPinState = 0;
  savedPinState = 0;
  inChar = 0;
  portDState = 0;
  portBState = 0;
  portCState = 0;

  
  // Set up pins as inputs
  // Bits on Port B which map to socket 1 on the box
  pinMode(2, INPUT_PULLUP);
  pinMode(3, INPUT_PULLUP);
  pinMode(4, INPUT_PULLUP);
  pinMode(5, INPUT_PULLUP);
  pinMode(6, INPUT_PULLUP);
  
  // Bits on Port D which map to socket 2 on the box
  pinMode(8, INPUT_PULLUP);
  pinMode(9, INPUT_PULLUP);
  pinMode(10, INPUT_PULLUP);
  pinMode(11, INPUT_PULLUP);
  pinMode(12, INPUT_PULLUP);

  // Bits on Port C which map to socket 3 on the box
  pinMode(A0, INPUT_PULLUP);
  pinMode(A1, INPUT_PULLUP);
  pinMode(A2, INPUT_PULLUP);
  pinMode(A3, INPUT_PULLUP);
  pinMode(A4, INPUT_PULLUP);

  // Start serial connection
  Serial.setTimeout(100);
  Serial.begin(250000);

}

void loop() 
{
  // Code in this loop executes repeatedly
     updatePinState();
}

// This function polls each of the pins connected to the seat pads
void updatePinState()
{
    //A semaphore is used to prevent reading the lastPinState variable while it is updating
    //This could happen due to an interrupt from the serial port requesting a read of the variable 
    while(locked);
    locked = true;      
    lastPinState = pinState;
    //Release lock
    locked = false;

    //Read ports
    portCState = PINC & B00011111;
    portDState = PIND & B01111100;
    portBState = PINB & B00011111;    
    
    //Clear pinStateVariable
    pinState = pinState & 0x0000;
    
    //Store bits in pinStateVariable
    portCState = reverse(portCState);
    pinState = pinState | portCState;
    pinState = pinState << 2;   
    portDState = portDState >> 2;
    pinState = pinState | portDState; 
    pinState = pinState << 5;
    pinState = pinState | portBState;    
  
    checkForChange();  
}

void checkForChange()
{
  //Check if any of the ports are different than last check
  if (lastPinState != pinState) 
  {    
    //Check if the armed state was previously set
    if (armed)
    {
      savedPinState = pinState;
      //Set 16th bit as a flag denoting that the armed state was tripped
      savedPinState = savedPinState | 0x8000; //Bitmask: 1000000000000000
      //Clear armed state
      armed = false;
      //Transmit the new state
      sendSavedPinState();
    }
  }
}

void arm()
{
  armed = true;
}

void sendSavedPinState()
{
  Serial.println(savedPinState);
}

void sendLastPinState()
{
  //make sure we aren't updating the pinState variable before trying to send it
  while(locked);
  locked = true;
  //transmit code here
  Serial.println(lastPinState);
  locked = false;
}

// This function handles events on the serial port
void serialEvent()
{
  inChar = Serial.read(); 

  switch (inChar)
  {
    // an 'a' character will arm the box to report on state change
    case 'a':
    arm();
    break;

    // an 'r' character will cause the box to report it's last pin state
    case 'r':
    sendLastPinState();
    break;

    // do nothing for other input
    default:
    break;    
  }
}

//The following array and function are used to reverse the order of bits in a byte
//This was borrowed from an example on Stack Overflow by user deft_code

static unsigned char lookup[16] = {
0x0, 0x8, 0x4, 0xc, 0x2, 0xa, 0x6, 0xe,
0x1, 0x9, 0x5, 0xd, 0x3, 0xb, 0x7, 0xf, };

uint8_t reverse(uint8_t n) {
   // Reverse the top and bottom nibble then swap them.
   return (lookup[n&0b1111] << 4) | lookup[n>>4];
}


