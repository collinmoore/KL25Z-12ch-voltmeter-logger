/* ###################################################################
**     Filename    : main.c
**     Project     : UART_24ch_PWMdutyCycle
**     Processor   : MKL25Z128VLK4
**     Version     : Driver 01.01
**     Compiler    : GNU C Compiler
**     Date/Time   : 2015-07-21, 09:16, # CodeGen: 0
**     Abstract    :
**         Main module.
**         This module contains user's application code.
**     Settings    :
**     Contents    :
**         No public methods
**
** ###################################################################*/
/*!
** @file main.c
** @version 01.01
** @brief
**         Main module.
**         This module contains user's application code.
*/         
/*!
**  @addtogroup main_module main module documentation
**  @{
*/         
/* MODULE main */


/* Including needed modules to compile this module/procedure */
#include "Cpu.h"
#include "Events.h"
#include "DMA1.h"
#include "AS1.h"
#include "RxBuf.h"
#include "PWM1.h"
#include "PwmLdd1.h"
#include "TU1.h"
#include "AD1.h"
#include "AdcLdd1.h"
#include "TU2.h"
/* Including shared modules, which are used for whole project */
#include "PE_Types.h"
#include "PE_Error.h"
#include "PE_Const.h"
#include "IO_Map.h"

/* User includes (#include below this line is not maintained by Processor Expert) */
#include "data.h"
#include "UART.h"

const uint16 PWM_RATIO_TX = 0x1DFFU;
const uint16 PWM_RATIO_RX = 0x2DFFU;
const uint16 PWM_RATIO_IDLE = 0x32FFU;
const uint16 PWM_RATIO_OFF = 0xFFFFU; 
const uint16 PWM_RATIO_100MA = 0x32FFU;
const uint16 PWM_RATIO_250MA = 0x2DFFU;
const uint16 PWM_RATIO_500MA = 0x27FFU;
const uint16 PWM_RATIO_750MA = 0x247FU;
const uint16 PWM_RATIO_1000MA = 0x225FU;
const uint16 PWM_RATIO_1500MA = 0x1DFFU;
const uint16 PWM_RATIO_2000MA = 0x1BFFU;
const uint16 PWM_RATIO_2500MA = 0x1A9FU;
const uint16 PWM_RATIO_3000MA = 0x1907U;


const uint16 ADC_COUNT_100MA = 0x04D0U;
const uint16 ADC_COUNT_250MA = 0x0B50U;
const uint16 ADC_COUNT_500MA = 0x1720U;
const uint16 ADC_COUNT_750MA = 0x2200U;
const uint16 ADC_COUNT_1000MA = 0x2DB0U;
const uint16 ADC_COUNT_1500MA = 0x4480U;
const uint16 ADC_COUNT_2000MA = 0x5AF0U;
const uint16 ADC_COUNT_2500MA = 0x71B0U;
const uint16 ADC_COUNT_3000MA = 0x8890U;

const uint16 ADC_COUNT_IDLE = 0x0B50U;
const uint16 ADC_COUNT_RX = 0x0B50U;
const uint16 ADC_COUNT_TX = 0x4480U;

/**************    control loop variables    ********************/
uint16 error = 0x0002; // used to determine the difference between the current and desired current

