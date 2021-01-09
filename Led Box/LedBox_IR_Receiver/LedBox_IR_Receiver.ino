//www.elegoo.com
//2016.12.9

#include "IRremote.h"

int receiver = 11; // Signal Pin of IR receiver to Arduino Digital Pin 11
int led_time = 1;
int led = 7;
bool trigger_blink = false;

/*-----( Declare objects )-----*/
IRrecv irrecv(receiver);     // create instance of 'irrecv'
decode_results results;      // create instance of 'decode_results'

/*-----( Function )-----*/
void translateIR() // takes action based on IR code received

// describing Remote IR codes 

{

  switch(results.value)

  {
  case 0xFFA25D: Serial.println("POWER"); break;
  case 0xFFE21D: Serial.println("FUNC/STOP"); break;
  case 0xFF629D: Serial.println("VOL+"); break;
  case 0xFF22DD: Serial.println("FAST BACK");    break;
  case 0xFF02FD: Serial.println("PAUSE");  trigger_blink=true;  break;
  case 0xFFC23D: Serial.println("FAST FORWARD");   break;
  case 0xFFE01F: Serial.println("DOWN");    break;
  case 0xFFA857: Serial.println("VOL-");    break;
  case 0xFF906F: Serial.println("UP");    break;
  case 0xFF9867: Serial.println("EQ");    break;
  case 0xFFB04F: Serial.println("ST/REPT");    break;
  case 0xFF6897: Serial.println("0"); led_time=0;   break;
  case 0xFF30CF: Serial.println("1"); led_time=1;  break;
  case 0xFF18E7: Serial.println("2"); led_time=2;   break;
  case 0xFF7A85: Serial.println("3"); led_time=3;   break;
  case 0xFF10EF: Serial.println("4"); led_time=4;   break;
  case 0xFF38C7: Serial.println("5"); led_time=5;   break;
  case 0xFF5AA5: Serial.println("6"); led_time=6;   break;
  case 0xFF42BD: Serial.println("7"); led_time=7;   break;
  case 0xFF4AB5: Serial.println("8"); led_time=8;   break;
  case 0xFF52AD: Serial.println("9"); led_time=9;   break;
  case 0xFFFFFFFF: Serial.println(" REPEAT");break;  

  default: 
    Serial.println(" other button   ");

  }// End Case

  delay(500); // Do not get immediate repeat


} //END translateIR
void setup()   /*----( SETUP: RUNS ONCE )----*/
{
  pinMode(led, OUTPUT);
  Serial.begin(9600);
  Serial.println("IR Receiver Button Decode"); 
  irrecv.enableIRIn(); // Start the receiver

}/*--(end setup )---*/


void loop()   /*----( LOOP: RUNS CONSTANTLY )----*/
{
  if (irrecv.decode(&results)) // have we received an IR signal?
  {
    translateIR(); 
    irrecv.resume(); // receive the next value
  }  

  if(trigger_blink){
    digitalWrite(led, HIGH);   // turn the LED on (HIGH is the voltage level)
    delay(led_time*1000);                       // wait for a second
    digitalWrite(led, LOW);    // turn the LED off by making the voltage LOW
    //delay(1000);                       // wait for a second
    trigger_blink=false;
    }
}/* --(end main loop )-- */