bool firstDutySet = TRUE;
bool oneSecondTick = FALSE;
uint8 status = ERR_OK;
uint8 dutyCycle = 0; // set duty cycle to 0 by default
uint32 dutyCycleCounter = 0; // counter for where the duty cycle is
uint16 pwmRatio = 0xFFFFU;
uint16 voltageValues[AD1_CHANNEL_COUNT] = {0}; // voltageValues stores the ADC counts
uint8 adcReadStatus = ERR_OK;
/*lint -save  -e970 Disable MISRA rule (6.3) checking. */
int main(void)
/*lint -restore Enable MISRA rule (6.3) checking. */
{
  /* Write your local variable definition here */


  /*** Processor Expert internal initialization. DON'T REMOVE THIS CODE!!! ***/
  PE_low_level_init();
  /*** End of Processor Expert internal initialization.                    ***/

  /* Write your code here */
  /* Enable the onboard RGB LED 
   *  port B pin 18 is red,
   *  port B pin 19 is green,
   *  port D pin 1 is blue         */
	SIM_SCGC5 |=0x00001400U; // enable port B and D clock control
	PORTB_PCR18 &=0xFFF0F8FFU; // disable interrupt requests from port B pin 18
	PORTB_PCR19 &=0xFFF0F8FFU; // disable interrupt requests from port B pin 19
	PORTD_PCR1 &=0xFFF0F0FFU; // disable interrupt requests from port D pin 1
	PORTB_PCR18 |=0x00000140U; // set port B pin 18 to have Drive Strength Enable
	PORTB_PCR19 |=0x00000140U; // set port B pin 19 to have Drive Strength Enable
	PORTD_PCR1 |=0x00000140U; // set port D pin 1 to have Drive Strength Enable (extra current to drive LED)
	GPIOB_PDDR |=0x000C0000U; // set gpio port B pins 18 and 19 to output
	GPIOD_PDDR |=0x00000002U; // set gpio port D pin 1 to output
	GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
	GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
	GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
  AD1_Calibrate(1); // Calibrate ADC and wait until done.
  UART_Init(); // initialize the UART
  (void)TU2_Init(NULL); // start 1-second timer for duty cycle
	
  for(;;) {
	  if(haveReceivedPacket){
		  // light to yellow for RX
			GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
			GPIOB_PCOR = 0x00080000U; // set pin B19 green to low, off (active low LEDs)
			GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
		  UART_ParseData();
		  // turn off after transmit
			GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
			GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
			GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
	  }
	  if(oneSecondTick){
		  // light to  teal for measurement
			GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
			GPIOB_PCOR = 0x00080000U; // set pin B19 green to low, on (active low LEDs)
			GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
		  	AD1_Measure(1); // measure from the ADC and wait for values
		  	status=AD1_GetValue16(voltageValues); // store all twelve ADC counts into the voltageValues array
		  	uint8 * halfword_ptr =(uint8*)&voltageValues[7]; // temporary pointer to get out data from the little-endian bytes in the word
		  	uint16 sixteenBitCurrentCount = *halfword_ptr; 
		  	sixteenBitCurrentCount |= (*(++halfword_ptr) <<8); // swap bytes from little to big endian
		  // turn off lights after measurement
			GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
			GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
			GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
		  switch(dutyCycle){
			case 0x00U:
				/* 0x00 is off, both circuits off all the time
				 */
				// start with no light for no load
				GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs) 
				pwmRatio = PWM_RATIO_OFF; // set PWM duty cycle to 0%, for off
				break;
			case 0x01U:
				/* 0x01 is 10-10-80 duty cycle:
				 * 	6 seconds tx mode
				 * 	6 seconds rx mode
				 * 	48 seconds standby circuit only
				 */
				if(dutyCycleCounter<0x06U){ // if less than 6 seconds, should be in TX mode
					// white for most current draw
					GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
					GPIOB_PCOR = 0x00080000U; // set pin B19 green to low, on (active low LEDs)
					GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_TX; // set PWM duty cycle for transmit
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_TX) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				if(dutyCycleCounter==0x06U) firstDutySet = TRUE;
				else if(dutyCycleCounter<0x0CU){ // if more than 6 seconds but less than 12 seconds, should be in rx mode			
					// purple for second highest load
					GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_RX; // set PWM duty cycle for receive
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_RX) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				if(dutyCycleCounter==0x0CU) firstDutySet = TRUE;
				else if(dutyCycleCounter<0x3CU){ // if more than 12 seconds but less than 60 seconds, should be in standby mode
					/* AS OF JUNE 1 2015, STANDBY MODE == RX MODE */
					// blue for least current draw
					GPIOB_PSOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_IDLE; // set PWM duty cycle for idle
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_IDLE) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				else{  // reset counter
					dutyCycleCounter=0;
					firstDutySet = TRUE;
				}
				break;
			case 0x02U:
				/* 0x02 is for 5-5-90 duty cycle:
				 * 	3 seconds tx mode,
				 * 	3 seconds rx mode,
				 * 	54 seconds standby circuit only
				 */
				if(dutyCycleCounter<0x03U){ // if less than 3 seconds, should be in TX mode
					// RED for most current draw
					GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_TX; // set PWM duty cycle for transmit
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_TX) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				if(dutyCycleCounter==0x06U) firstDutySet = TRUE;
				else if(dutyCycleCounter<0x06U){ // if more than 3 seconds but less than 6 seconds, should be in rx mode
					// purple for receive current draw
					GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_RX; // set PWM duty cycle for receive
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_RX) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				if(dutyCycleCounter==0x3CU) firstDutySet = TRUE;
				else if(dutyCycleCounter<0x3CU){ // if more than 6 seconds but less than 60 seconds, should be in standby mode
					/* AS OF JUNE 1 2015, STANDBY MODE == RX MODE */
					// blue for least current draw
					GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_IDLE; // set PWM duty cycle for idle
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_IDLE) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				else{  // reset counter
					dutyCycleCounter=0;
					firstDutySet = TRUE;
				}
				break;
			case 0x03U:
				/* 0x03 is for 5-45-50 duty cycle:
				 * 3 seconds tx mode
				 * 27 seconds rx mode
				 * 30 seconds standby circuit only
				 */
				if(dutyCycleCounter<0x03U){ // if less than 3 seconds, should be in TX mode
					// red for transmit current draw
					GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_TX; // set PWM duty cycle for transmit
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_TX) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				if(dutyCycleCounter==0x03U) firstDutySet = TRUE;
				else if(dutyCycleCounter<0x1EU){ // if more than 3 seconds but less than 30 seconds, should be in rx mode
					// purple for receive current draw
					GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_RX; // set PWM duty cycle for receive
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_RX) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				if(dutyCycleCounter==0x1EU) firstDutySet = TRUE;
				else if(dutyCycleCounter<0x3CU){ // if more than 30 seconds but less than 60 seconds, should be in standby mode
					/* AS OF JUNE 1 2015, STANDBY MODE == RX MODE */
					// blue for least current draw
					GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
					GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
					GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
					if(firstDutySet){
						pwmRatio = PWM_RATIO_IDLE; // set PWM duty cycle for idle
						firstDutySet = FALSE;
					}
					else{
						if(sixteenBitCurrentCount > ADC_COUNT_IDLE) pwmRatio+=error;
						else pwmRatio-=error;
					}
					dutyCycleCounter++;
				}
				else{ // reset counter
					dutyCycleCounter=0;
					firstDutySet = TRUE;
				}
				break;
			case 0x04U:
				/* 0x04 is for continuous 100mA load */
				// blue for <1A current draw
				GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_100MA; // set PWM duty cycle to 100mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_100MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
			case 0x05U:
				/* 0x05 is for continuous 250mA load */
				// blue for <1A current draw
				GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_250MA; // set PWM duty cycle to 250mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_250MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
			case 0x06U:
				/* 0x06 is for continuous 500mA load */
				// blue for <1A current draw
				GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_500MA; // set PWM duty cycle to 500mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_500MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
			case 0x07U:
				/* 0x07 is for continuous 750mA load */
				// purple for >700mA, <2A current draw
				GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_750MA; // set PWM duty cycle to 750mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_750MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
			case 0x08U:
				/* 0x08 is for continuous 1000mA load */
				// purple for >700mA, <2A current draw
				GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_1000MA; // set PWM duty cycle to 1000mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_1000MA) pwmRatio+=error;
					else pwmRatio-=error;
					/*if(sixteenBitCurrentCount > ADC_COUNT_1000MA){
						error = sixteenBitCurrentCount-ADC_COUNT_1000MA;
						pwmRatio+=error;
					}
					else{
						error = ADC_COUNT_1000MA-sixteenBitCurrentCount;
						pwmRatio-=error;
					}*/
				}				
				break;
			case 0x09U:
				/* 0x09 is for continuous 1500mA load */
				// purple for >700mA, <2A current draw
				GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_1500MA; // set PWM duty cycle to 1500mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_1500MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
			case 0x0A:
				/* 0x0A is for 2000mA load */
				// white for >=2A, <=3A current draw
				GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
				GPIOB_PCOR = 0x00080000U; // set pin B19 green to low, on (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_2000MA; // set PWM duty cycle to 2000mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_2000MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
			case 0x0B:
				/*0x0B is for continuous 2500mA load */
				// white for >=2A, <=3A current draw
				GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
				GPIOB_PCOR = 0x00080000U; // set pin B19 green to low, on (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_2500MA; // set PWM duty cycle to 2500mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_2500MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
			case 0x0C:
				/* 0x0C is for continuous 3000mA load */
				// white for >=2A, <=3A current draw
				GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
				GPIOB_PCOR = 0x00080000U; // set pin B19 green to low, on (active low LEDs)
				GPIOD_PCOR = 0x00000002U; // set pin D1 blue to low, on (active low LEDs)
				if(firstDutySet){
					pwmRatio = PWM_RATIO_3000MA; // set PWM duty cycle to 3000mA draw
					firstDutySet = FALSE;
				}
				else{
					if(sixteenBitCurrentCount > ADC_COUNT_3000MA) pwmRatio+=error;
					else pwmRatio-=error;
				}
				break;
				
			default:
				/* default is to catch errors
				 * 	set to off mode continuously
				 */
				dutyCycle = 0x00U; // set to off mode
				GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
				GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
				GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
				pwmRatio = PWM_RATIO_OFF; // set PWM duty cycle to 0%, for off
				break;
		}
		PWM1_SetRatio16(pwmRatio);
	  }
  }

  /*** Don't write any code pass this line, or it will be deleted during code generation. ***/
  /*** RTOS startup code. Macro PEX_RTOS_START is defined by the RTOS component. DON'T MODIFY THIS CODE!!! ***/
  #ifdef PEX_RTOS_START
    PEX_RTOS_START();                  /* Startup of the selected RTOS. Macro is defined by the RTOS component. */
  #endif
  /*** End of RTOS startup code.  ***/
  /*** Processor Expert end of main routine. DON'T MODIFY THIS CODE!!! ***/
  for(;;){}
  /*** Processor Expert end of main routine. DON'T WRITE CODE BELOW!!! ***/
} /*** End of main routine. DO NOT MODIFY THIS TEXT!!! ***/

/* END main */
/*!
** @}
*/
/*
** ###################################################################
**
**     This file was created by Processor Expert 10.3 [05.09]
**     for the Freescale Kinetis series of microcontrollers.
**
** ###################################################################
*/
